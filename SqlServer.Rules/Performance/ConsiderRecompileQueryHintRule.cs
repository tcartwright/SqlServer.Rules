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
    public sealed class ConsiderRecompileQueryHintRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0022";
        public const string RuleDisplayName = "Consider using RECOMPILE query hint instead of the WITH RECOMPILE option.";
        public const string Message = RuleDisplayName;

        public ConsiderRecompileQueryHintRule() : base(ModelSchema.Procedure)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var proc = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement)) as CreateProcedureStatement;

            if (proc.Options.Any(o => 
                o.OptionKind == ProcedureOptionKind.Recompile) 
                && Ignorables.ShouldNotIgnoreRule(proc.ScriptTokenStream, RuleId, proc.StartLine))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }

            return problems;
        }
    }
}
