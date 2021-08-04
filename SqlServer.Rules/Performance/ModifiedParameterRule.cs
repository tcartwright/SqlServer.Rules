﻿using SqlServer.Rules.Globals;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using System;
using SqlServer.Dac;

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
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class ModifiedParameterRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The base rule identifier
        /// </summary>
        public const string BaseRuleId = "SRP0021";
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + BaseRuleId;
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid modification of parameters in a stored procedure prior to use in a select query.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiedParameterRule"/> class.
        /// </summary>
        public ModifiedParameterRule() : base(ModelSchema.Procedure)
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
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }
            var name = sqlObj.Name.GetName();

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));
            if (fragment.ScriptTokenStream == null) { return problems; }

            var parameterVisitor = new ParameterVisitor();
            var selectVisitor = new SelectStatementVisitor();
            fragment.Accept(parameterVisitor);
            fragment.Accept(selectVisitor);

            if (parameterVisitor.Count == 0 || selectVisitor.Count == 0) { return problems; }

            var setVisitor = new SetVariableStatementVisitor();
            fragment.Accept(setVisitor);

            foreach (var param in parameterVisitor.Statements.Select(p => p.VariableName.Value))
            {
                var selectsUsingParam = selectVisitor.Statements.GetSelectsUsingParameterInWhere(param).ToList();
                if (!selectsUsingParam.Any()) { continue; }

                var selectStartLine = selectsUsingParam.FirstOrDefault()?.StartLine;
                var getAssignmentSelects = selectVisitor.NotIgnoredStatements(RuleId)
                    .GetSelectsSettingParameterValue(param).Where(sel => sel.StartLine < selectStartLine);
                var setStatements = setVisitor.NotIgnoredStatements(RuleId)
                    .Where(set => _comparer.Equals(set.Variable.Name, param) && set.StartLine < selectStartLine);

                problems.AddRange(getAssignmentSelects.Select(x => new SqlRuleProblem(Message, sqlObj, x)));
                problems.AddRange(setStatements.Select(x => new SqlRuleProblem(Message, sqlObj, x)));
            }

            return problems;
        }

    }
}