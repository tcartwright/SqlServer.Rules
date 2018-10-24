using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Dac.Visitors
{
    public class ConvertCallVisitor : BaseVisitor, IVisitor<ConvertCall>
    {
        public IList<ConvertCall> Statements { get; } = new List<ConvertCall>();
		public int Count { get { return this.Statements.Count; } }
		public override void Visit(ConvertCall node)
        {
            Statements.Add(node);
        }
    }
}