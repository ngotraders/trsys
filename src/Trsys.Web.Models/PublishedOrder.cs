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
        private decimal _Lots;
        public decimal Lots
        {
            get { return _Lots; }
            set { _Lots = value.Normalize(); }
        }
        public long Time { get; set; }

        private decimal _AccountBalance;
        public decimal BalanceInSymbol
        {
            get { return _AccountBalance; }
            set { _AccountBalance = value.Normalize(); }
        }

        public decimal Percentage => BalanceInSymbol == 0 ? 0 : Lots / BalanceInSymbol;

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
                Price = decimal.Parse(price),
                Lots = decimal.Parse(lots),
                Time = long.Parse(time),
            };
        }

        public static PublishedOrder ParseV2(string orderText)
        {
            if (!Regex.IsMatch(orderText, @"^\d+:[A-Z]+:[01]:\d+:\d+(\.\d+)?:\d+(\.\d+)?:\d+(\.\d+)?$"))
            {
                return null;
            }
            var splitted = orderText.Split(":");
            var ticketNo = splitted[0];
            var symbol = splitted[1];
            var orderType = (OrderType)int.Parse(splitted[2]);
            var time = splitted[3];
            var price = splitted[4];
            var lots = splitted[5];
            var balanceInSymbol = splitted[6];
            return new PublishedOrder()
            {
                TicketNo = int.Parse(ticketNo),
                Symbol = symbol,
                OrderType = orderType,
                Time = string.IsNullOrEmpty(time) ? default : long.Parse(time),
                Price = string.IsNullOrEmpty(price) ? default : decimal.Parse(price),
                Lots = string.IsNullOrEmpty(lots) ? default : decimal.Parse(lots),
                BalanceInSymbol = string.IsNullOrEmpty(price) ? default : decimal.Parse(balanceInSymbol),
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
