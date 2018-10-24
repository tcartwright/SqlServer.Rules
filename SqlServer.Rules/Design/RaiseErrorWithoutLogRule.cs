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
    public sealed class RaiseErrorWithoutLogRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0044";
        public const string RuleDisplayName = "The RAISERROR statement with severity above 18 requires the WITH LOG clause.";
        private const string Message = RuleDisplayName;

        public RaiseErrorWithoutLogRule() : base(ProgrammingSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);
            var visitor = new RaiseErrorVisitor();

            fragment.Accept(visitor);

            var offenders =
                from r in visitor.Statements
                where
                    r.SecondParameter is IntegerLiteral &&
                    int.Parse((r.SecondParameter as Literal)?.Value) > 18 &&
                    (r.RaiseErrorOptions & RaiseErrorOptions.Log) != RaiseErrorOptions.Log
                select r;

            problems.AddRange(offenders.Select(o => new SqlRuleProblem(Message, sqlObj, o)));

            return problems;
        }
    }
}