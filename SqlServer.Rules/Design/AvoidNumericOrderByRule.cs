using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
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
    public sealed class AvoidNumericOrderByRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0025";
        public const string RuleDisplayName = "Avoid using column numbers in ORDER BY clause.";
        private const string Message = RuleDisplayName;

        public AvoidNumericOrderByRule() : base(ProgrammingSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);
            var visitor = new OrderByVisitor();

            fragment.Accept(visitor);

            var offenders = (from o in visitor.NotIgnoredStatements(RuleId)
                             from e in o.OrderByElements
                             where e.Expression is IntegerLiteral
                             select e).Distinct();

            foreach (var offender in offenders)
                problems.Add(new SqlRuleProblem(Message, sqlObj, offender));

            return problems;
        }
    }
}