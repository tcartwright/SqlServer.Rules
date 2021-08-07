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
<<<<<<< HEAD:SqlServer.Rules/Design/AvoidUseOfIdentityFunction.cs
    /// <summary>
    /// 
    /// </summary>
    /// <FriendlyName></FriendlyName>
=======
    /// <summary>Avoid using cursors.</summary>
    /// <FriendlyName>Cursor Use</FriendlyName>
>>>>>>> Add XML Docs to Design Rule Classes (2):SqlServer.Rules/Design/DoNotUseIdentityFunction.cs
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The rule identifies CURSOR usage inside the code and notifies for cursor declarations.
    /// Review your code and consider using a set-based solution instead of the cursor/iterative
    /// solution for the given task.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public class AvoidUseOfIdentityFunction : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0056";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Use OUTPUT or SCOPE_IDENTITY() instead of @@IDENTITY.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidUseOfIdentityFunction"/> class.
        /// </summary>
        public AvoidUseOfIdentityFunction() : base(ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction, ModelSchema.DmlTrigger)
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

            if (sqlObj == null)
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement), typeof(CreateFunctionStatement), typeof(CreateTriggerStatement));

            var visitor = new GlobalVariableExpressionVisitor("@@identity");

            fragment.Accept(visitor);

            problems.AddRange(visitor.NotIgnoredStatements(RuleId).Select(s => new SqlRuleProblem(Message, sqlObj, s)));

            return problems;
        }
    }
}