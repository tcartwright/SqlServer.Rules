using SqlServer.Rules.Globals;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
    RuleDisplayName,
    Description = RuleDisplayName,
    Category = Constants.Design,
    RuleScope = SqlRuleScope.Element)]
    public sealed class NoPrecisionScaleRule : TypesMissingParametersRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0027";
        public const string RuleDisplayName = "Do not use DECIMAL or NUMERIC data types without specifying precision and scale.";
        private const string Message = RuleDisplayName;

        public NoPrecisionScaleRule() : base(new[]
                {
                    ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction, ModelSchema.Table
                }, new[] { SqlDataTypeOption.Decimal, SqlDataTypeOption.Numeric }, 2, Message)
        { }
    }
}