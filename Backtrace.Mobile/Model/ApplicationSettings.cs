namespace Backtrace.Mobile.Model
{
    public static class ApplicationSettings
    {
        public const string Host = @"https://myserver.sp.backtrace.io:6097";
        public const string Token = "4dca18e8769d0f5d10db0d1b665e64b3d716f76bf182fbcdad5d1d8070c12db0";

        //Check MainActivity.cs class to check how we prepare BacktraceDatabase instance
        //For Xamarin solution we prepare special directory called Backtrace in the root of file system
        //We store there our offline reports and data
    }
}