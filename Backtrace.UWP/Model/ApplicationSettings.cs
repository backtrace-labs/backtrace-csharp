using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.UWP.Model
{
    public static class ApplicationSettings
    {
        public const string Host = @"https://yolo.sp.backtrace.io:6098/";
        public const string Token = "328174ab5c377e2cdcb6c763ec2bbdf1f9aa5282c1f6bede693efe06a479db54";

        //Check App.xaml.cs class to check how we prepare BacktraceDatabase instance
        //For uwp we use localstorage class to retrieve directory path where we can store ]
        //offline reports and data
    }
}
