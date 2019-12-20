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
            
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage());
            
            //Configurando o Log do hangfire para elmah

            services.AddHangfireServer();

            services.AddHealthChecks();
            services.AddHealthChecksUI();
            services.AddElmah<XmlFileErrorLog>(options => 
            {
                options.LogPath = @".\log";
                options.Path = "log";
                options.ApplicationName = "Loader.Application";
                options.Notifiers.Add(new Middleware.Log.ElmahLogAnalyticsNotifier(new Service.Services.Analytics.GoogleAnalyticsService(Configuration["Analytics:ID"], Configuration["Customer:Name"], Configuration["Customer:ID"])));
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info() { Title = "Loader Swagger API", Version = "v1" });
            });

            services.AddTransient<Service.Services.UpdateService, Service.Services.UpdateService>(serviceProvider =>
            {
                return new Service.Services.UpdateService(new Infra.Data.Repository.UpdateRepository(_env.ContentRootPath));
            });

            services.AddSingleton<Service.Services.Analytics.BaseAnalyticsService, Service.Services.Analytics.BaseAnalyticsService>(serviceProvider =>
            {
                return new Service.Services.Analytics.GoogleAnalyticsService(Configuration["Analytics:ID"], Configuration["Customer:Name"], Configuration["Customer:ID"]);
            });

            services.AddSingleton<Service.Services.License.BaseLicenseService, Service.Services.License.BaseLicenseService>(serviceProvider =>
            {
                var analyticsService = serviceProvider.GetService<Service.Services.Analytics.BaseAnalyticsService>();
                var updateService = serviceProvider.GetService<Service.Services.UpdateService>();

                if (Convert.ToBoolean(Configuration["Customer:CheckLicense"]))
                    return new Service.Services.License.BaseLicenseService(analyticsService, updateService, Configuration["Customer:ID"], Configuration["Customer:Name"]);

                return new Service.Services.License.MVLicenseService(analyticsService, updateService, Configuration["Customer:ID"], Configuration["Customer:Name"]);

            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IBackgroundJobClient backgroundJobs, IRecurringJobManager recurringJobManager,
            ILoggerFactory LoggerFactory, IHostingEnvironment env, BaseAnalyticsService AnalyticsService, BaseLicenseService LicenseService)
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
        }
    }
}
