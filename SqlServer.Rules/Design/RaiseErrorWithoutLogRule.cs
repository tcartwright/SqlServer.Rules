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
    public sealed class RaiseErrorWithoutLogRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0044";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "The RAISERROR statement with severity above 18 requires the WITH LOG clause.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RaiseErrorWithoutLogRule"/> class.
        /// </summary>
        public RaiseErrorWithoutLogRule() : base(ProgrammingSchemas)
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
            var visitor = new RaiseErrorVisitor();

            fragment.Accept(visitor);

            var offenders =
                from r in visitor.Statements
                where
                    r.SecondParameter is IntegerLiteral &&
                    int.Parse((r.SecondParameter as Literal)?.Value) > 18 &&
                    (r.RaiseErrorOptions & RaiseErrorOptions.Log) != RaiseErrorOptions.Log
                select r;

            problems.AddRange(offenders.Select(o => new SqlRuleProblem(Message, sqlObj, o)));

            return problems;
        }
    }
}