using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SqlServer.Rules.Performance
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class DoNotUseDistinctInAggregatesRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0003";
        public const string RuleDisplayName = "Avoid using DISTINCT keyword inside of aggregate functions.";
        public const string Message = RuleDisplayName;


        public DoNotUseDistinctInAggregatesRule() : base(ProgrammingAndViewSchemas)
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

            foreach (var statement in selectStatementVisitor.Statements)
            {
                bool found = false;

                if (statement.QueryExpression is QuerySpecification selects)
                {
                    foreach (var selectElement in selects.SelectElements)
                    {
                        var functionCallVisitor = new FunctionCallVisitor();
                        selectElement.Accept(functionCallVisitor);

                        foreach (var function in functionCallVisitor.NotIgnoredStatements(RuleId))
                        {
                            if (function.UniqueRowFilter == UniqueRowFilter.Distinct
                                && Constants.Aggregates.Contains(function.FunctionName.Value.ToUpper()))
                            {
                                problems.Add(new SqlRuleProblem(Message, sqlObj, statement));
                            }
                        }
                        if (found) { break; }
                    }
                }
            }


            return problems;
        }
    }
}
