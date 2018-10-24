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
    public sealed class WrapStatementsWithTransactionRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0009";
        public const string RuleDisplayName = "Wrap multiple action statements within a transaction.";
        private const string Message = RuleDisplayName;

        public WrapStatementsWithTransactionRule() : base(ModelSchema.Procedure)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            List<SqlRuleProblem> problems = new List<SqlRuleProblem>();
            TSqlObject sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));
            var name = sqlObj.Name.GetName();

            var transactionVisitor = new TransactionVisitor();
            var actionStatementVisitor = new ActionStatementVisitor() { TypeFilter = ObjectTypeFilter.PermanentOnly };
            fragment.Accept(actionStatementVisitor);
            if (actionStatementVisitor.Count <= 1) { return problems; }
            fragment.Accept(transactionVisitor);
            if (transactionVisitor.Count == 0)
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
                return problems;
            }

            //eliminate rollbacks, and ensure all the action statements are wrapped inside the begin tran...commit tran
            var transactionStatements = transactionVisitor.Statements
                .Where(st => st.GetType() == typeof(BeginTransactionStatement) 
                    || st.GetType() == typeof(CommitTransactionStatement));
            var possibleOffenders = new List<DataModificationStatement>(actionStatementVisitor.Statements);

            for (int i = 0; i < transactionStatements.Count(); i += 2)
            {
                var beginTranLine = transactionStatements.ElementAt(i).StartLine;
                var commitTranLine = transactionStatements.ElementAt(i + 1).StartLine;

                possibleOffenders.RemoveAll(st => st.StartLine > beginTranLine && st.StartLine < commitTranLine);
            }

            problems.AddRange(possibleOffenders.Select(po => new SqlRuleProblem(Message, sqlObj, po)));

            return problems;
        }
    }
}