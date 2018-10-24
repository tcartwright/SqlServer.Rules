using SqlServer.Rules.Globals;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class TableMissingClusteredIndexRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0020";
        public const string RuleDisplayName = "Table does not have a clustered index.";
        public const string Message = RuleDisplayName;

        public TableMissingClusteredIndexRule() : base(ModelSchema.Table)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var indexes = sqlObj.GetChildren(DacQueryScopes.All)
                .Where(x => 
                    x.ObjectType == ModelSchema.Index 
                    || x.ObjectType == ModelSchema.PrimaryKeyConstraint).ToList();
            if (!indexes.Any(i => IsClustered(i)))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }
            
            return problems;
        }

        private bool IsClustered(TSqlObject i)
        {
            if (i.ObjectType == ModelSchema.Index)
            {
                return Convert.ToBoolean(i.GetProperty(Index.Clustered));
            }
            else
            {
                return Convert.ToBoolean(i.GetProperty(PrimaryKeyConstraint.Clustered));
            }
        }
    }
}