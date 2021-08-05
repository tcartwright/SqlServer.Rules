using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Naming
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SqlServer.Rules.Naming.NamingViolationRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Naming,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidFNPrefixRule : NamingViolationRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRN0001";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid 'fn_' prefix when naming functions.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidFNPrefixRule"/> class.
        /// </summary>
        public AvoidFNPrefixRule() : base(
            RuleId,
            RuleDisplayName,
            "fn_",
            new[] { ModelSchema.PartitionFunction, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction },
            n => n.StartsWith)
        { }
    }
}