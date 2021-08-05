﻿using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// 
    /// </summary>
    /// <FriendlyName></FriendlyName>
	/// <IsIgnorable>false</IsIgnorable>
	/// <ExampleMd></ExampleMd>
	/// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
    RuleDisplayName,
    Description = RuleDisplayName,
    Category = Constants.Design,
    RuleScope = SqlRuleScope.Element)]
    public sealed class UpdateWithoutWhereRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0018";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "UPDATE statement without row limiting conditions.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateWithoutWhereRule"/> class.
        /// </summary>
        public UpdateWithoutWhereRule() : base(ProgrammingSchemas)
        {
        }

        /// <summary>
        /// Performs analysis and returns a list of problems detected
        /// </summary>
        /// <param name="ruleExecutionContext">Contains the schema model and model element to analyze</param>
        /// <returns>
        /// The problems detected by the rule in the given element
        /// </returns>
        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);
            var visitor = new UpdateVisitor();

            fragment.Accept(visitor);

            foreach (var stmt in visitor.NotIgnoredStatements(RuleId))
            {
                if (stmt.UpdateSpecification.WhereClause != null
                    || !(stmt.UpdateSpecification.Target is NamedTableReference)) { continue; }

                var tableName = ((NamedTableReference)stmt.UpdateSpecification.Target).SchemaObject.Identifiers.Last().Value;

                if (stmt.UpdateSpecification.FromClause != null)
                {
                    var tableVisitor = new TableReferenceVisitor();
                    stmt.UpdateSpecification.FromClause.Accept(tableVisitor);

                    var table = tableVisitor.Statements.OfType<NamedTableReference>()
                        .FirstOrDefault(t => _comparer.Equals(t.Alias?.Value, tableName));
                    if (table != null)
                    {
                        tableName = table.SchemaObject.Identifiers.Last().Value;
                    }
                }
                if (!(tableName.StartsWith("#") || tableName.StartsWith("@")))
                {
                    problems.Add(new SqlRuleProblem(Message, sqlObj, stmt));
                }
            }

            return problems;
        }
    }
}