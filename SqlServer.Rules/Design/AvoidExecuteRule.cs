using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
    RuleDisplayName,
    Description = RuleDisplayName,
    Category = Constants.Design,
    RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidExecuteRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0024";
        public const string RuleDisplayName = "Avoid EXEC and EXECUTE with string literals. Use parameterized sp_executesql instead.";
        private const string Message = RuleDisplayName;

        public AvoidExecuteRule() : base(ModelSchema.Procedure)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));
            var visitor = new ExecutableStringListVisitor();
            fragment.Accept(visitor);

            problems.AddRange(visitor.NotIgnoredStatements(RuleId).Select(o => new SqlRuleProblem(Message, sqlObj, o)));

            return problems;
        }
    }
}