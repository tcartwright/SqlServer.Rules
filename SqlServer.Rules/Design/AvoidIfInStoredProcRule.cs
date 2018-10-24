using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
	[ExportCodeAnalysisRule(RuleId,
		RuleDisplayName,
		Description = RuleDisplayName,
		Category = Constants.Design,
		RuleScope = SqlRuleScope.Element)]
	public sealed class AvoidIfInStoredProcRule : BaseSqlCodeAnalysisRule
	{
		public const string RuleId = Constants.RuleNameSpace + "SRD0063";
		public const string RuleDisplayName = "Do not use IF statements containing queries in stored procedures.";
		private const string Message = RuleDisplayName;

		public AvoidIfInStoredProcRule() : base(ModelSchema.Procedure)
		{
		}

		public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
		{
			var problems = new List<SqlRuleProblem>();
			var sqlObj = ruleExecutionContext.ModelElement;

			if (sqlObj == null) { return problems; }
			var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));

			IfStatementVisitor ifVisitor = new IfStatementVisitor();

			fragment.Accept(ifVisitor);

			if (!ifVisitor.Statements.Any()) { return problems; }

			foreach (var ifStatement in ifVisitor.Statements)
			{
				var tableVisitor = new NamedTableReferenceVisitor() { TypeFilter = ObjectTypeFilter.PermanentOnly };
				ifStatement.ThenStatement?.Accept(tableVisitor);
				ifStatement.ElseStatement?.Accept(tableVisitor);
				problems.AddRange(tableVisitor.Statements.Select(s => new SqlRuleProblem(Message, sqlObj, s)));
			}

			return problems;
		}
	}
}