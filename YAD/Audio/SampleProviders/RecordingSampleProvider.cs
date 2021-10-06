using NAudio.Wave;
using YAD.Audio.Utils;

namespace YAD.Audio.SampleProviders
{
    public class RecordingSampleProvider : ISampleProvider
    {
        public WaveFormat WaveFormat { get; private set; }
        public int SamplesRecorded { get; private set; }
        private readonly float[] sourceBuffer;

        public RecordingSampleProvider(WaveFormat waveFormat, WaveInEventArgs e)
        {
            WaveFormat = waveFormat;
            WaveBuffer inputBuffer = AudioHelper.GetFloatBuffer(e);

            sourceBuffer = inputBuffer.FloatBuffer;
            SamplesRecorded = inputBuffer.FloatBufferCount;
        }

        /// <summary>
        /// Ignores count because it is not possible to request a certain amount of samples due to how
        /// capture inputs work. Share mode capture does not reliably return a certain amount of samples.
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            for (int i = 0; i < SamplesRecorded; i++)
            {
                buffer[offset + i] = sourceBuffer[i];
            }

            return SamplesRecorded;
        }
    }
}
