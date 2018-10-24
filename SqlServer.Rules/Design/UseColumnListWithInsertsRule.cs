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
    public sealed class UseColumnListWithInsertsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0015";
        public const string RuleDisplayName = "Always use a column list in INSERT statements.";
        private const string Message = "Always use a column list in INSERT statements.";

        public UseColumnListWithInsertsRule() : base(ModelSchema.Procedure)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            List<SqlRuleProblem> problems = new List<SqlRuleProblem>();
            TSqlObject sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));
            var name = sqlObj.Name.GetName();

            var visitor = new InsertStatementVisitor();
            fragment.Accept(visitor);
            if (visitor.Count == 0) { return problems; }

            var offenders = visitor.Statements.Where(s => s.InsertSpecification.Columns.Count == 0).ToList();

            problems.AddRange(offenders.Select(o => new SqlRuleProblem(Message, sqlObj, o)));

            return problems;
        }
    }
}