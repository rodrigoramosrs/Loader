using System;
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
        public class RollbackPayload
        {
            public Guid RollbackUpdateID { get; set; }
            public Guid UpdateInstructionID { get; set; }
        }

        public class UpdatePayload
        {
            public Guid UpdateInstructionID { get; set; }
        }

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
            this.DoAnalytics("GetUpdateInstructionList", "Getting List of UpdateInstructions");
            return _UpdateService.GetUpdateInstructionList();
        }
        [HttpGet("[action]")]
        public object GetUpdateUpdateInstructionByID(string id)
        {
            var returnData = _UpdateService.GetUpdateInstructionByID(new Guid(id));
            this.DoAnalytics("GetUpdateUpdateInstructionByID",  $"Getting update instruction for '{returnData.Name}'");
            return returnData;
        }

        [HttpGet("[action]")]
        public object GetUpdateEntry(string id)
        {
            var instruction = _UpdateService.GetUpdateInstructionByID(new Guid(id));
            this.DoAnalytics("GetUpdateEntry",  $"Getting update entry for '{instruction.Name}'");
            return _UpdateService.HasUpdate(instruction);
        }

        [HttpPost("[action]")]
        public object DoUpdate([FromBody]UpdatePayload updatePayload)
        {
            UpdateInstruction instruction = _UpdateService.GetUpdateInstructionByID(updatePayload.UpdateInstructionID);

            string ReturnData = _UpdateService.DoScheduledUpdate(instruction); //_backgroundJobs.Enqueue(() => _UpdateService.DoUpdate(instruction));
            this.DoAnalytics("DoUpdate",  $"Rolling update for '{instruction.Name}'. Schedule number is '{ReturnData}'");

            return ReturnData;
        }


        [HttpGet("[action]")]
        public object GetUpdateBackupEntryList(string id)
        {
            
            UpdateInstruction instruction = _UpdateService.GetUpdateInstructionByID(new Guid(id));
            this.DoAnalytics("GetUpdateBackupEntryList",  $"Getting backup list for '{instruction.Name}'");
            return _UpdateService.GetUpdateBackupEntryList(instruction);
        }

        [HttpGet("[action]")]
        public object GetUpdateHistory(string id)
        {
            var updateInstruction = _UpdateService.GetUpdateInstructionByID(new Guid(id));
            this.DoAnalytics("GetUpdateHistory",  $"Getting update list for '{updateInstruction.Name}'");
            var resultData = _UpdateService.GetUpdateHistory(updateInstruction);
            return resultData;
        }

        [HttpPost("[action]")]
        public object DoRollback([FromBody]RollbackPayload rollbackPayload)
        {
            UpdateInstruction instructionData = _UpdateService.GetUpdateInstructionByID(rollbackPayload.UpdateInstructionID);
            string ReturnData = _UpdateService.DoScheduledRollback(rollbackPayload.UpdateInstructionID, rollbackPayload.RollbackUpdateID);

            this.DoAnalytics("DoRollback",  $"Rolling back version of '{instructionData.Name}'. Schedule number is '{ReturnData}'");
            return ReturnData;
            //return _backgroundJobs.Enqueue(() => _UpdateService.DoRollback(instruction, new UpdateBackupEntry("","")));
        }


        private async void DoAnalytics(string Action, string Description)
        {
            Task.Run(() => this._AnalyticsService.SendInformation($"UpdateJobController.{Action}", Description) );
        }




    }
}
