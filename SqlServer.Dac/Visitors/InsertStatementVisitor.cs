﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class InsertStatementVisitor : BaseVisitor, IVisitor<InsertStatement>
    {
        public IList<InsertStatement> Statements { get; } = new List<InsertStatement>();
        public int Count { get { return this.Statements.Count; } }
        public override void Visit(InsertStatement node)
        {
            Statements.Add(node);
        }
    }
}