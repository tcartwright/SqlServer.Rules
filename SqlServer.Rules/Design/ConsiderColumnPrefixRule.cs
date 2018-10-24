using SqlServer.Rules.Globals;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using System;
using SqlServer.Dac;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class ConsiderColumnPrefixRule : BaseSqlCodeAnalysisRule

    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0028";
        public const string RuleDisplayName = "Consider prefixing column names with table name or table alias.";
        private const string Message = "Consider prefixing column names with table name or table alias. Preferably an alias.";

        public ConsiderColumnPrefixRule() : base(ProgrammingAndViewSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);
            var selectStatementVisitor = new SelectStatementVisitor();
            fragment.Accept(selectStatementVisitor);

            if (selectStatementVisitor.Count == 0) { return problems; }

            foreach (var select in selectStatementVisitor.Statements)
            {
                if (select.QueryExpression is QuerySpecification query)
                {
                    var fromClause = query.FromClause;
                    if (fromClause == null) { continue; }
                    //check to ensure we have more than one table
                    var namedTableVisitor = new NamedTableReferenceVisitor();
                    fromClause.Accept(namedTableVisitor);
                    if (namedTableVisitor.Count <= 1) { continue; }

                    var columnReferences = new ColumnReferenceExpressionVisitor();
                    query.Accept(columnReferences);

                    var offenders = columnReferences.NotIgnoredStatements(RuleId)
                        .Where(c => CheckName(c))
                        .Select(n => n.MultiPartIdentifier.Identifiers[0]);

                    if (offenders.Any())
                    {
                        problems.Add(new SqlRuleProblem(Message, sqlObj, select));
                    }

                }
            }
            return problems;
        }

        private bool CheckName(ColumnReferenceExpression col)
        {
            var names = col.MultiPartIdentifier?.Identifiers;
            if (names == null) { return false; }
            return names.Count == 1 && !Constants.DateParts.Contains(names.First().Value.ToLower());
        }
    }
}