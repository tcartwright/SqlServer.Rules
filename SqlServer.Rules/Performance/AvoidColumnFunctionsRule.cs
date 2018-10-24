using SqlServer.Rules.Globals;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using System;
using SqlServer.Dac;

namespace SqlServer.Rules.Performance
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidColumnFunctionsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0009";
        public const string RuleDisplayName = "Avoid wrapping columns within a function in the WHERE clause. (Sargeable)";
        public const string Message = RuleDisplayName;


        public AvoidColumnFunctionsRule() : base(ProgrammingAndViewSchemas)
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
                var functionVisitor = new FunctionCallVisitor();
                whereClause.Accept(functionVisitor);

                var offenders = from FunctionCall f in functionVisitor.NotIgnoredStatements(RuleId)
                                where CheckFunction(f)
                                select f;

                problems.AddRange(offenders.Select(f => new SqlRuleProblem(Message, sqlObj, f)));
            }

            return problems;
        }

        private bool CheckFunction(FunctionCall func)
        {
            if (func == null) { return false; }

            return func.Parameters.OfType<ColumnReferenceExpression>().Any(col =>
            {
                var colId = col.MultiPartIdentifier?.GetObjectIdentifier();
                if (colId == null || colId.Parts.Count() == 0) { return false; }
                return !Constants.DateParts.Contains(colId.Parts.Last().ToLower());
            }) && !Constants.Aggregates.Contains(func.FunctionName.Value.ToUpper());
        }
    }
}
