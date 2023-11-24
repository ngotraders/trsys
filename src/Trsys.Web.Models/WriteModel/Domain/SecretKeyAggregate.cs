using CQRSlite.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Trsys.Web.Models.Events;

namespace Trsys.Web.Models.WriteModel.Domain
{
    public class SecretKeyAggregate : AggregateRoot
    {
        private string _key;
        private bool _approved;
        private SecretKeyType? _keyType;
        private string _description;
        private string _token;
        private bool _deleted;

        public bool IsApproved => _approved;
        public string Key => _key;

        public string Token => _token;

        private readonly HashSet<int> _publishedOrderTickets = new();
        private readonly HashSet<int> _subscribedOrderTickets = new();

        public void Apply(SecretKeyCreated e) => _key = e.Key;
        public void Apply(SecretKeyApproved _) => _approved = true;
        public void Apply(SecretKeyRevoked _) => _approved = false;
        public void Apply(SecretKeyKeyTypeChanged e) => _keyType = e.KeyType;
        public void Apply(SecretKeyDescriptionChanged e) => _description = e.Description;
        public void Apply(SecretKeyTokenGenerated e) => _token = e.Token;
        public void Apply(SecretKeyTokenInvalidated _) => _token = null;
        public void Apply(SecretKeyDeleted _) => _deleted = true;

        public void Apply(OrderPublisherOpenedOrder e) => _publishedOrderTickets.Add(e.Order.TicketNo);
        public void Apply(OrderPublisherClosedOrder e) => _publishedOrderTickets.Remove(e.TicketNo);
        public void Apply(OrderSubscriberOpenedOrder e) => _subscribedOrderTickets.Add(e.TicketNo);
        public void Apply(OrderSubscriberClosedOrder e) => _subscribedOrderTickets.Remove(e.TicketNo);

        public SecretKeyAggregate()
        {
        }

        public SecretKeyAggregate(Guid id, string key)
        {
            Id = id;
            ApplyChange(new SecretKeyCreated(id, key));
        }

        public void ChangeKeyType(SecretKeyType keyType)
        {
            EnsureNotDeleted();
            if (_keyType.HasValue && _keyType == keyType)
            {
                return;
            }
            if (_approved)
            {
                throw new InvalidOperationException("Cannot change key type if secret key is approved.");
            }
            ApplyChange(new SecretKeyKeyTypeChanged(Id, keyType));
        }

        public void ChangeDescription(string description)
        {
            EnsureNotDeleted();
            if (_description == description)
            {
                return;
            }
            ApplyChange(new SecretKeyDescriptionChanged(Id, description));
        }

        public void Approve()
        {
            EnsureNotDeleted();
            if (!_keyType.HasValue)
            {
                throw new InvalidOperationException("Cannot approve if key type is not set.");
            }
            if (!_approved)
            {
                ApplyChange(new SecretKeyApproved(Id));
            }
        }

        public void Revoke()
        {
            EnsureNotDeleted();
            if (_approved)
            {
                ApplyChange(new SecretKeyRevoked(Id));
            }
        }

        public string GenerateToken()
        {
            EnsureNotDeleted();
            if (!_approved)
            {
                throw new InvalidOperationException("Cannot generate token if secret key is approved.");
            }
            if (!string.IsNullOrEmpty(_token))
            {
                ApplyChange(new SecretKeyTokenInvalidated(Id, _token));
            }
            var token = Guid.NewGuid().ToString();
            ApplyChange(new SecretKeyTokenGenerated(Id, token));
            return token;
        }

        public void InvalidateToken(string token)
        {
            EnsureNotDeleted();
            if (string.IsNullOrEmpty(_token))
            {
                throw new InvalidOperationException("Token is not generated yet.");
            }
            if (token != _token)
            {
                throw new InvalidOperationException("The token is not valid.");
            }

            ApplyChange(new SecretKeyTokenInvalidated(Id, _token));
        }
        public void Delete()
        {
            EnsureNotDeleted();
            if (_approved)
            {
                throw new InvalidOperationException("Cannot delete secret key if approved.");
            }
            ApplyChange(new SecretKeyDeleted(Id));
        }

        private void EnsureNotDeleted()
        {
            if (_deleted)
            {
                throw new InvalidOperationException("This data has already deleted.");
            }
        }

        public void ReplaceOrders(IEnumerable<PublishedOrder> orders)
        {
            EnsureNotDeleted();
            var tickets = orders.Select(o => o.TicketNo).ToList();
            var added = tickets.Except(_publishedOrderTickets).ToList();
            var removed = _publishedOrderTickets.Except(tickets).ToList();

            foreach (var ticket in removed.OrderBy(e => e))
            {
                ApplyChange(new OrderPublisherClosedOrder(Id, ticket));
            }
            if (added.Any())
            {
                var ordersDictionary = orders.ToDictionary(o => o.TicketNo, o => o);
                foreach (var ticket in added.OrderBy(e => e))
                {
                    ApplyChange(new OrderPublisherOpenedOrder(Id, ordersDictionary[ticket]));
                }
            }
        }

        public void Subscribed(int[] tickets)
        {
            EnsureNotDeleted();
            var added = tickets.Except(_subscribedOrderTickets).ToList();
            var removed = _subscribedOrderTickets.Except(tickets).ToList();

            foreach (var ticket in removed.OrderBy(e => e))
            {
                ApplyChange(new OrderSubscriberClosedOrder(Id, ticket));
            }

            foreach (var ticket in added.OrderBy(e => e))
            {
                ApplyChange(new OrderSubscriberOpenedOrder(Id, ticket));
            }
        }

        public void OpenOrder(PublishedOrder publishedOrder)
        {
            if (_publishedOrderTickets.Any(t => t == publishedOrder.TicketNo))
            {
                throw new InvalidOperationException("Ticket no already exists.");
            }
            ApplyChange(new OrderPublisherOpenedOrder(Id, publishedOrder));
        }

        public void CloseOrder(int ticketNo)
        {
            if (_publishedOrderTickets.Any(t => t == ticketNo))
            {
                ApplyChange(new OrderPublisherClosedOrder(Id, ticketNo));
            }
        }
    }
}
