using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Performance
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidTableVariableInJoinsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0014";
        public const string RuleDisplayName = "Avoid the use of table variables in join clauses.";
        public const string Message = RuleDisplayName;

        public AvoidTableVariableInJoinsRule() : base(ProgrammingSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed()) return problems;
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);
            var joinVisitor = new JoinVisitor();
            fragment.Accept(joinVisitor);

            foreach (var join in joinVisitor.Statements)
            {
                var tableVarVisitor = new TableVariableVisitor();
                join.Accept(tableVarVisitor);

                problems.AddRange(tableVarVisitor.NotIgnoredStatements(RuleId).Select(tv => new SqlRuleProblem(Message, sqlObj, tv)));

            }
            return problems;
        }
    }
}