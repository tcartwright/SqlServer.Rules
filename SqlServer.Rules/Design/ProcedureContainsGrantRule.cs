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
    public sealed class ProcedureContainsGrantRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0060";
        public const string RuleDisplayName = "The procedure grants itself permissions. Possible missing GO command.";
        private const string Message = RuleDisplayName;

        public ProcedureContainsGrantRule() : base(ModelSchema.Procedure)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));
            var objName = sqlObj.Name.GetName();

            var grantVisitor = new GrantVisitor();
            fragment.Accept(grantVisitor);

            if (grantVisitor.Statements
                .Any(g => g.SecurityTargetObject.ObjectName.MultiPartIdentifier.CompareTo(sqlObj.Name) >= 5))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }

            return problems;
        }
    }
}