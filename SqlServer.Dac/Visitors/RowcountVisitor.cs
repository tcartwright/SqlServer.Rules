using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class RowCountVisitor : BaseVisitor, IVisitor<SetRowCountStatement>
    {
        public IList<SetRowCountStatement> Statements { get; } = new List<SetRowCountStatement>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(SetRowCountStatement node)
        {
            Statements.Add(node);
        }
    }
}