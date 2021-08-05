using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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