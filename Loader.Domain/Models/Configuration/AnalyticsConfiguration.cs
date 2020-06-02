using Loader.Domain.Models.Configuration.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Domain.Models.Configuration
{
    public class AnalyticsConfiguration
    {
        public AnalyticsConfiguration()
        {
            GoogleProvider = new GoogleProvider();
            MvProvider = new MvProvider();
        }
        public AnalyticsConfigurationProviderTypes Provider { get; set; }
       
        public GoogleProvider GoogleProvider { get; set; }
        public MvProvider MvProvider { get; set; }
        public bool SaveAnalyticsToFile { get; set; }

        public bool CheckComputerMetrics { get; set; }
        
    }

    public class GoogleProvider
    {
        public string ID { get; set; }
        public string ExceptionID { get; set; }
    }

    public class MvProvider
    {
        public DatabaseType DbProvider { get; set; }
        public string ConnectionString { get; set; }
    }
}
