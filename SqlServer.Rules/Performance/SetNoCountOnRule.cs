using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;
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
    public sealed class SetNoCountOnRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0005";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "SET NOCOUNT ON is recommended to be enabled in stored procedures and triggers.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetNoCountOnRule"/> class.
        /// </summary>
        public SetNoCountOnRule() : base(ModelSchema.Procedure, ModelSchema.DmlTrigger)
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
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(
                typeof(CreateProcedureStatement),
                typeof(CreateTriggerStatement));
            var visitor = new PredicateVisitor();
            fragment.Accept(visitor);

            var predicates = from o
                            in visitor.Statements
                             where o.Options == SetOptions.NoCount && o.IsOn
                             select o;

            var createToken = fragment.ScriptTokenStream.FirstOrDefault(t => t.TokenType == TSqlTokenType.Create);

            if (predicates.Count() <= 0 && Ignorables.ShouldNotIgnoreRule(fragment.ScriptTokenStream, RuleId, createToken.Line))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }
            return problems;
        }
    }
}
