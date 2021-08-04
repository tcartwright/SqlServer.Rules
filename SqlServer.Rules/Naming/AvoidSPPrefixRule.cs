﻿using SqlServer.Rules.Globals;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;

namespace SqlServer.Rules.Naming
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SqlServer.Rules.Naming.NamingViolationRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Naming,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidSPPrefixRule : NamingViolationRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRN0002";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid 'sp_' prefix when naming stored procedures.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidSPPrefixRule"/> class.
        /// </summary>
        public AvoidSPPrefixRule() : base(
            RuleId,
            RuleDisplayName,
            "sp_",
            new[] { ModelSchema.Procedure },
            n => n.StartsWith)
        { }
    }
}