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
    public sealed class AvoidSPPrefixRule : NamingViolationRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRN0002";
        public const string RuleDisplayName = "Avoid 'sp_' prefix when naming stored procedures.";

        public AvoidSPPrefixRule() : base(
            RuleId,
            RuleDisplayName,
            "sp_",
            new[] { ModelSchema.Procedure },
            n => n.StartsWith)
        { }
    }
}