﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class TableVariableVisitor : BaseVisitor, IVisitor<VariableTableReference>
    {
        public IList<VariableTableReference> Statements { get; } = new List<VariableTableReference>();
        public int Count { get { return this.Statements.Count; } }
        public override void ExplicitVisit(VariableTableReference node)
        {
            Statements.Add(node);
        }
    }
}