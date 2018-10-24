using SqlServer.Rules.Globals;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;

namespace SqlServer.Rules.Naming
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Naming,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidFNPrefixRule : NamingViolationRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRN0001";
        public const string RuleDisplayName = "Avoid 'fn_' prefix when naming functions.";

        public AvoidFNPrefixRule() : base(
            RuleId,
            RuleDisplayName,
            "fn_",
            new[] { ModelSchema.PartitionFunction, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction },
            n => n.StartsWith)
        { }
    }
}