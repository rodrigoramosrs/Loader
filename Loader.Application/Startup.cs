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
            GlobalConfiguration.Configuration.UseLogProvider(new Loader.Application.Middleware.Log.ElmahLogProvider());
            GlobalJobFilters.Filters.Add(new HangfireElmhaJobExceptionFilter());

            services.AddSingleton<Loader.Service.Services.Configuration.ConfigurationService, Loader.Service.Services.Configuration.ConfigurationService>(serviceProvider =>
            {
                return new Loader.Service.Services.Configuration.ConfigurationService(Configuration);
            });

            services.AddSingleton<Service.Services.Analytics.BaseAnalyticsService, Service.Services.Analytics.BaseAnalyticsService>(serviceProvider =>
            {
                var ConfigurationService = serviceProvider.GetService<Service.Services.Configuration.ConfigurationService>();
                var AnalyticsConfiguration = ConfigurationService.AnalyticsConfiguration;
                var CustomerConfiguration = ConfigurationService.CustomerConfiguration;
                return new Service.Services.Analytics.GoogleAnalyticsService(
                    AnalyticsConfiguration.ID,
                    AnalyticsConfiguration.ExceptionID,
                    CustomerConfiguration.Name,
                    CustomerConfiguration.ID);
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info() { Title = "Loader Swagger API", Version = "v1" });
            });

            services.AddTransient<Service.Services.UpdateService, Service.Services.UpdateService>(serviceProvider =>
            {
                var analyticsService = serviceProvider.GetService<Service.Services.Analytics.BaseAnalyticsService>();
                var JobService = serviceProvider.GetService < Service.Services.Job.JobService>();
                
                return new Service.Services.UpdateService(new Infra.Data.Repository.UpdateRepository(_env.ContentRootPath), analyticsService, JobService);
            });

           
            services.AddSingleton<Service.Services.Job.JobService, Service.Services.Job.JobService>(serviceProvider =>
            {
                var analyticsService = serviceProvider.GetService<Service.Services.Analytics.BaseAnalyticsService>();
                var jobRepository = new Infra.Data.Repository.JobRepository();
                return new Service.Services.Job.JobService(analyticsService, jobRepository);
            });

            services.AddSingleton<Service.Services.License.BaseLicenseService, Service.Services.License.BaseLicenseService>(serviceProvider =>
            {
                var analyticsService = serviceProvider.GetService<Service.Services.Analytics.BaseAnalyticsService>();
                var updateService = serviceProvider.GetService<Service.Services.UpdateService>();
                var ConfigurationService = serviceProvider.GetService<Service.Services.Configuration.ConfigurationService>();

                var AnalyticsConfiguration = ConfigurationService.AnalyticsConfiguration;
                var CustomerConfiguration = ConfigurationService.CustomerConfiguration;

                if (ConfigurationService.CustomerConfiguration.CheckLicense)
                    return new Service.Services.License.BaseLicenseService(analyticsService, updateService,
                        CustomerConfiguration.ID, CustomerConfiguration.Name);

                return new Service.Services.License.MVLicenseService(analyticsService, updateService, CustomerConfiguration.ID, CustomerConfiguration.Name);

            });


            //Configurando o Log do hangfire para elmah
            services.AddElmah<XmlFileErrorLog>(options =>
            {
                options.LogPath = @".\log";
                options.Path = "log";
                options.ApplicationName = "Loader.Application";
                options.Notifiers.Add(new Middleware.Log.ElmahLogAnalyticsNotifier(services.BuildServiceProvider().GetService<Loader.Service.Services.Analytics.BaseAnalyticsService>()));
            });

            services.AddHealthChecks();
            services.AddHealthChecksUI();

            services.AddHangfire(configuration => configuration
               .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
               .UseSimpleAssemblyNameTypeSerializer()
               .UseRecommendedSerializerSettings()
               .UseSQLiteStorage());

            services.AddHangfireServer();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IBackgroundJobClient backgroundJobs, IRecurringJobManager recurringJobManager,
            ILoggerFactory LoggerFactory, IHostingEnvironment env, BaseAnalyticsService AnalyticsService, BaseLicenseService LicenseService, UpdateService UpdateService, JobService JobService)
        {
            

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            
            app.UseHangfireDashboard("/job");

            app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions());
            app.UseHealthChecksUI(config => config.UIPath = "/health-ui");
            app.UseElmah();
            
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Loader Swagger API");
                
            });

            //app.UseMiddleware<RequestResponseLoggingMiddleware>(AnalyticsService);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

            //Limpando todos os status anteriormente pendentes na inicialização
            JobService.ClearAllJobStatus();

            this.RegisterHangfireTasks();
           /* recurringJobManager.AddOrUpdate("LICENSE-CHECK", () =>
                LicenseService.SilentValidadeLicense()
            , "10 * * * * *"); 10 seconds Cron.MinuteInterval(1));*/
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
