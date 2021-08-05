using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;
using System;
using System.Collections.Generic;
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
    public sealed class AvoidTableVariableInJoinsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0014";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid the use of table variables in join clauses.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidTableVariableInJoinsRule"/> class.
        /// </summary>
        public AvoidTableVariableInJoinsRule() : base(ProgrammingSchemas)
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

            if (sqlObj == null || sqlObj.IsWhiteListed()) return problems;
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);
            var joinVisitor = new JoinVisitor();
            fragment.Accept(joinVisitor);

            foreach (var join in joinVisitor.Statements)
            {
                var tableVarVisitor = new TableVariableVisitor();
                join.Accept(tableVarVisitor);

                problems.AddRange(tableVarVisitor.NotIgnoredStatements(RuleId).Select(tv => new SqlRuleProblem(Message, sqlObj, tv)));

            }
            return problems;
        }
    }
}