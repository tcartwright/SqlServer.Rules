using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;
using SqlServer.Rules.ReferentialIntegrity;
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
    public sealed class ForeignKeysNeedIndexRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0004";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Columns on both sides of a foreign key should be indexed.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Columns on both sides of a foreign key should be indexed for performance reasons.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKeysNeedIndexRule"/> class.
        /// </summary>
        public ForeignKeysNeedIndexRule() : base(ModelSchema.ForeignKeyConstraint)
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

            var fkinfo = sqlObj.GetFKInfo();

            var table = ruleExecutionContext.SchemaModel.GetObject(Table.TypeClass, fkinfo.TableName, DacQueryScopes.All);
            if (!table.CheckForFkIndex(fkinfo.ColumnNames))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
                return problems;
            }

            table = ruleExecutionContext.SchemaModel.GetObject(Table.TypeClass, fkinfo.ToTableName, DacQueryScopes.All);
            if (!table.CheckForFkIndex(fkinfo.ToColumnNames))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
                return problems;
            }

            return problems;
        }
    }
}