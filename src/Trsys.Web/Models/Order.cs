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
        public int TicketNo { get; set; }
        public string Symbol { get; set; }
        public OrderType OrderType { get; set; }
        public decimal Price { get; set; }
        public decimal Lots { get; set; }
        public long Time { get; set; }
    }
}
