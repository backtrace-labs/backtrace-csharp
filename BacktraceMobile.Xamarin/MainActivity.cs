using Android.App;
using Android.Widget;
using Android.OS;
using Backtrace;

namespace BacktraceMobile.Xamarin
{
    [Activity(Label = "BacktraceMobile.Xamarin", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private static BacktraceClient backtraceClient = new BacktraceClient(new BacktraceCredentials(@"https://yolo.sp.backtrace.io:6098/", "328174ab5c377e2cdcb6c763ec2bbdf1f9aa5282c1f6bede693efe06a479db54"));
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            backtraceClient.Send("custom client message");
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
        }
    }
}

