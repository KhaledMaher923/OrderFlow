using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Infrastructure.Persistence.Configurations
{
    public class OrderDashboardViewConfiguration : IEntityTypeConfiguration<OrderDashboardView>
    {
        public void Configure(EntityTypeBuilder<OrderDashboardView> builder)
        {
            builder.ToTable("OrderDashboardView");

            builder.HasKey(v => v.OrderId);

            builder.Property(v => v.CustomerName).IsRequired().HasMaxLength(200);
            builder.Property(v => v.Status).IsRequired().HasMaxLength(20);
            builder.Property(v => v.Total).HasColumnType("decimal(18,2)");
        }
    }
}
