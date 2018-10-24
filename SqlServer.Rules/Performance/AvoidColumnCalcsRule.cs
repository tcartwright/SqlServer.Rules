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
    public sealed class AvoidColumnCalcsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0015";
        public const string RuleDisplayName = "Avoid the use of calculations on columns in the where clause. (Sargeable)";
        public const string Message = RuleDisplayName;


        public AvoidColumnCalcsRule() : base(ProgrammingAndViewSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);

            var whereClauseVisitor = new WhereClauseVisitor();
            fragment.Accept(whereClauseVisitor);

            foreach (var whereClause in whereClauseVisitor.Statements)
            {
                var binaryExpressionVisitor = new BinaryExpressionVisitor();
                whereClause.Accept(binaryExpressionVisitor);

                foreach (var comparison in binaryExpressionVisitor.NotIgnoredStatements(RuleId))
                {
                    if (CheckBinaryExpression(comparison).GetValueOrDefault(false))
                    {
                        problems.Add(new SqlRuleProblem(Message, sqlObj, comparison));
                    }
                }
            }

            return problems;
        }

        private bool? CheckBinaryExpression(BinaryExpression bin)
        {
            bool? ret = null;

            if (
                (bin.FirstExpression.GetType() == typeof(ColumnReferenceExpression)
                    || bin.SecondExpression.GetType() == typeof(ColumnReferenceExpression))
                && (bin.FirstExpression.GetType() == typeof(IntegerLiteral)
                    || bin.SecondExpression.GetType() == typeof(IntegerLiteral)))
            {
                return true;
            }

            if (!ret.HasValue && bin.FirstExpression is BinaryExpression)
            {
                ret = CheckBinaryExpression(bin.FirstExpression as BinaryExpression);
            }
            if (!ret.HasValue && bin.SecondExpression is BinaryExpression)
            {
                ret = CheckBinaryExpression(bin.SecondExpression as BinaryExpression);
            }

            return ret;
        }
    }
}
