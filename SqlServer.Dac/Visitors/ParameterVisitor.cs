using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Dac.Visitors
{
    public class ParameterVisitor : BaseVisitor, IVisitor<ProcedureParameter>
    {
        public IList<ProcedureParameter> Statements { get; } = new List<ProcedureParameter>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(ProcedureParameter node)
        {
            Statements.Add(node);
        }
    }
}