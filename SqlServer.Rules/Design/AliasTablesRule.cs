using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
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
	public sealed class AliasTablesRule : BaseSqlCodeAnalysisRule
	{
		public const string RuleId = Constants.RuleNameSpace + "SRD0038";
		public const string RuleDisplayName = "Consider aliasing all table sources in the query.";
		private const string Message = "Consider aliasing all table sources in the query.";

		public AliasTablesRule() : base(ProgrammingAndViewSchemas)
		{
		}

		public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
		{
			var problems = new List<SqlRuleProblem>();
			var sqlObj = ruleExecutionContext.ModelElement;

			if (sqlObj == null || sqlObj.IsWhiteListed())
				return problems;

			var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);

			var selectStatementVisitor = new SelectStatementVisitor();
			fragment.Accept(selectStatementVisitor);

			if (selectStatementVisitor.Count == 0) { return problems; }

			foreach (var select in selectStatementVisitor.Statements)
			{
				var fromClause = (select.QueryExpression as QuerySpecification)?.FromClause;
				//ignore selects that do not use a from clause with tables
				if (fromClause == null) { continue; }

				var visitor = new NamedTableReferenceVisitor() { TypeFilter = ObjectTypeFilter.PermanentOnly };
				fromClause.Accept(visitor);
				//only scan for aliases if there are more than 1 table in the from clause
				if (visitor.Count <= 1) { continue; }

				var offenders =
					from t in visitor.NotIgnoredStatements(RuleId)
					where t.Alias == null
					select t;

				problems.AddRange(offenders.Select(t => new SqlRuleProblem(Message, sqlObj, t)));
			}
			return problems;
		}
	}
}