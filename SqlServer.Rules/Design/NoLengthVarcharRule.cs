using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Rules.Globals;

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
    public sealed class NoLengthVarcharRule : TypesMissingParametersRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0026";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Do not use these data types (VARCHAR, NVARCHAR, CHAR, NCHAR) without specifying length.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoLengthVarcharRule"/> class.
        /// </summary>
        public NoLengthVarcharRule()
            : base(
                new[]
                {
                    ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction, ModelSchema.Table
                }, new[] { SqlDataTypeOption.VarChar, SqlDataTypeOption.NVarChar, SqlDataTypeOption.Char, SqlDataTypeOption.NChar }, 1, Message)
        {
        }
    }
}