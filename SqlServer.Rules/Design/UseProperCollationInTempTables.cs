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
	public class UseProperCollationInTempTables : BaseSqlCodeAnalysisRule
	{
		public const string RuleId = Constants.RuleNameSpace + "SRD0062";
		public const string RuleDisplayName = "Create SQL Server temporary tables with the correct collation or use database default as the tempdb having a different collation than the database can cause issues and or data instability.";
		private const string Message = RuleDisplayName;

		public UseProperCollationInTempTables() : base(ModelSchema.Procedure)
		{
		}

		public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
		{
			var problems = new List<SqlRuleProblem>();
			var sqlObj = ruleExecutionContext.ModelElement;

			if (sqlObj == null)
				return problems;

			var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));

			var createTableVisitor = new CreateTableVisitor();
			fragment.Accept(createTableVisitor);

			var statements = createTableVisitor
				.Statements
				.Where(p => p.SchemaObjectName
							.Identifiers
							.Any(a => a.Value.StartsWith("#", System.StringComparison.InvariantCultureIgnoreCase)));

			foreach (var statement in statements)
			{
				var noCollationColumns = statement.Definition.ColumnDefinitions.Where(p => p.Collation == null &&
							(((SqlDataTypeReference)p.DataType).SqlDataTypeOption == SqlDataTypeOption.VarChar
								|| ((SqlDataTypeReference)p.DataType).SqlDataTypeOption == SqlDataTypeOption.Char
								|| ((SqlDataTypeReference)p.DataType).SqlDataTypeOption == SqlDataTypeOption.NVarChar
								|| ((SqlDataTypeReference)p.DataType).SqlDataTypeOption == SqlDataTypeOption.NChar));
				problems.AddRange(noCollationColumns.Select(s => new SqlRuleProblem(Message, sqlObj, s)));
			}

			return problems;
		}
	}
}