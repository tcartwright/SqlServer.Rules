﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Dac.Visitors
{
    public class ActionStatementVisitor : BaseVisitor, IVisitor<DataModificationStatement>
    {
        public ObjectTypeFilter TypeFilter { get; set; } = ObjectTypeFilter.All;

        public IList<DataModificationStatement> Statements { get; } = new List<DataModificationStatement>();

        public int Count { get { return Statements.Count; } }

        public override void Visit(DataModificationStatement node)
        {
            switch (TypeFilter)
            {
                case ObjectTypeFilter.PermanentOnly:
                    if (!IsTempNode(node))
                    {
                        Statements.Add(node);
                    }

                    break;

                case ObjectTypeFilter.TempOnly:
                    if (IsTempNode(node))
                    {
                        Statements.Add(node);
                    }

                    break;

                default:
                    Statements.Add(node);
                    break;
            }
        }

        private bool IsTempNode(DataModificationStatement node)
        {
            var ret = false;
            NamedTableReference target = null;
            if (node is InsertStatement nodeInsert)
            {
                target = nodeInsert.InsertSpecification.Target as NamedTableReference;
            }
            else if (node is DeleteStatement nodeDelete)
            {
                target = nodeDelete.DeleteSpecification.Target as NamedTableReference;
            }
            else if (node is UpdateStatement nodeUpdate)
            {
                target = nodeUpdate.UpdateSpecification.Target as NamedTableReference;
            }

            if (target != null && target.SchemaObject.Identifiers.Any(x =>
                x.Value.Contains("#")
                || x.Value.Contains("@")))
            {
                ret = true;
            }
            return ret;
        }
    }
}