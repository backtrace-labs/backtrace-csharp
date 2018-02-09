using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
namespace Backtrace.Tests.ClientTests
{
    /// <summary>
    /// This test use app.config from Backtrace.Tests project. 
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Client.Credentials", Description = "Test client credential")]
    public class TestClientCredentials
    {
        [TestCase("EmptyCredentials")]
        [Test(Author = "Konrad Dysput", Description = "Test empty values in configuration section")]
        public void TestInvalidSectionName(string sectionName)
        {
            Assert.Throws<InvalidOperationException>(() => new BacktraceClient(sectionName));
        }

        [TestCase("InvalidUrl")]
        [Test(Author = "Konrad Dysput", Description = "Test invalid api url section")]
        public void TestInvalidUrlArgument(string sectionName)
        {
            //if programmer pass invalid url should throw UriFormatException
            //if programmer pass null or empty string as token should throw ArgumentNullException
            Assert.Throws<UriFormatException>(() => new BacktraceClient(sectionName));
            Assert.Throws<ArgumentException>(() => new BacktraceClient(new BacktraceCredentials("https://test.backtrace.io", string.Empty)));
        }


        [TestCase("EmptyToken")]
        [Test(Author = "Konrad Dysput", Description = "Test invalid token section")]
        public void TestInvalidTokenArgument(string sectionName)
        {
            Assert.Throws<ArgumentNullException>(() => new BacktraceClient(sectionName));
        }

        [TestCase("ValidCredentials")]
        [Test(Author = "Konrad Dysput", Description = "Test invalid configuration section")]
        public void TestValidSectionConfiguration(string sectionName)
        {
            Assert.DoesNotThrow(() => new BacktraceClient(sectionName));
        }
    }
}
