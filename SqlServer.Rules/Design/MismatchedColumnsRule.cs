using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
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
        RuleScope = SqlRuleScope.Model)]
    public sealed class MismatchedColumnsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0047";
        public const string RuleDisplayName = "Avoid using columns that match other columns by name, but are different in type or size.";
        private const string Message = "Avoid using columns ({0}) that match other columns by name in the database, but are different in type or size.";

        public MismatchedColumnsRule()
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlModel = ruleExecutionContext.SchemaModel;

            if (sqlModel == null)
                return problems;

            var tables = sqlModel.GetObjects(DacQueryScopes.UserDefined, Table.TypeClass).Where(t => !t.IsWhiteListed());
            var columnList = new List<TableColumnInfo>();

            foreach (var table in tables)
            {
                var fragment = table.GetFragment();
                var columnVisitor = new ColumnDefinitionVisitor();
                fragment.Accept(columnVisitor);
                columnList.AddRange(columnVisitor.NotIgnoredStatements(RuleId)
                    .Where(col => col.DataType != null)
                    .Select(col =>
                    new TableColumnInfo()
                    {
                        TableName = table.Name.GetName(),
                        ColumnName = col.ColumnIdentifier.Value,
                        DataType = col.DataType.Name.Identifiers.FirstOrDefault()?.Value,
                        DataTypeParameters = GetDataTypeLengthParameters(col),
                        Column = col,
                        Table = table
                    }
                ));
            }

            //find all the columns that match by name but differ by data type or length....
            var offenders = columnList.Where(x =>
                columnList.Any(y =>
                    !_comparer.Equals(x.TableName, y.TableName)
                    && _comparer.Equals(x.ColumnName, y.ColumnName)
                    && (
                        !_comparer.Equals(x.DataType, y.DataType)
                        || !_comparer.Equals(x.DataTypeParameters, y.DataTypeParameters)
                    )
                )
            );

            problems.AddRange(offenders
                .Select(col => new SqlRuleProblem(string.Format(Message, col.ToString()), col.Table, col.Column)));

            return problems;
        }

        internal string GetDataTypeLengthParameters(ColumnDefinition col)
        {
            if (col.DataType is SqlDataTypeReference dataType)
            {
                return string.Join(",", dataType.GetDataTypeParameters());
            }
            return string.Empty;
        }

        private class TableColumnInfo
        {
            public string TableName { get; set; }
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public string DataTypeParameters { get; set; }
            public ColumnDefinition Column { get; set; }
            public TSqlObject Table { get; set; }

            public override string ToString()
            {
                return $"{ColumnName} {DataType}({DataTypeParameters.Replace("-1", "MAX")})";
            }
        }
    }
}