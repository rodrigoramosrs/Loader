using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Loader.Infra.Manager
{
    

    public class MemoryManager
    {
        public class MemoryMetrics
        {

            public long Total;

            public string TotalFormatted
            {
                get
                {
                    return MemoryManager.GetSize(this.Total);
                }

            }




            public long Used;

            public string UsedFormatted
            {
                get
                {
                    return MemoryManager.GetSize(this.Used);

                }

            }

            public double UsedInPercent
            {
                get
                {
                    return Math.Round((double)(this.Used * 100) / this.Total, 2);
                }

            }

            public long Free;

            public string FreeFormatted
            {
                get
                {
                    return MemoryManager.GetSize(this.Free);
                }

            }

            public double FreeInPercent
            {
                get
                {
                    return Math.Round((double)(this.Free * 100) / this.Total, 2);
                }

            }





        }

        public MemoryMetrics GetMetrics()
        {
            if (IsUnix())
            {
                return GetUnixMetrics();
            }

            return GetWindowsMetrics();
        }

        private bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            return isUnix;
        }

        private MemoryMetrics GetWindowsMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo();
            info.FileName = "wmic";
            info.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Trim().Split("\n");
            var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
            var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetrics();
            metrics.Total = long.Parse(totalMemoryParts[1]) * 1024; // Math.Round(/ 1024, 2);
            metrics.Free = long.Parse(freeMemoryParts[1]) * 1024;// Math.Round( / 1024, 2);
            metrics.Used = metrics.Total - metrics.Free;

            return metrics;
        }

        private MemoryMetrics GetUnixMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo("free -m");
            info.FileName = "/bin/bash";
            info.Arguments = "-c \"free -m\"";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }

            var lines = output.Split("\n");
            var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetrics();
            metrics.Total = long.Parse(memory[1]);
            metrics.Used = long.Parse(memory[2]);
            metrics.Free = long.Parse(memory[3]);

            return metrics;
        }

        public static string GetSize(long bytes)
        {
            if (bytes > 1073741824)
                return Math.Ceiling(bytes / 1073741824M).ToString("#,### GB");
            else if (bytes > 1048576)
                return Math.Ceiling(bytes / 1048576M).ToString("#,### MB");
            else if (bytes >= 1)
                return Math.Ceiling(bytes / 1024M).ToString("#,### KB");
            else if (bytes < 0)
                return "";
            else
                return bytes.ToString("#,### B");

            /*
            string postfix = "Bytes";
            long result = size;
            if (size >= 1073741824)//more than 1 GB
            {
                result = size / 1073741824;
                postfix = "GB";
            }
            else if (size >= 1048576)//more that 1 MB
            {
                result = size / 1048576;
                postfix = "MB";
            }
            else if (size >= 1024)//more that 1 KB
            {
                result = size / 1024;
                postfix = "KB";
            }

            return result.ToString("F2") + " " + postfix;*/
        }
    }
}


