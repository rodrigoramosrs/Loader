using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Domain.Models.Analytics
{
    public class AnalyticsData
    {
        public AnalyticsData()
        {
            this.AnalyticsType = eAnalyticsType.Event;
        }
        public enum eAnalyticsType
        {
            Event,
            PageView

        }
        public eAnalyticsType AnalyticsType { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public int? Value { get; set; }

        public string Description { get; set; }


        public string HostName { get; set; }
        public string PageName{ get; set; }
        public string Title{ get; set; }
    }
}
