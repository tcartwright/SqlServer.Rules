using SqlServer.Rules.Globals;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using System;
using SqlServer.Dac;

namespace SqlServer.Rules.Performance
{
	[ExportCodeAnalysisRule(RuleId,
		RuleDisplayName,
		Description = RuleDisplayName,
		Category = Constants.Performance,
		RuleScope = SqlRuleScope.Element)]
	public sealed class AvoidUpdatingPrimaryKeyColumnsRule : BaseSqlCodeAnalysisRule
	{
		public const string RuleId = Constants.RuleNameSpace + "SRP0017";
		public const string RuleDisplayName = "Avoid updating columns that are part of the primary key.  (Halloween Protection)";
		public const string Message = RuleDisplayName;

		public AvoidUpdatingPrimaryKeyColumnsRule() : base(ProgrammingSchemas)
		{
		}

		public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
		{
			var problems = new List<SqlRuleProblem>();
			var sqlObj = ruleExecutionContext.ModelElement;
			var model = ruleExecutionContext.SchemaModel;
			if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

			var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);

			var updateVisitor = new UpdateVisitor();
			fragment.Accept(updateVisitor);
			foreach (var update in updateVisitor.NotIgnoredStatements(RuleId))
			{
				var target = update.UpdateSpecification.Target as NamedTableReference;
				if (target == null || target.GetName().Contains("#")) { continue; }

				//we have an aliased table we need to find out what the real table is so we can look up its columns
				if (update.UpdateSpecification.FromClause != null)
				{
					var namedTableVisitor = new NamedTableReferenceVisitor() { TypeFilter = ObjectTypeFilter.PermanentOnly };
					update.UpdateSpecification.FromClause.Accept(namedTableVisitor);

					target = namedTableVisitor.Statements
						.FirstOrDefault(t => _comparer.Equals(t.Alias?.Value, target.SchemaObject.Identifiers.LastOrDefault()?.Value));
					if (target == null) { continue; }
				}

				var targetSqlObj = model.GetObject(Table.TypeClass, target.GetObjectIdentifier(), DacQueryScopes.All);
				if (targetSqlObj == null) { continue; }

				var pk = targetSqlObj.GetReferencing(PrimaryKeyConstraint.Host, DacQueryScopes.UserDefined).FirstOrDefault();
				if (pk == null) { continue; }
				var primaryKeyColumns = pk.GetReferenced(PrimaryKeyConstraint.Columns, DacQueryScopes.All);

				var hasOffense = update.UpdateSpecification.SetClauses.OfType<AssignmentSetClause>().Any(setClause =>
				{
					if (setClause.Column?.MultiPartIdentifier == null) { return false; }
					return primaryKeyColumns.Any(pkc => pkc.Name.CompareTo(setClause.Column?.MultiPartIdentifier) >= 5);
				});

				if (hasOffense)
				{
					problems.Add(new SqlRuleProblem(Message, sqlObj, update));
				}
			}
			return problems;
		}
	}
}