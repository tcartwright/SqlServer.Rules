using SqlServer.Rules.Globals;
using SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.ReferentialIntegrity
{
    public class ForeignKeyInfo
    {
        public string Name { get; set; }
        public ObjectIdentifier TableName { get; set; }
        public ObjectIdentifier ToTableName { get; set; }
        public IList<ObjectIdentifier> ColumnNames { get; set; }
        public IList<ObjectIdentifier> ToColumnNames { get; set; }

        public override string ToString()
        {
            var cols = new List<string>();
            var toCols = new List<string>();
            foreach(var col in ColumnNames)     
            {
                cols.Add(col.Parts.Last());
            }
            foreach (var col in ToColumnNames)
            {
                toCols.Add(col.Parts.Last());
            }
            //CONSTRAINT [FK_Table2_ToTable1] FOREIGN KEY ([Tbl1Id], [Tbl1Id2]) REFERENCES [Table1]([Table1Id], [Table1Id2])
            return $"CONSTRAINT {Name} FOREIGN KEY {TableName.GetName()} ({string.Join(", ", cols)}) REFERENCES  {ToTableName.GetName()} ({string.Join(", ", toCols)})";
        }

    }
}