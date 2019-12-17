using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Loader.Domain.Models;
using Loader.Service.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Loader.Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /*
            UpdateInstruction definition = new UpdateInstruction()
            {
                AutoUpdate = false,
                CommandLineAfterUpdate = "%SYSTEMROOT%\\System32\\inetsrv\\appcmd start apppool /apppool.name:\"MPE.Servico.WCF\"",
                CommandLineBeforeUpdate = "%SYSTEMROOT%\\System32\\inetsrv\\appcmd stop apppool /apppool.name:\"MPE.Servico.WCF\"",
                ID = Guid.NewGuid(),
                IISAppID = "",
                MainAssembly = @"C:\temp\MPE.Servico.WCF\bin\MPE.Servico.WCF.dll",
                WorkingDirectory = @"C:\temp\MPE.Servico.WCF",
                Name = "Portal de exames - Servico",
                //FolderToGetUpdate = "c:\\TEMP\\Updates\\",
                UrlOrPathToUpdateDefinition = @"C:\temp\Updates\update.json",
                
            };

            UpdateEntry UpdateEntry = UpdateService.HasUpdate(definition);

            if (UpdateEntry.HasUpdate)
            {
               UpdateResult result = UpdateService.DoUpdate(UpdateEntry);
               Console.WriteLine(result.Message);
            }*/

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                  .UseIISIntegration()
                .UseStartup<Startup>();
    }
}
