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
    public sealed class QueryHighJoinCountRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0018";
        public const string RuleDisplayName = "Query uses a high number of joins. ";
        public const string Message = "Query uses {0} joins. This is a high number of joins and can lead to performance issues.";


        public QueryHighJoinCountRule() :  base(ProgrammingAndViewSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);
            var selectStatementVisitor = new SelectStatementVisitor();
            fragment.Accept(selectStatementVisitor);

            foreach (var stmt in selectStatementVisitor.Statements)
            {
                var hasForceOrder = stmt.OptimizerHints?.Any(oh => oh.HintKind == OptimizerHintKind.ForceOrder);
                if (hasForceOrder.GetValueOrDefault(false)) { continue; }

                var querySpecificationVisitor = new QuerySpecificationVisitor();
                stmt.QueryExpression.Accept(querySpecificationVisitor);

                foreach (var query in querySpecificationVisitor.Statements)
                {
                    var fromClause = query.FromClause;
                    if (fromClause == null) { continue; }

                    var namedTableVisitor = new NamedTableReferenceVisitor();
                    fromClause.Accept(namedTableVisitor);

                    var tableCount = namedTableVisitor.Count - 1;

                    if (tableCount > 8)
                    {
                        var msg = string.Format(Message, tableCount);
                        problems.Add(new SqlRuleProblem(msg, sqlObj, fromClause));
                    }
                }
            }


            return problems;
        }
    }
}
