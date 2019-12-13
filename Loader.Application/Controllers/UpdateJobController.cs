using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Loader.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Loader.Application.Controllers
{
    [Route("api/[controller]")]
    public class UpdateJobController : Controller
    {

        private readonly Service.Services.UpdateService _UpdateService;
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly IHostingEnvironment _hostingEnvironment;

        public UpdateJobController(Service.Services.UpdateService UpdateService, IBackgroundJobClient backgroundJobs, IHostingEnvironment hostingEnvironment)
        {
            _UpdateService = UpdateService;
            _backgroundJobs = backgroundJobs;
            _hostingEnvironment = hostingEnvironment;

            
        }

        [HttpGet("[action]")]
        public object GetUpdateInstructionList()
        {
            return _UpdateService.GetUpdateInstructionList();
        }
        [HttpGet("[action]")]
        public object GetUpdateUpdateInstructionByID(string id)
        {
            return _UpdateService.GetUpdateInstructionByID(new Guid(id));
        }

        [HttpGet("[action]")]
        public object GetUpdateEntry(string id)
        {
            var instruction = _UpdateService.GetUpdateInstructionByID(new Guid(id));
            return _UpdateService.HasUpdate(instruction);
        }

        [HttpPost("[action]")]
        public object DoUpdate([FromBody]UpdateInstruction updateInstruction)
        {
            UpdateInstruction instruction = _UpdateService.GetUpdateInstructionByID(updateInstruction.ID);
            return _backgroundJobs.Enqueue(() => _UpdateService.DoUpdate(instruction));
        }


        [HttpGet("[action]")]
        public object GetUpdateBackupEntryList(string id)
        {
            UpdateInstruction instruction = _UpdateService.GetUpdateInstructionByID(new Guid(id));
            return _UpdateService.GetUpdateBackupEntryList(instruction);
        }

        [HttpGet("[action]")]
        public object GetUpdateHistory(string id)
        {
            var updateInstruction = _UpdateService.GetUpdateInstructionByID(new Guid(id));
            var resultData = _UpdateService.GetUpdateHistory(updateInstruction);
            return resultData;
        }

        [HttpPost("[action]")]
        public object DoRollback([FromBody]UpdateInstruction updateInstruction)
        {
            UpdateInstruction instruction = _UpdateService.GetUpdateInstructionByID(updateInstruction.ID);
            return _backgroundJobs.Enqueue(() => _UpdateService.DoRollback(instruction, new UpdateBackupEntry("","")));
        }




    }
}
