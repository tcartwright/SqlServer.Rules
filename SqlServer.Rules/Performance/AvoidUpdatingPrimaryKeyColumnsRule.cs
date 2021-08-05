using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Performance
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
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidUpdatingPrimaryKeyColumnsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0017";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid updating columns that are part of the primary key.  (Halloween Protection)";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidUpdatingPrimaryKeyColumnsRule"/> class.
        /// </summary>
        public AvoidUpdatingPrimaryKeyColumnsRule() : base(ProgrammingSchemas)
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
            var model = ruleExecutionContext.SchemaModel;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);

            var updateVisitor = new UpdateVisitor();
            fragment.Accept(updateVisitor);
            foreach (var update in updateVisitor.NotIgnoredStatements(RuleId))
            {
                if (!(update.UpdateSpecification.Target is NamedTableReference target) || target.GetName().Contains("#")) { continue; }

                //we have an aliased table we need to find out what the real table is so we can look up its columns
                if (update.UpdateSpecification.FromClause != null)
                {
                    var namedTableVisitor = new NamedTableReferenceVisitor() { TypeFilter = ObjectTypeFilter.PermanentOnly };
                    update.UpdateSpecification.FromClause.Accept(namedTableVisitor);

                    target = namedTableVisitor.Statements
                        .FirstOrDefault(t => _comparer.Equals(t.Alias?.Value, target.SchemaObject.Identifiers.LastOrDefault()?.Value));
                    if (target == null) { continue; }
                }

                var targetSqlObj = model.GetObject(Table.TypeClass, target.GetObjectIdentifier(), DacQueryScopes.All);
                if (targetSqlObj == null) { continue; }

                var pk = targetSqlObj.GetReferencing(PrimaryKeyConstraint.Host, DacQueryScopes.UserDefined).FirstOrDefault();
                if (pk == null) { continue; }
                var primaryKeyColumns = pk.GetReferenced(PrimaryKeyConstraint.Columns, DacQueryScopes.All);

                var hasOffense = update.UpdateSpecification.SetClauses.OfType<AssignmentSetClause>().Any(setClause =>
                {
                    if (setClause.Column?.MultiPartIdentifier == null) { return false; }
                    return primaryKeyColumns.Any(pkc => pkc.Name.CompareTo(setClause.Column?.MultiPartIdentifier) >= 5);
                });

                if (hasOffense)
                {
                    problems.Add(new SqlRuleProblem(Message, sqlObj, update));
                }
            }
            return problems;
        }
    }
}