﻿namespace YAD.Audio.Utils
{
    public static class AudioConstants
    {
        public const int DefaultSampleRate = 44100;
        public const int DefaultBufferOffset = 0;
        public const int AllChannels = 0;
        public const int MonoChannel = 1;
        public const int DefaultBits = 16;
        public const int DefaultBitRate = 192;
    }

    public static class YADConstants
    {
        public const string LoopbackDevice = "YAD_WASAPI_LOOPBACK_ADAPTER";
        public const string LoopbackDeviceFriendly = "YAD Loopback Capture";
        public const string DialogWavFilter = "Waveform Audio Files (*.wav)|*.wav";
        public const string DialogMp3Filter = "MP3 Files (*.mp3)|*.mp3";
    }

    public enum RecordingState
    {
        Active,
        Idle
    }

    public enum TargetType
    {
        Wav,
        Mp3,
        Monitor
    }
}
