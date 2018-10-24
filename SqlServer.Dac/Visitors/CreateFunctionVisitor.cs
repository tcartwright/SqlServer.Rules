using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Dac.Visitors
{
    public class CreateFunctionVisitor : BaseVisitor, IVisitor<CreateFunctionStatement>
    {
        public IList<CreateFunctionStatement> Statements { get; } = new List<CreateFunctionStatement>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(CreateFunctionStatement node)
        {
            Statements.Add(node);
        }
    }
}
