using System;

namespace Trsys.Models.ReadModel.Dtos
{
    public class NotificationMessageDto
    {
        public NotificationMessageDto(string subject, string body)
        {
            Subject = subject;
            Body = body;
        }

        public string Subject { get; }
        public string Body { get; }

        public static NotificationMessageDto CreateTradeHistoryOpenedMessage(TradeHistoryDto order)
        {
            return new NotificationMessageDto("[copy-trading-system] Pub注文が作成されました", $@"
コピー注文が作成されました。

注文時刻: {order.OpenPublishedAt.ToOffset(TimeSpan.FromHours(9)):yyyy/MM/dd HH:mm:ss} (JST)
決済時刻: -
通貨ペア: {order.Symbol}
ポジション: {order.OrderType.ToString().ToUpper()}
注文時金額: {order.PriceOpened}
");
        }

        public static NotificationMessageDto CreateTradeHistoryClosedMessage(TradeHistoryDto order)
        {
            return new NotificationMessageDto("[copy-trading-system] Pub注文が決済されました", $@"
コピー注文が決済されました。

注文時刻: {order.OpenPublishedAt.ToOffset(TimeSpan.FromHours(9)):yyyy/MM/dd HH:mm:ss} (JST)
決済時刻: {order.ClosePublishedAt?.ToOffset(TimeSpan.FromHours(9)):yyyy/MM/dd HH:mm:ss} (JST)
通貨ペア: {order.Symbol}
ポジション: {order.OrderType.ToString().ToUpper()}
注文時金額: {order.PriceOpened}
");
        }
    }
}