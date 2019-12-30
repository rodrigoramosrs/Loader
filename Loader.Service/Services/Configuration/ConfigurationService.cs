using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Service.Services.Configuration
{
    public class ConfigurationService
    {
        
        private  IConfiguration _Configuration { get; set; }

        public Domain.Models.Configuration.AnalyticsConfiguration AnalyticsConfiguration
        {
            get
            {
                return new Domain.Models.Configuration.AnalyticsConfiguration()
                {
                    ID = this.GetConfiguration("Configuration:Analytics:ID"),
                    ExceptionID = this.GetConfiguration("Configuration:Analytics:ExceptionID"),
                };
            }
        }

        public Domain.Models.Configuration.CustomerConfiguration CustomerConfiguration
        {
            get
            {
                return new Domain.Models.Configuration.CustomerConfiguration()
                {
                    ID = this.GetConfiguration("Configuration:Customer:ID"),
                    Name = this.GetConfiguration("Configuration:Customer:Name"),
                    CheckLicense = Convert.ToBoolean(this.GetConfiguration("Configuration:Customer:CheckLicense")),
                };
            }
        }

        public Domain.Models.Configuration.GeneralConfiguration GeneralConfiguration
        {
            get
            {
                return new Domain.Models.Configuration.GeneralConfiguration();
            }
        }

        public ConfigurationService(IConfiguration Configuration)
        {
            this._Configuration = Configuration;
        }



        private string GetConfiguration(string Key)
        {
            return _Configuration[Key];
        }


    }
}
