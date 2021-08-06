using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Dac;
using SqlServer.Rules.Globals;
using System;
using System.Collections.Generic;

namespace SqlServer.Rules.Naming
{
    /// <summary>
    /// General naming violation.
    /// </summary>
    /// <FriendlyName>Name standard</FriendlyName>
	/// <IsIgnorable>false</IsIgnorable>
	/// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Violates configure naming convention. Please see docs on setting naming patterns
    /// </remarks>
	/// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    public class NamingViolationRule : BaseSqlCodeAnalysisRule
    {
        private readonly string _RuleId;
        /// <summary>
        /// The message
        /// </summary>
        protected readonly string Message;
        /// <summary>
        /// The bad characters
        /// </summary>
        protected readonly string BadCharacters;
        /// <summary>
        /// The partial predicate
        /// </summary>
        protected readonly Func<string, Predicate<string>> PartialPredicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamingViolationRule"/> class.
        /// </summary>
        /// <param name="ruleId">The rule identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="badPrefix">The bad prefix.</param>
        /// <param name="appliesTo">The applies to.</param>
        /// <param name="predicate">The predicate.</param>
        public NamingViolationRule(
            string ruleId,
            string message,
            string badPrefix,
            IList<ModelTypeClass> appliesTo,
            Func<string, Predicate<string>> predicate)
        {
            _RuleId = ruleId;
            Message = message;
            BadCharacters = badPrefix;
            SupportedElementTypes = appliesTo;
            PartialPredicate = predicate;
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

            var name = ruleExecutionContext.GetObjectName(sqlObj, ElementNameStyle.SimpleName).ToLower();
            var fragment = ruleExecutionContext.GetFragment();

            if (PartialPredicate(name)(BadCharacters)
                && Ignorables.ShouldNotIgnoreRule(fragment.ScriptTokenStream, _RuleId, fragment.StartLine))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }

            return problems;
        }
    }
}