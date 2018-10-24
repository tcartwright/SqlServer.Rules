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
    public sealed class DoNotUseNOLOCKRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0034";
        public const string RuleDisplayName = "Do not use the NOLOCK clause.";
        private const string Message = RuleDisplayName;

        public DoNotUseNOLOCKRule() : base(ProgrammingAndViewSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);
            var visitor = new TableHintVisitor();

            fragment.Accept(visitor);

            var offenders =
                from n in visitor.Statements
                where n.HintKind == TableHintKind.NoLock
                select n;

            problems.AddRange(offenders.Select(o => new SqlRuleProblem(Message, sqlObj, o)));

            return problems;
        }
    }
}