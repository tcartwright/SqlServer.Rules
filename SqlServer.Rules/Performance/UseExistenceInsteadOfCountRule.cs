using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
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
    public sealed class UseExistenceInsteadOfCountRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0023";
        public const string RuleDisplayName = "When checking for existence use EXISTS instead of COUNT";
        public const string Message = RuleDisplayName;


        public UseExistenceInsteadOfCountRule() : base(ProgrammingSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);

            var ifVisitor = new IfStatementVisitor();
            fragment.Accept(ifVisitor);

            if (ifVisitor.Count == 0) { return problems; }

            foreach (var ifstmt in ifVisitor.Statements)
            {
                var functionVisitor = new FunctionCallVisitor("count");
                ifstmt.Predicate.Accept(functionVisitor);

                if (functionVisitor.Statements.Any() && CheckIf(ifstmt))
                {
                    problems.Add(new SqlRuleProblem(Message, sqlObj, ifstmt));
                }
            }

            return problems;
        }

        private bool CheckIf(IfStatement ifstmt)
        {
            if (ifstmt.Predicate is BooleanComparisonExpression booleanCompare)
            {
                return (booleanCompare.FirstExpression is IntegerLiteral || booleanCompare.SecondExpression is IntegerLiteral);
            }
            return false;
        }
    }
}
