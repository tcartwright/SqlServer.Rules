using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;
using SqlServer.Rules.ReferentialIntegrity;
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
    public sealed class AvoidNotForReplication : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0065";
        public const string RuleDisplayName = Message;
        public const string Message = "Avoid 'NOT FOR REPLICATION' unless this is the desired behavior and replication is in use.";

        public AvoidNotForReplication() : base(
            ModelSchema.ForeignKeyConstraint,
            ModelSchema.CheckConstraint,
            ModelSchema.DmlTrigger,
            ModelSchema.Table)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var obj = ruleExecutionContext.SchemaModel.GetObject(sqlObj.ObjectType, sqlObj.Name, DacQueryScopes.All);

            bool notForReplication = false;

            if (sqlObj.ObjectType == ForeignKeyConstraint.TypeClass)
            {
                notForReplication = Convert.ToBoolean(sqlObj.GetProperty(ForeignKeyConstraint.NotForReplication));
            }
            else if (sqlObj.ObjectType == CheckConstraint.TypeClass)
            {
                notForReplication = Convert.ToBoolean(sqlObj.GetProperty(CheckConstraint.NotForReplication));
            }
            else if (sqlObj.ObjectType == DmlTrigger.TypeClass)
            {
                notForReplication = Convert.ToBoolean(sqlObj.GetProperty(DmlTrigger.NotForReplication));
            }
            else if (sqlObj.ObjectType == Table.TypeClass)
            {
                var createTable = ruleExecutionContext.ScriptFragment as CreateTableStatement;
                var identityColumn = createTable.Definition.ColumnDefinitions.FirstOrDefault(cd => cd.IdentityOptions != null);
                notForReplication = identityColumn.IdentityOptions.IsIdentityNotForReplication;
            }

            if (notForReplication)
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }

            return problems;
        }
    }
}