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
    public sealed class CursorNotClosedRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0007";
        public const string RuleDisplayName = "Local cursor not closed.";
        public const string Message = RuleDisplayName;


        public CursorNotClosedRule() : base(ProgrammingSchemas)
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
                var closeCursorVisitor = new CloseCursorVisitor();
                fragment.Accept(closeCursorVisitor);

                var localOpenCursors = openCursorVisitor.Statements.Where(c => !c.Cursor.IsGlobal);
                var localCloseCursors = closeCursorVisitor.Statements.Where(c => !c.Cursor.IsGlobal);

                var unclosedCursors = localOpenCursors.Where(c => 
                    !localCloseCursors.Any(c2 => _comparer.Equals(c.Cursor.Name.Value, c2.Cursor.Name.Value)));

                foreach (var cursor in unclosedCursors)
                {
                    problems.Add(new SqlRuleProblem(Message, sqlObj, cursor));
                }
            }

            return problems;
        }
    }
}
