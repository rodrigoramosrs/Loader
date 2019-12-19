using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Domain.Models.Analytics
{
    public class AnalyticsData
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public int? Value { get; set; }

        public string Description { get; set; }
    }
}
