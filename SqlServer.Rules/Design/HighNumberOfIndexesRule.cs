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
    public sealed class HighNumberOfIndexesRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0045";
        public const string RuleDisplayName = "Excessive number of indexes on table found on table.";
        private const string Message = RuleDisplayName;

        public HighNumberOfIndexesRule() : base(ModelSchema.Table)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var objName = sqlObj.Name.GetName();

            var indexes = sqlObj.GetReferencing(DacQueryScopes.All).Where(x => x.ObjectType == Index.TypeClass);
            
            var count = indexes.Count();
            if (count > 15)
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }

            return problems;
        }
    }
}