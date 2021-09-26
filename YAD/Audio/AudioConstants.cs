namespace YAD.Audio
{
    public static class AudioConstants
    {
        public const int DefaultSampleRate = 44100;
        public const int DefaultBufferOffset = 0;
        public const int AllChannels = 0;
        public const int MonoChannel = 1;
        public const int DefaultBits = 16;
    }

    public static class YADConstants
    {
        public const string LoopbackDevice = "YAD_WASAPI_LOOPBACK_ADAPTER";
        public const string LoopbackDeviceFriendly = "YAD Loopback Capture";
    }

    public enum State
    {
        Active,
        Idle
    }
}
