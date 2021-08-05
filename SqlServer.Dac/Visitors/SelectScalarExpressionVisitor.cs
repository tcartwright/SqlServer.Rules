﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class SelectScalarExpressionVisitor : BaseVisitor, IVisitor<SelectScalarExpression>
    {
        public IList<SelectScalarExpression> Statements { get; } = new List<SelectScalarExpression>();
        public int Count { get { return this.Statements.Count; } }
        public override void Visit(SelectScalarExpression node)
        {
            Statements.Add(node);
        }
    }
}