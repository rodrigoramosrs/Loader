using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ElmahCore.Mvc;
using ElmahCore;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Storage.SQLite;
using Hangfire.Logging;
using Hangfire.Logging.LogProviders;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Loader.Service.Services;
using Loader.Service.Services.Analytics;
using Loader.Application.Middleware.Log;
using Loader.Application.Middleware.Request;
using Loader.Service.Services.License;
using Loader.Service.Services.Job;
using System.Threading;

namespace Loader.Application
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment _env { get; }

        public BaseAnalyticsService BaseAnalyticsService { get; private set; }

        protected DateTime StartupDateTime = DateTime.Now;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Application.Configuration.ServiceConfiguration.DoConfiguration(services, Configuration, _env);
            this.BaseAnalyticsService = services.BuildServiceProvider().GetService<BaseAnalyticsService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IApplicationLifetime applicationLifetime, JobService JobService, BaseAnalyticsService AnalyticsService, Service.Services.ComputerMonitor.ComputerMonitorService ComputerMonitorService)
        {
            applicationLifetime.ApplicationStopping.Register(OnShutdown);
            applicationLifetime.ApplicationStarted.Register(OnStarted);

            Loader.Application.Configuration.AppConfiguration.DoAppConfiguration(app, _env);
            //Limpando todos os status anteriormente pendentes na inicialização
            JobService.ClearAllJobStatus();
            ComputerMonitorService.RegisterBackgroundJobs();
            this.RegisterHangfireTasks();

        }

        private async void OnStarted()
        {
#if !DEBUG
            this.BaseAnalyticsService.SendInformation("Loader.Startup", $"Loader version {this.GetType().Assembly.GetName().Version.ToString()} started at {this.StartupDateTime.ToString("dd/MM/yyyy HH:mm:ss")}");
#endif
        }
        private async void OnShutdown()
        {
            //this code is called when the application stops
#if !DEBUG
            TimeSpan AppLifeTime = DateTime.Now.Subtract(StartupDateTime);
            string FormattedLifeTime = string.Format("{0}d:{1:D2}h:{2:D2}m:{3:D2}s:{4:D3}ms",
                        AppLifeTime.Days,
                        AppLifeTime.Hours,
                        AppLifeTime.Minutes,
                        AppLifeTime.Seconds,
                        AppLifeTime.Milliseconds);
            await this.BaseAnalyticsService.SendInformation("Loader.Shutdown", $"Loader version {this.GetType().Assembly.GetName().Version.ToString()} stoped at {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}. Lifetime is {FormattedLifeTime}");
#endif
        }

        private void RegisterHangfireTasks()
        {
            RecurringJob.AddOrUpdate<BaseLicenseService>(
            "LICENSE-CHECK",
            s =>  s.SilentValidadeLicense(),
           //"*/10 * * * * *",
            Cron.Hourly,
            TimeZoneInfo.Local);

            RecurringJob.AddOrUpdate<UpdateService>(
            "VERSION-CHECK",
            s => s.DoSilentVersionValidation(),
            //"*/10 * * * * *",
            Cron.Hourly,
            TimeZoneInfo.Local);

            RecurringJob.AddOrUpdate<BaseAnalyticsService>(
               "FLUSH-ANALYTICS-DATA",
               s => s.FlushData(),
               //"*/10 * * * * *",
               Cron.Minutely,
               TimeZoneInfo.Local);

           
        }
    }
}
