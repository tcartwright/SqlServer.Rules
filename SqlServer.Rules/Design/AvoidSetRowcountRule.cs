﻿using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
	/// <summary>
	/// Do not use SET ROWCOUNT to restrict the number of rows. Use the TOP clause instead.
	/// </summary>
	/// <FriendlyName>Do not use SET ROWCOUNT</FriendlyName>
	/// <IsIgnorable>false</IsIgnorable>
	/// <ExampleMd></ExampleMd>
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

			problems.AddRange(visitor.Statements.Select(o => new SqlRuleProblem(Message, sqlObj, o)));

			return problems;
		}
	}
}