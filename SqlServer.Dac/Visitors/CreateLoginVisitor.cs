﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.SqlServer.TransactSql.ScriptDom.TSqlFragmentVisitor" />
    public class CreateLoginVisitor : BaseVisitor, IVisitor<CreateLoginStatement>
    {
        public IList<CreateLoginStatement> Statements { get; } = new List<CreateLoginStatement>();
        public int Count { get { return this.Statements.Count; } }
        public override void ExplicitVisit(CreateLoginStatement node)
        {
            Statements.Add(node);
        }
    }
}
