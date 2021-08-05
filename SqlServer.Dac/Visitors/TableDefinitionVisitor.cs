﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class TableDefinitionVisitor : BaseVisitor, IVisitor<TableDefinition>
    {
        public IList<TableDefinition> Statements { get; } = new List<TableDefinition>();
        public int Count { get { return this.Statements.Count; } }
        public override void ExplicitVisit(TableDefinition node)
        {
            Statements.Add(node);
        }
    }
}