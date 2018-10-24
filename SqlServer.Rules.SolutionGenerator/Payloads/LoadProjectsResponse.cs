using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.Rules.SolutionGenerator.Payloads
{
    internal class LoadProjectsResponse : BasePayloadResponse
    {
        internal IList<Project> Projects { get; } = new List<Project>();
    }
}