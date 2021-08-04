﻿using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// 
    /// </summary>
    /// <FriendlyName></FriendlyName>
	/// <IsIgnorable>false</IsIgnorable>
	/// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class ObjectCreatedWithInvalidOptionsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0055";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "The object was created with invalid options.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "The object was created with the invalid options: {0}.";
        /// <summary>
        /// The message no effect
        /// </summary>
        public const string MessageNoEffect = "The object was created with the invalid options: {0} but should have little to no effect upon behavior.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectCreatedWithInvalidOptionsRule"/> class.
        /// </summary>
        public ObjectCreatedWithInvalidOptionsRule() : base(ModelSchema.Table,
                ModelSchema.Procedure,
                ModelSchema.ScalarFunction,
                ModelSchema.TableValuedFunction,
                ModelSchema.DmlTrigger
        )
        {
        }

        /// <summary>
        /// Performs analysis and returns a list of problems detected
        /// </summary>
        /// <param name="ruleExecutionContext">Contains the schema model and model element to analyze</param>
        /// <returns>
        /// The problems detected by the rule in the given element
        /// </returns>
        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var objType = sqlObj.ObjectType;

            ModelPropertyClass ansiNullsOption = null;
            ModelPropertyClass quotedIdentifierOption = null;
            var impactsFunctionality = false;

            if (objType == Table.TypeClass)
            {
                impactsFunctionality = true;
                ansiNullsOption = Table.AnsiNullsOn;
                quotedIdentifierOption = Table.QuotedIdentifierOn;
            }
            else if (objType == ScalarFunction.TypeClass)
            {
                ansiNullsOption = ScalarFunction.AnsiNullsOn;
                quotedIdentifierOption = ScalarFunction.QuotedIdentifierOn;
            }
            else if (objType == TableValuedFunction.TypeClass)
            {
                ansiNullsOption = TableValuedFunction.AnsiNullsOn;
                quotedIdentifierOption = TableValuedFunction.QuotedIdentifierOn;
            }
            else if (objType == Procedure.TypeClass)
            {
                ansiNullsOption = Procedure.AnsiNullsOn;
                quotedIdentifierOption = Procedure.QuotedIdentifierOn;
            }
            else if (objType == DmlTrigger.TypeClass)
            {
                ansiNullsOption = DmlTrigger.AnsiNullsOn;
                quotedIdentifierOption = DmlTrigger.QuotedIdentifierOn;
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