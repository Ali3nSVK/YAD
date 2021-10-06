using NAudio.Lame;
using NAudio.Wave;
using System;
using System.IO;

namespace YAD.Audio.Utils
{
    public class AudioFileWriter
    {
        private readonly TargetType outputFormat;
        private readonly WaveFormat outputWaveFormat;
        private readonly Stream writer;
        private readonly string audioFile;

        public AudioFileWriter(TargetType targetFormat, WaveFormat waveFormat, string file)
        {
            outputFormat = targetFormat;
            audioFile = file;
            outputWaveFormat = new WaveFormat(waveFormat.SampleRate, waveFormat.Channels);

            switch (outputFormat)
            {
                case TargetType.Wav:
                    writer = new WaveFileWriter(audioFile, outputWaveFormat);
                    break;
                case TargetType.Mp3:
                    writer = new LameMP3FileWriter(audioFile, outputWaveFormat, AudioConstants.DefaultBitRate);
                    break;
                default:
                    throw new NotSupportedException("Output format not supported.");
            }
        }

        public void WriteToFile(ISampleProvider provider, int count)
        {
            float[] tempBuffer = new float[count];
            int samplesReceived = provider.Read(tempBuffer, AudioConstants.DefaultBufferOffset, count);

            WaveBuffer outputBuffer = AudioHelper.GetByteBuffer(tempBuffer, samplesReceived);

            writer.Write(outputBuffer, AudioConstants.DefaultBufferOffset, outputBuffer.ByteBufferCount);
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }
}
