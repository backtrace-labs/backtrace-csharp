using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.UWP.Model
{
    public static class ApplicationSettings
    {
        public const string Host = @"https://myserver.sp.backtrace.io:6097";
        public const string Token = "4dca18e8769d0f5d10db0d1b665e64b3d716f76bf182fbcdad5d1d8070c12db0";

        //Check App.xaml.cs class to check how we prepare BacktraceDatabase instance
        //For uwp we use localstorage class to retrieve directory path where we can store
        //offline reports and data
    }
}
