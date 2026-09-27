using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderFlow.Application.Common.Observability;

namespace OrderFlow.Api.HealthChecks
{
    // Runs on a timer (configured in Program.cs via HealthCheckPublisherOptions) and pushes each
    // health check's Healthy/Unhealthy status into a Prometheus-scrapable gauge, so Grafana/Prometheus
    // can alert on dependency failures (e.g. Redis) instead of only seeing them at /health.
    public class MetricsHealthCheckPublisher :IHealthCheckPublisher
    {
        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            foreach (var entry in report.Entries)
            {
                OrderFlowMetrics.ReportDependencyStatus(entry.Key, entry.Value.Status == HealthStatus.Healthy);
            }
            return Task.CompletedTask;
        }
    }
}
