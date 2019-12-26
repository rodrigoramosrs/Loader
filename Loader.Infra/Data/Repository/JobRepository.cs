using Loader.Domain.Models.Job;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Loader.Infra.Data.Repository
{
    public class JobRepository
    {
        private readonly string _JobStatusPath = ".\\job_status\\";

        public JobRepository()
        {
            if (!Directory.Exists(_JobStatusPath)) Directory.CreateDirectory(_JobStatusPath);
        }
        public bool CreateUpdateJobStatus(JobStatus JobStatus)//Guid ID, string QueuePosition, bool HasFinished = false)
        {
            switch (JobStatus.CurrentStatus)
            {
                case JobStatusTyes.Completed:
                    foreach (var item in Directory.GetFiles(_JobStatusPath, $"*{JobStatus.ID}"))
                        File.Delete(item);
                    break;
                case JobStatusTyes.None:
                case JobStatusTyes.Stoped:
                case JobStatusTyes.Scheduled:
                case JobStatusTyes.Processing:
                case JobStatusTyes.Error:
                default:
                    File.WriteAllText(Path.Combine(_JobStatusPath,JobStatus.ID.ToString()), JsonConvert.SerializeObject(JobStatus, Formatting.Indented));
                    break;
                
            }

            return true;
        }

        public JobStatus GetQueuePosition(JobStatus JobStatus)
        {
            string Filename = Path.Combine(_JobStatusPath, JobStatus.ID.ToString());
            if (!File.Exists(Filename)) return new JobStatus();
            
            string content = File.ReadAllText(Path.Combine(_JobStatusPath, JobStatus.ID.ToString()));
            return JsonConvert.DeserializeObject<JobStatus>(content);
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
