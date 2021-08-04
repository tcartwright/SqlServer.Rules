﻿using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SqlServer.Rules.Performance
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
		Category = Constants.Performance,
		RuleScope = SqlRuleScope.Element)]
	public sealed class DataTypesOnBothSidesOfEqualityRule : BaseSqlCodeAnalysisRule
	{
		/// <summary>
		/// The rule identifier
		/// </summary>
		public const string RuleId = Constants.RuleNameSpace + "SRP0016";
		/// <summary>
		/// The rule display name
		/// </summary>
		public const string RuleDisplayName = "Data types on both sides of an equality check should be the same in the where clause. (Sargeable)";
		/// <summary>
		/// The message
		/// </summary>
		public const string Message = RuleDisplayName;

		/// <summary>
		/// Initializes a new instance of the <see cref="DataTypesOnBothSidesOfEqualityRule"/> class.
		/// </summary>
		public DataTypesOnBothSidesOfEqualityRule() : base(ProgrammingAndViewSchemas)
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
			var sqlObj = ruleExecutionContext.ModelElement; //proc / view / function
			if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

			try
			{
				var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);
				//get the combined parameters and declare variables into one searchable list
				var variablesVisitor = new VariablesVisitor();
				fragment.AcceptChildren(variablesVisitor);
				var variables = variablesVisitor.GetVariables();

				var selectStatementVisitor = new SelectStatementVisitor();
				fragment.Accept(selectStatementVisitor);
				foreach (var select in selectStatementVisitor.Statements)
				{
					if (select.QueryExpression is QuerySpecification query && query.WhereClause != null && query.FromClause != null)
					{
						var booleanComparisonVisitor = new BooleanComparisonVisitor();
						query.WhereClause.Accept(booleanComparisonVisitor);
						var comparisons = booleanComparisonVisitor.Statements
							.Where(x =>
								(x.FirstExpression is ColumnReferenceExpression
								|| x.SecondExpression is ColumnReferenceExpression));

						if (!comparisons.Any()) { continue; }
						var dataTypesList = new Dictionary<NamedTableView, IDictionary<string, DataTypeView>>();
						select.GetTableColumnDataTypes(dataTypesList, ruleExecutionContext.SchemaModel);

						foreach (var comparison in comparisons)
						{
							var col1 = comparison.FirstExpression as ColumnReferenceExpression;
							var col2 = comparison.SecondExpression as ColumnReferenceExpression;
							var datatype1 = string.Empty;
							var datatype2 = string.Empty;

							if (col1 != null)
							{
								var dtView = dataTypesList.GetDataTypeView(col1);
								if (dtView != null) { datatype1 = dtView.DataType; }
							}
							else
							{
								datatype1 = GetDataType(sqlObj,
									query,
									comparison.FirstExpression,
									variables,
									ruleExecutionContext.SchemaModel);
							}

							if (col2 != null)
							{
								var dtView = dataTypesList.GetDataTypeView(col2);
								if (dtView != null) { datatype2 = dtView.DataType; }
							}
							else
							{
								datatype2 = GetDataType(sqlObj,
									query,
									comparison.SecondExpression,
									variables,
									ruleExecutionContext.SchemaModel);
							}

							if (string.IsNullOrWhiteSpace(datatype1) || string.IsNullOrWhiteSpace(datatype2)) { continue; }

							//when checking the numeric literal I am not sure if it is a bit or tinyint.
							if ((_comparer.Equals(datatype1, "bit") && _comparer.Equals(datatype2, "tinyint")) 
								|| (_comparer.Equals(datatype1, "tinyint") && _comparer.Equals(datatype2, "bit"))) { continue; }

							if (!_comparer.Equals(datatype1, datatype2))
							{
								problems.Add(new SqlRuleProblem(Message, sqlObj, comparison));
							}
						}
					}
				}

			}
			catch (System.Exception ex)
			{
				//TODO: PROPERLY LOG THIS ERROR
				Debug.WriteLine(ex.ToString());
				//throw;
			}

			return problems;
		}
	}
}