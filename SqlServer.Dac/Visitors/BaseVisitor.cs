using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace SqlServer.Dac.Visitors
{
	public abstract class BaseVisitor : TSqlFragmentVisitor
	{
		protected readonly StringComparer _comparer = StringComparer.InvariantCultureIgnoreCase;
	}
}
