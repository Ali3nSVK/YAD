using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace YAD.Audio.Utils
{
    public class AudioPlayer
    {
        private readonly WasapiOut player;
        private readonly WaveFormat waveFormat;
        private readonly BufferedWaveProvider buffer;

        private const int latencyMs = 10;

        public AudioPlayer(WaveFormat format)
        {
            waveFormat = format;

            buffer = new BufferedWaveProvider(waveFormat);
            player = new WasapiOut(AudioClientShareMode.Shared, latencyMs);
            player.Init(buffer);

            Play();
        }

        public void Enqueue(IWaveProvider provider, int samples)
        {
            int bytesExpected = samples * 4;
            byte[] playbackBuffer = new byte[bytesExpected];
            int bytesReceived = provider.Read(playbackBuffer, AudioConstants.DefaultBufferOffset, bytesExpected);

            buffer.AddSamples(playbackBuffer, AudioConstants.DefaultBufferOffset, bytesReceived);
        }

        public void Enqueue(ISampleProvider provider, int samples)
        {
            Enqueue(provider.ToWaveProvider(), samples);
        }

        public void Play()
        {
            player.Play();
        }

        public void Stop()
        {
            player.Stop();
        }
    }
}
