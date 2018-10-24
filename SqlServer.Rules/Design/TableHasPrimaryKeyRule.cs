using SqlServer.Rules.Globals;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class TableHasPrimaryKeyRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0002";
        public const string RuleDisplayName = "Table does not have a primary key.";
        public const string Message = RuleDisplayName;

        public TableHasPrimaryKeyRule() : base(ModelSchema.Table)
        {
            SupportedElementTypes = new[] { ModelSchema.Table };
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var child = sqlObj.GetChildren(DacQueryScopes.All)
                .FirstOrDefault(x => x.ObjectType == ModelSchema.PrimaryKeyConstraint);
            if (child == null)
            {
                var parentObj = sqlObj.Name.HasName ? sqlObj : sqlObj.GetParent(DacQueryScopes.All);

                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }


            return problems;
        }
    }
}