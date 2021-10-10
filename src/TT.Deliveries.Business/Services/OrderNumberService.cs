using TT.Deliveries.Core;

namespace TT.Deliveries.Business.Services
{
    /// <summary>
    /// This should probably connect to the sender 3rd party API to generate an order number
    /// </summary>
    public class OrderNumberService : IOrderNumberService
    {
        public string CreateOrderNumber(string sender) => $"{sender}_ON_{Clock.UtcNow.Ticks}";
    }
}