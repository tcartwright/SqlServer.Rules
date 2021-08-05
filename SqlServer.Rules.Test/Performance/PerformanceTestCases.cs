using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Performance;
using SqlServer.Rules.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.Rules.Tests.Performance
{
    [TestClass]
    [TestCategory("Performance")]
    public class PerformanceTestCases : TestCasesBase
    {
        [TestMethod]
        public void TestNonSARGablePattern()
        {
            var problems = GetTestCaseProblems(nameof(AvoidEndsWithOrContainsRule), AvoidEndsWithOrContainsRule.RuleId);

            Assert.AreEqual(2, problems.Count, "Expected 2 problem to be found");

            Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "nonsargable.sql")));
            Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "nonsargable2.sql")));

            Assert.IsTrue(problems.All(problem => Comparer.Equals(problem.Description, AvoidEndsWithOrContainsRule.Message)));
            Assert.IsTrue(problems.All(problem => problem.Severity == SqlRuleProblemSeverity.Warning));
        }
    }
}
