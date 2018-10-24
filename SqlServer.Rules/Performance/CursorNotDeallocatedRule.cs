using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Performance
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class CursorNotDeallocatedRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0008";
        public const string RuleDisplayName = "Local cursor not explicitly deallocated.";
        public const string Message = RuleDisplayName;


        public CursorNotDeallocatedRule() : base(ProgrammingSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);

            var openCursorVisitor = new OpenCursorVisitor();
            fragment.Accept(openCursorVisitor);

            if (openCursorVisitor.Count > 0)
            {
                var deallocateCursorVisitor = new DeallocateCursorVisitor();
                fragment.Accept(deallocateCursorVisitor);

                var localOpenCursors = openCursorVisitor.Statements.Where(c => !c.Cursor.IsGlobal);
                var localDeallocateCursors = deallocateCursorVisitor.Statements.Where(c => !c.Cursor.IsGlobal);

                var unDeallocatedCursors = localOpenCursors.Where(c => 
                    !localDeallocateCursors.Any(c2 => _comparer.Equals(c.Cursor.Name.Value, c2.Cursor.Name.Value)));

                foreach (var cursor in unDeallocatedCursors)
                {
                    problems.Add(new SqlRuleProblem(Message, sqlObj, cursor));
                }
            }

            return problems;
        }
    }
}
