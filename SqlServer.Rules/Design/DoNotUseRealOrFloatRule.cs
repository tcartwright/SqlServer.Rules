using SqlServer.Rules.Globals;
using SqlServer.Dac;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class DoNotUseRealOrFloatRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0046";
        public const string RuleDisplayName = "Do not use the real or float data types for parameters or columns as they are approximate value data types.";
        private const string Message = RuleDisplayName;

        public DoNotUseRealOrFloatRule() : base(ModelSchema.Table, ModelSchema.Procedure, ModelSchema.View)
        {
        }

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