using YAD.Audio.Utils;

namespace YAD.Audio
{
    public class AudioSettings
    {
        public float GainLevel { get; set; } = 1.0f;
        public TargetType TargetFormat { get; set; } = TargetType.Wav;
        public int Channel { get; set; } = AudioConstants.AllChannels;
    }
}
