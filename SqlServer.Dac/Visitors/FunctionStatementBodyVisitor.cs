using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Dac.Visitors
{
    public class FunctionStatementBodyVisitor : BaseVisitor, IVisitor<FunctionStatementBody>
    {
        public IList<FunctionStatementBody> Statements { get; } = new List<FunctionStatementBody>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(FunctionStatementBody node)
        {
            Statements.Add(node);
        }
    }
}