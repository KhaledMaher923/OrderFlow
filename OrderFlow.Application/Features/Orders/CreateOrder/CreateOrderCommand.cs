using FluentValidation;
using MediatR;
using OrderFlow.Application.Common.Interfaces;
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

        public CreateOrderCommandHandler(IApplicationDbContext dbcontext, ICacheService cache)
        {
            _dbcontext = dbcontext;
            _cache = cache;
        }

        public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var items = request.Items
                .Select(i => new OrderItem(i.ProductName, i.Quantity, i.UnitPrice))
                .ToList();

            var order = new Order(request.CustomerName, items);

            _dbcontext.Orders.Add(order);
            await _dbcontext.SaveChangesAsync(cancellationToken);

            await _cache.RemoveAsync($"order: {order.Id}", cancellationToken);

            return new CreateOrderResponse(order.Id, order.Total, order.ItemCount);
        }
    }
}
