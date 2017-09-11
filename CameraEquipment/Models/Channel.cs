namespace CameraEquipment.Models
{
    public class Channel
    {
        public string ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string ChannelNum { get; set; }
        public string Ip { get; set; }
        public string DeviceId { get; set; }
        public string Device { get; set; }
        public string CameraType { get; set; }
        public string ChannelType { get; set; }
        public Department Product { get; set; }
        public Department Building { get; set; }
        public Department DVRGroup { get; set; }
        public string Remark { get; set; } = "";
        public string ChannelSN { get; set; }
    }
}