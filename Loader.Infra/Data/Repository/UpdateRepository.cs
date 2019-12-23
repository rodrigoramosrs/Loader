using Loader.Domain.Interfaces;
using Loader.Domain.Models;
using Loader.Domain.Models.Update;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Loader.Infra.Data.Repository
{
    public class UpdateRepository : IUpdateRepository
    {
        public UpdateRepository(string CurrentRootPath)
        {
            _CurrentRootPath = CurrentRootPath;
        }

        private readonly string _CurrentRootPath;
        private readonly string _LoaderDefinitionFileName = "LoaderDefinition.json";
        private readonly string _UpdateResultFilenameTemplate = "{updateid}_{updateinstructionid}_{datetime}.json";
        private readonly string _JobResultPath = ".\\job_result\\";
        private readonly string _JobStatusPath = ".\\job_status\\";

        private string GetUpdateResultFileName(Guid UpdateInstructionID,  Guid UpdateID, DateTime dateTime)
        {
            return _UpdateResultFilenameTemplate
                .Replace("{updateinstructionid}", UpdateInstructionID.ToString())
                .Replace("{updateid}", UpdateID.ToString())
                .Replace("{datetime}", dateTime.ToString("yyyy-M-dd_HH-mm-ss"));
        }

        public bool WriteUpdateInstructionResult(UpdateResult updateResult)
        {
            string Filename = $"{_JobResultPath}{this.GetUpdateResultFileName(updateResult.UpdateInstructionID, updateResult.ID, DateTime.Now)}";

            Directory.CreateDirectory(_JobResultPath);

            File.WriteAllText(Filename, JsonConvert.SerializeObject(updateResult, Formatting.Indented));

            return File.Exists(Filename);
        }


        public List<UpdateInstruction> GetUpdateInstructionList()
        {
            var JSON = System.IO.File.ReadAllText(Path.Combine(this._CurrentRootPath, this._LoaderDefinitionFileName));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<UpdateInstruction>>(JSON);
        }

        public UpdateInstruction GetUpdateInstructionByID(Guid id)
        {
            var JSON = System.IO.File.ReadAllText(Path.Combine(this._CurrentRootPath, this._LoaderDefinitionFileName));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<UpdateInstruction>>(JSON).Where(x => x.ID == id).FirstOrDefault();
        }


        public List<UpdateBackupEntry> GetUpdateBackupEntryList(UpdateInstruction UpdateInstruction)
        {
            List<UpdateBackupEntry> returnData = new List<UpdateBackupEntry>();
            string RootDirectoryName = new DirectoryInfo(System.IO.Path.GetDirectoryName(UpdateInstruction.WorkingDirectory + "\\")).Name; 
            var Directories = Directory.GetDirectories(System.IO.Directory.GetParent(UpdateInstruction.WorkingDirectory).FullName, $"{RootDirectoryName}*")
                .Where(x => UpdateInstruction.WorkingDirectory.ToUpper() != x.ToUpper()).ToList();

            foreach (var directoryPath in Directories)
            {
                string folderSufix= directoryPath.Replace($"{UpdateInstruction.WorkingDirectory}_", string.Empty); ;
                returnData.Add(new UpdateBackupEntry(directoryPath, folderSufix));

            }
            return returnData.OrderByDescending(x => x.BackupDate).ToList();
        }

        public List<UpdateResult> GetUpdateHistory(UpdateInstruction instruction)
        {
            if (!Directory.Exists(_JobResultPath)) return new List<UpdateResult>();

            List<UpdateResult> returnResults = new List<UpdateResult>();
            foreach (var file in Directory.GetFiles(_JobResultPath, $"*{instruction.ID}*"))
            {
                returnResults.Add(JsonConvert.DeserializeObject<UpdateResult>(File.ReadAllText(file)));
            }

            return returnResults;
        }

        public string GetBackupFolderFromUpdateID(string ID)
        {
            return Directory.GetFiles(_JobResultPath, $"{ID}*").FirstOrDefault();
        }

        public bool WriteJobStatus(UpdateInstruction instruction, string QueuePosition, bool HasFinished = false)
        {
            if (!Directory.Exists(_JobStatusPath)) Directory.CreateDirectory(_JobStatusPath);

            //string Filename = Path.Combine(_JobStatusPath,instruction.ID.ToString())
            if (HasFinished)
            {
                foreach (var item in Directory.GetFiles(_JobStatusPath, $"*{instruction.ID}"))
                    File.Delete(item);
            }
            else
            {
                File.WriteAllText(Path.Combine(_JobStatusPath, instruction.ID.ToString()), QueuePosition);
            }

            return true;
        }

        public string GetQueuePositionFromUpdate(UpdateInstruction instruction)
        {
            string Filename = Path.Combine(_JobStatusPath, instruction.ID.ToString());
            if (!File.Exists(Filename)) return "";

            return File.ReadAllText(Path.Combine(_JobStatusPath, instruction.ID.ToString()));
        }

        public bool ClearAllJobStatus()
        {
            if (!Directory.Exists(_JobStatusPath)) Directory.CreateDirectory(_JobStatusPath);

            foreach (var item in Directory.GetFiles(_JobStatusPath, "*"))
                File.Delete(item);

            return true;
        }
    }
}
