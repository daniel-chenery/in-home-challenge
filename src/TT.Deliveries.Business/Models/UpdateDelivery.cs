using System;
using TT.Deliveries.Core.Models;

namespace TT.Deliveries.Business.Models
{
    public class UpdateDelivery
    {
        public UpdateDelivery(Guid guid, DeliveryState state)
        {
            Guid = guid;

            State = state;
        }

        public Guid Guid { get; set; }

        public DeliveryState State { get; set; }
    }
}