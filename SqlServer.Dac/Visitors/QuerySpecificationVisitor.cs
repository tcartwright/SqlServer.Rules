using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Dac.Visitors
{
    public class QuerySpecificationVisitor : BaseVisitor, IVisitor<QuerySpecification>
    {
        public IList<QuerySpecification> Statements { get; } = new List<QuerySpecification>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(QuerySpecification node)
        {
            Statements.Add(node);
        }
    }
}
