using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Domain.Models.Update
{
    public class UpdateBackupEntry
    {
        public UpdateBackupEntry(string BackupFullPath, string PathWithSufixData)
        {
            this.FullPath = BackupFullPath;

            string UpdateInstructionID = PathWithSufixData.Split('_')[0];
            string UpdateID = PathWithSufixData.Split('_')[1];
            string version = PathWithSufixData.Split('_')[2];

            string date = PathWithSufixData.Split('_')[3];
            string time = PathWithSufixData.Split('_')[4].Replace("-", ":");

            this.UpdateInstructionID = new Guid(UpdateInstructionID);
            this.UpdateID = new Guid(UpdateID);
            this.BackupDate = Convert.ToDateTime($"{date} {time}");
            this.BackupVersion = new Version(version);
        }
        public Guid UpdateInstructionID { get; set; }
        public Guid UpdateID {get;set;}
        public string FullPath { get; private set; }

        public DateTime BackupDate { get; private set; }

        public Version BackupVersion { get; private set; }
    }
}
