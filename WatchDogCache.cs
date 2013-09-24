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


namespace WatchDogProcess
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class WatchDogCache
    {
        static ManualResetEvent threadEvent = new ManualResetEvent(false);
        static public Process p = null;

        public static string InitProcess(string parameters)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            string dllReference = System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName;
            string function = "Start";
            string arguments = String.Format("{0},{1} {2}", dllReference, function, parameters);
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Arguments = arguments;
            psi.FileName = "Rundll32.exe";
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;

            //Start Process
            p = new Process();
            p.StartInfo = psi;
            p.Start();
            return p.ProcessName;
        }


        static void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            threadEvent.Set();
        }

        static void RequestWeb(string url)
        {
            string uri = url;
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
        public static void Start(IntPtr HWND, IntPtr hinst, string Ref, int nCmdShow)
        {
            String[] param = Ref.Split(',');
            string url = param[0].Trim();
            int timeout = Convert.ToInt32(param[1]);
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
