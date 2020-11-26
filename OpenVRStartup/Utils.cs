using System;

namespace OpenVRStartup
{
    static class Utils
    {
        public static void PrintColor(String text, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Print(String text)
        {
            PrintColor(text, ConsoleColor.White);
        }
        public static void PrintVerbose(String text)
        {
            PrintColor(text, ConsoleColor.Gray);
        }
        public static void PrintDebug(String text)
        {
            PrintColor(text, ConsoleColor.Cyan);
        }
        public static void PrintInfo(String text)
        {
            PrintColor(text, ConsoleColor.Green);
        }
        public static void PrintWarning(String text)
        {
            PrintColor(text, ConsoleColor.Yellow);
        }
        public static void PrintError(String text)
        {
            PrintColor(text, ConsoleColor.Red);
        }

        public static uint SizeOf(object o)
        {
            return (uint)System.Runtime.InteropServices.Marshal.SizeOf(o);
        }
    }
}
