using Backtrace.Model.Database;
using Backtrace.Tests.DatabaseTests.Model;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Tests.DatabaseTests
{
    [TestFixture(Author = "Konrad Dysput", Category = "Database.Consistency")]
    public class SaveConsistencyTests : DatabaseTestBase
    {
        [TestCase(0, 5)]
        [TestCase(5, 10)]
        [TestCase(10, 5)]
        [TestCase(50, 5)]
        [TestCase(100, 10)]
        public void TestWriteConsistency(int successRate, int numberOfEntries)
        {
            int totalFails = 0;
            Random random = new Random();
            var entries = new List<BacktraceDatabaseEntry<object>>();
            for (int i = 0; i < numberOfEntries; i++)
            {
                var percentage = random.Next(0, 100);
                bool writeFail = percentage < successRate;
                var entry = GetEntry();
                ((MockBacktraceDatabaseWriter)entry.EntryWriter).writeFail = writeFail;
                var result = entry.Save();
                if (result)
                {
                    entries.Add(entry);
                }
                else
                {
                    totalFails++;
                }
            }
            Assert.AreEqual(totalFails, numberOfEntries - entries.Count);
        }


    }
}
