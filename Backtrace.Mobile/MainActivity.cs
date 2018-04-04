using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Backtrace.Model;
using Backtrace.Mobile.Model;

namespace Backtrace.Mobile
{
    [Activity(Label = "Backtrace.Mobile", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Initialize new BacktraceClient 
            BacktraceClient client = new BacktraceClient(
                new BacktraceCredentials(ApplicationCredentials.Host, ApplicationCredentials.Token)
            );
            // Send async report to a server with custom client message
            var result = client.SendAsync("Hello from Xamarin").Result;
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
        }
    }
}

