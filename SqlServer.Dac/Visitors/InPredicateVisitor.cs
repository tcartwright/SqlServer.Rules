using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Dac.Visitors
{
    public class InPredicateVisitor : BaseVisitor, IVisitor<InPredicate>
    {
        public IList<InPredicate> Statements { get; } = new List<InPredicate>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(InPredicate node)
        {
            Statements.Add(node);
        }
    }
}
