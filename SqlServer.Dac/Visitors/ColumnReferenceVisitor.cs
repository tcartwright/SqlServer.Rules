﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class ColumnReferenceExpressionVisitor : BaseVisitor, IVisitor<ColumnReferenceExpression>
    {
        public IList<ColumnReferenceExpression> Statements { get; } = new List<ColumnReferenceExpression>();
        public int Count { get { return this.Statements.Count; } }
        public override void ExplicitVisit(ColumnReferenceExpression node)
        {
            Statements.Add(node);
        }

        public static IList<ColumnReferenceExpression> VisitSelectElements(IList<SelectElement> selectElements)
        {
            var columns = new List<ColumnReferenceExpression>();
            foreach (var item in selectElements)
            {
                var columnVisitor = new ColumnReferenceExpressionVisitor();
                item.Accept(columnVisitor);
                columns.AddRange(columnVisitor.Statements);
            }
            return columns;
        }
    }
}