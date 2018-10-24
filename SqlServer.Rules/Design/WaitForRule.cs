using SqlServer.Rules.Globals;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using System.Collections.Generic;
using System.Linq;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class WaitForRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0035";
        public const string RuleDisplayName = "Do not use WAITFOR DELAY/TIME statement in stored procedures, functions, and triggers.";
        private const string Message = RuleDisplayName;

        public WaitForRule() : base(ModelSchema.Procedure,
                ModelSchema.ScalarFunction,
                ModelSchema.TableValuedFunction,
                ModelSchema.DmlTrigger
        )
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(
                    typeof(CreateProcedureStatement),
                    typeof(CreateFunctionStatement),
                    typeof(CreateTriggerStatement)
                );
            var visitor = new WaitForVisitor();

            fragment.Accept(visitor);

            problems.AddRange(visitor.Statements.Select(o => new SqlRuleProblem(Message, sqlObj, o)));

            return problems;
        }
    }
}