using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVRStartup
{
    internal static class LogUtils
    {
        static List<string> cache = [];

        public static void WriteLineToCache(string line)
        {
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            cache.Add($"{time} {line}");
        }

        static public void FlushCache()
        {
            cache.Clear();
        }

        public static bool LogFileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public static void WriteCacheToLogFile(string filePath, int lineLimit)
        {
            var linesArr = File.Exists(filePath) ? File.ReadAllLines(filePath) : [];
            var linesList = linesArr.ToList();
            linesList.AddRange(cache);
            if (linesList.Count > lineLimit)
            {
                linesList.RemoveRange(0, linesList.Count - lineLimit);
                linesList.Insert(0, $"(Log is limited to {lineLimit} lines and has been truncated)");
            }

            File.WriteAllText(filePath, string.Join("\n", linesList));
            FlushCache();
        }
    }
}