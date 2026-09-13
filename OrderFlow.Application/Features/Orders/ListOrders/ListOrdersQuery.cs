using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Features.Orders.ListOrders
{
    public record ListOrdersQuery : IRequest<List<OrderSummaryResponse>>;

    public record OrderSummaryResponse(int OrderId, string CustomerName, string Status, decimal Total, DateTime CreatedAtUtc);

    public class ListOrdersQueryHandler : IRequestHandler<ListOrdersQuery, List<OrderSummaryResponse>>
    {
        private readonly IApplicationDbContext _dbContext;

        public ListOrdersQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<OrderSummaryResponse>> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAtUtc)
                .Select(o => new OrderSummaryResponse(o.Id, o.CustomerName, o.Status.ToString(), o.Total, o.CreatedAtUtc))
                .ToListAsync(cancellationToken);
        }
    }



}
