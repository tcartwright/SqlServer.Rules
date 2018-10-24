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
    public sealed class UnusedVariablesRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0012";
        public const string RuleDisplayName = "Variable declared but never referenced or assigned.";
        private const string Message = "Variable '{0}' is declared but never referenced or assigned.";

        public UnusedVariablesRule() : base(ProgrammingSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }


            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);
            if(fragment.ScriptTokenStream == null) { return problems; }
            var visitor = new DeclareVariableElementVisitor();
            fragment.Accept(visitor);

            var vars = from pp in visitor.Statements
                        join t in fragment.ScriptTokenStream
                            on new { Name = pp.VariableName.Value?.ToLower(), Type = TSqlTokenType.Variable }
                            equals new { Name = t.Text?.ToLower(), Type = t.TokenType }
                        select pp;

            var unusedVars = vars.GroupBy(p => p.VariableName.Value?.ToLower()).Where(g => g.Count() == 1).Select(g => g.First());

            problems.AddRange(unusedVars.Select(v => new SqlRuleProblem(string.Format(Message, v.VariableName.Value), sqlObj, v)));

            return problems;
        }
    }
}