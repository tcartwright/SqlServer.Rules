﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class TableReferenceWithAliasVisitor : BaseVisitor, IVisitor<TableReferenceWithAlias>
    {
        public IList<TableReferenceWithAlias> Statements { get; } = new List<TableReferenceWithAlias>();
        public int Count { get { return this.Statements.Count; } }
        public override void Visit(TableReferenceWithAlias node)
        {
            Statements.Add(node);
        }
    }
}