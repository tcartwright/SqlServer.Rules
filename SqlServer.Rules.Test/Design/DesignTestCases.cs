using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Test;
using System;
using System.Linq;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Performance;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using System.Text;
using System.Collections.Generic;
using SqlServer.Rules.Design;

namespace SqlServer.Rules.Tests.Performance
{
    [TestClass]
    [TestCategory("Performance")]
    public class DesignTestCases : TestCasesBase
    {
        [TestMethod]
        public void TestAvoidNotForReplication()
        {
            var problems = GetTestCaseProblems("AvoidNotForReplication", AvoidNotForReplication.RuleId);

            var expected = 4;
            Assert.AreEqual(expected, problems.Count, $"Expected {expected} problem(s) to be found");

            Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "dbo_table2_trigger_1_not_for_replication.sql")));
            Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "fk_table2_table1_1_not_for_replication.sql")));
            Assert.IsTrue(problems.Count(problem => Comparer.Equals(problem.SourceName, "table3.sql")) == 2);

            Assert.IsTrue(problems.All(problem => Comparer.Equals(problem.Description, AvoidNotForReplication.Message)));
            Assert.IsTrue(problems.All(problem => problem.Severity == SqlRuleProblemSeverity.Warning));
        }
    }
}
