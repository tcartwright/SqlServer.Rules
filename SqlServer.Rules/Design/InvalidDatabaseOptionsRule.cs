using SqlServer.Rules.Globals;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Model)]
    public sealed class InvalidDatabaseOptionsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0061";
        public const string RuleDisplayName = "The database is configured with invalid options.";
        private const string Message = "The database is configured with these invalid options: {0}.";

        public InvalidDatabaseOptionsRule()
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlModel = ruleExecutionContext.SchemaModel;

            if (sqlModel == null)
                return problems;

            var dbOptions = sqlModel.CopyModelOptions();
            var invalidOptions = new List<string>();

            if (!dbOptions.AnsiNullsOn.GetValueOrDefault(true)) { invalidOptions.Add("ANSI_NULLS OFF"); }
            if (!dbOptions.AnsiPaddingOn.GetValueOrDefault(true)) { invalidOptions.Add("ANSI_PADDING OFF"); }
            if (!dbOptions.AnsiWarningsOn.GetValueOrDefault(true)) { invalidOptions.Add("ANSI_WARNINGS OFF"); }
            if (!dbOptions.ArithAbortOn.GetValueOrDefault(true)) { invalidOptions.Add("ARITHABORT OFF"); }
            if (!dbOptions.ConcatNullYieldsNull.GetValueOrDefault(true)) { invalidOptions.Add("CONCAT_NULL_YIELDS_NULL OFF"); }
            if (!dbOptions.QuotedIdentifierOn.GetValueOrDefault(true)) { invalidOptions.Add("QUOTED_IDENTIFIER OFF"); }

            if (invalidOptions.Count > 0)
            {
                //cant pass null into the SqlRuleProblem ctor so we have to create or get a tsqlobject
                var options = sqlModel.GetObjects(DacQueryScopes.All, ModelSchema.DatabaseOptions).First();
                problems.Add(new SqlRuleProblem(string.Format(Message, string.Join(", ", invalidOptions)), options));
            }
            return problems;
        }
    }
}