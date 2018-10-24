using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Dac.Visitors
{
    public class StringLiteralVisitor : BaseVisitor, IVisitor<StringLiteral>
    {
        public IList<StringLiteral> Statements { get; } = new List<StringLiteral>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(StringLiteral node)
        {
            Statements.Add(node);
        }
    }
}
