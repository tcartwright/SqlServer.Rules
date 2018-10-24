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
    public sealed class AvoidNotEqualToRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0006";
        public const string RuleDisplayName = "Try to avoid using not equal operator (<>,!=) in the WHERE clause if possible. (Sargeable)";
        public const string Message = RuleDisplayName;

        public AvoidNotEqualToRule() : base(ProgrammingAndViewSchemas)
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
                var booleanComparisonVisitor = new BooleanComparisonVisitor();
                whereClause.Accept(booleanComparisonVisitor);

                var offenders = booleanComparisonVisitor.NotIgnoredStatements(RuleId)
                    .Where(c => 
                        c.ComparisonType == BooleanComparisonType.NotEqualToBrackets 
                        || c.ComparisonType == BooleanComparisonType.NotEqualToExclamation);

                var sqlObjName = ruleExecutionContext.GetObjectName(sqlObj);
                problems.AddRange(offenders.Select(t => new SqlRuleProblem(Message, sqlObj, t)));
            }

            return problems;
        }
    }
}