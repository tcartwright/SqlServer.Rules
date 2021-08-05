﻿using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Rules.Globals;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.ReferentialIntegrity
{
    /// <summary>
    /// 
    /// </summary>
    public class JoinInfo
    {
        /// <summary>
        /// Gets or sets the table1.
        /// </summary>
        /// <value>
        /// The table1.
        /// </value>
        public NamedTableReference Table1 { get; set; }
        /// <summary>
        /// Gets or sets the table2.
        /// </summary>
        /// <value>
        /// The table2.
        /// </value>
        public NamedTableReference Table2 { get; set; }

        /// <summary>
        /// Gets the name of the table1.
        /// </summary>
        /// <value>
        /// The name of the table1.
        /// </value>
        public ObjectIdentifier Table1Name
        {
            get { return this.Table1.SchemaObject.GetObjectIdentifier(); }
        }
        /// <summary>
        /// Gets the name of the table2.
        /// </summary>
        /// <value>
        /// The name of the table2.
        /// </value>
        public ObjectIdentifier Table2Name
        {
            get { return this.Table2.SchemaObject.GetObjectIdentifier(); }
        }

        /// <summary>
        /// Gets or sets the compares.
        /// </summary>
        /// <value>
        /// The compares.
        /// </value>
        public IList<BooleanComparisonExpression> Compares { get; set; }
        /// <summary>
        /// Gets or sets the table1 join columns.
        /// </summary>
        /// <value>
        /// The table1 join columns.
        /// </value>
        public IList<ColumnReferenceExpression> Table1JoinColumns { get; set; } = new List<ColumnReferenceExpression>();
        /// <summary>
        /// Gets or sets the table2 join columns.
        /// </summary>
        /// <value>
        /// The table2 join columns.
        /// </value>
        public IList<ColumnReferenceExpression> Table2JoinColumns { get; set; } = new List<ColumnReferenceExpression>();

        /// <summary>
        /// Checks the table names.
        /// </summary>
        /// <param name="fkInfo">The fk information.</param>
        /// <returns></returns>
        public bool CheckTableNames(ForeignKeyInfo fkInfo)
        {
            var table1Name = this.Table1Name;
            var table2Name = this.Table2Name;

            return (fkInfo.TableName.CompareTo(table1Name) >= 5 && fkInfo.ToTableName.CompareTo(table2Name) >= 5)
                || (fkInfo.TableName.CompareTo(table2Name) >= 5 && fkInfo.ToTableName.CompareTo(table1Name) >= 5);
        }

        /// <summary>
        /// Checks the full join.
        /// </summary>
        /// <param name="fkInfo">The fk information.</param>
        /// <returns></returns>
        public bool CheckFullJoin(ForeignKeyInfo fkInfo)
        {
            var table1Name = this.Table1Name;
            var table2Name = this.Table2Name;

            if (fkInfo.TableName.CompareTo(table1Name) >= 5
                && fkInfo.ToTableName.CompareTo(table2Name) >= 5)
            {
                var (table1Columns, table2Columns, fkInfoColumnNames, fkInfoToColumnNames) = GetColumnNames(fkInfo);

                return fkInfoColumnNames.Intersect(table1Columns).Count() == fkInfoColumnNames.Count()
                    && fkInfoToColumnNames.Intersect(table2Columns).Count() == fkInfoToColumnNames.Count();
            }

            if (fkInfo.TableName.CompareTo(table2Name) >= 5
                && fkInfo.ToTableName.CompareTo(table1Name) >= 5)
            {
                var (table1Columns, table2Columns, fkInfoColumnNames, fkInfoToColumnNames) = GetColumnNames(fkInfo);

                return fkInfoColumnNames.Intersect(table2Columns).Count() == fkInfoColumnNames.Count()
                    && fkInfoToColumnNames.Intersect(table1Columns).Count() == fkInfoToColumnNames.Count();
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

            return (table1Columns, table2Columns, fkInfoColumnNames, fkInfoToColumnNames);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
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
                    cols.Add($"{col1.MultiPartIdentifier.GetName()} {compare.ComparisonType} {col2.MultiPartIdentifier.GetName()}");
                }
            }
            return $"{Table1.GetName()} {Table1.Alias?.Value} JOIN {Table2.GetName()} {Table2.Alias?.Value} ON {string.Join(" + ", cols)}";
        }
    }

}