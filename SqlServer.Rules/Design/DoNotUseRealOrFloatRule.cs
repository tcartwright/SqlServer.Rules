﻿using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Rules.Globals;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    /// <summary>Do not use the real or float data types as they are approximate value data types.</summary>
    /// <FriendlyName>Use of approximate data type</FriendlyName>
	/// <IsIgnorable>false</IsIgnorable>
	/// <ExampleMd></ExampleMd>
    /// <remarks>
    /// <c>REAL</c> and <c>FLOAT</c> do not store accurate values. They store <b>Approximate</b> values.
	/// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class DoNotUseRealOrFloatRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0046";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Do not use the real or float data types for parameters or columns as they are approximate value data types.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoNotUseRealOrFloatRule"/> class.
        /// </summary>
        public DoNotUseRealOrFloatRule() : base(ModelSchema.Table, ModelSchema.Procedure, ModelSchema.View)
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
            List<SqlRuleProblem> problems = new List<SqlRuleProblem>();
            TSqlObject sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(
                typeof(CreateTableStatement),
                typeof(CreateProcedureStatement),
                typeof(CreateViewStatement)
            );

            if (sqlObj.ObjectType == ModelSchema.Procedure)
            {
                var parameters = sqlObj.GetReferenced().Where(x => x.ObjectType == ModelSchema.Parameter);

                foreach (var parameter in parameters)
                {
                    var datatypes = parameter.GetReferenced(Parameter.DataType).Where(t =>
                    {
                        var name = t.Name?.Parts.LastOrDefault()?.ToLower();
                        return name == "real" || name == "float";
                    });
                    if (datatypes.Any())
                    {
                        problems.Add(new SqlRuleProblem(Message, parameter));
                    }
                }
            }
            else //tables, views
            {
                var columns = sqlObj.GetReferenced().Where(x => x.ObjectType == ModelSchema.Column);

                foreach (var column in columns)
                {
                    var datatypes = column.GetReferenced(Column.DataType).Where(t =>
                    {
                        var name = t.Name?.Parts.LastOrDefault()?.ToLower();
                        return name == "real" || name == "float";
                    });
                    if (datatypes.Any())
                    {
                        problems.Add(new SqlRuleProblem(Message, column));
                    }
                }
            }

            return problems;
        }
    }
}