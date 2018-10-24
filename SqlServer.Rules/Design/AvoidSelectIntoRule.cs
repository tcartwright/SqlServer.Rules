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
    public sealed class AvoidSelectIntoRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0041";
        public const string RuleDisplayName = "Avoid use of the SELECT INTO syntax.";
        private const string Message = RuleDisplayName;

        public AvoidSelectIntoRule() : base(ProgrammingSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);
            var visitor = new SelectStatementVisitor();

            fragment.Accept(visitor);

            var offenders =
                from s in visitor.NotIgnoredStatements(RuleId)
                let tn = s.Into == null ? "" : s.Into.Identifiers?.LastOrDefault()?.Value
                where s.Into != null && !(tn.StartsWith("#") || !tn.StartsWith("@"))
                select s.Into;

            problems.AddRange(offenders.Select(s => new SqlRuleProblem(Message, sqlObj, s)));

            return problems;
        }
    }
}