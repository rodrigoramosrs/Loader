using Loader.Domain.Models.License;
using Loader.Service.Services.Analytics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Loader.Service.Services.License
{
    public class BaseLicenseService
    {
        protected readonly string _CustomerID;
        protected readonly string _CustomerName;
        protected readonly BaseAnalyticsService _AnalyticsService;
        protected readonly UpdateService _UpdateService;

        public BaseLicenseService(BaseAnalyticsService AnalyticsService, Service.Services.UpdateService UpdateService, string CustomerID, string CustomerName)
        {
            this._CustomerID = CustomerID;
            this._CustomerName = CustomerName;
            this._AnalyticsService = AnalyticsService;
            this._UpdateService = UpdateService;
        }

        public virtual bool HasPermissionToUse(LicenseData licenseData)
        {
            return true;
        }

        public virtual async Task SilentValidadeLicense()
        {
             HasPermissionToUse(new LicenseData());
        }
    }
}
