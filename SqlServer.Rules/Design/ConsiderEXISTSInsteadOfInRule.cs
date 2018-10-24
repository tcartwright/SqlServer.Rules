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
    public sealed class ConsiderEXISTSInsteadOfInRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0021";
        public const string RuleDisplayName = "Consider using EXISTS instead of IN when used with a subquery.";
        private const string Message = RuleDisplayName;

        public ConsiderEXISTSInsteadOfInRule() : base(ProgrammingAndViewSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var sqlObjName = ruleExecutionContext.GetObjectName(sqlObj);
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);

            var whereClauseVisitor = new WhereClauseVisitor();
            fragment.Accept(whereClauseVisitor);

            foreach (var whereClause in whereClauseVisitor.Statements)
            {
                var inPredicateVisitor = new InPredicateVisitor();
                whereClause.Accept(inPredicateVisitor);

                var offenders = inPredicateVisitor.NotIgnoredStatements(RuleId).Where(i => i.Subquery != null);
                problems.AddRange(offenders.Select(t => new SqlRuleProblem(Message, sqlObj, t)));
            }

            return problems;
        }
    }
}