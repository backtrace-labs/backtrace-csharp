namespace Backtrace.Mobile.Model
{
    public static class ApplicationSettings
    {
        public const string Host = @"https://yolo.sp.backtrace.io:6098/";
        public const string Token = "328174ab5c377e2cdcb6c763ec2bbdf1f9aa5282c1f6bede693efe06a479db54";

        //Check MainActivity.cs class to check how we prepare BacktraceDatabase instance
        //For Xamarin solution we prepare special directory called Backtrace in the root of file system
        //We store there our offline reports and data
    }
}