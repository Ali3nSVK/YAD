using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Text;

namespace YAD.Audio
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
                State.Active.ToString(),
                YADConstants.LoopbackDeviceFriendly
            ); ;
        }

        public static WaveBuffer GetFloatBuffer(WaveInEventArgs e)
        {
            WaveBuffer tmpBuffer = new WaveBuffer(e.Buffer);
            tmpBuffer.FloatBufferCount = e.BytesRecorded / 4;

            return tmpBuffer;
        }

        public static void SampleProviderWriter(WaveFileWriter writer, ISampleProvider provider, int count)
        {
            float[] outputBuffer = new float[count];
            int samplesReceived = provider.Read(outputBuffer, AudioConstants.DefaultBufferOffset, count);

            for (int i = 0; i < samplesReceived; i++)
            {
                writer.WriteSample(outputBuffer[i]);
            }
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
