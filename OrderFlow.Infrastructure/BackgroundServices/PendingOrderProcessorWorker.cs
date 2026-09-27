using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common.Interfaces;
using OrderFlow.Application.Common.Observability;
using OrderFlow.Domain.Enums;
using OrderFlow.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Infrastructure.BackgroundServices
{
    public class PendingOrderProcessorWorker : BackgroundService
    {
        private static readonly TimeSpan ProcessInterval = TimeSpan.FromSeconds(10);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PendingOrderProcessorWorker> _logger;

        public PendingOrderProcessorWorker(IServiceScopeFactory scopeFactory, ILogger<PendingOrderProcessorWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(ProcessInterval);
            
            do
            {
                try
                {
                    await ProcessPendingOrdersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Pending order processing failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task ProcessPendingOrdersAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var pendingOrders = await db.Orders
                .Where(o => o.Status == OrderStatus.Pending)
                .ToListAsync(cancellationToken);

            if (pendingOrders.Count == 0)
            {
                _logger.LogInformation("Pending order processor found no pending orders to process");
                OrderFlowMetrics.WorkerRuns.Add(1, new KeyValuePair<string, object?>("worker", "PendingOrderProcessor"));
                OrderFlowMetrics.ReportPendingOrdersCount(0);
                return; 
            }

            foreach ( var order in pendingOrders)
            {
                order.MarkCompleted();
            }

            await db.SaveChangesAsync(cancellationToken);

            OrderFlowMetrics.WorkerRuns.Add(1, new KeyValuePair<string, object?>("worker", "PendingOrderProcessor"));
            OrderFlowMetrics.OrdersCompletedByWorker.Add(pendingOrders.Count);
            OrderFlowMetrics.ReportPendingOrdersCount(0);

            _logger.LogInformation($"Pending Order processor moved {pendingOrders.Count} orders to Completed");
        }
    }
}
