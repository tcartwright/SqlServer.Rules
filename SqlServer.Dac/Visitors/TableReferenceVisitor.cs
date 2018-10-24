using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Dac.Visitors
{
    public class TableReferenceVisitor : BaseVisitor, IVisitor<TableReference>
    {
        public IList<TableReference> Statements { get; } = new List<TableReference>();
		public int Count { get { return this.Statements.Count; } }
		public override void Visit(TableReference node)
        {
            //as we are using the visit here, and NOT the explicit visit, all tables will be included that inherit from tablereference
            if (!(node is QualifiedJoin))
            {
                Statements.Add(node);
            }
        }
    }
}
