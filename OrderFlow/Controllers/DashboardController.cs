using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Dashboard.GetDashboard;

namespace OrderFlow.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET /api/dashboard/orders
        [HttpGet("orders")]
        public async Task<ActionResult<List<DashboardRowResponse>>> GetOrders(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetDashboardQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
