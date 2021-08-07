using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Do not use SET ROWCOUNT to restrict the number of rows. Use the TOP clause instead.
    /// </summary>
    /// <FriendlyName>Depreciated result limiting</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The rule checks for usage of the SET ROWCOUNT setting. It is recommended to use the TOP
    /// clause or the new in SQL 2012 FETCH keyword instead of SET ROWCOUNT as it will not be
    /// supported in the future versions of SQL Server for INSERT,UPDATE and DELETE statements. In
    /// addition to that is being phased out, the SET ROWCOUNT has another problem - when a ROWCOUNT
    /// is set and there is INSERT, UPDATE, DELETE or MERGE statements which fire a trigger, all the
    /// statements in the trigger will have the same row limit applied.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
    RuleDisplayName,
    Description = RuleDisplayName,
    Category = Constants.Design,
    RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidSetRowcountRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0036";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Do not use SET ROWCOUNT to restrict the number of rows.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidSetRowcountRule"/> class.
        /// </summary>
        public AvoidSetRowcountRule() : base(ProgrammingSchemas)
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

            var visitor = new RowCountVisitor();
            fragment.Accept(visitor);

            problems.AddRange(visitor.NotIgnoredStatements(RuleId).Select(o => new SqlRuleProblem(Message, sqlObj, o)));

            return problems;
        }
    }
}
