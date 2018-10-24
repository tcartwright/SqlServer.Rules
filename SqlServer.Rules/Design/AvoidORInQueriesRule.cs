using SqlServer.Rules.Globals;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using System;
using SqlServer.Dac;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidORInQueriesRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0032";
        public const string RuleDisplayName = "Try to avoid the OR operator in query where clauses if possible.  (Sargeable)";
        private const string Message = RuleDisplayName;

        public AvoidORInQueriesRule() : base(ProgrammingAndViewSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);

            // only visit the operators in the where clause
            var visitor = new WhereClauseVisitor();
            fragment.Accept(visitor);

            foreach (var clause in visitor.Statements)
            {
                var beVisitor = new BooleanParenthesesExpressionVisitor();
                clause.Accept(beVisitor);

                var offenders = (from BooleanParenthesisExpression be in beVisitor.NotIgnoredStatements(RuleId)
                                 where TestClause(be)
                                 select be).ToList();

                problems.AddRange(offenders.Select(o => new SqlRuleProblem(Message, sqlObj, o)));
            }

            return problems;
        }

        private bool TestClause(BooleanParenthesisExpression be)
        {
            if (be.Expression is BooleanBinaryExpression booleanExpression)
            {
                var expr1 = booleanExpression.FirstExpression;
                var expr2 = booleanExpression.SecondExpression;

                return booleanExpression.BinaryExpressionType == BooleanBinaryExpressionType.Or
                    && (expr1 is BooleanIsNullExpression || expr1 is BooleanComparisonExpression)
                    && (expr2 is BooleanIsNullExpression || expr2 is BooleanComparisonExpression)
                    && _comparer.Equals(GetVariableName(expr1), GetVariableName(expr2));
            }
            return false;
        }

        private string GetVariableName(BooleanExpression bex)
        {
            var ret = Guid.NewGuid().ToString();
            if (bex is BooleanIsNullExpression nullExpr && nullExpr.Expression is VariableReference varExpr)
            {
                ret = varExpr.Name;
            }
            if (bex is BooleanComparisonExpression compareExpr)
            {
                if (compareExpr.FirstExpression is VariableReference expr1)
                {
                    ret = expr1.Name;
                }
                else if (compareExpr.SecondExpression is VariableReference expr2)
                {
                    ret = expr2.Name;
                }
            }
            return ret;
        }
    }
}