using System.Text.RegularExpressions;

namespace Trsys.Web.Models
{
    public class PublishedOrder
    {
        public int TicketNo { get; set; }
        public string Symbol { get; set; }
        public OrderType OrderType { get; set; }
        public decimal Price { get; set; }
        public decimal Lots { get; set; }
        public long Time { get; set; }

        public static PublishedOrder Parse(string orderText)
        {
            if (!Regex.IsMatch(orderText, @"^\d+:[A-Z]+:[01]:\d+(\.\d+)?:\d+(\.\d+)?:\d+"))
            {
                return null;
            }
            var splitted = orderText.Split(":");
            var ticketNo = splitted[0];
            var symbol = splitted[1];
            var orderType = (OrderType)int.Parse(splitted[2]);
            var price = splitted[3];
            var lots = splitted[4];
            var time = splitted[5];
            return new PublishedOrder()
            {
                TicketNo = int.Parse(ticketNo),
                Symbol = symbol,
                OrderType = orderType,
                Price = decimal.Parse(price).Normalize(),
                Lots = decimal.Parse(lots).Normalize(),
                Time = long.Parse(time),
            };
        }
    }

    static class DecimalExtension
    {
        public static decimal Normalize(this decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }
    }
}
