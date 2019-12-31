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

namespace Loader.Application.Configuration
{
    public static class AppConfiguration
    {
        public static void DoAppConfiguration(IApplicationBuilder app, IHostingEnvironment env)
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


            app.UseHangfireDashboard("/services/job");

            app.UseHealthChecks("/services/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions());
            app.UseHealthChecksUI(config => config.UIPath = "/services/health-ui");
            app.UseElmah();

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "/services/swagger/{documentname}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/services/swagger/v1/swagger.json", "Loader Swagger API");
                c.RoutePrefix = "services/swagger";

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

            
        }
    }
}
