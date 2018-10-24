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
	public class CacheGetDateToVariable : BaseSqlCodeAnalysisRule
	{
		public const string RuleId = Constants.RuleNameSpace + "SRD0064";
		public const string RuleDisplayName = "Cache multiple calls to GETDATE or SYSDATETIME into a variable.";
		private const string Message = RuleDisplayName;

		private readonly List<string> FunctionNames = new List<string> { "getdate", "sysdatetime" };

		public CacheGetDateToVariable() : base(ProgrammingSchemas)
		{
		}

		public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
		{
			var problems = new List<SqlRuleProblem>();
			var candidates = new List<StatementWithCtesAndXmlNamespaces>();
			var sqlObj = ruleExecutionContext.ModelElement;

			if (sqlObj == null)
				return problems;

			var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);

			List<StatementWithCtesAndXmlNamespaces> statements = new List<StatementWithCtesAndXmlNamespaces>();

			var selectVisitor = new SelectStatementVisitor();
			fragment.Accept(selectVisitor);
			statements.AddRange(selectVisitor.NotIgnoredStatements(RuleId));

			var actionStatementVisitor = new ActionStatementVisitor();
			fragment.Accept(actionStatementVisitor);
			statements.AddRange(actionStatementVisitor.NotIgnoredStatements(RuleId));

			if (statements.Count() > 1)
			{
				statements.ForEach(statement =>
				{
					if (DoesStatementHaveDateFunction(statement)) { candidates.Add(statement); }
				});
			}

			problems.AddRange(candidates.Select(s => new SqlRuleProblem(Message, sqlObj, s)));

			return problems;
		}

		private bool DoesStatementHaveDateFunction(StatementWithCtesAndXmlNamespaces statement)
		{
			bool hasDateFunction = false;

			var allFunctions = new FunctionCallVisitor();

			statement.Accept(allFunctions);

			if (allFunctions.Statements.Any(p => FunctionNames.Contains(p.FunctionName.Value.ToLower())))
			{
				hasDateFunction = true;
			}
			else
			{
				hasDateFunction = CheckFunctionCallsForDateFunction(allFunctions.Statements);
			}

			return hasDateFunction;
		}

		private bool CheckFunctionCallsForDateFunction(IList<FunctionCall> functionCalls)
		{
			bool hasDateFunctions = false;

			foreach (var functionCall in functionCalls)
			{
				if (FunctionNames.Contains(functionCall.FunctionName.Value.ToLower()))
				{
					hasDateFunctions = true;
				}
				else
				{
					foreach (var param in functionCall.Parameters)
					{
						var functionVisitor = new FunctionCallVisitor();
						param.Accept(functionVisitor);
						hasDateFunctions = hasDateFunctions || CheckFunctionCallsForDateFunction(functionVisitor.Statements);
						if (hasDateFunctions) { break; }
					}
				}
				if (hasDateFunctions) { break; }
			}

			return hasDateFunctions;
		}
	}
}