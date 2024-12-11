using System.Text.RegularExpressions;

namespace Trsys.Models
{
    public partial class PublishedOrder
    {
        public int TicketNo { get; set; }
        public string Symbol { get; set; }
        public string OriginalSymbol { get; set; }
        public OrderType OrderType { get; set; }
        decimal _Price;
        public decimal Price
        {
            get { return _Price; }
            set { _Price = value.Normalize(); }
        }
        public long Time { get; set; }

        decimal _Percentage;
        public decimal Percentage
        {
            get { return _Percentage; }
            set { _Percentage = value.Normalize(); }
        }

        public static PublishedOrder Parse(string orderText)
        {
            var match = OrderTextExpression().Match(orderText);
            if (!match.Success)
            {
                return null;
            }
            var ticketNo = match.Groups[1].Value;
            var symbol = (match.Groups[2].Value.Length > 6 ? match.Groups[2].Value.Substring(0, 6) : match.Groups[2].Value).ToUpperInvariant();
            var originalSymbol = match.Groups[2].Value + match.Groups[3].Value;
            var orderType = (OrderType)int.Parse(match.Groups[4].Value);
            var time = match.Groups[5].Value;
            var price = match.Groups[6].Value;
            var percentage = match.Groups[8].Value; ;
            return new PublishedOrder()
            {
                TicketNo = int.Parse(ticketNo),
                Symbol = symbol,
                OriginalSymbol = originalSymbol,
                OrderType = orderType,
                Time = string.IsNullOrEmpty(time) ? default : long.Parse(time),
                Price = string.IsNullOrEmpty(price) ? default : decimal.Parse(price),
                Percentage = string.IsNullOrEmpty(percentage) ? default : decimal.Parse(percentage),
            };
        }

        [GeneratedRegex(@"^(\d+):([A-Za-z0-9]+)([.#][^@:]*)?:([01]):(\d+):(\d+(\.\d+)?):(\d+(\.\d+)??)$")]
        private static partial Regex OrderTextExpression();
    }

    static class DecimalExtension
    {
        public static decimal Normalize(this decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }
    }
}
