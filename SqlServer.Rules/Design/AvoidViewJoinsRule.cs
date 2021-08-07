using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    /// <summary>Avoid joining with views.</summary>
    /// <FriendlyName>Join with a derived set</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The rule checks for joining with views as this may have performance implication when used
    /// without having good knowledge of the underlying tables and may lead to unnecessary joins. 
    /// Although views are useful for many reasons, they hide the underlying sources and may mislead
    /// unacquainted developers and produce redundant joins.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidViewJoinsRule : BaseSqlCodeAnalysisRule

    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0019";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid joining tables with views.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidViewJoinsRule"/> class.
        /// </summary>
        public AvoidViewJoinsRule() : base(ProgrammingSchemas)
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

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);
            var visitor = new JoinVisitor();

            fragment.Accept(visitor);
            var views = sqlObj.GetReferenced(DacQueryScopes.UserDefined)
                .Where(x => x.ObjectType == ModelSchema.View).Select(v => v.Name.Parts.Last()).ToList();

            var joins = visitor.QualifiedJoins.Where(j => Ignorables.ShouldNotIgnoreRule(j.ScriptTokenStream, RuleId, j.StartLine));

            var leftSideOffenders =
                from o in joins
                where o.FirstTableReference != null
                    && views.Contains((o.FirstTableReference as NamedTableReference)?.SchemaObject.Identifiers.Last().Value)
                select o.FirstTableReference as NamedTableReference;

            var rightSideOffenders =
                from o in joins
                where o.SecondTableReference != null
                    && views.Contains((o.SecondTableReference as NamedTableReference)?.SchemaObject.Identifiers.Last().Value)
                select o.SecondTableReference as NamedTableReference;

            problems.AddRange(leftSideOffenders.Union(rightSideOffenders).Select(o => new SqlRuleProblem(Message, sqlObj, o)));

            return problems;
        }
    }
}