using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Loader.Helper
{
    public static class LogHelper
    {
        private static object LockerObject = new object();
        private static string GetUniquePrefixFilename()
        {
            return DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
        }
        public static void WriteErrorLog(string content)
        {
            lock (LockerObject)
            {
                string directory = Path.Combine(ConfigurationManager.RootPath, "error_log");

                if (Directory.Exists(directory)) Directory.CreateDirectory(directory);

                File.WriteAllText(directory + "\\exception_" + GetUniquePrefixFilename() + ".log", content);
            }
            
        }
    }
}
