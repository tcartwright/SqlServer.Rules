using SqlServer.Rules.Globals;
using SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.ReferentialIntegrity
{
    public class JoinInfo
    {
        public NamedTableReference Table1 { get; set; }
        public NamedTableReference Table2 { get; set; }

        public ObjectIdentifier Table1Name
        {
            get { return this.Table1.SchemaObject.GetObjectIdentifier(); }
        }
        public ObjectIdentifier Table2Name
        {
            get { return this.Table2.SchemaObject.GetObjectIdentifier(); }
        }

        public IList<BooleanComparisonExpression> Compares { get; set; }
        public IList<ColumnReferenceExpression> Table1JoinColumns { get; set; } = new List<ColumnReferenceExpression>();
        public IList<ColumnReferenceExpression> Table2JoinColumns { get; set; } = new List<ColumnReferenceExpression>();

        public bool CheckTableNames(ForeignKeyInfo fkInfo)
        {
            var table1Name = this.Table1Name;
            var table2Name = this.Table2Name;

            return (fkInfo.TableName.CompareTo(table1Name) >= 5 && fkInfo.ToTableName.CompareTo(table2Name) >= 5)
                || (fkInfo.TableName.CompareTo(table2Name) >= 5 && fkInfo.ToTableName.CompareTo(table1Name) >= 5);
        }

        public bool CheckFullJoin(ForeignKeyInfo fkInfo)
        {
            var table1Name = this.Table1Name;
            var table2Name = this.Table2Name;

            if (fkInfo.TableName.CompareTo(table1Name) >= 5
                && fkInfo.ToTableName.CompareTo(table2Name) >= 5)
            {
                var cols = GetColumnNames(fkInfo);

                return cols.fkInfoColumnNames.Intersect(cols.table1Columns).Count() == cols.fkInfoColumnNames.Count()
                    && cols.fkInfoToColumnNames.Intersect(cols.table2Columns).Count() == cols.fkInfoToColumnNames.Count();
            }

            if (fkInfo.TableName.CompareTo(table2Name) >= 5
                && fkInfo.ToTableName.CompareTo(table1Name) >= 5)
            {
                var cols = GetColumnNames(fkInfo);

                return cols.fkInfoColumnNames.Intersect(cols.table2Columns).Count() == cols.fkInfoColumnNames.Count()
                    && cols.fkInfoToColumnNames.Intersect(cols.table1Columns).Count() == cols.fkInfoToColumnNames.Count();
            }
            return false;
        }

        private (IList<string> table1Columns, IList<string> table2Columns, IList<string> fkInfoColumnNames, IList<string> fkInfoToColumnNames) GetColumnNames(ForeignKeyInfo fkInfo)
        {
            var table1Columns = this.Table1JoinColumns
                .Select(x => x.MultiPartIdentifier.Identifiers.Last().Value.ToLower()).ToList();
            var table2Columns = this.Table2JoinColumns
                .Select(x => x.MultiPartIdentifier.Identifiers.Last().Value.ToLower()).ToList();

            var fkInfoColumnNames = fkInfo.ColumnNames.Select(x => x.Parts.Last().ToLower()).ToList();
            var fkInfoToColumnNames = fkInfo.ToColumnNames.Select(x => x.Parts.Last().ToLower()).ToList();

            return (
                table1Columns: table1Columns,
                table2Columns: table2Columns,
                fkInfoColumnNames: fkInfoColumnNames,
                fkInfoToColumnNames: fkInfoToColumnNames
           );
        }

        public override string ToString()
        {
            List<string> cols = new List<string>();
            for (int i = 0; i < Table1JoinColumns.Count; i++)
            {
                var compare = Compares.ElementAt(i);
                var col1 = Table1JoinColumns.ElementAt(i);
                var col2 = Table2JoinColumns.ElementAt(i);
                if (col1 != null && col2 != null)
                {
                    cols.Add($"{col1.MultiPartIdentifier.GetName()} {compare.ComparisonType.ToString()} {col2.MultiPartIdentifier.GetName()}");
                }
            }
            return $"{Table1.GetName()} {Table1.Alias?.Value} JOIN {Table2.GetName()} {Table2.Alias?.Value} ON {string.Join(" + ", cols)}";
        }
    }

}