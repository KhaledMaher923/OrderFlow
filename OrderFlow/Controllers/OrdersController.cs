using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Orders.CreateOrder;
using OrderFlow.Application.Features.Orders.GetOrderById;
using OrderFlow.Application.Features.Orders.ListOrders;

namespace OrderFlow.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST /api/orders
        [HttpPost]
        public async Task<ActionResult<CreateOrderResponse>> Create(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.OrderId }, result);
        }

        // GET /api/orders/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDetailsResponse>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }

        // GET /api/orders
        [HttpGet]
        public async Task<ActionResult<List<OrderSummaryResponse>>> List(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ListOrdersQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
