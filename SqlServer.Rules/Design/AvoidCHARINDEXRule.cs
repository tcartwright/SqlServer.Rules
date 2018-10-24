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
    public sealed class AvoidCHARINDEXRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0031";
        public const string RuleDisplayName = "Avoid using CHARINDEX function in WHERE clauses.";
        private const string Message = RuleDisplayName;

        public AvoidCHARINDEXRule() : base(ProgrammingSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);

            // only visit the function calls inside the where clauses
            var visitor = new WhereClauseVisitor();
            fragment.Accept(visitor);

            foreach (var clause in visitor.Statements)
            {
                var functionVisitor = new FunctionCallVisitor("charindex");
                clause.Accept(functionVisitor);

                problems.AddRange(functionVisitor.NotIgnoredStatements(RuleId).Select(f => new SqlRuleProblem(Message, sqlObj, f)));
            }

            return problems;
        }
    }
}