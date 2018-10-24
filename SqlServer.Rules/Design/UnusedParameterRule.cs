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
    public sealed class UnusedParameterRule : BaseSqlCodeAnalysisRule
    {
        public const string BaseRuleId = "SRD0016";
        public const string RuleId = Constants.RuleNameSpace + BaseRuleId;
        public const string RuleDisplayName = "Input parameter never used. Consider removing the parameter or using it.";
        private const string Message = "Input parameter '{0}' is never used. Consider removing the parameter or using it.";

        public UnusedParameterRule() : base(ProgrammingSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = sqlObj.GetFragment();
            if (fragment.ScriptTokenStream == null) { return problems; }

            var visitor = new VariablesVisitor();
            fragment.Accept(visitor);


            var parms = from pp in visitor.ProcedureParameters
                        join t in fragment.ScriptTokenStream
                            on new { Name = pp.VariableName.Value?.ToLower(), Type = TSqlTokenType.Variable }
                            equals new { Name = t.Text?.ToLower(), Type = t.TokenType }
                        where Ignorables.ShouldNotIgnoreRule(fragment.ScriptTokenStream, RuleId, pp.StartLine)
                        select pp;

            var unusedParms = parms.GroupBy(p => p.VariableName.Value?.ToLower())
                .Where(g => g.Count() == 1).Select(g => g.First());

            problems.AddRange(unusedParms.Select(rp => new SqlRuleProblem(string.Format(Message, rp.VariableName.Value), sqlObj, rp)));

            return problems;
        }
    }
}