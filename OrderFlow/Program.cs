using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderFlow.Application;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Common.Observability;
using OrderFlow.Infrastructure;
using Serilog;
using System.Text.Json;
using OpenTelemetry;

namespace OrderFlow
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    "logs/orderflow-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddHealthChecks()
                .AddSqlServer(
                    connectionString: builder.Configuration.GetConnectionString("SqlServer")!,
                    name: "sqlserver",
                    tags: new[] { "db", "sql" })
                .AddRedis(
                    redisConnectionString: builder.Configuration.GetConnectionString("Redis")!,
                    name: "redis",
                    tags: new[] { "cache", "redis" });

            builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("OrderFlow.Api"))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter(OrderFlowMetrics.MeterName)
        .AddPrometheusExporter())
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddSource(OrderFlowActivitySource.SourceName));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var feature = context.Features.Get<IExceptionHandlerFeature>();

                    if(feature?.Error is ValidationException validationEx)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new { errors = validationEx.Errors });
                        return;
                    }
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
                });
            });

            app.UseHttpsRedirection();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";

                    var payload = JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            durationMs = e.Value.Duration.TotalMilliseconds
                        })
                    });

                    await context.Response.WriteAsync(payload);
                }
            });

            app.MapPrometheusScrapingEndpoint();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
