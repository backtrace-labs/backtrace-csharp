using Backtrace.Model;
using Backtrace.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Backtrace.Tests.IntegrationTests
{
    /// <summary>
    /// Runs Integration Tests
    /// </summary>
    [TestFixture(Author = "Arthur Tu", Category = "Submission tests", Description = "Test rate limiting with diffrent threads")]
    public class RateLimitingTests
    {
        private BacktraceClient _backtraceClient;

        [SetUp]
        public void Setup()
        {
            _backtraceClient = new BacktraceClient(
                new BacktraceCredentials(@"http://yolo.sp.backtrace.io:6097/", "328174ab5c377e2cdcb6c763ec2bbdf1f9aa5282c1f6bede693efe06a479db54"),
                reportPerMin: 0 //unlimited number of reports per secound
            );
            //Add new scoped attributes
            _backtraceClient.Attributes["ClientAttributeNumber"] = 1;
            _backtraceClient.Attributes["ClientAttributeString"] = "string attribute";
            _backtraceClient.Attributes["ClientAttributeCustomClass"] = new
            {
                Name = "BacktraceIntegrationTest",
                Type = "Library"
            };
            _backtraceClient.Attributes["ComplexObject"] = new Dictionary<string, Uri>()
            {
                {"backtrace.io" , new Uri("http://backtrace.io") },
                {"Google url" , new Uri("http://google.com") }
            };

            //Add your own handler to client API
            _backtraceClient.BeforeSend =
               (BacktraceData<object> model) =>
               {
                   var data = model;
                   data.Attributes.Add("eventAtrtibute", "EventAttributeValue");
                   if (data.Classifier == null || !data.Classifier.Any())
                   {
                       data.Attachments.Add("path to attachment");
                   }
                   return data;
               };
        }

        private void ExceptionMethod()
        {

            int x = 0;
            var result = 5 / x;
        }
        /// <summary>
        /// Test a initialization and submission sequence for backtrace client w/ threading w/o rate limiting
        /// </summary>
        [Test(Author = "Arthur Tu", Description = "Test rate limiting on single thread")]
        public void SingleThreadWithoutRateLimiting()
        {
            Assert.DoesNotThrow(() => { _backtraceClient.ChangeRateLimiting(0); });
            Assert.DoesNotThrow(() => { _backtraceClient.Send($"{DateTime.Now}: Library Initialized for single thread integration test w/o rate limiting"); });
            Assert.DoesNotThrow(() =>
            {
                try
                {
                    ExceptionMethod();
                }
                catch (DivideByZeroException ex)
                {
                    Assert.DoesNotThrow(() => { _backtraceClient.Send(ex); });
                }
            });
            Assert.DoesNotThrow(() => { _backtraceClient.Send($"{DateTime.Now}: Single thread integration test w/o rate limiting completed successfully."); });
        }


        /// <summary>
        /// Test a initialization and submission sequence for backtrace client w/ threading w/o rate limiting
        /// </summary>
        [Test(Author = "Arthur Tu", Description = "Test rate limiting on multiple threads = rate limiting is off")]
        public void ThreadedWithoutRateLimiting()
        {
            HashSet<Thread> threads = new HashSet<Thread>();
            Assert.DoesNotThrow(() => { _backtraceClient.Send($"{DateTime.Now}: Library Initialized for threaded integration test w/o rate limiting"); });
            // Create threads of divide by zero errors
            for (int i = 0; i < 5; i++)
                threads.Add(new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        ExceptionMethod();
                    }
                    catch (DivideByZeroException ex)
                    {
                        Assert.DoesNotThrow(() => { _backtraceClient.Send(ex); });
                    }
                })));

            // Create threads of index out of bounds errors
            for (int i = 0; i < 5; i++)
                threads.Add(new Thread(new ThreadStart(() =>
                {
                    int[] x = new int[1];
                    try
                    {
                        x[1] = 1 - 1;
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Assert.DoesNotThrow(() => { _backtraceClient.Send(ex); });
                    }
                })));

            // Start all threads
            foreach (Thread t in threads)
            {
                t.Start();
            }

            // Block calling thread until all threads are done running
            foreach (Thread t in threads)
            {
                t.Join();
            }
            Assert.DoesNotThrow(() => { _backtraceClient.Send($"{DateTime.Now}: Threaded integration test w/o rate limiting completed successfully."); });
        }

        /// <summary>
        /// Test a initialization and submission sequence for backtrace client w/ threading w/o rate limiting
        /// </summary>
        [Test(Author = "Athur Tu", Description = "Test a initialization and submission sequence for backtrace client w/ threading w/o rate limiting")]
        public void ThreadedWithRateLimiting()
        {
            HashSet<Thread> threads = new HashSet<Thread>();
            uint rpm = 10;
            int count_sends = 0;
            int count_drops = 0;

            Assert.DoesNotThrow(() => { _backtraceClient.ChangeRateLimiting(rpm); });
            Assert.DoesNotThrow(() => { _backtraceClient.Send($"{DateTime.Now}: Library Initialized for threaded integration test with rate limiting"); });
            _backtraceClient.OnClientSiteRatingLimit += () =>
            {
                count_drops++;
            };
            _backtraceClient.AfterSend += (BacktraceReport report) =>
            {
                count_sends++;
            };

            // Create threads of divide by zero errors
            for (int i = 0; i < 4; i++)
                threads.Add(new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        ExceptionMethod();
                    }
                    catch (DivideByZeroException ex)
                    {
                        Assert.DoesNotThrow(() => { _backtraceClient.Send(ex); });
                    }
                })));



            // Create threads of index out of bounds errors
            for (int i = 0; i < 4; i++)
                threads.Add(new Thread(new ThreadStart(() =>
                {
                    int[] x = new int[1];
                    try
                    {
                        x[1] = 1 - 1;
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Assert.DoesNotThrow(() => { _backtraceClient.Send(ex); });
                    }
                })));

            // Start all threads
            foreach (Thread t in threads)
            {
                t.Start();
            }

            // Block calling thread until all threads are done running
            foreach (Thread t in threads)
            {
                t.Join();
            }
            Assert.DoesNotThrow(() => { _backtraceClient.Send($"{DateTime.Now}: Threaded integration test - after this point no reports should go through."); });
            threads = new HashSet<Thread>();

            // Create threads of divide by zero errors
            for (int i = 0; i < 6; i++)
                threads.Add(new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        ExceptionMethod();
                    }
                    catch (DivideByZeroException ex)
                    {

                        Assert.DoesNotThrow(() => { _backtraceClient.Send(ex); });
                    }
                })));
            // Create threads of index out of bounds errors
            for (int i = 0; i < 6; i++)
                threads.Add(new Thread(new ThreadStart(() =>
                {
                    int[] x = new int[1];
                    try
                    {
                        x[1] = 1 - 1;
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Assert.DoesNotThrow(() => { _backtraceClient.Send(ex); });
                    }
                })));

            // Start all threads
            foreach (Thread t in threads)
            {
                t.Start();
            }

            // Block calling thread until all threads are done running
            foreach (Thread t in threads)
            {
                t.Join();
            }

            // by now there should be 12 drops

            System.Diagnostics.Trace.WriteLine("there are currently " + count_drops + " drops out of " + count_sends + " submissions.");
            Assert.AreEqual(count_drops, 12);
            System.Diagnostics.Trace.WriteLine("Threaded integration test - going to sleep for a minute. Reports should start coming in after this point.");
            // sleep for a minute
            Thread.Sleep(60000);
            // restart threaded reporting
            threads = new HashSet<Thread>();
            // Create threads of divide by zero errors
            for (int i = 0; i < 5; i++)
                threads.Add(new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        ExceptionMethod();
                    }
                    catch (DivideByZeroException ex)
                    {

                        Assert.DoesNotThrow(() => { _backtraceClient.Send(ex); });
                    }
                })));

            // Create threads of index out of bounds errors
            for (int i = 0; i < 5; i++)
                threads.Add(new Thread(new ThreadStart(() =>
                {
                    int[] x = new int[1];
                    try
                    {
                        x[1] = 1 - 1;
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Assert.DoesNotThrow(() => { _backtraceClient.Send(ex); });
                    }
                })));

            // Start all threads
            foreach (Thread t in threads)
            {
                t.Start();
            }

            // Block calling thread until all threads are done running
            foreach (Thread t in threads)
            {
                t.Join();
            }
            Assert.DoesNotThrow(() => { _backtraceClient.Send($"{DateTime.Now}: Threaded integration test with rate limiting completed successfully."); });
        }
    }
}
