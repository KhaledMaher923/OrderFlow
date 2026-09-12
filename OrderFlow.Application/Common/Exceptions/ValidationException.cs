using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get;}

        public ValidationException() : base("One or more validation failures occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }
        public ValidationException(IEnumerable<ValidationFailure> failures)
        {
            Errors = failures
                .GroupBy(F => F.PropertyName, f => f.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray() );
        }

    }
}
