using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
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
    public sealed class UseParameterNamesWithProcsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0058";
        public const string RuleDisplayName = "Always use parameter names when calling stored procedures.";
        private const string Message = RuleDisplayName;

        public UseParameterNamesWithProcsRule() : base(ModelSchema.Procedure)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));

            var execVisitor = new ExecuteVisitor();
            fragment.Accept(execVisitor);

            if (execVisitor.Count == 0) { return problems; }

            foreach (var exec in execVisitor.Statements)
            {
                if (exec.ExecuteSpecification.ExecutableEntity is ExecutableProcedureReference proc
                    && proc.Parameters.Any(p => p.Variable == null))
                {
                    problems.Add(new SqlRuleProblem(Message, sqlObj, exec));
                }
            }
            return problems;
        }
    }
}