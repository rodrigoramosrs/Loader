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

namespace Loader.Application
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment _env { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Application.Configuration.ServiceConfiguration.DoConfiguration(services, Configuration, _env);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IBackgroundJobClient backgroundJobs, IRecurringJobManager recurringJobManager,
            ILoggerFactory LoggerFactory, IHostingEnvironment env, BaseAnalyticsService AnalyticsService, BaseLicenseService LicenseService, UpdateService UpdateService, JobService JobService)
        {
            Loader.Application.Configuration.AppConfiguration.DoAppConfiguration(app, _env);
            
            //Limpando todos os status anteriormente pendentes na inicialização
            JobService.ClearAllJobStatus();
            this.RegisterHangfireTasks();
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
        }
    }
}
