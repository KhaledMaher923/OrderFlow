using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Common.Observability
{
    public static class OrderFlowActivitySource
    {
        public const string SourceName = "OrderFlow";
        public static readonly ActivitySource Source = new(SourceName);
    }
}
