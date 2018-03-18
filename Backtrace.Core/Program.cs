using Backtrace.Core.Model.DataStructures.Trees;
using Backtrace.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Backtrace.Core
{
    class Program
    {
        private bool treeGenerated = false;
        private Tree tree;
        private HashSet<Thread> threads = new HashSet<Thread>();
        //initialize new BacktraceClient with custom configuration section readed from file App.config
        //Client will be initialized with values stored in default section name "BacktraceCredentials"
        private BacktraceClient backtraceClient = new BacktraceClient(
            new BacktraceCredentials(@"http://yolo.sp.backtrace.io:6097/", "328174ab5c377e2cdcb6c763ec2bbdf1f9aa5282c1f6bede693efe06a479db54"),
            reportPerMin: 0 //unlimited number of reports per secound
        );

        public Program()
        {
            SetupBacktraceLibrary();
            SetupStartupTasks();
            StartTasks();
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

        private void StartTasks()
        {
            foreach (var task in threads)
            {
                task.Start();
            }
            foreach (var task in threads)
            {
                task.Join();
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

        private void GenerateRandomStrings()
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
            Thread.Sleep(100);
            tree = new Tree(backtraceClient);
            for (int i = 0; i < totalStrings + 3; i++)
            {
                try
                {
                    tree.Add(randomStrings[i]);
                }
                catch (Exception exception)
                {
                    backtraceClient.Send(exception);
                    //we catch exception inside three add method
                    continue;
                }
            }
            Thread.Sleep(150);
            EndTreeGeneration();
        }

        private void TryClean()
        {
            if (!treeGenerated)
            {
                Thread.Sleep(100);
                TryClean();
                return;
            }

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
            RemoveWords(shuffledWords);
        }

        private void RemoveWords(string[] shuffledWords)
        {
            for (int i = 0; i < shuffledWords.Length; i++)
            {
                try
                {
                    string word = shuffledWords[i];
                    tree.Remove(word);
                }
                catch
                {
                    //we catch exception inside three add method
                    continue;
                }
            }
            EndCleaning();

        }

        private void EndCleaning()
        {
            backtraceClient.Send($"{DateTime.Now}: Tree clean");
        }

        private void EndTreeGeneration()
        {
            treeGenerated = true;
            backtraceClient.Send($"{DateTime.Now}: Tree generated");
        }

        /// <summary>
        /// Prepare multithreading calculations
        /// </summary>
        private void SetupStartupTasks()
        {
            threads.Add(new Thread(new ThreadStart(() => { TryClean(); })));
            threads.Add(new Thread(new ThreadStart(() => { GenerateRandomStrings(); })));
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
                   if(data.Classifier == null || !data.Classifier.Any())
                   {
                       data.Attachments.Add("path to attachment");
                   }
                   return data;
               };

            backtraceClient.Send($"{DateTime.Now}: Library Initialized");
        }
        static void Main(string[] args)
        {
            Program program = new Program();
        }
    }
}
