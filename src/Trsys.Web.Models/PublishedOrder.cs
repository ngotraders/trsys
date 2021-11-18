using System.Text.RegularExpressions;

namespace Trsys.Web.Models
{
    public class PublishedOrder
    {
        public int TicketNo { get; set; }
        public string Symbol { get; set; }
        public OrderType OrderType { get; set; }
        private decimal _Price;
        public decimal Price
        {
            get { return _Price; }
            set { _Price = value.Normalize(); }
        }
        public long Time { get; set; }

        private decimal _Percentage;
        public decimal Percentage
        {
            get { return _Percentage; }
            set { _Percentage = value.Normalize(); }
        }

        public static PublishedOrder Parse(string orderText)
        {
            if (!Regex.IsMatch(orderText, @"^\d+:[A-Z]+:[01]:\d+:\d+(\.\d+)?:\d+(\.\d+)??$"))
            {
                return null;
            }
            var splitted = orderText.Split(":");
            var ticketNo = splitted[0];
            var symbol = splitted[1];
            var orderType = (OrderType)int.Parse(splitted[2]);
            var time = splitted[3];
            var price = splitted[4];
            var percentage = splitted[5];
            return new PublishedOrder()
            {
                TicketNo = int.Parse(ticketNo),
                Symbol = symbol,
                OrderType = orderType,
                Time = string.IsNullOrEmpty(time) ? default : long.Parse(time),
                Price = string.IsNullOrEmpty(price) ? default : decimal.Parse(price),
                Percentage = string.IsNullOrEmpty(percentage) ? default : decimal.Parse(percentage),
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
