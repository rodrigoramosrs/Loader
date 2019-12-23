using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Domain.Models.Analytics
{
    public class AnalyticsExceptionData
    {
        public AnalyticsType AnalyticsType { get { return Analytics.AnalyticsType.Exception; } }
       
        public Exception Exception { get; set; }
    }
}
