using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Text;

namespace YAD.Audio.Utils
{
    public static class AudioHelper
    {
        public static DeviceContainer LoopbackDevice(int number)
        {
            return new DeviceContainer
            (
                YADConstants.LoopbackDevice,
                number,
                AudioConstants.AllChannels,
                RecordingState.Active.ToString(),
                YADConstants.LoopbackDeviceFriendly
            ); ;
        }

        public static WaveBuffer GetFloatBuffer(WaveInEventArgs e)
        {
            WaveBuffer tmpBuffer = new WaveBuffer(e.Buffer)
            {
                FloatBufferCount = e.BytesRecorded / 4
            };

            return tmpBuffer;
        }

        public static WaveBuffer GetByteBuffer(float[] buffer, int samples)
        {
            byte[] pcm = new byte[samples * 2];
            int sampleIndex = 0,
                pcmIndex = 0;

            while (sampleIndex < samples)
            {
                var outsample = (short)(buffer[sampleIndex] * short.MaxValue);
                pcm[pcmIndex] = (byte)(outsample & 0xff);
                pcm[pcmIndex + 1] = (byte)((outsample >> 8) & 0xff);

                sampleIndex++;
                pcmIndex += 2;
            }

            WaveBuffer tmpBuffer = new WaveBuffer(pcm)
            {
                ByteBufferCount = samples * 2
            };

            return tmpBuffer;
        }

        public static string GetWasapiDeviceCapabilities(MMDevice device)
        {
            StringBuilder sb = new StringBuilder();

            using (WasapiCapture c = new WasapiCapture(device))
            {
                sb.AppendFormat("{0}: {1}", "Name", device.FriendlyName).AppendLine();
                sb.AppendFormat("{0}: {1}", "Channels", c.WaveFormat.Channels).AppendLine();
                sb.AppendFormat("{0}: {1}", "Sample rate", c.WaveFormat.SampleRate).AppendLine();
                sb.AppendFormat("{0}: {1}", "Average B/s", c.WaveFormat.AverageBytesPerSecond).AppendLine();
                sb.AppendFormat("{0}: {1}", "Bits per sample", c.WaveFormat.BitsPerSample).AppendLine();

                sb.AppendFormat("{0}: {1}", "ID", device.ID).AppendLine();
                sb.AppendFormat("{0}: {1}", "Instance ID", device.InstanceId).AppendLine();
            }

            return sb.ToString();
        }
    }
}
