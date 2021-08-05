﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class TableHintVisitor : BaseVisitor, IVisitor<TableHint>
    {
        public IList<TableHint> Statements { get; } = new List<TableHint>();
        public int Count { get { return this.Statements.Count; } }
        public override void ExplicitVisit(TableHint node)
        {
            //TODO: Does not visit FORCESEEK and possible others
            Statements.Add(node);
        }
    }
}