using SqlServer.Rules.Globals;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class TableHasUniqueConstraintRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0001";
        public const string RuleDisplayName = "Table does not have a natural key.";
        public const string Message = RuleDisplayName;

        public TableHasUniqueConstraintRule() : base(ModelSchema.Table)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var key = sqlObj.GetChildren(DacQueryScopes.All).FirstOrDefault(x => x.ObjectType == ModelSchema.PrimaryKeyConstraint);
            if (key == null) { return problems; }

            var keyColumns = key.GetReferenced().Where(x => x.ObjectType == Column.TypeClass);

            if (keyColumns.Count() == 1)
            {
                var keyColumn = keyColumns.First();
                bool? isIdentity = keyColumn.GetProperty<bool?>(Column.IsIdentity);
                var keyColumnDefault = keyColumn.GetReferencing().FirstOrDefault(x => x.ObjectType == DefaultConstraint.TypeClass);
                bool? isSequence = keyColumnDefault?.GetReferenced().Any(x => x.ObjectType == Sequence.TypeClass);

                //if our primary key consists of a identity or sequence, check to see if we have a unique constraint. assume the unique constraint is a natural key if we find one. else, problem.
                if (isIdentity.GetValueOrDefault(false) || isSequence.GetValueOrDefault(false))
                {
                    var uc = sqlObj.GetChildren(DacQueryScopes.All).FirstOrDefault(x => x.ObjectType == ModelSchema.UniqueConstraint);
                    if (uc == null)
                    {
                        //no unique constraint. search for a unique index
                        var indexes = sqlObj.GetChildren(DacQueryScopes.All).Where(x => x.ObjectType == ModelSchema.Index);
                        if (!indexes.Any(ix => Convert.ToBoolean(ix.GetProperty(Index.Unique))))
                        {
                            problems.Add(new SqlRuleProblem(Message, sqlObj));
                        }
                    }
                }
            }


            return problems;
        }
    }
}