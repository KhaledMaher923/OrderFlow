using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Domain.Entities
{
    public class Order
    {
        private readonly List<OrderItem> _items = new();

        public int Id { get; private set; }
        public string CustomerName { get; private set; } = null!;
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        public decimal Total => _items.Sum(i => i.LineTotal);
        public int ItemCount => _items.Sum(i => i.Quantity);


        public Order() { } //EF Core

        public Order(string customerName, IEnumerable<OrderItem> items)
        {
            if(string.IsNullOrWhiteSpace(customerName))  
                throw new DomainException("Customer name is required.");

            var itemList = items.ToList();
            if (itemList.Count == 0)
                throw new DomainException("An order must contain at least one item.");

            CustomerName = customerName;
            _items.AddRange(itemList);
            Status = OrderStatus.Pending;
            CreatedAtUtc = DateTime.UtcNow;
        }
        public void MarkCompleted()
        {
            if(Status == OrderStatus.Completed)
                return; 
            
            Status = OrderStatus.Completed;
        }

    }
}
