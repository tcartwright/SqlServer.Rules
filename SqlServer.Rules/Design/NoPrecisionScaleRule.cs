using SqlServer.Rules.Globals;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SqlServer.Rules.Design.TypesMissingParametersRule" />
    [ExportCodeAnalysisRule(RuleId,
    RuleDisplayName,
    Description = RuleDisplayName,
    Category = Constants.Design,
    RuleScope = SqlRuleScope.Element)]
    public sealed class NoPrecisionScaleRule : TypesMissingParametersRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0027";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Do not use DECIMAL or NUMERIC data types without specifying precision and scale.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoPrecisionScaleRule"/> class.
        /// </summary>
        public NoPrecisionScaleRule() : base(new[]
                {
                    ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction, ModelSchema.Table
                }, new[] { SqlDataTypeOption.Decimal, SqlDataTypeOption.Numeric }, 2, Message)
        { }
    }
}