﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Loader.Domain.Models;
using Loader.Domain.Models.Update;
using Loader.Service.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Loader.Application.Controllers
{
    [Route("api/[controller]")]
    public class UpdateJobController : Controller
    {

        private readonly Service.Services.UpdateService _UpdateService;
        private readonly Service.Services.Analytics.BaseAnalyticsService _AnalyticsService;
        
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly IHostingEnvironment _hostingEnvironment;

        public UpdateJobController(Service.Services.UpdateService UpdateService, Service.Services.Analytics.BaseAnalyticsService AnalyticsService, IBackgroundJobClient backgroundJobs, IHostingEnvironment hostingEnvironment)
        {
            _UpdateService = UpdateService;
            _AnalyticsService = AnalyticsService;
            _backgroundJobs = backgroundJobs;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("[action]")]
        public object GetUpdateInstructionList()
        {
            this.DoAnalytics("UpdateJobController", "GetUpdateInstructionList","", "GetUpdateInstructionList");
            return _UpdateService.GetUpdateInstructionList();
        }
        [HttpGet("[action]")]
        public object GetUpdateUpdateInstructionByID(string id)
        {
            var returnData = _UpdateService.GetUpdateInstructionByID(new Guid(id));
            this.DoAnalytics("UpdateJobController", "GetUpdateUpdateInstructionByID", "", $"Getting update instruction for id {returnData.Name}");
            return returnData;
        }

        [HttpGet("[action]")]
        public object GetUpdateEntry(string id)
        {
            var instruction = _UpdateService.GetUpdateInstructionByID(new Guid(id));
            this.DoAnalytics("UpdateJobController", "GetUpdateEntry", "", $"Getting update entry for {instruction.Name}");
            return _UpdateService.HasUpdate(instruction);
        }

        [HttpPost("[action]")]
        public object DoUpdate([FromBody]UpdateInstruction updateInstruction)
        {
            UpdateInstruction instruction = _UpdateService.GetUpdateInstructionByID(updateInstruction.ID);
            string ReturnData = _backgroundJobs.Enqueue(() => _UpdateService.DoUpdate(instruction));
            this.DoAnalytics("UpdateJobController", "DoUpdate", "", $"Rolling update for '{instruction.Name}'. Schedule number is " + ReturnData);

            return ReturnData;
        }


        [HttpGet("[action]")]
        public object GetUpdateBackupEntryList(string id)
        {
            
            UpdateInstruction instruction = _UpdateService.GetUpdateInstructionByID(new Guid(id));
            this.DoAnalytics("UpdateJobController", "GetUpdateBackupEntryList", "", $"Getting backup list for '{instruction.Name}'");
            return _UpdateService.GetUpdateBackupEntryList(instruction);
        }

        [HttpGet("[action]")]
        public object GetUpdateHistory(string id)
        {
            var updateInstruction = _UpdateService.GetUpdateInstructionByID(new Guid(id));
            this.DoAnalytics("UpdateJobController", "GetUpdateHistory", "", $"Getting update list for '{updateInstruction.Name}'");
            var resultData = _UpdateService.GetUpdateHistory(updateInstruction);
            return resultData;
        }

        [HttpPost("[action]")]
        public object DoRollback([FromBody]UpdateInstruction updateInstruction)
        {
            UpdateInstruction instruction = _UpdateService.GetUpdateInstructionByID(updateInstruction.ID);
            this.DoAnalytics("UpdateJobController", "GetUpdateHistory", "", $"Rolling back version of '{updateInstruction.Name}'");
            return _backgroundJobs.Enqueue(() => _UpdateService.DoRollback(instruction, new UpdateBackupEntry("","")));
        }


        private async void DoAnalytics(string Category, string Action, string Label, string Description)
        {
            var modelData = new Domain.Models.Analytics.AnalyticsData() { Name = Description, Category = $"Loader.Application.{Category}.{Action}",  Description = Description };
            Task.Run(() => this._AnalyticsService.Send(modelData) );
        }




    }
}
