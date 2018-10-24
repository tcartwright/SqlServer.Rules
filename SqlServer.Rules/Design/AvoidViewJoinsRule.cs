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
    public sealed class AvoidViewJoinsRule : BaseSqlCodeAnalysisRule

    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0019";
        public const string RuleDisplayName = "Avoid joining tables with views.";
        private const string Message = RuleDisplayName;

        public AvoidViewJoinsRule() : base(ProgrammingSchemas)
        {
        }

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