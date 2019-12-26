using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Domain.Models.Job
{
   public enum JobStatusTyes
    {
        None,
        Stoped,
        Scheduled,
        Processing,
        Completed,
        Error
    }
}
