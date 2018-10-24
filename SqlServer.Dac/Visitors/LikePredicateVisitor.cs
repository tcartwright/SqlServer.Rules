using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Dac.Visitors
{
    public class LikePredicateVisitor : BaseVisitor, IVisitor<LikePredicate>
    {
        public IList<LikePredicate> Statements { get; } = new List<LikePredicate>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(LikePredicate node)
        {
            Statements.Add(node);
        }
    }
}
