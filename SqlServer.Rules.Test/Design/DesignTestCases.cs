using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Design;
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
    public class DesignTestCases : TestCasesBase
    {
        [TestMethod]
        public void TestAvoidNotForReplication()
        {
            var problems = GetTestCaseProblems(nameof(NotForReplication), NotForReplication.RuleId);

            var expected = 4;
            Assert.AreEqual(expected, problems.Count, $"Expected {expected} problem(s) to be found");

            Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "dbo_table2_trigger_1_not_for_replication.sql")));
            Assert.IsTrue(problems.Any(problem => Comparer.Equals(problem.SourceName, "fk_table2_table1_1_not_for_replication.sql")));
            Assert.IsTrue(problems.Count(problem => Comparer.Equals(problem.SourceName, "table3.sql")) == 2);

            Assert.IsTrue(problems.All(problem => Comparer.Equals(problem.Description, NotForReplication.Message)));
            Assert.IsTrue(problems.All(problem => problem.Severity == SqlRuleProblemSeverity.Warning));
        }
    }
}
