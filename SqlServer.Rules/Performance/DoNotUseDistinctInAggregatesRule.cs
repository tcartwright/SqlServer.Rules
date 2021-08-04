﻿using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SqlServer.Rules.Performance
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
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class DoNotUseDistinctInAggregatesRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0003";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid using DISTINCT keyword inside of aggregate functions.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;


        /// <summary>
        /// Initializes a new instance of the <see cref="DoNotUseDistinctInAggregatesRule"/> class.
        /// </summary>
        public DoNotUseDistinctInAggregatesRule() : base(ProgrammingAndViewSchemas)
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
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);
            var selectStatementVisitor = new SelectStatementVisitor();
            fragment.Accept(selectStatementVisitor);

            foreach (var statement in selectStatementVisitor.Statements)
            {
                bool found = false;

                if (statement.QueryExpression is QuerySpecification selects)
                {
                    foreach (var selectElement in selects.SelectElements)
                    {
                        var functionCallVisitor = new FunctionCallVisitor();
                        selectElement.Accept(functionCallVisitor);

                        foreach (var function in functionCallVisitor.NotIgnoredStatements(RuleId))
                        {
                            if (function.UniqueRowFilter == UniqueRowFilter.Distinct
                                && Constants.Aggregates.Contains(function.FunctionName.Value.ToUpper()))
                            {
                                problems.Add(new SqlRuleProblem(Message, sqlObj, statement));
                            }
                        }
                        if (found) { break; }
                    }
                }
            }


            return problems;
        }
    }
}
