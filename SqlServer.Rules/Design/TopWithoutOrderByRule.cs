using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class TopWithoutOrderByRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0014";
        public const string RuleDisplayName = "TOP clause used in a query without an ORDER BY clause.";
        private const string Message = RuleDisplayName;

        public TopWithoutOrderByRule() : base(ProgrammingAndViewSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);
            var topVisitor = new TopRowFilterVisitor();

            fragment.Accept(topVisitor);

            if (topVisitor.Count > 0)
            {
                var orderByvisitor = new OrderByVisitor();
                fragment.Accept(orderByvisitor);
                if (orderByvisitor.Count < 1)
                    problems.Add(new SqlRuleProblem(Message, sqlObj, topVisitor.Statements[0]));
            }

            return problems;
        }
    }
}