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
    public sealed class ConsiderIndexingInClauseColumnsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0012";
        public const string RuleDisplayName = "Consider indexing the columns referenced by IN predicates in order to avoid table scans.";
        public const string Message = RuleDisplayName;


        public ConsiderIndexingInClauseColumnsRule() : base(ProgrammingAndViewSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;


            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);
            var selectStatementVisitor = new SelectStatementVisitor();
            fragment.Accept(selectStatementVisitor);

            foreach (var stmt in selectStatementVisitor.Statements)
            {
                var querySpecificationVisitor = new QuerySpecificationVisitor();
                stmt.QueryExpression.Accept(querySpecificationVisitor);

                foreach (var query in querySpecificationVisitor.Statements)
                {
                    var inPredicateVisitor = new InPredicateVisitor();
                    query.Accept(inPredicateVisitor);
                    var inClauses = inPredicateVisitor.NotIgnoredStatements(RuleId)
                        .Where(i => !i.NotDefined && i.Expression is ColumnReferenceExpression);

                    if (inClauses.Count() == 0) { continue; }

                    foreach (var inClause in inClauses)
                    {
                        var indexColumnExists = false;
                        var column = inClause.Expression as ColumnReferenceExpression;

                        var table = GetTableFromColumn(sqlObj, query, column);

                        //most likely the base is a view.... /sigh
                        if (table == null) { continue; }

                        var indexes = table.GetChildren(DacQueryScopes.All).Where(x => x.ObjectType == ModelSchema.Index);

                        foreach (var index in indexes)
                        {
                            indexColumnExists = index.GetReferenced(DacQueryScopes.All)
                                .Any(x => 
                                    x.ObjectType == ModelSchema.Column 
                                    && _comparer.Equals(x.Name.Parts.Last(), column.MultiPartIdentifier.Identifiers.Last().Value));
                            if (indexColumnExists) { break; }
                        }

                        if (!indexColumnExists) { problems.Add(new SqlRuleProblem(Message, sqlObj, inClause)); }
                    }

                }
            }

            return problems;
        }
    }
}
