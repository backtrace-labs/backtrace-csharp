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
        public void TestWriteConsistency(int successRate, int numberOfRecords)
        {
            int totalFails = 0;
            Random random = new Random();
            var records = new List<BacktraceDatabaseRecord<object>>();
            for (int i = 0; i < numberOfRecords; i++)
            {
                var percentage = random.Next(0, 100);
                bool writeFail = percentage < successRate;
                var record = GetRecord();
                ((MockBacktraceDatabaseWriter)record.RecordWriter).writeFail = writeFail;
                var result = record.Save();
                if (result)
                {
                    records.Add(record);
                }
                else
                {
                    totalFails++;
                }
            }
            Assert.AreEqual(totalFails, numberOfRecords - records.Count);
        }


    }
}
