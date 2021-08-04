﻿using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// 
    /// </summary>
    /// <FriendlyName></FriendlyName>
	/// <IsIgnorable>false</IsIgnorable>
	/// <ExampleMd></ExampleMd>
	/// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class WrapStatementsWithTransactionRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0009";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Wrap multiple action statements within a transaction.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="WrapStatementsWithTransactionRule"/> class.
        /// </summary>
        public WrapStatementsWithTransactionRule() : base(ModelSchema.Procedure)
        {
        }

        /// <summary>
        /// Performs analysis and returns a list of problems detected
        /// </summary>
        /// <param name="ruleExecutionContext">Contains the schema model and model element to analyze</param>
        /// <returns>
        /// The problems detected by the rule in the given element
        /// </returns>
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