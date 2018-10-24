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
    public sealed class SetNoCountOnRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0005";
        public const string RuleDisplayName = "SET NOCOUNT ON is recommended to be enabled in stored procedures and triggers.";
        public const string Message = RuleDisplayName;

        public SetNoCountOnRule() : base(ModelSchema.Procedure, ModelSchema.DmlTrigger)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(
                typeof(CreateProcedureStatement), 
                typeof(CreateTriggerStatement));
            var visitor = new PredicateVisitor();
            fragment.Accept(visitor);

            var predicates = from o
                            in visitor.Statements
                             where o.Options == SetOptions.NoCount && o.IsOn
                             select o;

            var createToken = fragment.ScriptTokenStream.FirstOrDefault(t => t.TokenType == TSqlTokenType.Create);

            if (predicates.Count() <= 0 && Ignorables.ShouldNotIgnoreRule(fragment.ScriptTokenStream, RuleId, createToken.Line))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }
            return problems;
        }
    }
}
