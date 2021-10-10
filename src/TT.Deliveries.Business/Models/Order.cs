namespace TT.Deliveries.Business.Models
{
    public class Order
    {
        public Order(string orderNumber, string sender)
        {
            OrderNumber = orderNumber;
            Sender = sender;
        }

        public string OrderNumber { get; set; }

        public string Sender { get; set; }
    }
}