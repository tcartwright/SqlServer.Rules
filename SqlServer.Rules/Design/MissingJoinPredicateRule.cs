using SqlServer.Rules.Globals;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using SqlServer.Rules.ReferentialIntegrity;
using SqlServer.Dac;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class MissingJoinPredicateRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0020";
        public const string RuleDisplayName = "The query has issues with the join clause. It is either missing a backing foreign key or the join is missing one or more columns.";
        private const string Message = "The query is missing one or more columns in the join clause. This may affect the results or may result in an unexpected row difference.";
        private const string MessageNoJoin = "The query is using a join, but there is no backing foreign key to match the join.";

        public MissingJoinPredicateRule() : base(ProgrammingAndViewSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            var db = ruleExecutionContext.SchemaModel;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);

            var fromClauseVisitor = new FromClauseVisitor();
            fragment.Accept(fromClauseVisitor);

            if (fromClauseVisitor.Count == 0) { return problems; }

            //cache all the fks we find 
            var fkList = new Dictionary<string, ForeignKeyInfo>();

            foreach (var from in fromClauseVisitor.Statements.Where(x => x.TableReferences.First() is QualifiedJoin))
            {
                var joinInfos = from.GetFromClauseJoinTables()
                    .Where(x => x.Table1JoinColumns.Count > 0 && (x.Table1JoinColumns.Count == x.Table2JoinColumns.Count)
                ).ToList();
                if (!joinInfos.Any()) { continue; }

                foreach (var join in joinInfos)
                {
                    if (join.Table1 == null || join.Table2 == null) { continue; }
                    //get the tables belonging to this join
                    var table1 = db.GetObject(Table.TypeClass, join.Table1Name, DacQueryScopes.All);
                    var table2 = db.GetObject(Table.TypeClass, join.Table2Name, DacQueryScopes.All);
                    //this can happen when one of the tables is a temp table, in that case we do not care to inspect the fks
                    if (table1 == null || table2 == null) { continue; }

                    fkList.AddRange(table1.GetTableFKInfos());
                    //only get the fks for table2 if it is a diff table, otherwise it is a self join
                    if (table1.Name.CompareTo(table2.Name) < 5)
                    {
                        fkList.AddRange(table2.GetTableFKInfos());
                    }

                    //find any possible fks where the table names match exactly
                    var possibleFks = fkList.Where(f => join.CheckTableNames(f.Value)).ToList();

                    //we did not find any fks where the tables in the join matched the tables in the fk
                    if (!possibleFks.Any())
                    {
                        problems.Add(new SqlRuleProblem(MessageNoJoin, sqlObj, join.Table2));
                        continue;
                    }

                    //check to see if all of the columns match
                    if (!possibleFks.Any(f => join.CheckFullJoin(f.Value)))
                    {
                        problems.Add(new SqlRuleProblem(Message, sqlObj, join.Table2));
                    }
                }
            }
            return problems;
        }

    }
}