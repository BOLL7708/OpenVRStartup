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
                Utils.PrintInfo("========================");
                Utils.Print("\nThis app automatically sets itself to auto-launch with SteamVR.");
                Utils.Print("\nWhen it runs it will in turn run all .cmd files in the same folder.");
                Utils.Print("\nThis message is only shown once, to see it again delete the log file.");
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

                // Add app manifest and set auto-launch
                var appKey = "boll7708.openvrstartup";
                if (!OpenVR.Applications.IsApplicationInstalled(appKey))
                {
                    var manifestError = OpenVR.Applications.AddApplicationManifest(Path.GetFullPath("./app.vrmanifest"), false);
                    if (manifestError == EVRApplicationError.None) LogUtils.WriteLineToCache("Successfully installed app manifest");
                    else LogUtils.WriteLineToCache($"Error: Failed to add app manifest: {Enum.GetName(typeof(EVRApplicationError), manifestError)}");
                    
                    var autolaunchError = OpenVR.Applications.SetApplicationAutoLaunch(appKey, true);
                    if (autolaunchError == EVRApplicationError.None) LogUtils.WriteLineToCache("Successfully set app to auto launch");
                    else LogUtils.WriteLineToCache($"Error: Failed to turn on auto launch: {Enum.GetName(typeof(EVRApplicationError), autolaunchError)}");
                }
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
                    var path = Path.Combine(Environment.CurrentDirectory, file);
                    Process p = new Process();
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe");
                    p.StartInfo.Arguments = $"/C \"{path}\"";
                    p.Start();
                }
                if(files.Length == 0) LogUtils.WriteLineToCache("Did not find any .cmd files to execute.");
            } catch(Exception e)
            {
                LogUtils.WriteLineToCache($"Error: Could not load scripts: {e.Message}");
            }
        }
    }
}
