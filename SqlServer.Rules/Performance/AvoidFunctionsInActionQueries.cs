using SqlServer.Rules.Globals;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SqlServer.Rules.Performance
{
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidFunctionsInActionQueries : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0010";
        public const string RuleDisplayName = "Avoid the use of user defined functions with UPDATE/INSERT/DELETE statements. (Halloween Protection)";
        public const string Message = RuleDisplayName;

        public AvoidFunctionsInActionQueries() : base(ProgrammingSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }
            var model = ruleExecutionContext.SchemaModel;
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);

            var visitor = new DataModificationStatementVisitor();
            fragment.Accept(visitor);

            var modelFunctions = model.GetObjects(DacQueryScopes.UserDefined, new[] { ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction });

            foreach (var stmt in visitor.Statements)
            {
                var functionCallVisitor = new FunctionCallVisitor();
                stmt.Accept(functionCallVisitor);

                foreach (var functionCall in functionCallVisitor.NotIgnoredStatements(RuleId))
                {
                    var createFunctionVisitor = new CreateFunctionVisitor();
                    IList<ParseError> parseErrors;
                    TSqlFragment fnFragment;

                    var fnName = functionCall.GetName();
                    var modelFunction = modelFunctions.FirstOrDefault(mf => _comparer.Equals(mf.Name.GetName(), fnName));
                    if (modelFunction == null) { continue; }

                    //we need to parse the sql into a fragment, so we can use the visitors on it
                    fnFragment = modelFunction.GetFragment(out parseErrors);
                    fnFragment.Accept(createFunctionVisitor);

                    if (!createFunctionVisitor.Statements.Any(crfn => crfn.Options != null && crfn.Options.Any(o => o.OptionKind == FunctionOptionKind.SchemaBinding)))
                    {
                        problems.Add(new SqlRuleProblem(Message, sqlObj, functionCall));
                    }
                }
            }


            return problems;
        }
    }
}