using SqlServer.Rules.Globals;
using SqlServer.Rules.ReferentialIntegrity;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class ForeignKeysNeedIndexRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0004";
        public const string RuleDisplayName = "Columns on both sides of a foreign key should be indexed.";
        private const string Message = "Columns on both sides of a foreign key should be indexed for performance reasons.";

        public ForeignKeysNeedIndexRule() : base(ModelSchema.ForeignKeyConstraint)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fkinfo = sqlObj.GetFKInfo();

            var table = ruleExecutionContext.SchemaModel.GetObject(Table.TypeClass, fkinfo.TableName, DacQueryScopes.All);
            if (!table.CheckForFkIndex(fkinfo.ColumnNames))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
                return problems;
            }

            table = ruleExecutionContext.SchemaModel.GetObject(Table.TypeClass, fkinfo.ToTableName, DacQueryScopes.All);
            if (!table.CheckForFkIndex(fkinfo.ToColumnNames))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
                return problems;
            }

            return problems;
        }
    }
}