using FluentValidation;
using MediatR;
using OrderFlow.Application.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationException = OrderFlow.Application.Common.Exceptions.ValidationException;

namespace OrderFlow.Application.Common.Behaviors
{
 // Runs before every Command/Query that has a registered validator (same approach used
 // on OrderFlow's "Add Product to Cart" slice). Keeps validation out of handlers/controllers.

    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
            )
        {
            if (!_validators.Any())
            { 
                return await next(); 
            }

            var context = new ValidationContext<TRequest>(request);

            var failures = (await Task.WhenAll(
                        _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
                    .SelectMany(result => result.Errors)
                    .Where(failure => failure != null)
                    .ToList();
        
        
            if(failures.Count!=0)
            {
                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
