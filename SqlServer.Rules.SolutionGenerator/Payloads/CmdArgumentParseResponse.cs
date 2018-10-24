using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.Rules.SolutionGenerator.Payloads
{
    internal class CmdArgumentParseResponse : BasePayloadResponse
    {
        internal CmdLineOptions Options { get; set; } = new CmdLineOptions();
    }
}