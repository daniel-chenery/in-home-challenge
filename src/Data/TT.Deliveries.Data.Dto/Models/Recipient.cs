using System;

namespace TT.Deliveries.Data.Models
{
    public class Recipient : DataModel<Guid>
    {
        public string? Name { get; set; }

        public string? Address { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
    }
}