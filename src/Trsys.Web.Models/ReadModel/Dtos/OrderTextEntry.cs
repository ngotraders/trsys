using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trsys.Web.Models.ReadModel.Dtos
{
    public class OrdersTextEntry
    {
        public int[] Tickets { get; set; }
        public string Hash { get; set; }
        public string Text { get; set; }

        public static OrdersTextEntry Create(List<PublishedOrder> orders)
        {
            var responseText = string.Join("@", orders.Select(o => $"{o.TicketNo}:{o.Symbol}:{(int)o.OrderType}:{o.Time}:{o.Price}:{o.Percentage}"));
            return new OrdersTextEntry
            {
                Hash = CalculateHash(responseText),
                Text = responseText,
                Tickets = orders.Select(o => o.TicketNo).ToArray(),
            };
        }

        private static string CalculateHash(string text)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(text));
            var str = BitConverter.ToString(hash);
            str = str.Replace("-", string.Empty);
            return str;
        }
    }
}
