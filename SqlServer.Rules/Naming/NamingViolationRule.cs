using SqlServer.Rules.Globals;
using SqlServer.Dac;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;

namespace SqlServer.Rules.Naming
{
    public class NamingViolationRule : BaseSqlCodeAnalysisRule
    {
        private readonly string _RuleId;
        protected readonly string Message;
        protected readonly string BadCharacters;
        protected readonly Func<string, Predicate<string>> PartialPredicate;

        public NamingViolationRule(
            string ruleId,
            string message,
            string badPrefix,
            IList<ModelTypeClass> appliesTo,
            Func<string, Predicate<string>> predicate)
        {
            _RuleId = ruleId;
            Message = message;
            BadCharacters = badPrefix;
            SupportedElementTypes = appliesTo;
            PartialPredicate = predicate;
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
                return problems;

            var name = ruleExecutionContext.GetObjectName(sqlObj, ElementNameStyle.SimpleName).ToLower();
            var fragment = ruleExecutionContext.GetFragment();

            if (PartialPredicate(name)(BadCharacters) 
                && Ignorables.ShouldNotIgnoreRule(fragment.ScriptTokenStream, _RuleId, fragment.StartLine))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }

            return problems;
        }
    }
}