using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trsys.Web.Models.Orders;

namespace Trsys.Web.Services
{
    public class OrdersTextEntry
    {
        public string Hash { get; set; }
        public string Text { get; set; }

        public static OrdersTextEntry Create(List<Order> orders)
        {
            var responseText = string.Join("@", orders.Select(o => $"{o.TicketNo}:{o.Symbol}:{(int)o.OrderType}:{o.Price}:{o.Lots}:{o.Time}"));
            return new OrdersTextEntry
            {
                Hash = CalculateHash(responseText),
                Text = responseText,
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
