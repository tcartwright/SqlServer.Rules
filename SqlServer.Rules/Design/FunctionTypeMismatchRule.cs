﻿using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
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
	/// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class FunctionTypeMismatchRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0043";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "The arguments of the function '{0}' are not of the same datatype.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionTypeMismatchRule"/> class.
        /// </summary>
        public FunctionTypeMismatchRule()
            : base(ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(
                typeof(CreateProcedureStatement),
                typeof(CreateFunctionStatement)
            );

            var variablesVisitor = new VariablesVisitor();
            fragment.Accept(variablesVisitor);
            var variables = variablesVisitor.GetVariables();

            var queries = new QueryStatementVisitor();
            fragment.Accept(queries);

            foreach (var query in queries.Statements)
            {
                var visitor = new FunctionCallVisitor("isnull", "coalesce");
                query.Accept(visitor);

                if (!visitor.Statements.Any()) { continue; }

                var columnDataTypes = new Dictionary<NamedTableView, IDictionary<string, DataTypeView>>();
                query.GetTableColumnDataTypes(columnDataTypes, ruleExecutionContext.SchemaModel);

                foreach (var func in visitor.Statements)
                {
                    var paramTypes = new List<string>();
                    foreach (var parameter in func.Parameters)
                    {
                        if (parameter is ColumnReferenceExpression colRef)
                        {
                            var dtView = columnDataTypes.GetDataTypeView(colRef);
                            if (dtView != null) { paramTypes.Add(dtView.DataType); }
                        }
                        else
                        {
                            paramTypes.Add(GetDataType(parameter, variables));
                        }
                    }
                    if (!paramTypes.All(x => _comparer.Equals(x, paramTypes.First())))
                    {
                        var funcName = func.FunctionName.Value.ToUpper();
                        problems.Add(new SqlRuleProblem(string.Format(Message, funcName), sqlObj, func));
                    }
                }
            }

            return problems;
        }
    }
}