using SqlServer.Rules.Globals;
using SqlServer.Dac;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Performance
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class NestedViewRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0001";
        public const string RuleDisplayName = "Views should not use other views as a data source.";
        public const string Message = RuleDisplayName;

        public NestedViewRule() : base(ModelSchema.View)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var sqlObjName = ruleExecutionContext.GetObjectName(sqlObj);
            foreach (var child in sqlObj.GetReferenced(DacQueryScopes.UserDefined).Where(x => x.ObjectType == ModelSchema.View))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj) /* { Severity = SqlRuleProblemSeverity.Error } */);
            }

            return problems;
        }
    }
}