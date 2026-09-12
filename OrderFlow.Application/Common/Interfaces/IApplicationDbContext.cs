using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Common.Interfaces
{
    // Application depends on this abstraction only — Infrastructure provides the real EF Core DbContext.
    // Keeps the write model (Orders/OrderItems) accessible to handlers without a hard EF Core dependency here.
    public interface IApplicationDbContext
    {
        DbSet<Order> Orders { get; }
        DbSet<OrderDashboardView> DashboardView { get; }
    
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }

    // Plain read-model row for the order dashboard. Infrastructure maps this to a SQL table that the
    // refresh worker populates from Orders/OrderItems — it is intentionally not a domain entity.
    public class OrderDashboardView
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = null!;
        public int ItemCount { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = null!;
        public DateTime RefreshedAtUtc { get; set; }

    }

}
