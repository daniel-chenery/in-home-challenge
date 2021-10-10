using System;
using TT.Deliveries.Core.Models;

namespace TT.Deliveries.Business.Models
{
    public class CreateDelivery
    {
        public DeliveryState State { get; set; }

        public Guid RecipientId { get; set; }

        public string? SenderName { get; set; }
    }
}