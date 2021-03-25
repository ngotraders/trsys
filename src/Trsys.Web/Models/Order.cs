namespace Trsys.Web.Models
{
    public enum OrderType
    {
        BUY,
        SELL,
    }

    public class Order
    {
        public int Id { get; set; }
        public string TicketNo { get; set; }
        public string Symbol { get; set; }
        public OrderType OrderType { get; set; }
    }
}
