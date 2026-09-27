using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common.Interfaces;
using OrderFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Features.Orders.GetOrderById
{
    public record GetOrderByIdQuery(int OrderId) : IRequest<OrderDetailsResponse?>;

    public record OrderItemResponse(string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

    public record OrderDetailsResponse(
    int OrderId,
    string CustomerName,
    string Status,
    decimal Total,
    List<OrderItemResponse> Items);

    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailsResponse?>
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        private readonly IApplicationDbContext _dbcontext;
        private readonly ICacheService _cache;
        private readonly ILogger<GetOrderByIdQueryHandler> _logger;

        public GetOrderByIdQueryHandler(IApplicationDbContext dbcontext, ICacheService cache, ILogger<GetOrderByIdQueryHandler> logger)
        {
            _dbcontext = dbcontext;
            _cache = cache;
            _logger = logger;
        }

        public async Task<OrderDetailsResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"order:{request.OrderId}";

            var cached = await _cache.GetAsync<OrderDetailsResponse>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                _logger.LogInformation($"Cache hit for the order {request.OrderId}");
                return cached;
            }

            _logger.LogInformation($"Cache miss for order {request.OrderId}, querying database");

            var order = await _dbcontext.Orders
                .AsNoTracking()
                .Where(o => o.Id == request.OrderId)
                .Select(o => new OrderDetailsResponse(
                    o.Id,
                    o.CustomerName,
                    o.Status.ToString(),
                    o.Total,
                    o.Items
                        .Select(i => new OrderItemResponse(i.ProductName, i.Quantity, i.UnitPrice, i.LineTotal))
                        .ToList()))
                .FirstOrDefaultAsync(cancellationToken);

            if(order is  not null)
            {
                await _cache.SetAsync(cacheKey, order, CacheTtl, cancellationToken);
                _logger.LogInformation($"Order {request.OrderId} retrieved and cached");
            }
            else
            {
                _logger.LogWarning($"Order {request.OrderId} not found");
            }


            return order;
        }

    }

}
    