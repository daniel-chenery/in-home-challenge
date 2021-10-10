using System;
using System.Runtime.Serialization;

namespace TT.Deliveries.Business
{
    public class DeliveryException : Exception
    {
        public DeliveryException()
        {
        }

        public DeliveryException(Guid deliveryId)
            : this(deliveryId, string.Empty)
        {
        }

        public DeliveryException(Guid deliveryId, string message)
            : base(message)
        {
            DeliveryId = deliveryId;
        }

        public DeliveryException(string message)
            : base(message)
        {
        }

        public DeliveryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DeliveryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public Guid DeliveryId { get; }
    }
}