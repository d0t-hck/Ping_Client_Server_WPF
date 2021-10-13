using System;

namespace PingServer
{
    public class ReplyStatus
    {
        public string Resource { get; set; }
        public string Status { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return string.Format("{0:HH:mm} - {1}: {2}", TimeStamp, Resource, Status);
        }
    }
}
