using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Dac.Visitors
{
    public class DataTypeReferenceVisitor : BaseVisitor, IVisitor<DataTypeReference>
    {
        public IList<DataTypeReference> Statements { get; } = new List<DataTypeReference>();
		public int Count { get { return this.Statements.Count; } }
		public override void ExplicitVisit(DataTypeReference node)
        {
            Statements.Add(node);
        }
    }
}
