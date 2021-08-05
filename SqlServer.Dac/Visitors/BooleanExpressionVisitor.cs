﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class BooleanExpressionVisitor : BaseVisitor, IVisitor<BooleanExpression>
    {
        public IList<BooleanExpression> Statements { get; } = new List<BooleanExpression>();
        public int Count { get { return this.Statements.Count; } }
        public override void Visit(BooleanExpression node)
        {
            Statements.Add(node);
        }
    }
}