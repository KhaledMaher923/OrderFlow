using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common.Interfaces;
using OrderFlow.Application.Common.Observability;
using OrderFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Features.Orders.CreateOrder
{
    public record CreateOrderItemDto(string ProductName, int Quantity, decimal UnitPrice);

    public record CreateOrderCommand(string CustomerName, List<CreateOrderItemDto> Items)
        : IRequest<CreateOrderResponse>;

    public record CreateOrderResponse(int OrderId, decimal Total, int ItemCount);
        
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.CustomerName)
            .NotEmpty()
            .MaximumLength(200);

            RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("An order must contain at least one item.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(200);
                item.RuleFor(i => i.Quantity).GreaterThan(0);
                item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
            });
        }
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
    {
        private readonly IApplicationDbContext _dbcontext;
        private readonly ICacheService _cache;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(IApplicationDbContext dbcontext, ICacheService cache, ILogger<CreateOrderCommandHandler> logger)
        {
            _dbcontext = dbcontext;
            _cache = cache;
            _logger = logger;
        }

        public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            using var activity = OrderFlowActivitySource.Source.StartActivity("CreateOrder");

            var items = request.Items
                .Select(i => new OrderItem(i.ProductName, i.Quantity, i.UnitPrice))
                .ToList();

            var order = new Order(request.CustomerName, items);

            _dbcontext.Orders.Add(order);
            await _dbcontext.SaveChangesAsync(cancellationToken);

            await _cache.RemoveAsync($"order: {order.Id}", cancellationToken);

            OrderFlowMetrics.OrdersCreated.Add(1);
            activity?.SetTag("order.id", order.Id);
            activity?.SetTag("order.total", order.Total);


            _logger.LogInformation(
                $"Order {order.Id} created for {order.CustomerName} with {order.ItemCount} items, total {order.Total}"
                );

            return new CreateOrderResponse(order.Id, order.Total, order.ItemCount);
        }
    }
}
