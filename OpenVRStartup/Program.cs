using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Valve.VR;

namespace OpenVRStartup
{
    class Program
    {
        static void Main(string[] args)
        {
            // Starting worker
            var t = new Thread(Worker);
            Utils.PrintDebug("Starting worker thread.");
            if (!t.IsAlive) t.Start();
            else Utils.PrintError("Could not start worker thread.");

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
                else
                {
                    Utils.Print("Waiting for 10 seconds...");
                    Thread.Sleep(10000);
                    RunScripts();
                    OpenVR.Shutdown();
                    shouldRun = false;
                }
                if (!shouldRun)
                {
                    Thread.Sleep(5000);
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
                Utils.PrintError($"OpenVR initialization errored: {Enum.GetName(typeof(EVRInitError), error)}");
                return false;
            }
            else
            {
                Utils.PrintInfo("OpenVR initialized successfully.");

                // Load app manifest, I think this is needed for the application to show up in the input bindings at all
                Utils.PrintVerbose("Loading app.vrmanifest");
                var appError = OpenVR.Applications.AddApplicationManifest(Path.GetFullPath("./app.vrmanifest"), false);
                if (appError != EVRApplicationError.None) Utils.PrintError($"Failed to load Application Manifest: {Enum.GetName(typeof(EVRApplicationError), appError)}");
                else Utils.PrintInfo("Application manifest loaded successfully.");
                return true;
            }
        }

        // Scripts
        private static void RunScripts()
        {
            try {
                var files = Directory.GetFiles("./", "*.cmd");
                Utils.Print($"Found {files.Length} file(s)");
                foreach (var file in files)
                {
                    Utils.PrintInfo($"Running {file}");
                    Process p = new Process();
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, file);
                    p.Start();
                }
            } catch(Exception e)
            {
                Utils.PrintError($"Error loading scripts: {e.Message}");
            }
            
        }
    }
}
