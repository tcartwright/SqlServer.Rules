﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class QueryHintVisitor : BaseVisitor, IVisitor<QuerySpecification>
    {
        public IList<QuerySpecification> Statements { get; } = new List<QuerySpecification>();
        public int Count { get { return this.Statements.Count; } }
        public override void ExplicitVisit(QuerySpecification node)
        {
            Statements.Add(node);
        }
    }
}