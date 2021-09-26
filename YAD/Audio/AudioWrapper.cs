using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace YAD.Audio
{
    public class AudioWrapper
    {
        private WasapiCapture wasapiCapture;
        private WaveFileWriter waveWriter;
        private string audioFile;
        private int selectedChannel;

        public MMDeviceCollection DeviceCollection { get; }
        public List<DeviceContainer> CaptureDeviceCollection { get; }
        public State RecordingState { get; private set; }

        public AudioWrapper()
        {
            int deviceNumber = 0;
            MMDeviceEnumerator devEnum = new MMDeviceEnumerator();

            DeviceCollection = devEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            CaptureDeviceCollection = DeviceCollection.Select(device => new DeviceContainer(device, deviceNumber++)).ToList();
            CaptureDeviceCollection.Add(LoopbackDevice(deviceNumber++));

            RecordingState = State.Idle;
        }

        #region Recording

        public void RecordFromDevice(string deviceId, int channel)
        {
            RecordingState = State.Active;
            selectedChannel = channel;

            WasapiCaptureStart(deviceId, false);
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

            if (selectedChannel != AudioConstants.AllChannels)
            {
                audioFile = ExtractSpecificWaveChannel(audioFile);
            }

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

        private void WasapiCaptureStart(string deviceId, bool loopback)
        {
            if (!loopback)
            {
                wasapiCapture = new WasapiCapture(DeviceById(deviceId))
                {
                    ShareMode = AudioClientShareMode.Shared
                };
                wasapiCapture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(wasapiCapture.WaveFormat.SampleRate, wasapiCapture.WaveFormat.Channels);
            }
            else
            {
                wasapiCapture = new WasapiLoopbackCapture();
            }
            wasapiCapture.DataAvailable += OnDataAvailable;
            wasapiCapture.RecordingStopped += OnRecordingStopped;

            audioFile = Path.GetTempFileName();
            wasapiCapture.StartRecording();
        }

        private void WasapiLoopbackStart()
        {
            wasapiCapture = new WasapiLoopbackCapture();
            wasapiCapture.DataAvailable += OnDataAvailable;
            wasapiCapture.RecordingStopped += OnRecordingStopped;

            wasapiCapture.StartRecording();
        }

        private string ExtractSpecificWaveChannel(string file)
        {
            string tempFile = Path.GetTempFileName();
            WaveFileReader reader = new WaveFileReader(file);
            WaveFormat format = WaveFormat.CreateIeeeFloatWaveFormat(reader.WaveFormat.SampleRate, AudioConstants.MonoChannel);
            WaveFileWriter writer = new WaveFileWriter(tempFile, format);

            float[] buffer;
            while ((buffer = reader.ReadNextSampleFrame())?.Length > 0)
            {
                writer.WriteSample(buffer[selectedChannel - 1]);
            }

            writer.Dispose();
            reader.Dispose();
            File.Delete(file);

            return tempFile;
        }

        private DeviceContainer LoopbackDevice(int number)
        {
            return new DeviceContainer
            (
                YADConstants.LoopbackDevice,
                number,
                AudioConstants.AllChannels,
                DeviceState.Active.ToString(),
                YADConstants.LoopbackDeviceFriendly
            ); ;
        }

        #endregion

        #region Event Handlers

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveWriter == null)
            {
                waveWriter = new WaveFileWriter(audioFile, wasapiCapture.WaveFormat);
            }
            else
            {
                waveWriter.Write(e.Buffer, AudioConstants.DefaultBufferOffset, e.BytesRecorded);
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

            RecordingState = State.Idle;
        }

        #endregion
    }
}
