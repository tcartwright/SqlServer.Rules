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
	public class DoNotUseIdentityFunction : BaseSqlCodeAnalysisRule
	{
		public const string RuleId = Constants.RuleNameSpace + "SRD0056";
		public const string RuleDisplayName = "Use OUTPUT or SCOPE_IDENTITY() instead of @@IDENTITY.";
		private const string Message = RuleDisplayName;

		public DoNotUseIdentityFunction() : base(ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction, ModelSchema.DmlTrigger)
		{
		}

		public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
		{
			var problems = new List<SqlRuleProblem>();
			var sqlObj = ruleExecutionContext.ModelElement;

			if (sqlObj == null)
				return problems;

			var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement), typeof(CreateFunctionStatement), typeof(CreateTriggerStatement));

			var visitor = new GlobalVariableExpressionVisitor("@@identity");

			fragment.Accept(visitor);

			problems.AddRange(visitor.NotIgnoredStatements(RuleId).Select(s => new SqlRuleProblem(Message, sqlObj, s)));

			return problems;
		}
	}
}