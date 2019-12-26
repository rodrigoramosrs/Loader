using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Domain.Models.Job
{
    public class JobStatus
    {
        public JobStatus()
        {
            ID = Guid.Empty;
            CurrentStatus = JobStatusTyes.None;
        }

        public Guid ID { get; set; }
        public JobStatusTyes CurrentStatus { get; set; }

        public string QueuePosition {get;set;}

    }
}
