using System;

namespace TT.Deliveries.Data.Models
{
    public class AccessWindow : DataModel<Guid>
    {
        public Guid DeliveryId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}