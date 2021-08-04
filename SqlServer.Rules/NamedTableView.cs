﻿using SqlServer.Rules.Globals;
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
    /// <summary>
    /// 
    /// </summary>
    public class NamedTableView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedTableView"/> class.
        /// </summary>
        /// <param name="namedTable">The named table.</param>
        public NamedTableView(NamedTableReference namedTable)
        {
            this.Name = namedTable.GetName("dbo");
            if (namedTable.Alias != null && !HasAlias(namedTable.Alias.Value))
            {
                Aliases.Add(namedTable.Alias.Value);
            }
        }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the aliases.
        /// </summary>
        /// <value>
        /// The aliases.
        /// </value>
        public IList<string> Aliases { get; set; } = new List<string>();

        /// <summary>
        /// Names to identifier.
        /// </summary>
        /// <returns></returns>
        public ObjectIdentifier NameToId()
        {
            return new ObjectIdentifier((Name ?? string.Empty).Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Determines whether the specified alias has alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns>
        ///   <c>true</c> if the specified alias has alias; otherwise, <c>false</c>.
        /// </returns>
        public bool HasAlias(string alias)
        {
            return Aliases.Any(a => a.StringEquals(alias));
        }

        /// <summary>
        /// Determines whether the specified identifier is match.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        ///   <c>true</c> if the specified identifier is match; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMatch(ObjectIdentifier id)
        {
            var tableNameOrAlias = new ObjectIdentifier(id.Parts.Take(id.Parts.Count - 1));

            return this.NameToId().CompareTo(tableNameOrAlias) >= 5
                || HasAlias(tableNameOrAlias.Parts.First());
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is NamedTableView tmp)) { return false; }
            if (!string.IsNullOrWhiteSpace(Name)) { return Name.StringEquals(tmp.Name); }
            return false;
        }
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            if (!string.IsNullOrWhiteSpace(Name)) { return Name.GetHashCode(); }
            return base.GetHashCode();
        }
        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
