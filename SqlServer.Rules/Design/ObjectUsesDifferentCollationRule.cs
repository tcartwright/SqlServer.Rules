using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
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
    public sealed class ObjectUsesDifferentCollationRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0053";
        public const string RuleDisplayName = "Object has different collation than the rest of the database. Try to avoid using a different collation unless by design.";
        private const string MessageColumn = "This column has a different collation than the rest of the database. Try to avoid using a different collation unless by design.";
        private const string MessageDefault = "This default constraint has a different collation than the rest of the database. Try to avoid using a different collation unless by design.";

        public ObjectUsesDifferentCollationRule() : base(ModelSchema.Table)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            List<SqlRuleProblem> problems = new List<SqlRuleProblem>();
            TSqlObject sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateTableStatement));
            var objName = sqlObj.Name.GetName();

            var dbCollation = ruleExecutionContext.SchemaModel.CopyModelOptions().Collation;

            ColumnDefinitionVisitor columnVisitor = new ColumnDefinitionVisitor();
            fragment.Accept(columnVisitor);

            var statements = columnVisitor.NotIgnoredStatements(RuleId).ToList();

            var columnOffenders = statements.Where(col => 
                (col.Collation != null && !_comparer.Equals(col.Collation?.Value, dbCollation))
            ).ToList();

            problems.AddRange(columnOffenders.Select(col => new SqlRuleProblem(MessageColumn, sqlObj, col)));

            var defaultOffenders = statements.Where(col => {
                var collation = (col.DefaultConstraint?.Expression as PrimaryExpression)?.Collation;

                return collation != null && !_comparer.Equals(collation.Value, dbCollation);
            }).ToList();

            problems.AddRange(defaultOffenders.Select(col => new SqlRuleProblem(MessageDefault, sqlObj, col.DefaultConstraint)));

            return problems;
        }

        private decimal GetDataTypeLength(ColumnDefinition col)
        {
            var dataType = col.DataType as SqlDataTypeReference;
            if (dataType == null) { return 0; }
            return dataType.GetDataTypeParameters().FirstOrDefault();
        }
    }
}