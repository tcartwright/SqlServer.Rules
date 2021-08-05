﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Dac.Visitors
{
    public class FunctionCallVisitor : BaseVisitor, IVisitor<FunctionCall>
    {
        private readonly IList<string> _functionNames = null;
        public FunctionCallVisitor()
        {
            _functionNames = new List<string>();
        }
        public FunctionCallVisitor(params string[] functionNames)
        {
            _functionNames = functionNames.ToList();
        }
        public IList<FunctionCall> Statements { get; } = new List<FunctionCall>();
        public int Count { get { return this.Statements.Count; } }
        public override void ExplicitVisit(FunctionCall node)
        {
            if (!_functionNames.Any())
            {
                Statements.Add(node);
            }
            else if (_functionNames.Any(f => _comparer.Equals(f, node.FunctionName.Value)))
            {
                Statements.Add(node);
            }
        }
    }
}
