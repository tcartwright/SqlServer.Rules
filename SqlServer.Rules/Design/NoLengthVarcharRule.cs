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
    public sealed class NoLengthVarcharRule : TypesMissingParametersRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0026";
        public const string RuleDisplayName = "Do not use these data types (VARCHAR, NVARCHAR, CHAR, NCHAR) without specifying length.";
        private const string Message = RuleDisplayName;

        public NoLengthVarcharRule()
            : base(
                new[]
                {
                    ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction, ModelSchema.Table
                }, new [] { SqlDataTypeOption.VarChar, SqlDataTypeOption.NVarChar, SqlDataTypeOption.Char, SqlDataTypeOption.NChar }, 1, Message)
        {
        }
    }
}