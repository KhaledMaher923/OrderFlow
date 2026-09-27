    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using OrderFlow.Application.Common.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace OrderFlow.Infrastructure.BackgroundServices
    {
        public class MaterializedViewRefreshWorker : BackgroundService
        {
            private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(15);

            private readonly IServiceScopeFactory _scopeFactory;
            private readonly ILogger<MaterializedViewRefreshWorker> _logger;
            public MaterializedViewRefreshWorker(IServiceScopeFactory scopeFactory, ILogger<MaterializedViewRefreshWorker> logger)
            {
                _scopeFactory = scopeFactory;
                _logger = logger;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                using var timer = new PeriodicTimer(RefreshInterval);

                do
                {
                    try
                    {
                        await RefreshAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        // Never let a bad refresh cycle kill the worker loop.
                        _logger.LogError(ex, "Materialized view refresh failed.");
                    }
                }
                while(await timer.WaitForNextTickAsync(stoppingToken));
            }

            private async Task RefreshAsync(CancellationToken cancellationToken)
            {
                // the worker is a singleton, but DbContext is scoped — create a scope per cycle
                // rather than resolving scoped services directly into the worker's constructor.
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                var rows = await db.Orders
                    .AsNoTracking()
                    .Select(o => new
                    {
                        o.Id,
                        o.CustomerName,
                        Status = o.Status.ToString(),
                        Total = o.Total,
                        ItemCount = o.ItemCount
                    })
                    .ToListAsync(cancellationToken);

                var existing = await db.DashboardView.ToListAsync(cancellationToken);
                db.DashboardView.RemoveRange(existing);

                var now = DateTime.UtcNow;
                foreach (var row in rows)
                {
                    db.DashboardView.Add(new OrderDashboardView
                    {
                        OrderId = row.Id,
                        CustomerName = row.CustomerName,
                        ItemCount = row.ItemCount,
                        Total = row.Total,
                        Status = row.Status,
                        RefreshedAtUtc = now
                    });
                }

                await db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Materialized view refreshed with {rows.Count} rows");
            }
        }
    }
