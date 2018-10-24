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
    public sealed class StartIdentity1000Rule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0010";
        public const string RuleDisplayName = "Start identity column used in a primary key with a seed of 1000 or higher.";
        private const string Message = RuleDisplayName;

        public StartIdentity1000Rule() : base(ModelSchema.Table)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(
                    typeof(CreateTableStatement)
                );

            var createTable = fragment as CreateTableStatement;
            //find the pk columns
            var pk = createTable.Definition.TableConstraints
                .FirstOrDefault(c => 
                    c.GetType() == typeof(UniqueConstraintDefinition) 
                    && ((UniqueConstraintDefinition)c).IsPrimaryKey);
            if (pk == null) { return problems; }

            //reduce our pks, just down to a list of their names
            var pkColNames = ((UniqueConstraintDefinition)pk).Columns
                .Select(c => c.Column.MultiPartIdentifier.Identifiers.GetName().ToUpper()).ToList();
            //try to find the identity column that is a member of the pk
            var identityColumn = createTable.Definition.ColumnDefinitions
                .FirstOrDefault(cd => pkColNames.Contains($"[{cd.ColumnIdentifier.Value.ToUpper()}]") && cd.IdentityOptions != null);

            if(identityColumn == null) { return problems; }
            //if the seed starts less than 1000, flag it
            if(((IntegerLiteral)identityColumn.IdentityOptions.IdentitySeed)?.GetValue() < 1000)
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj, identityColumn));
            }

            return problems;
        }
    }
}