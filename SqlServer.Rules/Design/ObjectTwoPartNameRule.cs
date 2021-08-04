﻿using SqlServer.Rules.Globals;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using System;
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
	public sealed class ObjectTwoPartNameRule : BaseSqlCodeAnalysisRule
	{
		/// <summary>
		/// The rule identifier
		/// </summary>
		public const string RuleId = Constants.RuleNameSpace + "SRD0039";
		/// <summary>
		/// The rule display name
		/// </summary>
		public const string RuleDisplayName = "Use fully qualified object names in SELECT, UPDATE, DELETE, MERGE and EXECUTE statements. [schema].[name].";
		/// <summary>
		/// The message
		/// </summary>
		public const string Message = RuleDisplayName;

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectTwoPartNameRule"/> class.
		/// </summary>
		public ObjectTwoPartNameRule() : base(ProgrammingAndViewSchemas)
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

			var objName = sqlObj.Name.GetName();

			var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);

			var fromClauseVisitor = new FromClauseVisitor();
			var execVisitor = new ExecuteVisitor();
			fragment.Accept(fromClauseVisitor, execVisitor);

			var tableVisitor = new NamedTableReferenceVisitor() { TypeFilter = ObjectTypeFilter.PermanentOnly };
			foreach (var from in fromClauseVisitor.Statements)
			{
				from.Accept(tableVisitor);
			}

			var offenders = tableVisitor.Statements.Where(tbl =>
			{
				var id = tbl.GetObjectIdentifier(null);
				return id.Parts.Count < 2 || string.IsNullOrWhiteSpace(id.Parts.First());
			});

			var execOffenders = execVisitor.Statements.Where(proc => CheckProc(proc));

			problems.AddRange(offenders.Select(t => new SqlRuleProblem(Message, sqlObj, t)));
			problems.AddRange(execOffenders.Select(t => new SqlRuleProblem(Message, sqlObj, t)));

			return problems;
		}

		private bool CheckProc(ExecuteStatement proc)
		{
			if (!(proc.ExecuteSpecification.ExecutableEntity is ExecutableProcedureReference execProc))
			{
				return false;
			}

			var id = execProc.ProcedureReference.ProcedureReference.GetObjectIdentifier(null);
			return id.Parts.Count < 2 || string.IsNullOrWhiteSpace(id.Parts.First());
		}
	}
}