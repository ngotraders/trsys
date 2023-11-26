using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.Events;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Notifications;
using Trsys.Models.ReadModel.Queries;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Models.ReadModel.Handlers
{
    public class OrderHistoryQueryHandler :
        INotificationHandler<OrderPublisherOpenedOrder>,
        INotificationHandler<OrderPublisherClosedOrder>,
        INotificationHandler<OrderSubscriberOpenedOrder>,
        INotificationHandler<OrderSubscriberClosedOrder>,
        INotificationHandler<LogNotification>,
        IRequestHandler<GetOrderHistories, List<OrderHistoryDto>>
    {
        private readonly IOrderHistoryDatabase db;
        private readonly ISecretKeyDatabase secretKeyDatabase;
        private readonly IUserNotificationDispatcher notificationDispatcher;

        public OrderHistoryQueryHandler(IOrderHistoryDatabase db, ISecretKeyDatabase secretKeyDatabase, IUserNotificationDispatcher notificationDispatcher)
        {
            this.db = db;
            this.secretKeyDatabase = secretKeyDatabase;
            this.notificationDispatcher = notificationDispatcher;
        }

        public async Task Handle(OrderPublisherOpenedOrder notification, CancellationToken cancellationToken = default)
        {
            var id = $"{notification.Id}:{notification.Order.TicketNo}";
            var order = new OrderHistoryDto()
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
                await notificationDispatcher.DispatchSystemNotificationAsync(NotificationMessageDto.CreateCopyTradeOpenedMessage(order));
            }
        }

        public async Task Handle(OrderPublisherClosedOrder notification, CancellationToken cancellationToken = default)
        {
            var order = await db.FindByIdAsync($"{notification.Id}:{notification.TicketNo}");
            if (order == null)
            {
                return;
            }
            order.ClosePublishedAt = notification.TimeStamp;
            await db.UpdateAsync(order);
            if (Math.Abs((DateTimeOffset.UtcNow - notification.TimeStamp).TotalSeconds) < 10)
            {
                await notificationDispatcher.DispatchSystemNotificationAsync(NotificationMessageDto.CreateCopyTradeClosedMessage(order));
            }
        }

        public async Task Handle(OrderSubscriberOpenedOrder notification, CancellationToken cancellationToken)
        {
            var order = await db.FindByIdAsync($"{notification.Id}:{notification.TicketNo}");
            if (order == null)
            {
                return;
            }
            order.SubscriberOrderHistories.Add(new SubscriberOrderHistoryDto()
            {
                SubscriberId = notification.Id,
                OpenDeliveredAt = notification.TimeStamp,
            });
            await db.UpdateAsync(order);
        }

        public async Task Handle(OrderSubscriberClosedOrder notification, CancellationToken cancellationToken)
        {
            var order = await db.FindByIdAsync($"{notification.Id}:{notification.TicketNo}");
            if (order == null)
            {
                return;
            }
            var subscriber = order.SubscriberOrderHistories.Find(x => x.SubscriberId == notification.Id);
            if (subscriber == null)
            {
                return;
            }
            subscriber.ClosedDeliveredAt = notification.TimeStamp;
            await db.UpdateAsync(order);
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
                    var pubSymbol = splitted[4];
                    var pubOrderType = splitted[5];
                    var subTicketNo = splitted[6];
                    var subTradeTicketNo = splitted[7];
                    var subSymbol = splitted[8];
                    var subOrderType = splitted[9];
                    var subPriceOpened = splitted[10];
                    var subLotsOpened = splitted[11];
                    var subTimeOpened = splitted[12];
                    var order = await db.FindByPublisherTicketNoAsync(pubTicketNo);
                    if (order == null)
                    {
                        return;
                    }
                    var subscriberHistory = order.SubscriberOrderHistories.Find(x => x.SubscriberId == key.Id);
                    if (subscriberHistory == null)
                    {
                        return;
                    }
                    subscriberHistory.TicketNo = int.Parse(subTicketNo);
                    subscriberHistory.Lots = decimal.Parse(subLotsOpened).Normalize();
                    subscriberHistory.PriceOpened = decimal.Parse(subPriceOpened).Normalize();
                    subscriberHistory.TimeOpened = DateTimeOffset.FromUnixTimeSeconds(long.Parse(subTimeOpened)).UtcDateTime;
                    await db.UpdateAsync(order);
                }
                if (splitted[1] == "DEBUG" && splitted[2] == "CLOSE" && splitted.Length == 14)
                {
                    var pubTicketNo = splitted[3];
                    var pubSymbol = splitted[4];
                    var pubOrderType = splitted[5];
                    var subTicketNo = splitted[6];
                    var subTradeTicketNo = splitted[7];
                    var subSymbol = splitted[8];
                    var subOrderType = splitted[9];
                    var subPriceClosed = splitted[10];
                    var subLotsClosed = splitted[11];
                    var subTimeClosed = splitted[12];
                    var subProfit = splitted[13];
                    var order = await db.FindByPublisherTicketNoAsync(pubTicketNo);
                    if (order == null)
                    {
                        return;
                    }
                    var subscriberHistory = order.SubscriberOrderHistories.Find(x => x.SubscriberId == key.Id);
                    if (subscriberHistory == null)
                    {
                        return;
                    }
                    subscriberHistory.TicketNo = int.Parse(subTicketNo);
                    subscriberHistory.Lots = decimal.Parse(subLotsClosed).Normalize();
                    subscriberHistory.PriceClosed = decimal.Parse(subPriceClosed).Normalize();
                    subscriberHistory.TimeClosed = DateTimeOffset.FromUnixTimeSeconds(long.Parse(subTimeClosed)).UtcDateTime;
                    subscriberHistory.Profit = decimal.Parse(subProfit).Normalize();
                    await db.UpdateAsync(order);
                }
            }
        }

        public Task<List<OrderHistoryDto>> Handle(GetOrderHistories request, CancellationToken cancellationToken = default)
        {
            return db.SearchAsync();
        }
    }
}
