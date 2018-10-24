using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Performance
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidNotInRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0011";
        public const string RuleDisplayName = "Avoid using the NOT IN predicate in a WHERE clause. (Sargeable)";
        public const string Message = RuleDisplayName;

        public AvoidNotInRule() : base(ProgrammingAndViewSchemas)
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

                var offenders = inPredicateVisitor.NotIgnoredStatements(RuleId).Where(i => i.NotDefined);
                problems.AddRange(offenders.Select(t => new SqlRuleProblem(Message, sqlObj, t)));

            }


            return problems;
        }
    }
}