using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Dac.Visitors
{
    public class CloseCursorVisitor : BaseVisitor, IVisitor<CloseCursorStatement>
    {
        public IList<CloseCursorStatement> Statements { get; } = new List<CloseCursorStatement>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(CloseCursorStatement node)
        {
            Statements.Add(node);
        }
    }
}