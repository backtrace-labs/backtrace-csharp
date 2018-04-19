using Backtrace.Framework45Example.Model;
using Backtrace.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Backtrace.Framework45Example
{
    class Program
    {
        private Tree tree;
        //initialize new BacktraceClient with custom configuration section readed from file App.config
        //Client will be initialized with values stored in default section name "BacktraceCredentials"
        private BacktraceClient backtraceClient = new BacktraceClient(
            new BacktraceCredentials(ApplicationCredentials.Host, ApplicationCredentials.Token),
            new BacktraceDatabaseSettings(@"D:\data\Backtracelogs"),
            reportPerMin: 0, //unlimited number of reports per secound
            tlsLegacySupport: true
        );

        public Program()
        {
            SetupBacktraceLibrary();
        }

        public async Task Start()
        {
            
            await GenerateRandomStrings();
            await TryClean();

            //handle uncaught exception from asyn task methods
            //Task.WaitAll(Task.Factory.StartNew(async () =>
            //{
            //    var client = new HttpClient();
            //    string getStringTask = await client.GetStringAsync(string.Empty);
            //}));

            //handle uncaught exception from unsafe code
            ThrowUnsafeException();
        }

        private void ThrowUnsafeException()
        {
            Console.WriteLine("This is expected behaviour. BacktraceClient can try to handle unhandled exception from your application");
            Console.WriteLine("To catch all unhandled application exception use HandleApplicationException on BacktraceClient");
            Console.WriteLine("If you want to add your custom event after BacktraceClient send a report to server use OnUnhandledApplicationException event");
            unsafe
            {
                int t = 0;
                int j = 0;
                DividePtrParam(&t, &j);
            }
        }

        private string GetRandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var tempString = Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray();
            return new string(tempString);
        }

        private async Task GenerateRandomStrings()
        {
            Random random = new Random();
            int totalStrings = random.Next(20, 25);
            //+3 because we want to add two duplicats
            string[] randomStrings = new string[totalStrings + 3];
            for (int i = 0; i < totalStrings; i++)
            {
                int stringLength = random.Next(0, 8);
                randomStrings[i] = GetRandomString(stringLength);
            }

            for (int i = 0; i < 3; i++)
            {
                int copyIndex = random.Next(0, totalStrings);
                randomStrings[totalStrings + i] = randomStrings[copyIndex];
            }
            tree = new Tree();
            for (int i = 0; i < totalStrings + 3; i++)
            {
                try
                {
                    tree.Add(randomStrings[i]);
                }
                catch (Exception exception)
                {
                    await backtraceClient.SendAsync(exception);
                    //we catch exception inside three add method
                    continue;
                }
            }
            var response = await backtraceClient.SendAsync($"{DateTime.Now}: Tree generated");
            Console.WriteLine($"Tree generated! Backtrace object id for last message: {response.Object}");
        }

        private async Task TryClean()
        {
            var orderedWords = tree.ToList();
            int total = orderedWords.Count();
            Random random = new Random();
            for (int i = 0; i < 3; i++)
            {
                int randomDuplicateIndex = random.Next(0, total);
                orderedWords.Add(orderedWords.ElementAt(randomDuplicateIndex));
            }
            orderedWords.Add(GetRandomString(5));
            orderedWords.Add(string.Empty);
            var shuffledWords = orderedWords.OrderBy(x => random.Next()).ToArray();
            await RemoveWords(shuffledWords);
        }

        private async Task RemoveWords(string[] shuffledWords)
        {
            for (int i = 0; i < shuffledWords.Length; i++)
            {
                try
                {
                    string word = shuffledWords[i];
                    tree.Remove(word);
                }
                catch (KeyNotFoundException keyNotFound)
                {
                    await backtraceClient.SendAsync(keyNotFound);
                }
                catch (ArgumentException argumentException)
                {
                    await backtraceClient.SendAsync(argumentException);
                }
            }
            await backtraceClient.SendAsync($"{DateTime.Now}: Tree clean");

        }

        unsafe static void DividePtrParam(int* p, int* j)
        {
            *p = *p / *j;
        }


        /// <summary>
        /// Setup client behaviour - attributes, events
        /// </summary>
        private void SetupBacktraceLibrary()
        {
            backtraceClient.HandleApplicationException();
            //Add new scoped attributes
            backtraceClient.Attributes["ClientAttributeNumber"] = 1;
            backtraceClient.Attributes["ClientAttributeString"] = "string attribute";
            backtraceClient.Attributes["ClientAttributeCustomClass"] = new
            {
                Name = "Backtrace",
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
            //backtraceClient.HandleUnobservedTaskExceptions();
        }
            
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start().Wait();
        }
    }
}