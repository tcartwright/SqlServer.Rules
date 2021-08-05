﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.SqlServer.TransactSql.ScriptDom.TSqlFragmentVisitor" />
    public class CreateUserVisitor : BaseVisitor, IVisitor<CreateUserStatement>
    {
        public IList<CreateUserStatement> Statements { get; } = new List<CreateUserStatement>();
        public int Count { get { return this.Statements.Count; } }
        public override void ExplicitVisit(CreateUserStatement node)
        {
            Statements.Add(node);
        }
    }
}
