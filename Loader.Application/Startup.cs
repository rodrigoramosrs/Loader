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

            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage());
            
            //Configurando o Log do hangfire para elmah

            services.AddHangfireServer();
            services.AddTransient<Service.Services.UpdateService, Service.Services.UpdateService>(serviceProvider =>
            {
                return new Service.Services.UpdateService(new Infra.Data.Repository.UpdateRepository(_env.ContentRootPath));
            });

            services.AddTransient<Service.Services.Analytics.BaseAnalyticsService, Service.Services.Analytics.BaseAnalyticsService>(serviceProvider =>
            {
                return new Service.Services.Analytics.GoogleAnalyticsService(Configuration["Analytics:ID"], Configuration["Customer:Name"], Configuration["Customer:ID"]);
            });

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


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IBackgroundJobClient backgroundJobs, IRecurringJobManager recurringJobManager, ILoggerFactory LoggerFactory, IHostingEnvironment env, BaseAnalyticsService AnalyticsService)
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

            app.UseMiddleware<RequestResponseLoggingMiddleware>(AnalyticsService);

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

            recurringJobManager.AddOrUpdate("update", () => 
                Console.WriteLine()
            , Cron.Hourly);
        }
    }
}
