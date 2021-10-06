using NAudio.Wave;

namespace YAD.Audio.SampleProviders
{
    public class StripingSampleProvider : ISampleProvider
    {
        public WaveFormat WaveFormat => sourceProvider.WaveFormat;
        public int RetainChannel { get; set; }
        private readonly ISampleProvider sourceProvider;

        public StripingSampleProvider(ISampleProvider provider)
        {
            sourceProvider = provider;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int channels = sourceProvider.WaveFormat.Channels;
            float[] sourceBuffer = new float[count];

            int samplesRead = sourceProvider.Read(sourceBuffer, offset, count);

            for (int i = 0; i < samplesRead; i += channels)
            {
                for (int j = 0; j < channels; j++)
                {
                    buffer[offset + i + j] = sourceBuffer[i] / channels;
                }
            }

            return samplesRead;
        }
    }
}
