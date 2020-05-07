using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Loader.Infra.Manager
{
    

    public class CpuManager
    {
        public class CpuMetrics
        {

            public int LoadPercentage;




        }

        public CpuMetrics GetMetrics()
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

        private CpuMetrics GetWindowsMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo();
            info.FileName = "wmic";
            info.Arguments = "cpu get loadpercentage /value";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Trim().Split("\n");
            var totalPercentage = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
            //var totalPercentage = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

            var metrics = new CpuMetrics();
            try
            {
                metrics.LoadPercentage = int.Parse(totalPercentage[1]); // Math.Round(/ 1024, 2);
            }
            catch (Exception)
            {
                metrics.LoadPercentage = - 1;
            }
            
            
            return metrics;
        }

        private CpuMetrics GetUnixMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo("Iostat -c");
            info.FileName = "/bin/bash";
            info.Arguments = "-c \"Iostat -c\"";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }

            var lines = output.Split("\n");
            var cpu = lines[2].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            var metrics = new CpuMetrics();
            metrics.LoadPercentage = int.Parse(cpu[0]);

            return metrics;
        }

    }
}


