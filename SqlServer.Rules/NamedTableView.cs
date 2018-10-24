using SqlServer.Rules.Globals;
using SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.Rules
{
    public class NamedTableView
    {
        public NamedTableView(NamedTableReference namedTable)
        {
            this.Name = namedTable.GetName("dbo");
            if (namedTable.Alias != null && !HasAlias(namedTable.Alias.Value))
            {
                Aliases.Add(namedTable.Alias.Value);
            }
        }
        public string Name { get; set; }
        public IList<string> Aliases { get; set; } = new List<string>();

        public ObjectIdentifier NameToId()
        {
            return new ObjectIdentifier((Name ?? string.Empty).Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public bool HasAlias(string alias)
        {
            return Aliases.Any(a => a.StringEquals(alias));
        }

        public bool IsMatch(ObjectIdentifier id)
        {
            var tableNameOrAlias = new ObjectIdentifier(id.Parts.Take(id.Parts.Count - 1));

            return this.NameToId().CompareTo(tableNameOrAlias) >= 5
                || HasAlias(tableNameOrAlias.Parts.First());
        }

        public override bool Equals(object obj)
        {
            var tmp = obj as NamedTableView;
            if (tmp == null) { return false; }
            if (!string.IsNullOrWhiteSpace(Name)) { return Name.StringEquals(tmp.Name); }
            return false;
        }
        public override int GetHashCode()
        {
            if (!string.IsNullOrWhiteSpace(Name)) { return Name.GetHashCode(); }
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return this.Name;
        }
    }
}
