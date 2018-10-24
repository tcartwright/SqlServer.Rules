using SqlServer.Rules.Globals;
using SqlServer.Dac;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlServer.Rules.Performance
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Naming,
        RuleScope = SqlRuleScope.Element)]
    public sealed class GeneralNamingRules : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRN0007";
        public const string RuleDisplayName = "General naming rules.";


        public GeneralNamingRules() : base(ModelSchema.Table,
                ModelSchema.View,
                ModelSchema.ScalarFunction,
                ModelSchema.TableValuedFunction,
                ModelSchema.Procedure,

                ModelSchema.PrimaryKeyConstraint,
                ModelSchema.Index,
                ModelSchema.ForeignKeyConstraint,
                ModelSchema.DefaultConstraint,
                ModelSchema.CheckConstraint,
                ModelSchema.DmlTrigger
        )
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment();

            var name = sqlObj.Name.Parts.LastOrDefault();
            var objectType = sqlObj.ObjectType.Name;
            var parentObj = sqlObj.GetParent(DacQueryScopes.All);

            if (string.IsNullOrWhiteSpace(name))
            {
                problems.Add(new SqlRuleProblem($"{objectType} found without a name.", parentObj));
                return problems;
            }

            if (Regex.IsMatch(name, @"^\d"))
            {
                problems.Add(new SqlRuleProblem($"Name '{name}' starts with a number.", sqlObj, fragment));
            }

            if (Regex.IsMatch(name, @"^[^A-z0-9_]*$"))
            {
                problems.Add(new SqlRuleProblem($"Name '{name}' contains invalid characters. Please only use alphanumerics and underscores.", sqlObj, fragment));
            }

            string tableName = parentObj.Name.Parts.LastOrDefault();
            switch (objectType.ToLower())
            {
                case "primarykeyconstraint":
                    if (!Regex.IsMatch(name, $"^PK_{tableName}$", RegexOptions.IgnoreCase))
                    {
                        problems.Add(new SqlRuleProblem($"Primary Key '{name}' does not follow the company naming standard. Please use the name PK_{tableName}.", sqlObj, fragment));
                    }
                    break;
                case "index":
                    if (!Regex.IsMatch(name, $@"^IX\d{{2}}_{tableName}$", RegexOptions.IgnoreCase))
                    {
                        problems.Add(new SqlRuleProblem($"Index '{name}' does not follow the company naming standard. Please use the name IX##_{tableName}.", sqlObj, fragment));
                    }
                    break;
                case "foreignkeyconstraint":
                    if (!Regex.IsMatch(name, $@"^FK\d{{2}}_{tableName}$", RegexOptions.IgnoreCase))
                    {
                        problems.Add(new SqlRuleProblem($"Foreign Key '{name}' does not follow the company naming standard. Please use the format FK##_{tableName}.", sqlObj, fragment));
                    }
                    break;
                case "checkconstraint":
                    if (!Regex.IsMatch(name, $@"^CK\d{{2}}_{tableName}$", RegexOptions.IgnoreCase))
                    {
                        problems.Add(new SqlRuleProblem($"Check Constraint '{name}' does not follow the company naming standard. Please use the format CK##_{tableName}*.", sqlObj, fragment));
                    }
                    break;
                case "defaultconstraint":
                    var columnName = GetReferencedName(sqlObj, DefaultConstraint.TargetColumn, "Column");
                    //allow two formats for this one
                    if (!Regex.IsMatch(name, $@"^DF(\d{{2}})?_{tableName}_{columnName}$|^DF\d{{2}}_{tableName}$", RegexOptions.IgnoreCase))
                    {
                        problems.Add(new SqlRuleProblem($"Constraint '{name}' does not follow the company naming standard. Please use the name DF##_{tableName}.", sqlObj, fragment));
                    }
                    break;
            }

            return problems;
        }

        private string GetReferencedName(TSqlObject sqlObj, ModelRelationshipClass relation = null, string typeToLookFor = "Table")
        {
            if (relation == null)
            {
                return sqlObj.GetReferenced().FirstOrDefault(o => _comparer.Equals(o.ObjectType.Name, typeToLookFor)).Name.Parts.LastOrDefault();
            }
            {
                return sqlObj.GetReferenced(relation).FirstOrDefault(o => _comparer.Equals(o.ObjectType.Name, typeToLookFor)).Name.Parts.LastOrDefault();
            }
        }


    }
}
