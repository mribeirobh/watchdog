// -----------------------------------------------------------------------
// <copyright file="WatchDogCache.cs" company="CI&T">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;

[assembly: CLSCompliant(true)]
namespace WatchDogProcess
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class WatchDogCache
    {
        //private WatchDogCache() { }

        static ManualResetEvent threadEvent = new ManualResetEvent(false);
        static private Process _watchdogProcessHandle;

        public static Process WatchdogProcessHandle
        {
            get { return WatchDogCache._watchdogProcessHandle; }
            //set { WatchDogCache._p = value; }
        }


        public static string InitProcess(string parameters)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            string dllReference = System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName.ToString(CultureInfo.InvariantCulture);
            string function = "Start";
            string arguments = String.Format(CultureInfo.InvariantCulture, "{0},{1} {2}", dllReference, function, parameters);
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Arguments = arguments;
            psi.FileName = "Rundll32.exe";
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;

            //Start Process
            _watchdogProcessHandle = new Process();
            _watchdogProcessHandle.StartInfo = psi;
            _watchdogProcessHandle.Start();
            return _watchdogProcessHandle.ProcessName;
        }


        static void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            threadEvent.Set();
        }

        static void RequestWeb(string url)
        {
            var br = new WebBrowser();
            br.DocumentCompleted += browser_DocumentCompleted;
            br.Navigate(url);
            Application.Run();
        }


        private static void Monitoring(object url)
        {
            RequestWeb((string)url);
        }

        [DllExport("Start", CallingConvention = CallingConvention.Cdecl)]
        public static void Start(IntPtr hwnd, IntPtr hinst, string parameters, int nCmdShow)
        {
            String[] param = parameters.Split(',');
            string url = param[0].Trim();
            int timeout = Convert.ToInt32(param[1], CultureInfo.InvariantCulture);
            while (true)
            {
                Thread t = new Thread(Monitoring);
                t.SetApartmentState(ApartmentState.STA);
                threadEvent.Reset();
                t.Start(url);
                threadEvent.WaitOne();
                t.Abort();
                Thread.Sleep(timeout);
            }
        }
    }
}
