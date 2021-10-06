using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YAD.Audio.SampleProviders;
using YAD.Audio.Utils;

namespace YAD.Audio
{
    public class AudioEngine
    {
        private WasapiCapture wasapiCapture;
        private AudioFileWriter waveWriter;
        private AudioPlayer player;
        private WaveFormat outputFormat;

        private string audioFile;

        public AudioSettings Settings { get; }
        public MMDeviceCollection DeviceCollection { get; }
        public List<DeviceContainer> CaptureDeviceCollection { get; }
        public RecordingState RecordingState { get; private set; }

        public AudioEngine()
        {
            int deviceNumber = 0;
            MMDeviceEnumerator devEnum = new MMDeviceEnumerator();

            DeviceCollection = devEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            CaptureDeviceCollection = DeviceCollection.Select(device => new DeviceContainer(device, deviceNumber++)).ToList();
            CaptureDeviceCollection.Add(AudioHelper.LoopbackDevice(deviceNumber++));

            Settings = new AudioSettings();

            RecordingState = RecordingState.Idle;
        }

        #region Recording

        public void RecordFromDevice(DeviceContainer device)
        {
            RecordingState = RecordingState.Active;

            WasapiCaptureStart(DeviceById(device.ID), false);
        }

        public void RecordLoopback()
        {
            RecordingState = RecordingState.Active;

            WasapiCaptureStart(null, true);
        }

        public async Task<string> StopRecording()
        {
            wasapiCapture.StopRecording();

            await Task.Run(() =>
            {
                while (RecordingState == RecordingState.Active) { }
            });

            return audioFile;
        }

        public void AudioDataSubscriber(EventHandler<WaveInEventArgs> DataAvailableHandler)
        {
            if (wasapiCapture != null)
            {
                wasapiCapture.DataAvailable += DataAvailableHandler;
            }
        }

        #endregion

        #region Wasapi

        private MMDevice DeviceById(string id)
        {
            return DeviceCollection.Single(d => d.ID == id);
        }

        private void WasapiCaptureStart(MMDevice device, bool loopback)
        {
            if (!loopback)
            {
                wasapiCapture = new WasapiCapture(device)
                {
                    ShareMode = AudioClientShareMode.Shared
                };
                wasapiCapture.WaveFormat = InitializeWaveFormat(wasapiCapture.WaveFormat);
            }
            else
            {
                wasapiCapture = new WasapiLoopbackCapture();
                _ = InitializeWaveFormat(wasapiCapture.WaveFormat);

            }
            
            wasapiCapture.DataAvailable += OnDataAvailable;
            wasapiCapture.RecordingStopped += OnRecordingStopped;

            WasapiOutputInit(Settings.TargetFormat);
            wasapiCapture.StartRecording();
        }

        private WaveFormat InitializeWaveFormat(WaveFormat intputWaveFormat)
        {
            outputFormat = WaveFormat.CreateIeeeFloatWaveFormat(intputWaveFormat.SampleRate, intputWaveFormat.Channels);
            return outputFormat;
        }

        private void WasapiOutputInit(TargetType target)
        {
            if (target == TargetType.Monitor)
            {
                player = new AudioPlayer(outputFormat);
            }
            else
            {
                audioFile = Path.GetTempFileName();
                waveWriter = new AudioFileWriter(Settings.TargetFormat, outputFormat, audioFile);
            }
        }

        #endregion

        #region Event Handlers

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            ISampleProvider outputProvider;

            RecordingSampleProvider inputProvider = new RecordingSampleProvider(outputFormat, e);
            int samplesRecorded = inputProvider.SamplesRecorded;
            outputProvider = inputProvider;

            if (Settings.Channel != AudioConstants.AllChannels)
            {
                StripingSampleProvider channelStripper = new StripingSampleProvider(inputProvider)
                {
                    RetainChannel = Settings.Channel - 1
                };
                outputProvider = channelStripper;
            }

            VolumeSampleProvider volumeProvider = new VolumeSampleProvider(outputProvider)
            {
                Volume = Settings.GainLevel
            };
            outputProvider = volumeProvider;

            if (Settings.TargetFormat == TargetType.Monitor)
            {
                player.Enqueue(outputProvider, samplesRecorded);
            }
            else
            {
                waveWriter.WriteToFile(outputProvider, samplesRecorded);
            }
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (wasapiCapture != null)
            {
                wasapiCapture.Dispose();
                wasapiCapture = null;
            }

            if (waveWriter != null)
            {
                waveWriter.Dispose();
                waveWriter = null;
            }

            if (player != null)
            {
                player.Stop();
                player = null;
            }

            RecordingState = RecordingState.Idle;
        }

        #endregion

        #region Misc

        public string GetDeviceCapabilities(DeviceContainer device)
        {
            if (device.ID == YADConstants.LoopbackDevice)
                return YADConstants.LoopbackDeviceFriendly;

            return AudioHelper.GetWasapiDeviceCapabilities(DeviceById(device.ID));
        }

        #endregion
    }
}
