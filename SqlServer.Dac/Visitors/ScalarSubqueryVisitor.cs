using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Dac.Visitors
{
    public class ScalarSubqueryVisitor : BaseVisitor, IVisitor<ScalarSubquery>
    {
        public IList<ScalarSubquery> Statements { get; } = new List<ScalarSubquery>();
		public int Count { get { return this.Statements.Count; } }
		public override void Visit(ScalarSubquery node)
        {
            Statements.Add(node);
        }
    }
}