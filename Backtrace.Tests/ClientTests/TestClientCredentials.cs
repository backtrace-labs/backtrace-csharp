using Backtrace.Model;
using NUnit.Framework;
using System;
namespace Backtrace.Tests.ClientTests
{
    /// <summary>
    /// This test use app.config from Backtrace.Tests project. 
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Client.Credentials", Description = "Test client credential")]
    public class TestClientCredentials
    {

#if NET35 || NET48
        [TestCase("EmptyCredentials")]
        [Test(Author = "Konrad Dysput", Description = "Test empty values in configuration section")]
        public void TestInvalidSectionName(string sectionName)
        {
            Assert.Throws<InvalidOperationException>(() => new BacktraceClient(sectionName));
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
#endif

        [TestCase("https://backtrace.sp.backtrace.io")]
        [TestCase("http://backtrace.sp.backtrace.io")]
        [TestCase("http://backtrace.sp.backtrace.io:6098")]
        [TestCase("http://backtrace.sp.backtrace.io:7777")]
        [TestCase("http://backtrace.sp.backtrace.io:7777/")]
        [TestCase("http://backtrace.sp.backtrace.io/")]
        [Test(Author = "Konrad Dysput", Description = "Test valid submission url")]
        public void GenerateSubmissionUrl_FromValidHostName_ValidSubmissionUrl(string host)
        {
            const string token = "1234";
            var credentials = new BacktraceCredentials(host, token);

            string expectedUrl = $"{credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={credentials.Token}";
            Assert.AreEqual(credentials.GetSubmissionUrl(), expectedUrl);
        }

        [TestCase("https://www.submit.backtrace.io")]
        [TestCase("http://www.submit.backtrace.io")]
        [TestCase("https://submit.backtrace.io")]
        [TestCase("https://submit.backtrace.io/12312/312312/")]
        [TestCase("https://submit.backtrace.io/uri/")]
        [TestCase("https://submit.backtrace.io/uri?sumbissionToken=123123134&value=123123/")]
        [TestCase("http://submit.backtrace.io")]
        [TestCase("http://submit.backtrace.io/")]
        public void GenerateBacktraceSubmitUrl_FromSubmitUrl_ValidSubmissionUrl(string host)
        {
            var credentials = new BacktraceCredentials(host);

            if (!host.StartsWith("https://") && !host.StartsWith("http://"))
            {
                host = $"https://{host}";
            }

            if (!host.EndsWith("/"))
            {
                host += '/';
            }
            Assert.AreEqual(host, credentials.GetSubmissionUrl().ToString());
        }

        [TestCase("")]
        [TestCase("not url")]
        [TestCase("123123..")]
        [Test(Author = "Konrad Dysput", Description = "Test invalid api url")]
        public void ThrowInvalidUrlException_FromINvalidUrl_ThrowException(string host)
        {
            Assert.Throws<UriFormatException>(() => new BacktraceCredentials(host));
            Assert.Throws<UriFormatException>(() => new BacktraceCredentials(host, "123"));
        }

        [TestCase("https://backtrace.sp.backtrace.io")]
        [TestCase("http://backtrace.sp.backtrace.io")]
        [TestCase("https://totallynoValidSubmitUrl.submit.backtrace.io/")]
        [Test(Author = "Konrad Dysput", Description = "Test invalid api url")]
        public void ThrowInvalidArgumentException_FromInvalidHostName_ThrowException(string host)
        {
            Assert.Throws<ArgumentException>(() => new BacktraceCredentials(host));
        }

        [TestCase("InvalidUrl")]
        [Test(Author = "Konrad Dysput", Description = "Test invalid api url section")]
        public void TestInvalidUrlArgument(string sectionName)
        {
            //if programmer pass invalid url should throw UriFormatException
            //if programmer pass null or empty string as token should throw ArgumentNullException

#if NET35 || NET48
            Assert.Throws<UriFormatException>(() => new BacktraceClient(sectionName));
#endif
            Assert.Throws<ArgumentException>(() => new BacktraceClient(new BacktraceCredentials("https://test.backtrace.io", string.Empty)));
        }
    }
}
