using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    public sealed class EqualityCompareWithNULLRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0011";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Equality and inequality comparisons involving a NULL constant found. Use IS NULL or IS NOT NULL.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualityCompareWithNULLRule"/> class.
        /// </summary>
        public EqualityCompareWithNULLRule() : base(ProgrammingAndViewSchemas)
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
            var sqlObj = ruleExecutionContext.ModelElement; //proc / view / function
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            try
            {
                var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);

                //get the combined parameters and declare variables into one searchable list
                var variablesVisitor = new VariablesVisitor();
                fragment.AcceptChildren(variablesVisitor);
                var variables = variablesVisitor.GetVariables();

                var selectStatementVisitor = new SelectStatementVisitor();
                fragment.Accept(selectStatementVisitor);
                foreach (var select in selectStatementVisitor.Statements)
                {
                    if (select.QueryExpression is QuerySpecification query && query.WhereClause != null)
                    {
                        var booleanComparisonVisitor = new BooleanComparisonVisitor();
                        query.WhereClause.Accept(booleanComparisonVisitor);

                        foreach (var comparison in booleanComparisonVisitor.Statements)
                        {
                            if ((comparison.FirstExpression is NullLiteral || comparison.SecondExpression is NullLiteral) &&
                                (comparison.ComparisonType == BooleanComparisonType.Equals
                                || comparison.ComparisonType == BooleanComparisonType.NotEqualToBrackets
                                || comparison.ComparisonType == BooleanComparisonType.NotEqualToExclamation) //probably can remove the ComparisonTypeCheck
                                )
                            {
                                problems.Add(new SqlRuleProblem(Message, sqlObj, comparison));
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                //TODO: PROPERLY LOG THIS ERROR
                Debug.WriteLine(ex.ToString());
                //throw;
            }

            return problems;
        }
    }
}