using SqlServer.Rules.Globals;
using SqlServer.Dac;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidWidePKsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0003";
        public const string RuleDisplayName = "Primary Keys should avoid using GUIDS or wide VARCHAR columns.";

        private const string GuidMessage = "Guids should not be used as the first column in a primary key.";
        private const string WideVarcharMessage = "Wide (n)varchar columns should not be used in primary keys.";

        public AvoidWidePKsRule() : base(ModelSchema.PrimaryKeyConstraint)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            List<SqlRuleProblem> problems = new List<SqlRuleProblem>();
            TSqlObject sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var objName = sqlObj.Name.GetName();

            var columns = sqlObj.GetReferenced(DacQueryScopes.All).Where(x => x.ObjectType == Column.TypeClass).ToList();
            if (!columns.Any())
            {
                return problems;
            }

            var keyColumn = columns.FirstOrDefault();
            var dataType = keyColumn.GetReferenced(Column.DataType, DacQueryScopes.All).FirstOrDefault();
            if (dataType == null || dataType.Name == null)
            {
                return problems;
            }

            var dataTypeName = dataType.Name.Parts.Last();
            if (_comparer.Equals(dataTypeName, "uniqueidentifier"))
            {
                problems.Add(new SqlRuleProblem(GuidMessage, sqlObj));
            }

            if (columns.Any(col =>
            {
                var len = col.GetProperty<int>(Column.Length);
                dataTypeName = col.GetReferenced(Column.DataType).First().Name.Parts.Last();
                return (_comparer.Equals(dataTypeName, "varchar") && len > 50)
                    || (_comparer.Equals(dataTypeName, "nvarchar") && len > 100);
            }))
            {
                problems.Add(new SqlRuleProblem(WideVarcharMessage, sqlObj));
            }

            return problems;
        }
    }
}