using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Backtrace.Model;

namespace Backtrace.Mobile
{
    [Activity(Label = "Backtrace.Mobile", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            BacktraceClient client = new BacktraceClient(
                new BacktraceCredentials(@"https://myserver.sp.backtrace.io", "4dca18e8769d0f5d10db0d1b665e64b3d716f76bf182fbcdad5d1d8070c12db0")
            )
            {
                OnServerError = (Exception e) =>
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                },
                OnServerResponse = (BacktraceServerResponse res) =>
                {
                    System.Diagnostics.Trace.WriteLine(res.Response);
                }
            };
            client.Send("Hello from Xamarin");
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
        }
    }
}

