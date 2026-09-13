using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Features.Dashboard.GetDashboard
{
    public record GetDashboardQuery : IRequest<List<DashboardRowResponse>>;

    public record DashboardRowResponse(int OrderId, string CustomerName, int ItemCount, decimal Total, string Status);

    public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, List<DashboardRowResponse>>
    {
        private readonly IApplicationDbContext _dbcontext;

        public GetDashboardQueryHandler(IApplicationDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<List<DashboardRowResponse>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
        {
            return await _dbcontext.DashboardView
                .AsNoTracking()
                .OrderByDescending(v => v.RefreshedAtUtc)
                .Select(v => new DashboardRowResponse(v.OrderId, v.CustomerName, v.ItemCount, v.Total, v.Status))
                .ToListAsync(cancellationToken);
        }
    }



}
