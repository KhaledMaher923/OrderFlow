using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

            builder.Property(o => o.Status)
           .HasConversion<string>()
           .HasMaxLength(20);

            builder.Property(o => o.CreatedAtUtc)
            .IsRequired();

            // Total/ItemCount are computed properties on the domain entity, not persisted columns.
            builder.Ignore(o => o.Total);
            builder.Ignore(o => o.ItemCount);

            builder.Metadata
            .FindNavigation(nameof(Order.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
