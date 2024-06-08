using System;
using System.Runtime.InteropServices;

namespace OpenVRStartup
{
    internal static class Utils
    {
        public static void PrintColor(string text, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Print(string text)
        {
            PrintColor(text, ConsoleColor.White);
        }

        public static void PrintVerbose(string text)
        {
            PrintColor(text, ConsoleColor.Gray);
        }

        public static void PrintDebug(string text)
        {
            PrintColor(text, ConsoleColor.Cyan);
        }

        public static void PrintInfo(string text)
        {
            PrintColor(text, ConsoleColor.Green);
        }

        public static void PrintWarning(string text)
        {
            PrintColor(text, ConsoleColor.Yellow);
        }

        public static void PrintError(string text)
        {
            PrintColor(text, ConsoleColor.Red);
        }

        public static uint SizeOf(object o)
        {
            return (uint)Marshal.SizeOf(o);
        }
    }
}