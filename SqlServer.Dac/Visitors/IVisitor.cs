using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.Dac.Visitors
{
	public interface IVisitor<T> : IBaseVisitor where T : TSqlFragment
    {
        IList<T> Statements { get; }
    }
}
