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
    public sealed class DoNotUseDeprecatedTypesRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0051";
        public const string RuleDisplayName = "Don’t use deprecated TEXT, NTEXT and IMAGE data types.";
        private const string Message = RuleDisplayName;

        public DoNotUseDeprecatedTypesRule() : base(ModelSchema.Table)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            List<SqlRuleProblem> problems = new List<SqlRuleProblem>();
            TSqlObject sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateTableStatement));
            var objName = sqlObj.Name.GetName();

            var columnVisitor = new ColumnDefinitionVisitor();
            fragment.Accept(columnVisitor);

            var offenders = columnVisitor.Statements
                .Where(col => col.DataType != null && col.DataType.Name != null)
                .Select(col => new
                {
                    column = col,
                    name = col.ColumnIdentifier.Value,
                    type = col.DataType.Name.Identifiers.FirstOrDefault()?.Value
                })
                .Where(x => _comparer.Equals(x.type, "text") 
                    || _comparer.Equals(x.type, "ntext") 
                    || _comparer.Equals(x.type, "image"));

            problems.AddRange(offenders.Select(col => new SqlRuleProblem(Message, sqlObj, col.column)));

            return problems;
        }
    }
}