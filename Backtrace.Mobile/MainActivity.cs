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
            BacktraceClient client = new BacktraceClient(
                new BacktraceCredentials(ApplicationCredentials.Host, ApplicationCredentials.Token)
            );
            client.OnServerError = (Exception e) =>
            {
                System.Diagnostics.Trace.Write(e.Message);
                throw e;
            };
            client.SendAsync("Hello from Xamarin").Wait();
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
        }
    }
}

