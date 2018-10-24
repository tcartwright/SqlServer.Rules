using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.SqlServer.Dac.CodeAnalysis;

namespace SqlServer.Dac.Visitors
{
    public class ExecutableStringListVisitor : BaseVisitor, IVisitor<ExecutableStringList>
    {
        public IList<ExecutableStringList> Statements { get; } = new List<ExecutableStringList>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(ExecutableStringList node)
        {
            Statements.Add(node);
        }
    }
}