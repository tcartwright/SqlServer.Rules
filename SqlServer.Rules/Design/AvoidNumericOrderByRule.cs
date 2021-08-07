using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    /// <summary>Avoid the use of column numbers in a where clause. If someone changes the select
    ///  query without updating the column number, then the sorting could inadvertently change.</summary>
    /// <FriendlyName>Order by with ordinal references</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The rule checks for `ORDER BY` clauses which reference select list column using the column
    /// number instead of the column name. The column numbers in the `ORDER BY` clause as it impairs
    /// the readability of the SQL statement. Further, changing the order of columns in the `SELECT`
    /// list has no impact on the `ORDER BY` when the columns are referred by names instead of numbers.
    /// </remarks>
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
