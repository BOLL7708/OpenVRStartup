using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Valve.VR;

namespace OpenVRStartup
{
    class Program
    {
        static readonly string PATH_LOGFILE = "./OpenVRStartup.log";
        static readonly string PATH_STARTFOLDER = "./start/";
        static readonly string PATH_STOPFOLDER = "./stop/";
        static readonly string FILE_PATTERN = "*.cmd";

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public const int SW_SHOWMINIMIZED = 2;
        private volatile static bool _isReady = false;

        static void Main(string[] _)
        {
            // Window setup
            Console.Title = Properties.Resources.AppName;

            // Starting worker
            var t = new Thread(Worker);
            LogUtils.WriteLineToCache($"Application starting ({Properties.Resources.Version})");
            if (!t.IsAlive) t.Start();
            else LogUtils.WriteLineToCache("Error: Could not start worker thread");

            // Check if first run, if so do NOT minimize but write instructions.
            if (LogUtils.LogFileExists(PATH_LOGFILE))
            {
                _isReady = true;
                Minimize();
            }
            else {
                Utils.PrintInfo("\n========================");
                Utils.PrintInfo(" First Run Instructions ");
                Utils.PrintInfo("========================");
                Utils.Print("\nThis app automatically sets itself to auto-launch with SteamVR.");
                Utils.Print($"\nWhen it runs it will in turn run all {FILE_PATTERN} files in the {PATH_STARTFOLDER} folder.");
                Utils.Print($"\nIf there are {FILE_PATTERN} files in {PATH_STOPFOLDER} it will stay and run those on shutdown.");
                Utils.Print("\nThis message is only shown once, to see it again delete the log file.");
                Utils.Print("\nPress [Enter] in this window to continue execution.\nIf there are shutdown scripts the window will remain in the task bar.");
                Console.ReadLine();
                Minimize();
                _isReady = true;
            }

            Console.ReadLine();
            t.Abort();

            OpenVR.Shutdown();
        }

        private static void Minimize() {
            IntPtr winHandle = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(winHandle, SW_SHOWMINIMIZED);
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
                    RunScripts(PATH_STARTFOLDER);
                    if(WeHaveScripts(PATH_STOPFOLDER)) WaitForQuit();
                    OpenVR.Shutdown();
                    RunScripts(PATH_STOPFOLDER);
                    shouldRun = false;
                }
                if (!shouldRun)
                {
                    LogUtils.WriteLineToCache("Application exiting, writing log");
                    LogUtils.WriteCacheToLogFile(PATH_LOGFILE, 100);
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
        private static void RunScripts(string folder) {
            try
            {
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                var files = Directory.GetFiles(folder, FILE_PATTERN);
                LogUtils.WriteLineToCache($"Found: {files.Length} script(s) in {folder}");
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
                if (files.Length == 0) LogUtils.WriteLineToCache($"Did not find any {FILE_PATTERN} files to execute in {folder}");
            }
            catch (Exception e)
            {
                LogUtils.WriteLineToCache($"Error: Could not load scripts from {folder}: {e.Message}");
            }
        }

        private static void WaitForQuit()
        {
            Utils.Print("This window remains to wait for the shutdown of SteamVR to run additional scripts on exit.");
            var shouldRun = true;
            while(shouldRun)
            {
                var vrEvents = new List<VREvent_t>();
                var vrEvent = new VREvent_t();
                uint eventSize = (uint)Marshal.SizeOf(vrEvent);
                try
                {
                    while (OpenVR.System.PollNextEvent(ref vrEvent, eventSize))
                    {
                        vrEvents.Add(vrEvent);
                    }
                }
                catch (Exception e)
                {
                    Utils.PrintError($"Could not get new events: {e.Message}");
                }

                foreach (var e in vrEvents)
                {
                    if ((EVREventType)e.eventType == EVREventType.VREvent_Quit)
                    {
                        OpenVR.System.AcknowledgeQuit_Exiting();
                        shouldRun = false;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private static bool WeHaveScripts(string folder)
        {
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return Directory.GetFiles(folder, FILE_PATTERN).Length > 0;
        }
    }
}
