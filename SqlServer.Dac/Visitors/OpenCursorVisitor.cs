using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Dac.Visitors
{
    public class OpenCursorVisitor : BaseVisitor, IVisitor<OpenCursorStatement>
    {
        public IList<OpenCursorStatement> Statements { get; } = new List<OpenCursorStatement>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(OpenCursorStatement node)
        {
            Statements.Add(node);
        }
    }
}