/*
 * TIM C: 1/19/2018 Commented this out AS the dacpac ALWAYS reports two part names even when the file is missing the schema. 
 */

using SqlServer.Rules.Globals;
using SqlServer.Dac;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Naming
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Naming,
        RuleScope = SqlRuleScope.Element)]
    public sealed class UseTwoPartNames : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRN0006";
        public const string RuleDisplayName = "Two part naming on objects is required.";
        private const string Message = RuleDisplayName;

        public UseTwoPartNames() : base(
            ModelSchema.Table,
            ModelSchema.View,
            ModelSchema.Procedure,
            ModelSchema.ScalarFunction,
            ModelSchema.TableValuedFunction,
            ModelSchema.DmlTrigger,
            ModelSchema.Sequence,
            ModelSchema.Default,
            ModelSchema.UserDefinedType,
            ModelSchema.TableType
        )
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = ruleExecutionContext.GetFragment();
            var objectId = fragment.GetObjectName(null);

            if (objectId != null && objectId.Parts.Count < 2)
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }
            return problems;
        }
    }
}