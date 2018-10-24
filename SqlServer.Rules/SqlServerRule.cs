//using SqlServer.Rules.Globals;
//using Microsoft.SqlServer.Dac.CodeAnalysis;
//using Microsoft.SqlServer.Dac.Model;
//using Microsoft.SqlServer.TransactSql.ScriptDom;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using SqlServer.Dac.Visitors;

//namespace $rootnamespace$
//{
//	[ExportCodeAnalysisRule(Ruleid,
//    RuleDisplayName,
//    Description = RuleDisplayName,
//    Category = Constants,
//    RuleScope = SqlRuleScope.Element)]
//    public sealed class $safeitemrootname$ : BaseSqlCodeAnalysisRule
//    {
//        public const string BaseRuleId = "SRD0016";
//        public const string RuleId = Constants.RuleNameSpace + BaseRuleId;
//		  public const string RuleId = Constants.RuleNameSpace + "SR";
//        public const string RuleDisplayName = "";
//        private const string Message = "";

//        public $safeitemname$() : base(ModelSchema.???){}

//        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
//        {
//            var problems = new List<SqlRuleProblem>();
//            var sqlObj = ruleExecutionContext.ModelElement;

//            if (sqlObj == null || sqlObj.IsWhiteListed())
//                return problems;

//            var fragment = ruleExecutionContext.ScriptFragment.GetFragment();
//            var visitor = new ;

//            fragment.Accept(visitor);

//            var offenders =
//                from o in visitor.Statements
//                where

//                select o;

//            problems.AddRange(offenders.Select(o => new SqlRuleProblem(Message, sqlObj, o)));

//            return problems;
//        }
//	}
//}