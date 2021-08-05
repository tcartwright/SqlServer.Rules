﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class VariableReferenceVisitor : BaseVisitor, IVisitor<VariableReference>
    {
        public IList<VariableReference> Statements { get; } = new List<VariableReference>();
        public int Count { get { return this.Statements.Count; } }
        public override void Visit(VariableReference node)
        {
            Statements.Add(node);
        }
    }
}