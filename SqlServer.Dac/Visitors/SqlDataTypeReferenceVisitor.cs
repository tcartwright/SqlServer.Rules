using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Dac.Visitors
{
    public class SqlDataTypeReferenceVisitor : BaseVisitor, IVisitor<SqlDataTypeReference>
    {
        public IList<SqlDataTypeReference> Statements { get; } = new List<SqlDataTypeReference>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(SqlDataTypeReference node)
        {
            Statements.Add(node);
        }
    }
}
