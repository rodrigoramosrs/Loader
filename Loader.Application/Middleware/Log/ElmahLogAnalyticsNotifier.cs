﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElmahCore;
using Loader.Service.Services.Analytics;

namespace Loader.Application.Middleware.Log
{
    public class ElmahLogAnalyticsNotifier : ElmahCore.IErrorNotifier
    {
        public string Name { get { return "Loader.Application.Middleware.Log.ElmahLogNotifier"; } }

        private readonly BaseAnalyticsService _AnalyticsService;

        public ElmahLogAnalyticsNotifier(BaseAnalyticsService AnalyticsService)
        {
            this._AnalyticsService = AnalyticsService;
        }

        public void Notify(Error error)
        {
            this._AnalyticsService.SendException($"ElmahLogAnalyticsNotifier - {error.Message}", error.Exception);
            

            /* this._AnalyticsService.SendInformation(new Domain.Models.Analytics.AnalyticsInformationData()
            {
                Name = error.ToString() + ".\r\n" + error.Exception.ToString(),
                Category = "Loader.Application.Middleware.ElmahLogNotifier.Exception",
                Description = error.ToString() + ".\r\n" + error.Exception.ToString()
            }
            );*/
        }
    }
}
