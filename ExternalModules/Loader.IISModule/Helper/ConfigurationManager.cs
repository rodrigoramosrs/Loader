using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Loader.Helper
{
    public static class ConfigurationManager
    {
        private static readonly string _ConfigurationDir = Path.GetPathRoot(Environment.SystemDirectory) + @"\MDMV\Loader.IISModule";
        private static readonly string ConfigurationFullPath = _ConfigurationDir + "\\configuration.config" ;
        static ConfigurationManager()
        {
            if (!Directory.Exists(_ConfigurationDir))
            {
                Directory.CreateDirectory(_ConfigurationDir);
                File.WriteAllText(ConfigurationFullPath, "");
            }
        }

        public static string LoaderURL
        {
            get
            {
                string returnData = File.ReadAllText(ConfigurationFullPath);
                if (string.IsNullOrEmpty(returnData)) return "http://localhost:5555/";

                return returnData;
            }
        }
    }
}
