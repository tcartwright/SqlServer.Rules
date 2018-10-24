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
    public sealed class WrapStatementsWithTryCatchRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0013";
        public const string RuleDisplayName = "Wrap multiple action statements within a try catch.";
        private const string Message = RuleDisplayName;

        public WrapStatementsWithTryCatchRule() : base(ModelSchema.Procedure)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            List<SqlRuleProblem> problems = new List<SqlRuleProblem>();
            TSqlObject sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));
            var name = sqlObj.Name.GetName();

            var tryCatchVisitor = new TryCatchVisitor();
            var actionStatementVisitor = new ActionStatementVisitor(); // not going to ignore temps for this rule as they should be wrapped in a try
            fragment.Accept(actionStatementVisitor);
            if (actionStatementVisitor.Count <= 1) { return problems; }
            fragment.Accept(tryCatchVisitor);
            if (tryCatchVisitor.Count == 0)
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
                return problems;
            }

            var possibleOffenders = new List<DataModificationStatement>(actionStatementVisitor.Statements);

            foreach (var statement in tryCatchVisitor.Statements)
            {
                var startLine = statement.StartLine;
                var endline = statement.CatchStatements.StartLine;
                possibleOffenders.RemoveAll(st => st.StartLine > startLine && st.StartLine < endline);
            }

            problems.AddRange(possibleOffenders.Select(po => new SqlRuleProblem(Message, sqlObj, po)));

            return problems;
        }
    }
}