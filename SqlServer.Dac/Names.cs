﻿using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Dac
{
    public static class Names
    {
        //public static ObjectIdentifier GetObjectIdentifier(this SchemaObjectName name, string assumedSchema = "dbo")
        //{
        //	if (name.Identifiers.Count == 1 && !string.IsNullOrWhiteSpace(assumedSchema))
        //	{
        //		return new ObjectIdentifier(new[] { assumedSchema, name.Identifiers.First().Value });
        //	}
        //	return new ObjectIdentifier(name.Identifiers.Select(x => x.Value));
        //}
        //public static ObjectIdentifier GetObjectIdentifier(this MultiPartIdentifier name, string assumedSchema = "dbo")
        //{
        //	if (name.Identifiers.Count == 1 && !string.IsNullOrWhiteSpace(assumedSchema))
        //	{
        //		return new ObjectIdentifier(new[] { assumedSchema, name.Identifiers.First().Value });
        //	}
        //	return new ObjectIdentifier(name.Identifiers.Select(x => x.Value));
        //}

        public static ObjectIdentifier GetFragmentObjectId(this TSqlFragment fragment, string assumedSchema = "dbo")
        {
            ObjectIdentifier ret = null;

            switch (fragment)
            {
                case CreateTableStatement createTable:
                    ret = createTable.SchemaObjectName.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateProcedureStatement createproc:
                    ret = createproc.ProcedureReference.Name.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateViewStatement createView:
                    ret = createView.SchemaObjectName.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateFunctionStatement createFunction:
                    ret = createFunction.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateTriggerStatement createTrigger:
                    ret = createTrigger.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateSequenceStatement createSequence:
                    ret = createSequence.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateDefaultStatement createDefault:
                    ret = createDefault.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateTypeUddtStatement createUddtType:
                    ret = createUddtType.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateTypeUdtStatement createUdtType:
                    ret = createUdtType.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateTypeTableStatement createTypeTable:
                    ret = createTypeTable.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateSynonymStatement createSynonymStatement:
                    ret = createSynonymStatement.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case ExecuteStatement executeStatement:
                    if (executeStatement.ExecuteSpecification.ExecutableEntity is ExecutableProcedureReference execProc)
                    {
                        ret = execProc.ProcedureReference.ProcedureReference.Name.GetObjectIdentifier(assumedSchema);
                    }
                    break;
                case TSqlScript script:
                case TSqlStatementSnippet frag:
                    ret = null;
                    break;
                default:
                    throw new ApplicationException("Unable to determine fragment type");
            }

            return ret;
        }
        public static ObjectIdentifier GetObjectIdentifier(this NamedTableReference table, string assumedSchema = "dbo")
        {
            var identifiers = table.SchemaObject.Identifiers;
            if (identifiers.Count == 1 && !string.IsNullOrWhiteSpace(assumedSchema))
            {
                return new ObjectIdentifier(new[] { assumedSchema, identifiers.First().Value });
            }
            return new ObjectIdentifier(identifiers.Skip(Math.Max(0, identifiers.Count - 2)).Select(x => x.Value));
        }
        public static ObjectIdentifier GetObjectIdentifier(this ProcedureReference proc, string assumedSchema = "dbo")
        {
            var identifiers = proc.Name.Identifiers;
            if (identifiers.Count == 1 && !string.IsNullOrWhiteSpace(assumedSchema))
            {
                return new ObjectIdentifier(new[] { assumedSchema, identifiers.First().Value });
            }
            return new ObjectIdentifier(identifiers.Skip(Math.Max(0, identifiers.Count - 2)).Select(x => x.Value));
        }
        public static ObjectIdentifier GetObjectIdentifier(this SchemaObjectName name, string assumedSchema = "dbo")
        {
            if (name.Identifiers.Count == 1 && !string.IsNullOrWhiteSpace(assumedSchema))
            {
                return new ObjectIdentifier(new[] { assumedSchema, name.Identifiers.First().Value });
            }
            return new ObjectIdentifier(name.Identifiers.Select(x => x.Value));
        }
        public static ObjectIdentifier GetObjectIdentifier(this MultiPartIdentifier name, string assumedSchema = "dbo")
        {
            if (name.Identifiers.Count == 1 && !string.IsNullOrWhiteSpace(assumedSchema))
            {
                return new ObjectIdentifier(new[] { assumedSchema, name.Identifiers.First().Value });
            }
            return new ObjectIdentifier(name.Identifiers.Select(x => x.Value));
        }
        public static IList<string> GetNameParts(this MultiPartIdentifier multiPartId)
        {
            return multiPartId.Identifiers.Select(i => i.Value).ToList();
        }
        public static IList<string> GetNameParts(this SchemaObjectName name)
        {
            return name.Identifiers.Select(i => i.Value).ToList();
        }
        public static IList<string> GetNameParts(this IList<Identifier> identifiers)
        {
            return identifiers.Select(i => i.Value).ToList();
        }
        public static string GetName(this ObjectIdentifier identifier)
        {
            return $"[{string.Join("].[", identifier.Parts.Select(x => x))}]";
        }
        public static string GetName(this SchemaObjectName name)
        {
            return $"[{string.Join("].[", name.Identifiers.Select(x => x.Value))}]";
        }
        public static string GetName(this MultiPartIdentifier name)
        {
            return $"[{string.Join("].[", name.Identifiers.Select(x => x.Value))}]";
        }
        public static string GetName(this IEnumerable<Identifier> identifiers)
        {
            return $"[{string.Join("].[", identifiers.Select(x => x.Value))}]";
        }
        public static string GetName(this IEnumerable<string> identifiers)
        {
            return $"[{string.Join("].[", identifiers)}]";
        }
        public static string GetName(this FunctionCall stmt)
        {
            var first = string.Empty;
            if (stmt.CallTarget is MultiPartIdentifierCallTarget mpi)
            {
                var ids = mpi.MultiPartIdentifier.Identifiers.Select(x => x.Value);
                first = $"[{string.Join("].[", ids)}].";
            }

            return $"{first}[{stmt.FunctionName.Value}]";
        }
        public static string GetName(this NamedTableReference table)
        {
            var identifiers = table.SchemaObject.Identifiers;

            return identifiers.Skip(Math.Max(0, identifiers.Count - 2)).Select(x => x.Value).GetName();
        }
        public static string GetName(this TableReference table, string assumedSchema = null)
        {
            if (table is NamedTableReference t1)
            {
                return t1.SchemaObject.GetName();
            }
            return string.Empty;
        }
        public static string GetName(this ColumnReferenceExpression column)
        {
            int cnt = column.MultiPartIdentifier.Identifiers.Count;
            if (cnt == 1)
            {
                return $"[{column.MultiPartIdentifier.Identifiers.First().Value}]";
            }
            var tname = "[" + string.Join("].[", column.MultiPartIdentifier.Identifiers.Take(cnt - 1).Select(i => i.Value)) + "]";

            return tname;
        }

        /// <summary>
        /// Gets a formatted element name
        /// </summary>
        public static string GetObjectName(this SqlRuleExecutionContext ruleExecutionContext, TSqlObject modelElement, ElementNameStyle style = ElementNameStyle.EscapedFullyQualifiedName)
        {
            // Get the element name using the built in DisplayServices. This provides a number of useful formatting options to
            // make a name user-readable
            var displayServices = ruleExecutionContext.SchemaModel.DisplayServices;
            string elementName = displayServices.GetElementName(modelElement, style);
            return elementName;
        }

        public static string GetConstraintName(this ConstraintDefinition constraint)
        {
            string ret = null;
            if (constraint == null) { return null; }

            switch (constraint)
            {
                case DefaultConstraintDefinition defaultConstraint:
                    ret = defaultConstraint.ConstraintIdentifier?.Value;
                    break;
                case UniqueConstraintDefinition uniqueConstraint:
                    ret = uniqueConstraint.ConstraintIdentifier?.Value;
                    break;
                case ForeignKeyConstraintDefinition foreignKeyConstraint:
                    ret = foreignKeyConstraint.ConstraintIdentifier?.Value;
                    break;
                case CheckConstraintDefinition checkConstraint:
                    ret = checkConstraint.ConstraintIdentifier?.Value;
                    break;
                case NullableConstraintDefinition nullableConstraint:
                    ret = null;
                    break;
                default:
                    throw new ApplicationException("Unable to determine constraint name");
            }

            return ret;
        }

        public static ObjectIdentifier GetObjectName(this TSqlFragment fragment, string assumedSchema = "dbo")
        {
            ObjectIdentifier ret = null;
            if (fragment == null) { return null; }

            switch (fragment)
            {
                case CreateTableStatement createTable:
                    ret = createTable.SchemaObjectName.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateProcedureStatement createProcedure:
                    ret = createProcedure.ProcedureReference.Name.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateViewStatement createView:
                    ret = createView.SchemaObjectName.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateFunctionStatement createFunction:
                    ret = createFunction.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateTriggerStatement createTrigger:
                    ret = createTrigger.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateSequenceStatement createSequence:
                    ret = createSequence.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateDefaultStatement createDefault:
                    ret = createDefault.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateTypeUddtStatement createTypeUddt:
                    ret = createTypeUddt.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateTypeUdtStatement createTypeUdt:
                    ret = createTypeUdt.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateTypeTableStatement createTypeTable:
                    ret = createTypeTable.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateTypeStatement createType:
                    ret = createType.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateSynonymStatement createSynonym:
                    ret = createSynonym.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateOrAlterFunctionStatement createOrAlterFunction:
                    ret = createOrAlterFunction.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateOrAlterTriggerStatement createOrAlterTrigger:
                    ret = createOrAlterTrigger.Name?.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateOrAlterViewStatement createOrAlterView:
                    ret = createOrAlterView.SchemaObjectName.GetObjectIdentifier(assumedSchema);
                    break;
                case CreateOrAlterProcedureStatement createOrAlterProcedure:
                    ret = createOrAlterProcedure.ProcedureReference.Name.GetObjectIdentifier(assumedSchema);
                    break;
                //case TSqlScript script:
                //case TSqlStatementSnippet frag:
                //    ret = null;
                //    break;
                default:
                    throw new ApplicationException("Unable to determine fragment type");
            }

            return ret;
        }
    }
}
