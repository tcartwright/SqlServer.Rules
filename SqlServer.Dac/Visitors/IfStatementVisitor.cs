﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class IfStatementVisitor : BaseVisitor, IVisitor<IfStatement>
    {
        public IList<IfStatement> Statements { get; } = new List<IfStatement>();
        public int Count { get { return this.Statements.Count; } }
        public override void ExplicitVisit(IfStatement node)
        {
            Statements.Add(node);
        }
    }
}