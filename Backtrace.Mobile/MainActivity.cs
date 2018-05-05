using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Backtrace.Model;
using Backtrace.Mobile.Model;
using Backtrace.Model.Database;
using System.IO;

namespace Backtrace.Mobile
{
    [Activity(Label = "Backtrace.Mobile", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //setup database 
            // we get external storage directory and special directory created for databse - Backtrace directory
            string directoryPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Backtrace");
            var database = new BacktraceDatabase<object>(new BacktraceDatabaseSettings(directoryPath));

            //setup client configuration
            var credentials = new BacktraceCredentials(ApplicationCredentials.Host, ApplicationCredentials.Token);
            var clientConfiguration = new BacktraceClientConfiguration(credentials);

            // Initialize new BacktraceClient 
            BacktraceClient client = new BacktraceClient(clientConfiguration, database);
            // Send async report to a server with custom client message
            var result = client.SendAsync("Hello from Xamarin").Result;
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
        }
    }
}

