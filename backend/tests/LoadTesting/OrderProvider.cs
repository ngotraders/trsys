using System;

namespace LoadTesting
{
    public class OrderProvider
    {
        static readonly string[] ORDER_DATA = new[] {
            "1:USDJPY:0:1616746000:1.0:98",
            "1:USDJPY:0:1616746000:1.0:98@2:EURUSD:1:1616746100:0.5:98",
            "2:EURUSD:1:1616746100:0.5:98",
            "2:EURUSD:1:1616746100:0.5:98@3:CNYUSD:1:1616746200:0.05:98",
            "3:CNYUSD:1:1616746200:0.05:98",
            "3:CNYUSD:1:1616746200:0.05:98@4:GBPUSD:0:1616746300:10.05:98",
            "4:GBPUSD:0:1616746300:10.05:98"
        };

        private DateTime? start;
        private readonly TimeSpan step;

        public OrderProvider(TimeSpan duration)
        {
            step = duration / ORDER_DATA.Length;
        }

        public static int PreparedOrdersCount => ORDER_DATA.Length;

        public void SetStart()
        {
            start = DateTime.Now;
        }

        public string GetCurrentOrder()
        {
            if (!start.HasValue)
            {
                return ORDER_DATA[0];
            }
            var current = DateTime.Now - start;
            var index = (int)(current / step);
            return ORDER_DATA[Math.Max(0, Math.Min(index, ORDER_DATA.Length - 1))];
        }
    }
}
