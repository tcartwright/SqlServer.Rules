using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Test;
using System;
using System.Linq;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Performance;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using System.Text;
using System.Collections.Generic;

namespace SqlServer.Rules.Tests
{
    [TestClass]
    public class RulesTestCases
    {
        public const SqlServerVersion SqlVersion = SqlServerVersion.Sql120;
        private StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestNonSARGablePattern()
        {
            string testCases = "AvoidNonsargableLikePattern";

            using (BaselineSetup test = new BaselineSetup(TestContext, testCases, new TSqlModelOptions(), SqlVersion))
            {
                try
                {
                    test.RunTest(AvoidEndsWithOrContainsRule.RuleId, (result, problemString) =>
                    {
                        var problems = result.Problems;

                        Assert.AreEqual(2, problems.Count, "Expected 2 problem to be found");

                        Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "nonsargable.sql")));
                        Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "nonsargable2.sql")));

                        Assert.IsTrue(problems.All(problem => Comparer.Equals(problem.Description, AvoidEndsWithOrContainsRule.Message)));

                        Assert.AreEqual(problems[0].Severity, SqlRuleProblemSeverity.Warning);
                    });
                }
                catch (AssertFailedException)
                {
                    throw;
                }
                catch(Exception ex)
                {
                    Assert.Fail($"Exception thrown in '{nameof(TestNonSARGablePattern)}' for test cases '{testCases}': {ex.Message}");
                }
            }
        }
    }
}
