using NAudio.CoreAudioApi;

namespace YAD.Audio.Utils
{
    public struct DeviceContainer
    {
        public DeviceContainer(MMDevice device, int deviceNumber)
        {
            using (WasapiCapture c = new WasapiCapture(device))
            {
                ID = device.ID;
                DeviceNumber = deviceNumber;
                Channels = c.WaveFormat.Channels;
                DeviceState = device.State.ToString();
                FriendlyName = device.FriendlyName;
                Exclusive = false;
            }
        }

        public DeviceContainer(string id, int deviceNumber, int channels, string state, string friendlyName)
        {
            ID = id;
            DeviceNumber = deviceNumber;
            Channels = channels;
            DeviceState = state;
            FriendlyName = friendlyName;
            Exclusive = false;
        }

        public string ID { get; }
        public int DeviceNumber { get; }
        public int Channels { get; }
        public string DeviceState { get; }
        public string FriendlyName { get; }
        public bool Exclusive { get; set; }
    }
}
