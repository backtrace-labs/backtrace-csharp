using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Backtrace.UniversalWindowsPlatform
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        private static BacktraceClient backtraceClient = new BacktraceClient(
            new BacktraceCredentials(@"https://yolo.sp.backtrace.io:6098/", "328174ab5c377e2cdcb6c763ec2bbdf1f9aa5282c1f6bede693efe06a479db54"),
            databaseDirectory: localFolder.Path
            );

        private static void StartJob()
        {
            CalculateDifference(-12);
        }
        private static void CalculateDifference(int i = 0)
        {
            if (i == 2)
            {
                throw new ArgumentException("i");
            }
            try
            {
                CalculateDifference(++i);

            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
                System.Diagnostics.Trace.WriteLine(Thread.CurrentThread.Name);
                backtraceClient.Send(e);
            }
        }

        public MainPage()
        {           
            this.InitializeComponent();
            var thread = new Thread(new ThreadStart(() => {
                Thread.CurrentThread.Name = "Universal windows platform main thread";
                StartJob();
            }));
            thread.Start();
            thread.Join();
        }
    }
}
