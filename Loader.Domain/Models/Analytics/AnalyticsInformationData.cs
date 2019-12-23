using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Domain.Models.Analytics
{
    public class AnalyticsInformationData
    {
        public AnalyticsInformationData()
        {
            this.AnalyticsType = AnalyticsType.Event;
        }

        public AnalyticsType AnalyticsType { get; set; }
        public string Category { get; set; }
        public string ActionName { get; set; }
        public string Label { get; set; }
        public int? Value { get; set; }

        public string Description { get; set; }


        public string HostName { get; set; }
        public string PageName { get; set; }
        public string Title { get; set; }
    }
}
