using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlServer.Rules.Tests
{
    [TestClass]
    public class TestCasesBase
    {
        protected const SqlServerVersion SqlVersion = SqlServerVersion.Sql130;
        protected StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
    }
}