using System;

namespace LoadTesting
{
    public class OrderProvider
    {
        static readonly string[] ORDER_DATA = new[] {
            "1:USDJPY:0:1.0:1:1616746000",
            "1:USDJPY:0:1.0:1:1616746000@2:EURUSD:1:0.5:2:1616746100",
            "2:EURUSD:1:0.5:2:1616746100",
            "2:EURUSD:1:0.5:2:1616746100@3:CNYUSD:1:0.05:3:1616746200",
            "3:CNYUSD:1:0.05:3:1616746200",
            "3:CNYUSD:1:0.05:3:1616746200@4:GBPUSD:0:10.05:4:1616746300",
            "4:GBPUSD:0:10.05:4:1616746300"
        };

        private DateTime? start;
        private readonly TimeSpan step;

        public OrderProvider(TimeSpan duration)
        {
            step = duration / ORDER_DATA.Length;
        }

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
