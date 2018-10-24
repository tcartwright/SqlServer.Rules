using Microsoft.SqlServer.Dac.CodeAnalysis;
using System.Linq;

namespace SqlServer.Rules.Report
{
    public static class ProblemExtensions
    {
        public static string Rule(this SqlRuleProblem problem) => problem.RuleId.Split('.').Last();
    }
}