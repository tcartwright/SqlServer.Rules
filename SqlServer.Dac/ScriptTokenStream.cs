using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.Dac
{
    public static class Utils
    {
        public static string ToScript(this IList<TSqlParserToken> scriptTokenStream)
        {
            if (scriptTokenStream == null) { return null; }
            return string.Join("", scriptTokenStream.Select(t => t.Text));

        }
    }
}
