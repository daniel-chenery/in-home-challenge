using System;

namespace TT.Deliveries.Data.Models
{
    public class Order : DataModel<string>
    {
        public Guid DeliveryId { get; set; }

        public string? Sender { get; set; }
    }
}