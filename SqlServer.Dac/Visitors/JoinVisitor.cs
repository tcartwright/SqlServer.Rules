﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Dac.Visitors
{
    public class JoinVisitor : BaseVisitor, IVisitor<JoinTableReference>
    {
        public IList<JoinTableReference> Statements { get; } = new List<JoinTableReference>();
        public int Count { get { return this.Statements.Count; } }
        public override void Visit(JoinTableReference node)
        {
            Statements.Add(node);
        }

        public IEnumerable<QualifiedJoin> QualifiedJoins
        {
            get { return Statements.OfType<QualifiedJoin>(); }
        }

        public IEnumerable<UnqualifiedJoin> UnqualifiedJoins
        {
            get { return Statements.OfType<UnqualifiedJoin>(); }
        }
    }
}