using Loader.Domain.Models.License;
using Loader.Service.Services.Analytics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Loader.Service.Services.License
{
    public class MVLicenseService : BaseLicenseService
    {
        public MVLicenseService(BaseAnalyticsService AnalyticsService, UpdateService UpdateService, string CustomerID, string CustomerName) : base(AnalyticsService, UpdateService, CustomerID, CustomerName)
        {
        }

        public override bool HasPermissionToUse(LicenseData licenseData)
        {
            return base.HasPermissionToUse(licenseData);
        }

        public override async Task SilentValidadeLicense()
        {
            //base.SilentValidadeLicense();
            var updateInstructionList = this._UpdateService.GetUpdateInstructionList();
            foreach (var updateInstruction in updateInstructionList)
            {
                var licenseData = new LicenseData
                {
                    ProductID = updateInstruction.Name,
                    ProductName = updateInstruction.Name,
                    ProductVersion = this._UpdateService.GetCurrentAssemblyVersion(updateInstruction).ToString()
                };

                string ValidationResult = string.Empty;

                if (!this.HasPermissionToUse(licenseData))
                {
                    ValidationResult = $"Not authorized to use '{updateInstruction.Name}' - '{_UpdateService.GetCurrentAssemblyVersion(updateInstruction)}'";
                }
                else
                {
                    ValidationResult = $"Authorized to use '{updateInstruction.Name}' - '{_UpdateService.GetCurrentAssemblyVersion(updateInstruction)}'";
                }

                await this._AnalyticsService.SendInformation("MVLicenseService.SilentValidadeLicense", ValidationResult);
            }

            
        }
    }
}
