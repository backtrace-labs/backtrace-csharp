using Android.App;
using Android.Widget;
using Android.OS;

namespace Backtrace.Mobile
{
    [Activity(Label = "Backtrace.Mobile", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            BacktraceClient client = new BacktraceClient(
                backtraceCredentials: new BacktraceCredentials(@"https://yolo.sp.backtrace.io:6098/", "328174ab5c377e2cdcb6c763ec2bbdf1f9aa5282c1f6bede693efe06a479db54")
            );
            client.Send("Hello from Xamarin");
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
        }
    }
}

