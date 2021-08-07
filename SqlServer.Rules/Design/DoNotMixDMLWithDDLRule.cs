using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    /// <summary>Do not mix DML with DDL statements. Group DDL statements at the beginning of
    /// procedures followed by DML statements</summary>
    /// <FriendlyName>Mixed DDL and DML</FriendlyName>
	/// <IsIgnorable>false</IsIgnorable>
	/// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The rule checks stored procedures, triggers and functions for having a DDL statements mixed
    /// between DML statements. If DDL operations are performed within a procedure or batch, the
    /// procedure or batch is recompiled when it encounters the first subsequent DML operation
    /// affecting the table involved in the DDL.
    /// </remarks>
	/// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class DoNotMixDMLWithDDLRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0057";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Do not mix DML with DDL statements. Group DDL statements at the beginning of procedures followed by DML statements.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoNotMixDMLWithDDLRule"/> class.
        /// </summary>
        public DoNotMixDMLWithDDLRule() : base(ModelSchema.Procedure)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));

            var typesVisitor = new TypesVisitor(
                typeof(SelectStatement),
                typeof(UpdateStatement),
                typeof(DeleteStatement),
                typeof(InsertStatement),
                typeof(CreateTableStatement)
            );
            fragment.Accept(typesVisitor);

            if (typesVisitor.Count == 0) { return problems; }

            var offenders = typesVisitor.Statements
                .SkipWhile(t => t.GetType() == typeof(CreateTableStatement))
                .Where(t1 => t1.GetType() == typeof(CreateTableStatement));

            problems.AddRange(offenders.Select(t => new SqlRuleProblem(Message, sqlObj, t)));

            return problems;
        }
    }
}