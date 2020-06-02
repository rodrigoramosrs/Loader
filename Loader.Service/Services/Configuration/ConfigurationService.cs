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
                var returnModel = new Domain.Models.Configuration.AnalyticsConfiguration();

                switch (this.GetConfiguration("Configuration:Analytics:Provider").ToUpper())
                {

                    case "MV":
                        returnModel.Provider = Domain.Models.Configuration.Types.AnalyticsConfigurationProviderTypes.MV;
                        break;
                    default:
                        returnModel.Provider = Domain.Models.Configuration.Types.AnalyticsConfigurationProviderTypes.Google;
                        break;
                }

                returnModel.SaveAnalyticsToFile = this.GetConfiguration("Configuration:Analytics:SaveAnalyticsToFile").ToUpper() == "TRUE";
                returnModel.CheckComputerMetrics = this.GetConfiguration("Configuration:Analytics:CheckComputerMetrics").ToUpper() == "TRUE";

                returnModel.GoogleProvider.ID = this.GetConfiguration("Configuration:Analytics:GoogleProvider:ID");
                returnModel.GoogleProvider.ExceptionID = this.GetConfiguration("Configuration:Analytics:GoogleProvider:ExceptionID");


                switch (this.GetConfiguration("Configuration:Analytics:MvProvider:DbProvider").ToUpper())
                {
                    case "POSTGRE":
                        returnModel.MvProvider.DbProvider = Domain.Models.Configuration.Types.DatabaseType.Postgre;
                        break;
                    case "ORACLE":
                    default:
                        returnModel.MvProvider.DbProvider = Domain.Models.Configuration.Types.DatabaseType.Oracle;
                        break;
                }
                returnModel.MvProvider.ConnectionString = this.GetConfiguration("Configuration:Analytics:MvProvider:ConnectionString");

                return returnModel;
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
