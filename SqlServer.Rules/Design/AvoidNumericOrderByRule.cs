using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
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
    public sealed class AvoidNumericOrderByRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0025";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid using column numbers in ORDER BY clause.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidNumericOrderByRule"/> class.
        /// </summary>
        public AvoidNumericOrderByRule() : base(ProgrammingSchemas)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);
            var visitor = new OrderByVisitor();

            fragment.Accept(visitor);

            var offenders = (from o in visitor.NotIgnoredStatements(RuleId)
                             from e in o.OrderByElements
                             where e.Expression is IntegerLiteral
                             select e).Distinct();

            foreach (var offender in offenders)
                problems.Add(new SqlRuleProblem(Message, sqlObj, offender));

            return problems;
        }
    }
}