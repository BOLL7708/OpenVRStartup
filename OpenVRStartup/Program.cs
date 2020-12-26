using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Valve.VR;

namespace OpenVRStartup
{
    class Program
    {
        static string LOG_FILE_PATH = "./OpenVRStartup.log";

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public const int SW_SHOWMINIMIZED = 2;
        private volatile static bool _isReady = false;

        static void Main(string[] args)
        {
            // Starting worker
            var t = new Thread(Worker);
            LogUtils.WriteLineToCache($"Application starting ({Properties.Resources.Version})");
            if (!t.IsAlive) t.Start();
            else LogUtils.WriteLineToCache("Error: Could not start worker thread");

            // Check if first run, if so do NOT minimize but write instructions.
            if (LogUtils.LogFileExists(LOG_FILE_PATH))
            {
                _isReady = true;
                IntPtr winHandle = Process.GetCurrentProcess().MainWindowHandle;
                ShowWindow(winHandle, SW_SHOWMINIMIZED);
            }
            else {
                Utils.PrintInfo("\n========================");
                Utils.PrintInfo(" First Run Instructions ");
                Utils.PrintInfo("========================\n");
                Utils.Print("To get this to launch every time you start SteamVR, do the following steps:");
                Utils.Print("  1. Launch SteamVR.");
                Utils.Print("  2. Open [Settings] from the hamburger menu in the SteamVR status window.");
                Utils.Print("  3. Select the [Startup / Shutdown] section in the menu to the left.");
                Utils.Print("  4. Click [Choose startup overlay apps].");
                Utils.Print("  5. Locate OpenVRStartup and toggle the switch to [On].");
                Utils.Print("\nThe next time this program runs it will be minimized and terminate as soon as scripts have been launched.");
                Utils.Print("\nTo see this message again, delete the log file that is in the same folder.");
                Utils.Print("\nPress [Enter] in this window to continue execution.");
                Console.ReadLine();
                _isReady = true;
            }

            Console.ReadLine();
            t.Abort();

            OpenVR.Shutdown();
        }

        private static bool _isConnected = false;

        private static void Worker()
        {
            var shouldRun = true;

            Thread.CurrentThread.IsBackground = true;
            while (shouldRun)
            {
                if (!_isConnected)
                {
                    Thread.Sleep(1000);
                    _isConnected = InitVR();
                }
                else if(_isReady)
                {    
                    RunScripts();
                    OpenVR.Shutdown();
                    shouldRun = false;
                }
                if (!shouldRun)
                {
                    LogUtils.WriteLineToCache("Application exiting, writing log");
                    LogUtils.WriteCacheToLogFile(LOG_FILE_PATH, 100);
                    Environment.Exit(0);
                }
            }
        }

        // Initializing connection to OpenVR
        private static bool InitVR()
        {
            var error = EVRInitError.None;
            OpenVR.Init(ref error, EVRApplicationType.VRApplication_Overlay);
            if (error != EVRInitError.None)
            {
                LogUtils.WriteLineToCache($"Error: OpenVR init failed: {Enum.GetName(typeof(EVRInitError), error)}");
                return false;
            }
            else
            {
                LogUtils.WriteLineToCache("OpenVR init success");

                // Load app manifest
                var appError = OpenVR.Applications.AddApplicationManifest(Path.GetFullPath("./app.vrmanifest"), false);
                if (appError != EVRApplicationError.None) LogUtils.WriteLineToCache($"Error: Failed to load app manifest: {Enum.GetName(typeof(EVRApplicationError), appError)}");
                return true;
            }
        }

        // Scripts
        private static void RunScripts()
        {
            try {
                var files = Directory.GetFiles("./", "*.cmd");
                LogUtils.WriteLineToCache($"Found: {files.Length} script(s)");
                foreach (var file in files)
                {
                    LogUtils.WriteLineToCache($"Executing: {file}");
                    Process p = new Process();
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, file);
                    p.Start();
                }
                if(files.Length == 0) LogUtils.WriteLineToCache($"Did not find any .cmd files to execute.");
            } catch(Exception e)
            {
                LogUtils.WriteLineToCache($"Error: Could not load scripts: {e.Message}");
            }
        }
    }
}
