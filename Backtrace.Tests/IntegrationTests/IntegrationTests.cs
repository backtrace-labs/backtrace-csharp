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
    [TestFixture(Author = "", Category = "", Description = "")]
    public class IntegrationTests
    {

        /// <summary>
        /// Test a initialization and submission sequence for backtrace client w/ threading w/o rate limiting
        /// </summary>
        [Test(Author = "", Description = "")]
        public void SingleThreadWithoutRateLimiting()
        {
            HashSet<Thread> threads = new HashSet<Thread>();

            BacktraceClient backtraceClient = null;

            Assert.DoesNotThrow(() => { backtraceClient = SampleInitializeClient(); });

            Assert.DoesNotThrow(() => { backtraceClient.Send($"{DateTime.Now}: Library Initialized for single thread integration test w/o rate limiting"); });

            var x = 1;

            try{

                var new_int = 5 / (x - 1);

            }
            catch (DivideByZeroException ex)
            {
                Assert.DoesNotThrow(() => { backtraceClient.Send(ex); });
            }

            Assert.DoesNotThrow(() => { backtraceClient.Send($"{DateTime.Now}: Single thread integration test w/o rate limiting completed successfully."); });

        }

        
        /// <summary>
        /// Test a initialization and submission sequence for backtrace client w/ threading w/o rate limiting
        /// </summary>
        [Test(Author = "", Description = "")]
        public void ThreadedWithoutRateLimiting()
        {
            HashSet<Thread> threads = new HashSet<Thread>();

            BacktraceClient backtraceClient = null;

            Assert.DoesNotThrow(() => { backtraceClient = SampleInitializeClient(); });

            Assert.DoesNotThrow(() => { backtraceClient.Send($"{DateTime.Now}: Library Initialized for threaded integration test w/o rate limiting"); });


            // Create threads of divide by zero errors

            for (int i = 0; i < 5; i ++)
                threads.Add(new Thread(new ThreadStart(() => {

                    var x = 1;

                    try{
                        
                        var new_int = 5 / (x - 1);

                    }catch(DivideByZeroException ex)
                    {
                        Assert.DoesNotThrow(() => { backtraceClient.Send(ex); });
                    }

                })));



            // Create threads of index out of bounds errors

            for (int i = 0; i < 5; i++)
                threads.Add(new Thread(new ThreadStart(() => { 

                    int[] x = new int[1];

                    try
                    {
                        x[1] = 1 - 1;
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Assert.DoesNotThrow(() => {backtraceClient.Send(ex); });
                    }

                })));

            // Start all threads

            foreach (Thread t in threads)
                t.Start();


            // Block calling thread until all threads are done running

            foreach (Thread t in threads)
            {
                t.Join();
            }


            Assert.DoesNotThrow(() => { backtraceClient.Send($"{DateTime.Now}: Threaded integration test w/o rate limiting completed successfully."); });

        }





        /// <summary>
        /// Test a initialization and submission sequence for backtrace client w/ threading w/o rate limiting
        /// </summary>
        [Test(Author = "", Description = "")]
        public void ThreadedWithRateLimiting()
        {
            HashSet<Thread> threads = new HashSet<Thread>();

            BacktraceClient backtraceClient = null;

            uint rpm = 10;
            int count_sends = 0;
            int count_drops = 0;

            Assert.DoesNotThrow(() => { backtraceClient = SampleInitializeClient(rpm); });

            Assert.DoesNotThrow(() => { backtraceClient.Send($"{DateTime.Now}: Library Initialized for threaded integration test with rate limiting"); });

            backtraceClient.OnClientSiteRatingLimit += () => {
                Assert.DoesNotThrow(() => { count_drops ++; });
            };

            backtraceClient.AfterSend += (BacktraceReport report) =>
            {
                Assert.DoesNotThrow(() => { count_sends++; });
            };


            // Create threads of divide by zero errors

            for (int i = 0; i < 4; i++)
                threads.Add(new Thread(new ThreadStart(() => {

                    var x = 1;

                    try
                    {

                        var new_int = 5 / (x - 1);

                    }
                    catch (DivideByZeroException ex)
                    {
                
                        Assert.DoesNotThrow(() => { backtraceClient.Send(ex); });
                    }

                })));



            // Create threads of index out of bounds errors

            for (int i = 0; i < 4; i++)
                threads.Add(new Thread(new ThreadStart(() => {

                    int[] x = new int[1];

                    try
                    {
                        x[1] = 1 - 1;
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Assert.DoesNotThrow(() => { backtraceClient.Send(ex); });
                    }

                })));

            // Start all threads

            foreach (Thread t in threads)
                t.Start();


            // Block calling thread until all threads are done running

            foreach (Thread t in threads)
            {
                t.Join();
            }


            Assert.DoesNotThrow(() => { backtraceClient.Send($"{DateTime.Now}: Threaded integration test - after this point no reports should go through."); });


            threads = new HashSet<Thread>();

            // Create threads of divide by zero errors

            for (int i = 0; i < 6; i++)
                threads.Add(new Thread(new ThreadStart(() => {

                    var x = 1;

                    try
                    {

                        var new_int = 5 / (x - 1);

                    }
                    catch (DivideByZeroException ex)
                    {

                        Assert.DoesNotThrow(() => { backtraceClient.Send(ex); });
                    }

                })));



            // Create threads of index out of bounds errors

            for (int i = 0; i < 6; i++)
                threads.Add(new Thread(new ThreadStart(() => {

                    int[] x = new int[1];

                    try
                    {
                        x[1] = 1 - 1;
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Assert.DoesNotThrow(() => { backtraceClient.Send(ex); });
                    }

                })));

            // Start all threads

            foreach (Thread t in threads)
                t.Start();


            // Block calling thread until all threads are done running

            foreach (Thread t in threads)
            {
                t.Join();
            }

            // by now there should be 12 drops

            Console.WriteLine("there are currently " + count_drops + " drops out of " + count_sends + " submissions.");

            Assert.AreEqual(count_drops, 12);

            Console.WriteLine("Threaded integration test - going to sleep for a minute. Reports should start coming in after this point.");


            // sleep for a minute

            Thread.Sleep(60000);



            // restart threaded reporting

            threads = new HashSet<Thread>();

            // Create threads of divide by zero errors

            for (int i = 0; i < 5; i++)
                threads.Add(new Thread(new ThreadStart(() => {

                    var x = 1;

                    try
                    {

                        var new_int = 5 / (x - 1);

                    }
                    catch (DivideByZeroException ex)
                    {

                        Assert.DoesNotThrow(() => { backtraceClient.Send(ex); });
                    }

                })));



            // Create threads of index out of bounds errors

            for (int i = 0; i < 5; i++)
                threads.Add(new Thread(new ThreadStart(() => {

                    int[] x = new int[1];

                    try
                    {
                        x[1] = 1 - 1;
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Assert.DoesNotThrow(() => { backtraceClient.Send(ex); });
                    }

                })));

            // Start all threads

            foreach (Thread t in threads)
                t.Start();


            // Block calling thread until all threads are done running

            foreach (Thread t in threads)
            {
                t.Join();
            }

            Assert.DoesNotThrow(() => { backtraceClient.Send($"{DateTime.Now}: Threaded integration test with rate limiting completed successfully."); });


        }





        private BacktraceClient SampleInitializeClient(uint rpm = 0)
        {

            BacktraceClient backtraceClient = new BacktraceClient(
            new BacktraceCredentials(@"https://yolo.sp.backtrace.io:6098/", "328174ab5c377e2cdcb6c763ec2bbdf1f9aa5282c1f6bede693efe06a479db54"),
            reportPerMin: rpm //unlimited number of reports per secound
            );

            //Add new scoped attributes

            backtraceClient.Attributes["ClientAttributeNumber"] = 1;
            backtraceClient.Attributes["ClientAttributeString"] = "string attribute";
            backtraceClient.Attributes["ClientAttributeCustomClass"] = new
            {
                Name = "BacktraceIntegrationTest",
                Type = "Library"
            };
            backtraceClient.Attributes["ComplexObject"] = new Dictionary<string, Uri>()
            {
                {"backtrace.io" , new Uri("http://backtrace.io") },
                {"Google url" , new Uri("http://google.com") }
            };

            //Add your own handler to client API
            backtraceClient.BeforeSend =
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


            return backtraceClient;
        }

    }
}
