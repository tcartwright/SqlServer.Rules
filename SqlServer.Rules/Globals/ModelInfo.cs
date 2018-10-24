using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.Rules.Globals
{
    public static class ModelInfo
    {
        /// <summary>
        /// checks to see if the name of a TSqlObject is white-listed so we do not want to check rules against it
        /// </summary>
        /// <param name="sqlObj">The SQL object.</param>
        /// <returns>
        ///   <c>true</c> if [is white listed] [the specified SQL object]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWhiteListed(this TSqlObject sqlObj)
        {
            if (sqlObj == null) { throw new ArgumentNullException(nameof(sqlObj)); }
            return "[dbo].[RfcVersionHistory]".StringEquals(sqlObj.Name.ToString());
        }

        /// <summary>
        /// Gets the data type view.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public static DataTypeView GetDataTypeView(this IDictionary<NamedTableView, IDictionary<string, DataTypeView>> list, ColumnReferenceExpression column)
        {
            if (column == null) { return null; }
            var colIdentifier = column.MultiPartIdentifier.GetObjectIdentifier(null);
            var colName = colIdentifier.Parts.Last();
            if (colIdentifier.Parts.Count == 1)
            {
                var dt = list.SelectMany(cols => cols.Value).FirstOrDefault(x => colName.StringEquals(x.Key));
                return dt.Value;
            }
            else
            {
                var columnList = list.Where(t => t.Key.IsMatch(colIdentifier)).ToList();
                if (columnList.Any())
                {
                    //ok, if there are multiples.... really dont know how to handle that.
                    return columnList.First().Value.FirstOrDefault(x => x.Value.Name.StringEquals(colIdentifier.Parts.Last())).Value;
                }
            }
            return null;
        }

        public static string GetSetVariable(this SelectStatement select, string variableName)
        {
            if (select.QueryExpression is QuerySpecification query)
            {
                var result = query.SelectElements.OfType<SelectSetVariable>()
                    .FirstOrDefault(s => s.Variable.Name.StringEquals(variableName));

                return result?.Variable.Name;
            }
            return null;
        }

        public static int GetValue(this IntegerLiteral literal)
        {
            var result = 0;

            if (literal == null || string.IsNullOrEmpty(literal.Value))
                return result;

            int.TryParse(literal.Value, out result);

            return result;
        }

        public static void GetTableColumnDataTypes(this TSqlStatement query, IDictionary<NamedTableView, IDictionary<string, DataTypeView>> list, TSqlModel model)
        {
            if (query == null) { return; }
            //new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            var namedTableReferenceVisitor = new NamedTableReferenceVisitor();
            query.Accept(namedTableReferenceVisitor);

            foreach (var tbl in namedTableReferenceVisitor.Statements)
            {
                var tblView = new NamedTableView(tbl);
                if (!list.ContainsKey(tblView))
                {
                    list.Add(tblView, tbl.GetColumnsAndDataTypes(model));
                }
                else if (!string.IsNullOrEmpty(tbl.Alias?.Value))
                {
                    var kv = list.First(x => x.Key.Equals(tblView));
                    if (!kv.Key.HasAlias(tbl.Alias?.Value))
                    {
                        kv.Key.Aliases.Add(tbl.Alias?.Value);
                        list.Remove(kv.Key);
                        list.Add(kv.Key, kv.Value);
                    }
                }
            }
        }
        public static IDictionary<string, DataTypeView> GetColumnsAndDataTypes(this NamedTableReference table, TSqlModel model)
        {
            var ret = new Dictionary<string, DataTypeView>(StringComparer.InvariantCultureIgnoreCase);

            var tbl = model.GetObject(ModelSchema.Table, table.SchemaObject.GetObjectIdentifier(), DacQueryScopes.All);
            if (tbl == null) { return ret; }

            var columns = tbl.GetChildren(DacQueryScopes.UserDefined).Where(x => x.ObjectType == ModelSchema.Column);

            foreach (var column in columns)
            {
                ret.Add(column.Name.Parts.Last(), new DataTypeView(column));
            }

            return ret;
        }

        public static IEnumerable<SelectStatement> GetSelectsSettingParameterValue(this IEnumerable<SelectStatement> selects, string parameter)
        {
            if (!selects.Any() || string.IsNullOrWhiteSpace(parameter)) { yield break; }

            foreach (var select in selects)
            {
                if (parameter.StringEquals(select.GetSetVariable(parameter)))
                {
                    yield return select;
                }
            }

            yield break;
        }

        public static IEnumerable<SelectStatement> GetSelectsUsingParameterInWhere(this IEnumerable<SelectStatement> selects, string parameter)
        {
            if (!selects.Any() || string.IsNullOrWhiteSpace(parameter)) { yield break; }

            foreach (var select in selects)
            {
                if (select.QueryExpression is QuerySpecification query)
                {
                    if (query.FromClause == null || query.WhereClause == null) { continue; }

                    var variableVisitor = new VariableReferenceVisitor();
                    query.WhereClause.Accept(variableVisitor);

                    if (variableVisitor.Statements.Any(v => parameter.StringEquals(v.Name)))
                    {
                        yield return select;
                    }
                }
            }

            yield break;
        }

        public static IEnumerable<decimal> GetDataTypeParameters(this DataTypeReference dataType)
        {
            if (dataType == null) { return Enumerable.Empty<decimal>(); }
            if (dataType is ParameterizedDataTypeReference parameterizedDataType)
            {
                Type type = typeof(decimal);
                return parameterizedDataType.Parameters.Select(l =>
                    (decimal)Convert.ChangeType((l.Value.StringEquals("MAX") ? "-1" : l.Value), type)
                );
            }
            return Enumerable.Empty<decimal>();
        }


    }
}
