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
    public sealed class AvoidHintsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0030";
        public const string RuleDisplayName = "Avoid using Hints to force a particular behavior.";
        private const string Message = RuleDisplayName;

        public AvoidHintsRule() : base(ProgrammingAndViewSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);

            var tableHintVisitor = new TableHintVisitor();
            var queryHintVisitor = new StatementListVisitor();
            var joinHintVisitor = new JoinVisitor();

            fragment.Accept(tableHintVisitor, queryHintVisitor, joinHintVisitor);

            var tableOffenders =
               from n in tableHintVisitor.NotIgnoredStatements(RuleId)
               where n.HintKind != TableHintKind.NoLock
               select n;

            var queryOffenders =
                from o in queryHintVisitor.NotIgnoredStatements(RuleId)
                from o1 in o.Statements
                let s = o1 as StatementWithCtesAndXmlNamespaces
                where s?.OptimizerHints != null && s?.OptimizerHints.Count > 0
                select s;

            var joinOffenders =
                from j in joinHintVisitor.NotIgnoredStatements(RuleId)
                where j is QualifiedJoin
                    &&  ((QualifiedJoin)j).JoinHint != JoinHint.None
                select j as QualifiedJoin;

            problems.AddRange(tableOffenders.Select(o => new SqlRuleProblem(Message, sqlObj, o)));
            problems.AddRange(queryOffenders.Select(o => new SqlRuleProblem(Message, sqlObj, o)));
            problems.AddRange(joinOffenders.Select(o => new SqlRuleProblem(Message, sqlObj, o)));

            return problems;
        }
    }
}