using OrderFlow.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; private set; }
        public int OrderId { get; private set; }
        public string ProductName { get; private set; } = null!;
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal LineTotal => Quantity * UnitPrice;
        
        public OrderItem() { } // EF Core

        public OrderItem(string productName, int quantity, decimal unitPrice)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new DomainException("Product name is required.");
            if (quantity <= 0)
                throw new DomainException("Quantity must be greater than zero.");
            if (unitPrice <= 0)
                throw new DomainException("Unit price cannot be negative.");


            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
    }
}
