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
    public sealed class ObjectCreatedWithInvalidOptionsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0055";
        public const string RuleDisplayName = "The object was created with invalid options.";
        private const string Message = "The object was created with the invalid options: {0}.";
        private const string MessageNoEffect = "The object was created with the invalid options: {0} but should have little to no effect upon behavior.";

        public ObjectCreatedWithInvalidOptionsRule() : base(ModelSchema.Table,
                ModelSchema.Procedure,
                ModelSchema.ScalarFunction,
                ModelSchema.TableValuedFunction,
                ModelSchema.DmlTrigger
        )
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(
                typeof(CreateTableStatement),
                typeof(CreateProcedureStatement),
                typeof(CreateFunctionStatement),
                typeof(CreateTriggerStatement)
            );
            var objName = sqlObj.Name.GetName();
            var objType = sqlObj.ObjectType;

            ModelPropertyClass ansiNullsOption = null;
            ModelPropertyClass quotedIdentifierOption = null;
            var impactsFunctionality = false;
            var objTypeName = string.Empty;

            if (objType == Table.TypeClass)
            {
                impactsFunctionality = true;
                ansiNullsOption = Table.AnsiNullsOn;
                quotedIdentifierOption = Table.QuotedIdentifierOn;
                objTypeName = "table";
            }
            else if (objType == ScalarFunction.TypeClass)
            {
                ansiNullsOption = ScalarFunction.AnsiNullsOn;
                quotedIdentifierOption = ScalarFunction.QuotedIdentifierOn;
                objTypeName = "function";
            }
            else if (objType == TableValuedFunction.TypeClass)
            {
                ansiNullsOption = TableValuedFunction.AnsiNullsOn;
                quotedIdentifierOption = TableValuedFunction.QuotedIdentifierOn;
                objTypeName = "function";
            }
            else if (objType == Procedure.TypeClass)
            {
                ansiNullsOption = Procedure.AnsiNullsOn;
                quotedIdentifierOption = Procedure.QuotedIdentifierOn;
                objTypeName = "stored procedure";
            }
            else if (objType == DmlTrigger.TypeClass)
            {
                ansiNullsOption = DmlTrigger.AnsiNullsOn;
                quotedIdentifierOption = DmlTrigger.QuotedIdentifierOn;
                objTypeName = "trigger";
            }

            var ansiNullsOn = sqlObj.GetProperty<bool>(ansiNullsOption);
            var quotedIdentifierOn = sqlObj.GetProperty<bool>(quotedIdentifierOption);

            if (!ansiNullsOn || !quotedIdentifierOn)
            {
                var options = new List<string>();
                if (!ansiNullsOn) { options.Add("ANSI_NULLS OFF"); }
                if (!quotedIdentifierOn) { options.Add("QUOTED_IDENTIFIER OFF"); }

                var errorMessage = string.Format(impactsFunctionality ? Message : MessageNoEffect, string.Join(", ", options));
                problems.Add(new SqlRuleProblem(errorMessage, sqlObj));
            }

            return problems;
        }
    }
}