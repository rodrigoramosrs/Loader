using Hangfire;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Loader.Service.Services.ComputerMonitor
{
    public class ComputerMonitorService
    {

        private readonly Analytics.BaseAnalyticsService _AnalyticsService;
        private readonly Job.JobService _JobService;
        public ComputerMonitorService(Analytics.BaseAnalyticsService AnalyticsService, Job.JobService JobService)
        {
            _JobService = JobService;
            _AnalyticsService = AnalyticsService;
        }

        public void RegisterBackgroundJobs()
        {
            RecurringJob.AddOrUpdate<ComputerMonitorService>(
              "VALIDATE-COMPUTER-HEALTH",
              s => s.ValidateComputerHealth(),
              //"*/10 * * * * *",
              Cron.Minutely,
              TimeZoneInfo.Local);
        }
        public async Task ValidateComputerHealth()
        {
            StringBuilder message = new StringBuilder();
            Infra.Manager.MemoryManager memoryManager = new Infra.Manager.MemoryManager();
            Infra.Manager.DiskManager diskManager = new Infra.Manager.DiskManager();
            Infra.Manager.CpuManager cpuManager = new Infra.Manager.CpuManager();
            Infra.Manager.NetworkManager networkManager = new Infra.Manager.NetworkManager();

            var memoryInformation = memoryManager.GetMetrics();
            var diskInformation = diskManager.CheckAllDisksSpace();
            var cpuInformation = cpuManager.GetMetrics();
            //var network = networkManager.GetMetrics();

            message.AppendLine($"[CPU: {cpuInformation.LoadPercentage} % " + ((cpuInformation.LoadPercentage > 70  ) ? " [WARNING] " : "") + "]");
            message.AppendLine($"[RAM: total {memoryInformation.TotalFormatted} | used {memoryInformation.UsedFormatted} | free {memoryInformation.FreeFormatted } - {memoryInformation.FreeInPercent } %" + ((memoryInformation.FreeInPercent < 20  ) ? " [WARNING] " : "") + "]");
            foreach (var disk in diskInformation)
            {
                message.AppendLine($"[HD {disk.DriveLetter}: total {disk.TotalFormatted} used {disk.UsedFormatted} | free {disk.FreeFormatted } - {disk.FreeInPercent } %" + ((disk.FreeInPercent < 20) ? " [WARNING] " : "") + "]");
            }


            _AnalyticsService.SendInformation("LOADER.COMPUTER.HEALTH", message.ToString());

            //return Task.FromResult(true);
        }
    }
}
