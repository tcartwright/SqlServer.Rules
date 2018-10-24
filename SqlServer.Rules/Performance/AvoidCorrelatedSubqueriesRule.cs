using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Performance
{
	[ExportCodeAnalysisRule(RuleId,
		RuleDisplayName,
		Description = RuleDisplayName,
		Category = Constants.Performance,
		RuleScope = SqlRuleScope.Element)]
	public sealed class AvoidCorrelatedSubqueriesRule : BaseSqlCodeAnalysisRule
	{
		public const string RuleId = Constants.RuleNameSpace + "SRP0024";
		public const string RuleDisplayName = "Avoid the use of correlated subqueries except for very small tables.";
		public const string Message = RuleDisplayName;

		public AvoidCorrelatedSubqueriesRule() : base(ProgrammingAndViewSchemas)
		{
		}

		public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
		{
			var problems = new List<SqlRuleProblem>();
			var sqlObj = ruleExecutionContext.ModelElement;
			if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

			var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);

			var scalarSubqueryVisitor = new ScalarSubqueryVisitor();
			fragment.Accept(scalarSubqueryVisitor);

			var offenders = scalarSubqueryVisitor.NotIgnoredStatements(RuleId).Where(s =>
			{
				var whereClause = (s.QueryExpression as QuerySpecification)?.WhereClause;
				if (whereClause == null) { return false; }

				var booleanCompares = new BooleanComparisonVisitor();
				whereClause.Accept(booleanCompares);

				foreach(var booleanCompare in booleanCompares.Statements)
				{
					var colVisitor = new ColumnReferenceExpressionVisitor();
					booleanCompare.AcceptChildren(colVisitor);
					if(colVisitor.Count > 1) { return true; }
				}

				return false;
			}).ToList();

			problems.AddRange(offenders.Select(o => new SqlRuleProblem(Message, sqlObj, o)));

			return problems;
		}
	}
}
