using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
    RuleDisplayName,
    Description = RuleDisplayName,
    Category = Constants.Design,
    RuleScope = SqlRuleScope.Element)]
    public sealed class DeleteWithoutWhereRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0017";
        public const string RuleDisplayName = "DELETE statement without row limiting conditions.";
        private const string Message = RuleDisplayName;

        public DeleteWithoutWhereRule() : base(ProgrammingSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);
            var visitor = new DeleteVisitor();

            fragment.Accept(visitor);

            foreach (var stmt in visitor.NotIgnoredStatements(RuleId))
            {
                if (stmt.DeleteSpecification.WhereClause != null 
                    || !(stmt.DeleteSpecification.Target is NamedTableReference)) { continue; }

                var tableName = ((NamedTableReference)stmt.DeleteSpecification.Target).SchemaObject.Identifiers.Last().Value;

                if (stmt.DeleteSpecification.FromClause != null)
                {
                    var tableVisitor = new TableReferenceVisitor();
                    stmt.DeleteSpecification.FromClause.Accept(tableVisitor);

                    var table = tableVisitor.Statements.OfType<NamedTableReference>()
                        .FirstOrDefault(t => _comparer.Equals(t.Alias?.Value, tableName));
                    if (table != null)
                    {
                        tableName = table.SchemaObject.Identifiers.Last().Value;
                    }
                }
                if (!(tableName.StartsWith("#") || tableName.StartsWith("@")))
                {
                    problems.Add(new SqlRuleProblem(Message, sqlObj, stmt));
                }
            }

            return problems;
        }
    }
}