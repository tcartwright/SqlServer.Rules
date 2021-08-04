using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

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
    public sealed class UnusedParameterRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The base rule identifier
        /// </summary>
        public const string BaseRuleId = "SRD0016";
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + BaseRuleId;
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Input parameter never used. Consider removing the parameter or using it.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Input parameter '{0}' is never used. Consider removing the parameter or using it.";

        /// <summary>
        /// Initializes a new instance of the <see cref="UnusedParameterRule"/> class.
        /// </summary>
        public UnusedParameterRule() : base(ProgrammingSchemas)
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

            var fragment = sqlObj.GetFragment();
            if (fragment.ScriptTokenStream == null) { return problems; }

            var visitor = new VariablesVisitor();
            fragment.Accept(visitor);


            var parms = from pp in visitor.ProcedureParameters
                        join t in fragment.ScriptTokenStream
                            on new { Name = pp.VariableName.Value?.ToLower(), Type = TSqlTokenType.Variable }
                            equals new { Name = t.Text?.ToLower(), Type = t.TokenType }
                        where Ignorables.ShouldNotIgnoreRule(fragment.ScriptTokenStream, RuleId, pp.StartLine)
                        select pp;

            var unusedParms = parms.GroupBy(p => p.VariableName.Value?.ToLower())
                .Where(g => g.Count() == 1).Select(g => g.First());

            problems.AddRange(unusedParms.Select(rp => new SqlRuleProblem(string.Format(Message, rp.VariableName.Value), sqlObj, rp)));

            return problems;
        }
    }
}