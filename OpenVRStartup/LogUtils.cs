using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVRStartup
{

    static class LogUtils
    {
        static List<string> cache = new List<string>();

        static public void WriteLineToCache(string line) {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            cache.Add($"{time} {line}");
        }

        static public void FlushCache() {
            cache.Clear();
        }

        static public bool LogFileExists(string filePath) {
            return File.Exists(filePath);
        }

        static public void WriteCacheToLogFile(string filePath, int lineLimit) {
            var linesArr = File.Exists(filePath) ? File.ReadAllLines(filePath) : new string[0];
            var linesList = linesArr.ToList();
            linesList.AddRange(cache);
            if (linesList.Count > lineLimit)
            {
                linesList.RemoveRange(0, linesList.Count-lineLimit);
                linesList.Insert(0, $"(Log is limited to {lineLimit} lines and has been truncated)");
            }
            File.WriteAllText(filePath, string.Join("\n", linesList));
            FlushCache();
        }
    }
}
