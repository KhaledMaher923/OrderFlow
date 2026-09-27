using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Common.Observability
{
    public static class OrderFlowMetrics
    {
        public const string MeterName = "OrderFlow";

        private static readonly Meter Meter = new(MeterName);

        // Counter: a number that only goes up — fits "how many orders have ever been created".
        public static readonly Counter<long> OrdersCreated = Meter.CreateCounter<long>(
            "orderflow.orders.created",
            description: "Total number of orders created");

        // Counter: total worker execution cycles, and total orders the worker has moved to Completed.
        public static readonly Counter<long> WorkerRuns = Meter.CreateCounter<long>(
            "orderflow.worker.runs",
            description: "Total number of background worker cycles executed, tagged by worker name");

        public static readonly Counter<long> OrdersCompletedByWorker = Meter.CreateCounter<long>(
            "orderflow.worker.orders_completed",
            description: "Total number of orders moved from Pending to Completed by the background worker");

        // Gauge: pending orders is a point-in-time count, not a running total, so a counter would be
        // the wrong shape here — a gauge reports "what is it right now" each time it's scraped.
        private static long _pendingOrdersCount;

        public static readonly ObservableGauge<long> PendingOrders = Meter.CreateObservableGauge(
            "orderflow.orders.pending",
            () => _pendingOrdersCount,
            description: "Current number of orders in Pending status, as of the last worker cycle");

        public static void ReportPendingOrdersCount(long count) => _pendingOrdersCount = count;
    }
}
