﻿using SqlServer.Rules.Globals;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using System.Collections.Generic;
using System.Linq;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// 
    /// </summary>
    /// <FriendlyName></FriendlyName>
	/// <IsIgnorable>false</IsIgnorable>
	/// <ExampleMd></ExampleMd>
	/// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class WaitForRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0035";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Do not use WAITFOR DELAY/TIME statement in stored procedures, functions, and triggers.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitForRule"/> class.
        /// </summary>
        public WaitForRule() : base(ModelSchema.Procedure,
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(
                    typeof(CreateProcedureStatement),
                    typeof(CreateFunctionStatement),
                    typeof(CreateTriggerStatement)
                );
            var visitor = new WaitForVisitor();

            fragment.Accept(visitor);

            problems.AddRange(visitor.Statements.Select(o => new SqlRuleProblem(Message, sqlObj, o)));

            return problems;
        }
    }
}