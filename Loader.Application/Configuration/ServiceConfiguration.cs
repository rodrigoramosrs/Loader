using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ElmahCore.Mvc;
using ElmahCore;
using Hangfire;
using Hangfire.Storage.SQLite;
using Loader.Application.Middleware.Log;

namespace Loader.Application.Configuration
{
    public static class ServiceConfiguration
    {
        public static void DoConfiguration(IServiceCollection services, IConfiguration Configuration, IHostingEnvironment _env)
        {
            DoMvcServiceConfiguration(services);
            DoSpaServiceConfiguration(services);

            DoConfigurationServiceConfiguration(services, Configuration);
            DoAnalyticsServiceConfiguration(services);
            DoSwaggerServiceConfiguration(services);
            DoUpdateServiceConfiguration(services, _env);
            DoJobServiceConfiguration(services);
            DoLicenseServiceConfiguration(services);
            DoElmahServiceConfiguration(services);
            DoHealthChecksServiceConfiguration(services);
            DoHangFireServiceConfiguration(services);
            DoComputerMonitorServiceConfiguration(services);
        }

        private static void DoHealthChecksServiceConfiguration(IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddHealthChecksUI();
        }

        private static void DoSpaServiceConfiguration(IServiceCollection services)
        {
            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        private static void DoMvcServiceConfiguration(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        private static void DoElmahServiceConfiguration(IServiceCollection services)
        {
            GlobalConfiguration.Configuration.UseLogProvider(new Loader.Application.Middleware.Log.ElmahLogProvider());
            //Configurando o Log do hangfire para elmah
            services.AddElmah<XmlFileErrorLog>(options =>
            {
                options.LogPath = @".\log";
                options.Path = "services/log";
                options.ApplicationName = "Loader.Application";
                options.Notifiers.Add(new Middleware.Log.ElmahLogAnalyticsNotifier(services.BuildServiceProvider().GetService<Loader.Service.Services.Analytics.BaseAnalyticsService>()));
            });

        }

        private static void DoConfigurationServiceConfiguration(IServiceCollection services, IConfiguration Configuration)
        {
            services.AddSingleton<Loader.Service.Services.Configuration.ConfigurationService, Loader.Service.Services.Configuration.ConfigurationService>(serviceProvider =>
            {
                return new Loader.Service.Services.Configuration.ConfigurationService(Configuration);
            });
        }

        private static void DoSwaggerServiceConfiguration(IServiceCollection services)
        {
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info() { Title = "Loader Swagger API", Version = "v1" });
            });
        }

        private static void DoHangFireServiceConfiguration(IServiceCollection services)
        {

            services.AddHangfire(configuration => configuration
                           .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                           .UseSimpleAssemblyNameTypeSerializer()
                           .UseRecommendedSerializerSettings()
                           .UseSQLiteStorage());

            services.AddHangfireServer();
            GlobalJobFilters.Filters.Add(new HangfireElmhaJobExceptionFilter());
        }

        private static void DoLicenseServiceConfiguration(IServiceCollection services)
        {
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
        }

        private static void DoJobServiceConfiguration(IServiceCollection services)
        {
            services.AddSingleton<Service.Services.Job.JobService, Service.Services.Job.JobService>(serviceProvider =>
            {
                var analyticsService = serviceProvider.GetService<Service.Services.Analytics.BaseAnalyticsService>();
                var jobRepository = new Infra.Data.Repository.JobRepository();
                return new Service.Services.Job.JobService(analyticsService, jobRepository);
            });
        }

        private static void DoUpdateServiceConfiguration(IServiceCollection services, IHostingEnvironment _env)
        {
            services.AddTransient<Service.Services.UpdateService, Service.Services.UpdateService>(serviceProvider =>
            {
                var analyticsService = serviceProvider.GetService<Service.Services.Analytics.BaseAnalyticsService>();
                var JobService = serviceProvider.GetService<Service.Services.Job.JobService>();

                return new Service.Services.UpdateService(new Infra.Data.Repository.UpdateRepository(_env.ContentRootPath), analyticsService, JobService);
            });
        }

        private static void DoAnalyticsServiceConfiguration(IServiceCollection services)
        {
            services.AddSingleton<Service.Services.Analytics.BaseAnalyticsService, Service.Services.Analytics.BaseAnalyticsService>(serviceProvider =>
            {
                var ConfigurationService = serviceProvider.GetService<Service.Services.Configuration.ConfigurationService>();
                var AnalyticsConfiguration = ConfigurationService.AnalyticsConfiguration;
                var CustomerConfiguration = ConfigurationService.CustomerConfiguration;
                Service.Services.Analytics.BaseAnalyticsService baseService;
                switch (AnalyticsConfiguration.Provider)
                {

                    case Domain.Models.Configuration.Types.AnalyticsConfigurationProviderTypes.MV:
                        baseService = new Service.Services.Analytics.MVAnalyticsService(
                           AnalyticsConfiguration.GoogleProvider.ID,
                           AnalyticsConfiguration.GoogleProvider.ExceptionID,
                           CustomerConfiguration.Name,
                           CustomerConfiguration.ID,
                           AnalyticsConfiguration.SaveAnalyticsToFile);
                        break;
                    case Domain.Models.Configuration.Types.AnalyticsConfigurationProviderTypes.Google:
                    default:
                        baseService = new Service.Services.Analytics.GoogleAnalyticsService(
                           AnalyticsConfiguration.GoogleProvider.ID,
                           AnalyticsConfiguration.GoogleProvider.ExceptionID,
                           CustomerConfiguration.Name,
                           CustomerConfiguration.ID,
                           AnalyticsConfiguration.SaveAnalyticsToFile);
                        break;
                }

                return baseService;
            });
        }

        private static void DoComputerMonitorServiceConfiguration(IServiceCollection services)
        {
            services.AddSingleton<Service.Services.ComputerMonitor.ComputerMonitorService, Service.Services.ComputerMonitor.ComputerMonitorService>(serviceProvider =>
            {
                var analyticsService = serviceProvider.GetService<Service.Services.Analytics.BaseAnalyticsService>();
                var JobService = serviceProvider.GetService<Service.Services.Job.JobService>();
                var ConfigurationService = serviceProvider.GetService<Service.Services.Configuration.ConfigurationService>();
                return new Service.Services.ComputerMonitor.ComputerMonitorService(analyticsService, JobService, ConfigurationService);
            });
        }
        
    }
}
