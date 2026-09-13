using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Common.Interfaces;
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

        public GetOrderByIdQueryHandler(IApplicationDbContext dbcontext, ICacheService cache)
        {
            _dbcontext = dbcontext;
            _cache = cache;
        }

        public async Task<OrderDetailsResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"order:{request.OrderId}";

            var cached = await _cache.GetAsync<OrderDetailsResponse>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

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
                await _cache.SetAsync(cacheKey, order, CacheTtl, cancellationToken); ;
            }

            return order;
        }

    }

}
