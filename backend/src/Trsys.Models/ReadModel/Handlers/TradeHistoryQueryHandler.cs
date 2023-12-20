using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.Events;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;
using Trsys.Models.ReadModel.Notifications;
using Trsys.Models.ReadModel.Queries;

namespace Trsys.Models.ReadModel.Handlers
{
    public class TradeHistoryQueryHandler :
        INotificationHandler<OrderPublisherOpenedOrder>,
        INotificationHandler<OrderPublisherClosedOrder>,
        INotificationHandler<OrderSubscriberOpenedOrder>,
        INotificationHandler<OrderSubscriberClosedOrder>,
        INotificationHandler<LogNotification>,
        IRequestHandler<GetTradeHistory, TradeHistoryDto>,
        IRequestHandler<SearchTradeHistories, SearchResponseDto<TradeHistoryDto>>
    {
        private readonly ITradeHistoryDatabase db;
        private readonly ISecretKeyDatabase secretKeyDatabase;
        private readonly IUserNotificationDispatcher notificationDispatcher;

        public TradeHistoryQueryHandler(ITradeHistoryDatabase db, ISecretKeyDatabase secretKeyDatabase, IUserNotificationDispatcher notificationDispatcher)
        {
            this.db = db;
            this.secretKeyDatabase = secretKeyDatabase;
            this.notificationDispatcher = notificationDispatcher;
        }

        public async Task Handle(OrderPublisherOpenedOrder notification, CancellationToken cancellationToken = default)
        {
            var id = $"{notification.Id}-{notification.Order.TicketNo}";
            var order = new TradeHistoryDto()
            {
                Id = id,
                PublisherId = notification.Id,
                OpenPublishedAt = notification.TimeStamp,
                TicketNo = notification.Order.TicketNo,
                Symbol = notification.Order.Symbol,
                OrderType = notification.Order.OrderType,
                PriceOpened = notification.Order.Price,
                TimeOpened = DateTimeOffset.FromUnixTimeSeconds(notification.Order.Time).UtcDateTime,
                Percentage = notification.Order.Percentage,
            };

            await db.AddAsync(order);
            if (Math.Abs((DateTimeOffset.UtcNow - notification.TimeStamp).TotalSeconds) < 10)
            {
                await notificationDispatcher.DispatchSystemNotificationAsync(NotificationMessageDto.CreateTradeHistoryOpenedMessage(order));
            }
        }

        public async Task Handle(OrderPublisherClosedOrder notification, CancellationToken cancellationToken = default)
        {
            var order = await db.UpdateClosePublishedAtAsync($"{notification.Id}-{notification.TicketNo}", notification.TimeStamp);
            if (order == null)
            {
                return;
            }
            if (Math.Abs((DateTimeOffset.UtcNow - notification.TimeStamp).TotalSeconds) < 10)
            {
                await notificationDispatcher.DispatchSystemNotificationAsync(NotificationMessageDto.CreateTradeHistoryClosedMessage(order));
            }
        }

        public async Task Handle(OrderSubscriberOpenedOrder notification, CancellationToken cancellationToken)
        {
            var order = await db.FindByPublisherTicketNoAsync(notification.TicketNo);
            if (order == null)
            {
                return;
            }
            await db.AddSubscriberTradeHistoryAsync(order.Id, notification.Id.ToString(), notification.TimeStamp);
        }

        public async Task Handle(OrderSubscriberClosedOrder notification, CancellationToken cancellationToken)
        {
            var order = await db.FindByPublisherTicketNoAsync(notification.TicketNo);
            if (order == null)
            {
                return;
            }
            await db.UpdateSubscriberTradeHistoryClosedDeliveredAtAsync(order.Id, notification.Id.ToString(), notification.TimeStamp);
        }

        public async Task Handle(LogNotification notification, CancellationToken cancellationToken)
        {
            var key = await secretKeyDatabase.FindByKeyAsync(notification.Key);
            if (key == null || (key.KeyType & SecretKeyType.Subscriber) == 0)
            {
                return;
            }
            foreach (var line in notification.Lines)
            {
                var splitted = line.Split(':');
                if (splitted[1] == "DEBUG" && splitted[2] == "OPEN" && splitted.Length == 13)
                {
                    var pubTicketNo = splitted[3];
                    //var pubSymbol = splitted[4];
                    //var pubOrderType = splitted[5];
                    var subTicketNo = splitted[6];
                    var subTradeTicketNo = splitted[7];
                    //var subSymbol = splitted[8];
                    //var subOrderType = splitted[9];
                    var subPriceOpened = splitted[10];
                    var subLotsOpened = splitted[11];
                    var subTimeOpened = splitted[12];
                    var order = await db.FindByPublisherTicketNoAsync(int.Parse(pubTicketNo));
                    if (order == null)
                    {
                        return;
                    }
                    await db.UpdateSubscriberOrderOpenInfoAsync(
                        order.Id,
                        key.Id.ToString(),
                        int.Parse(subTicketNo),
                        int.Parse(subTradeTicketNo),
                        decimal.Parse(subPriceOpened).Normalize(),
                        decimal.Parse(subLotsOpened).Normalize(),
                        DateTimeOffset.FromUnixTimeSeconds(long.Parse(subTimeOpened)).ToUniversalTime());
                }
                if (splitted[1] == "DEBUG" && splitted[2] == "CLOSE" && splitted.Length == 14)
                {
                    var pubTicketNo = splitted[3];
                    //var pubSymbol = splitted[4];
                    //var pubOrderType = splitted[5];
                    var subTicketNo = splitted[6];
                    var subTradeTicketNo = splitted[7];
                    //var subSymbol = splitted[8];
                    //var subOrderType = splitted[9];
                    var subPriceClosed = splitted[10];
                    var subLotsClosed = splitted[11];
                    var subTimeClosed = splitted[12];
                    var subProfit = splitted[13];
                    var order = await db.FindByPublisherTicketNoAsync(int.Parse(pubTicketNo));
                    if (order == null)
                    {
                        return;
                    }
                    await db.UpdateSubscriberOrderCloseInfoAsync(
                        order.Id,
                        key.Id.ToString(),
                        int.Parse(subTicketNo),
                        int.Parse(subTradeTicketNo),
                        decimal.Parse(subPriceClosed).Normalize(),
                        decimal.Parse(subLotsClosed).Normalize(),
                        DateTimeOffset.FromUnixTimeSeconds(long.Parse(subTimeClosed)).ToUniversalTime(),
                        decimal.Parse(subProfit).Normalize());
                }
            }
        }

        public Task<TradeHistoryDto> Handle(GetTradeHistory request, CancellationToken cancellationToken)
        {
            return db.FindByIdAsync(request.Id);
        }

        public async Task<SearchResponseDto<TradeHistoryDto>> Handle(SearchTradeHistories request, CancellationToken cancellationToken = default)
        {
            var count = await db.CountAsync();
            return new SearchResponseDto<TradeHistoryDto>(
                count,
                await db.SearchAsync(
                    request.Start ?? 0,
                    request.End ?? int.MaxValue,
                    request.Sort,
                    request.Order));
        }
    }
}
