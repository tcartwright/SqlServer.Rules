using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Performance
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidReturningResultsFromTriggersRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0004";
        public const string RuleDisplayName = "Avoid returning results in triggers.";
        public const string Message = RuleDisplayName;

        public AvoidReturningResultsFromTriggersRule() : base(ModelSchema.DmlTrigger)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(
                typeof(CreateTriggerStatement)
            );

            var selectVisitor = new SelectStatementVisitor();
            fragment.Accept(selectVisitor);

            problems.AddRange(selectVisitor.NotIgnoredStatements(RuleId).Select(t => new SqlRuleProblem(Message, sqlObj, t)));

            return problems;
        }
    }
}
