using System;
using TT.Deliveries.Core.Models;

namespace TT.Deliveries.Data.Models
{
    public class Delivery : DataModel<Guid>
    {
        public DeliveryState State { get; set; }
    }
}