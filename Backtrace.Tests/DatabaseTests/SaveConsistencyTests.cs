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
        public override void Setup()
        {
            base.Setup();
            //mock exception for saving entry json
            _entryWriter.Setup(n => n.SaveTemporaryFile(It.Is<string>(m => m.EndsWith("entry.json")), It.IsAny<byte[]>()))
                .Throws<UnauthorizedAccessException>();

            //mock saving other files
            _entryWriter.Setup(n => n.SaveTemporaryFile(It.Is<string>(m => !m.EndsWith("entry.json")), It.IsAny<byte[]>()));
        }
    }
}
