using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YAD.Audio.SampleProviders;

namespace YAD.Audio
{
    public class AudioWrapper
    {
        private WasapiCapture wasapiCapture;
        private WaveFileWriter waveFileWriter;
        private WaveFormat outputFormat;

        private int selectedChannel;
        private string audioFile;

        public MMDeviceCollection DeviceCollection { get; }
        public List<DeviceContainer> CaptureDeviceCollection { get; }
        public State RecordingState { get; private set; }

        public AudioWrapper()
        {
            int deviceNumber = 0;
            MMDeviceEnumerator devEnum = new MMDeviceEnumerator();

            DeviceCollection = devEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            CaptureDeviceCollection = DeviceCollection.Select(device => new DeviceContainer(device, deviceNumber++)).ToList();
            CaptureDeviceCollection.Add(AudioHelper.LoopbackDevice(deviceNumber++));

            RecordingState = State.Idle;
        }

        #region Recording

        public void RecordFromDevice(DeviceContainer device, int channel)
        {
            RecordingState = State.Active;
            selectedChannel = channel;

            WasapiCaptureStart(DeviceById(device.ID), false);
        }

        public void RecordLoopback()
        {
            RecordingState = State.Active;

            WasapiCaptureStart(null, true);
        }

        public async Task<string> StopRecording()
        {
            wasapiCapture.StopRecording();

            await Task.Run(() =>
            {
                while (RecordingState == State.Active) { }
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
                InitializeWaveFormat(wasapiCapture.WaveFormat);
            }
            wasapiCapture.DataAvailable += OnDataAvailable;
            wasapiCapture.RecordingStopped += OnRecordingStopped;

            audioFile = Path.GetTempFileName();

            waveFileWriter = new WaveFileWriter(audioFile, wasapiCapture.WaveFormat);
            wasapiCapture.StartRecording();
        }

        private WaveFormat InitializeWaveFormat(WaveFormat intputWaveFormat)
        {
            outputFormat = WaveFormat.CreateIeeeFloatWaveFormat(intputWaveFormat.SampleRate, intputWaveFormat.Channels);
            return outputFormat;
        }

        #endregion

        #region Event Handlers

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            ISampleProvider outputProvider;

            RecordingSampleProvider inputProvider = new RecordingSampleProvider(outputFormat, e);
            int samplesRecorded = inputProvider.SamplesRecorded;
            outputProvider = inputProvider;

            if (selectedChannel != AudioConstants.AllChannels)
            {
                StripingSampleProvider channelStripper = new StripingSampleProvider(inputProvider)
                {
                    RetainChannel = selectedChannel - 1
                };
                outputProvider = channelStripper;
            }

            AudioHelper.SampleProviderWriter(waveFileWriter, outputProvider, samplesRecorded);
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (wasapiCapture != null)
            {
                wasapiCapture.Dispose();
                wasapiCapture = null;
            }

            if (waveFileWriter != null)
            {
                waveFileWriter.Dispose();
                waveFileWriter = null;
            }

            RecordingState = State.Idle;
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
