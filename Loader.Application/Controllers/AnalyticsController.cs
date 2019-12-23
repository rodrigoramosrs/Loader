using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Loader.Domain.Models.Analytics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Loader.Application.Controllers
{
    [Route("api/[controller]")]
    public class AnalyticsController : Controller
    {
        private readonly Service.Services.UpdateService _UpdateService;
        private readonly Service.Services.Analytics.BaseAnalyticsService _AnalyticsService;

        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly IHostingEnvironment _hostingEnvironment;

        public AnalyticsController( Service.Services.Analytics.BaseAnalyticsService AnalyticsService, IBackgroundJobClient backgroundJobs, IHostingEnvironment hostingEnvironment)
        {
            _AnalyticsService = AnalyticsService;
            _backgroundJobs = backgroundJobs;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("[action]")]
        public object Send(AnalyticsInformationData AnalyticsData)
        {
            return this._AnalyticsService.SendInformation($"{AnalyticsData.Category}.{AnalyticsData.ActionName}" , AnalyticsData.Description).Result;
        }
    }
}