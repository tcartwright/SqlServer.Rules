﻿using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlServer.Dac.Visitors
{
    public class ExecuteVisitor : BaseVisitor, IVisitor<ExecuteStatement>
    {
        private readonly IList<string> _procNames = null;
        public ExecuteVisitor()
        {
            _procNames = new List<string>();
        }
        public ExecuteVisitor(params string[] procNames)
        {
            _procNames = procNames.ToList();
        }
        public IList<ExecuteStatement> Statements { get; } = new List<ExecuteStatement>();
        public int Count { get { return this.Statements.Count; } }
        public override void ExplicitVisit(ExecuteStatement node)
        {
            if (!_procNames.Any())
            {
                Statements.Add(node);
            }
            else if (_procNames.Any(f => CheckProcName(node, f)))
            {
                Statements.Add(node);
            }
        }

        private bool CheckProcName(ExecuteStatement exec, string name)
        {
            if (!(exec.ExecuteSpecification.ExecutableEntity is ExecutableProcedureReference execProc))
            {
                return false;
            }
            var procName = execProc.ProcedureReference.ProcedureReference.Name.GetName();
            return Regex.IsMatch(procName, name, RegexOptions.IgnoreCase);
        }
    }
}