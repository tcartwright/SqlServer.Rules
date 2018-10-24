using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Dac.Visitors
{
    public class SelectElementVisitor : BaseVisitor, IVisitor<SelectElement>
    {
        public IList<SelectElement> Statements { get; } = new List<SelectElement>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(SelectElement node)
        {
            Statements.Add(node);
        }
    }
}
