using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.Rules.SolutionGenerator.Payloads
{
    internal enum CompletionResult
    {
        Success,
        Failure
    }

    internal class BasePayloadResponse
    {
        internal CompletionResult Result { get; set; }
        internal List<string> Messages { get; } = new List<string>();
    }
}