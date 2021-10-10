using System;

namespace TT.Deliveries.Business.Models
{
    public class AccessWindow
    {
        public AccessWindow(DateTime startTime, DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}