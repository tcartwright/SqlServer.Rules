using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Dac.Visitors
{
    public class SelectStarExpressionVisitor : BaseVisitor, IVisitor<SelectStarExpression>
    {
        public IList<SelectStarExpression> Statements { get; } = new List<SelectStarExpression>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(SelectStarExpression node)
        {
            Statements.Add(node);
        }
    }
}
