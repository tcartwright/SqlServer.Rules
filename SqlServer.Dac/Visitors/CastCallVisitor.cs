using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Dac.Visitors
{
    public class CastCallVisitor : BaseVisitor, IVisitor<CastCall>
    {
        public IList<CastCall> Statements { get; } = new List<CastCall>();
		public int Count { get { return this.Statements.Count; } }
		public override void Visit(CastCall node)
        {
            Statements.Add(node);
        }
    }
}