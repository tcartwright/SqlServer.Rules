using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidCHARTypeRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0005";
        public const string RuleDisplayName = "Avoid the (n)char column type except for short static length data.";
        private const string Message = "Avoid the (n)char column type except for short static length data.";

        public AvoidCHARTypeRule() : base(ModelSchema.Table)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            List<SqlRuleProblem> problems = new List<SqlRuleProblem>();
            TSqlObject sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateTableStatement));
            var tableName = sqlObj.Name.GetName();

            ColumnDefinitionVisitor columnVisitor = new ColumnDefinitionVisitor();
            fragment.Accept(columnVisitor);

            var longChars = columnVisitor.Statements
                .Where(col => col.DataType != null && col.DataType.Name != null)
                .Select(col => new
                {
                    column = col,
                    name = col.ColumnIdentifier.Value,
                    type = col.DataType.Name.Identifiers.FirstOrDefault()?.Value,
                    length = GetDataTypeLength(col)
                })
                .Where(x => (_comparer.Equals(x.type, "char") || _comparer.Equals(x.type, "nchar")) && x.length > 9);

            problems.AddRange(longChars.Select(col => new SqlRuleProblem(Message, sqlObj, col.column)));

            return problems;
        }

        private decimal GetDataTypeLength(ColumnDefinition col)
        {
            if (col.DataType is SqlDataTypeReference dataType)
            {
                return dataType.GetDataTypeParameters().FirstOrDefault();
            }
            return 0;
        }
    }
}