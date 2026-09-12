using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Thrown when an operation would violate a domain invariant (e.g. an order with no items)
namespace OrderFlow.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message)  : base(message) { }
    }
}
