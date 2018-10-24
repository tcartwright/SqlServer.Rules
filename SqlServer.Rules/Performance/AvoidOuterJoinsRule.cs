using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

/*
 TIM C: NOTES:

The rule checks for OUTER JOIN-s which could be replaced with EXISTS.
Prefer use of EXISTS keyword for existence checks, unless performance issues are encountered. In these cases, it is better to resort to using a LEFT JOIN and null check.
The traditional method of checking for row existence is to use a LEFT JOIN and checking the nullability of a LEFT JOIN'ed column in the WHERE clause. The problem with this method is that SQL Server needs to load all of the rows from the OUTER JOIN'ed table. In cases where the matched rows are significantly less than the total rows, it is unnecessary work for SQL Server.
Another method of checking for existence is using the EXISTS predicate function. This is preferably to the LEFT JOIN method, since it allows SQL Server to find a row and quit (using a row count spool), avoiding unnecessary row loading.
Of course, there is always a catch - using EXISTS() can incur a performance issue if the following are encountered:
If there are joins in the EXISTS subquery, SQL Server will favor performing loop joins through the tables, hoping to find a row quickly. In certain cases, loop joins may be inefficient.
If the SQL optimizer underestimates the rowcount from the table in the EXISTS subquery, the query plan may show an optimal plan but the query will perform much worse.

SHOULD FLAG AS PROBLEM:
    select a.*
    from a 
    left join b on a.id = b.id
    where b.id is null 

SHOULD NOT FLAG AS PROBLEM:
    select a.*, b.*
    from a 
    left join b on a.id = b.id
    where b.id is null 

    select a.*, b.*
    from a 
    left join b on a.id = b.id
 
 */

namespace SqlServer.Rules.Performance
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidOuterJoinsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0013";
        public const string RuleDisplayName = "Consider replacing the OUTER JOIN with EXISTS.";
        private const string Message = RuleDisplayName;

        public AvoidOuterJoinsRule() : base(ProgrammingAndViewSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);
            var visitor = new SelectStatementVisitor();
            fragment.Accept(visitor);

            foreach (var stmt in visitor.Statements)
            {
                var querySpecificationVisitor = new QuerySpecificationVisitor();
                stmt.QueryExpression.Accept(querySpecificationVisitor);

                foreach (var query in querySpecificationVisitor.Statements)
                {
                    var fromClause = query.FromClause;
                    if (fromClause == null) { continue; }
                    var joinVisitor = new JoinVisitor();
                    fromClause.Accept(joinVisitor);

                    var outerJoins =
                        (from j in joinVisitor.QualifiedJoins
                         let t = j.QualifiedJoinType
                         where (t == QualifiedJoinType.LeftOuter || t == QualifiedJoinType.RightOuter)
                            && Ignorables.ShouldNotIgnoreRule(j.ScriptTokenStream, RuleId, j.StartLine)
                         select j).ToList();

                    if (outerJoins.Count == 0) { continue; }
                    var columns = ColumnReferenceExpressionVisitor.VisitSelectElements(query.SelectElements);

                    var whereClause = query.WhereClause;
                    if (whereClause == null) { continue; }
                    var isnullVisitor = new ISNULLVisitor();
                    whereClause.Accept(isnullVisitor);
                    if (isnullVisitor.Count == 0) { continue; }

                    foreach (var join in outerJoins)
                    {
                        TableReference table = null;
                        if (join.QualifiedJoinType == QualifiedJoinType.LeftOuter)
                        {
                            table = join.SecondTableReference as TableReference;
                        }
                        else
                        {
                            table = join.FirstTableReference as TableReference;
                        }

                        var tableName = table.GetName();
                        var alias = (table as TableReferenceWithAlias)?.Alias.Value;

                        //are there any columns in the select that match this table? 
                        if (columns.Any(c =>
                        {
                            var colTableName = c.GetName();
                            return _comparer.Equals(tableName, colTableName) || _comparer.Equals(alias, colTableName);
                        }))
                        {
                            continue;
                        }

                        //no columns, now we need to look in the where clause for a null check againt this table.
                        if (isnullVisitor.Statements.Any(nc =>
                        {
                            var col = nc.FirstExpression as ColumnReferenceExpression ?? nc.SecondExpression as ColumnReferenceExpression;
                            if (col == null) { return false; }
                            var colTableName = col.GetName();
                            return _comparer.Equals(tableName, colTableName) || _comparer.Equals(alias, colTableName);
                        }))
                        {
                            problems.Add(new SqlRuleProblem(Message, sqlObj, join));
                        }
                    }
                }
            }

            return problems;
        }
    }
}