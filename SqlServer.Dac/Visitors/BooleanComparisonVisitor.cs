﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class BooleanComparisonVisitor : BaseVisitor, IVisitor<BooleanComparisonExpression>
    {
        public IList<BooleanComparisonExpression> Statements { get; } = new List<BooleanComparisonExpression>();
        public int Count { get { return this.Statements.Count; } }
        public override void ExplicitVisit(BooleanComparisonExpression node)
        {
            Statements.Add(node);
        }
    }
}