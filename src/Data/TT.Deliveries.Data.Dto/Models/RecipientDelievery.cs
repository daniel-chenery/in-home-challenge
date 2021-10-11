using System;

namespace TT.Deliveries.Data.Models
{
    public class RecipientDelievery : DataModel<Guid>
    {
        public Guid RecipientId { get; set; }

        public Guid DeliveryId { get; set; }
    }
}