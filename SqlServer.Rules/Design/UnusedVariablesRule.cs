using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;
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
    public sealed class UnusedVariablesRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0012";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Variable declared but never referenced or assigned.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Variable '{0}' is declared but never referenced or assigned.";

        /// <summary>
        /// Initializes a new instance of the <see cref="UnusedVariablesRule"/> class.
        /// </summary>
        public UnusedVariablesRule() : base(ProgrammingSchemas)
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


            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);
            if (fragment.ScriptTokenStream == null) { return problems; }
            var visitor = new DeclareVariableElementVisitor();
            fragment.Accept(visitor);

            var vars = from pp in visitor.Statements
                       join t in fragment.ScriptTokenStream
                           on new { Name = pp.VariableName.Value?.ToLower(), Type = TSqlTokenType.Variable }
                           equals new { Name = t.Text?.ToLower(), Type = t.TokenType }
                       select pp;

            var unusedVars = vars.GroupBy(p => p.VariableName.Value?.ToLower()).Where(g => g.Count() == 1).Select(g => g.First());

            problems.AddRange(unusedVars.Select(v => new SqlRuleProblem(string.Format(Message, v.VariableName.Value), sqlObj, v)));

            return problems;
        }
    }
}