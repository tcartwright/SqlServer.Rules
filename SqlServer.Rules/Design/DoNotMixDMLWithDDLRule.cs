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
    public sealed class DoNotMixDMLWithDDLRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0057";
        public const string RuleDisplayName = "Do not mix DML with DDL statements. Group DDL statements at the beginning of procedures followed by DML statements.";
        private const string Message = RuleDisplayName;

        public DoNotMixDMLWithDDLRule() : base(ModelSchema.Procedure)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));

            var typesVisitor = new TypesVisitor(
                typeof(SelectStatement),
                typeof(UpdateStatement),
                typeof(DeleteStatement),
                typeof(InsertStatement),
                typeof(CreateTableStatement)
            );
            fragment.Accept(typesVisitor);

            if (typesVisitor.Count == 0) { return problems; }

            var offenders = typesVisitor.Statements
                .SkipWhile(t => t.GetType() == typeof(CreateTableStatement))
                .Where(t1 => t1.GetType() == typeof(CreateTableStatement));

            problems.AddRange(offenders.Select(t => new SqlRuleProblem(Message, sqlObj, t)));

            return problems;
        }
    }
}