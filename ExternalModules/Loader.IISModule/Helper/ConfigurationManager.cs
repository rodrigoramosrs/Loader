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


        private static readonly string ConfigurationFullPath = _ConfigurationDir + "\\configuration.ini" ;

        private static readonly IniFile _IniFile;

        public static string RootPath
        {
            get
            { 
                return _ConfigurationDir;
            }
        }

        static ConfigurationManager()
        {
            if (!Directory.Exists(_ConfigurationDir))
            {
                Directory.CreateDirectory(_ConfigurationDir);
            }

            if(!File.Exists(ConfigurationFullPath))
                File.WriteAllText(ConfigurationFullPath,
                    @"[general]
loder_url=http://localhost:5555/
log_to_file=false
log_request_time_at=2000
valid_slow_down_request_at=2000
ignore_list=");

            else
            {
                _IniFile = new IniFile(ConfigurationFullPath);
            }

            
        }

        public static string LoaderURL
        {
            get
            {
                return _IniFile.GetValue("general", "loder_url", "http://localhost:5555/");
            }
        }

        public static bool LogToFile
        {
            get
            {
                return _IniFile.GetBoolean("general", "log_to_file",false);
            }
        }


        public static string LogPath
        {
            get
            {
                return _IniFile.GetValue("general", "log_path", _ConfigurationDir + "\\log\\");
            }
        }



        public static int LogRequestTimeAt
        {
            get
            {
                return _IniFile.GetInteger("general", "log_request_time_at", 2000);
            }
        }

        public static int ValidSlowDownRequestAt
        {
            get
            {
                return _IniFile.GetInteger("general", "valid_slow_down_request_at", 2000);
            }
        }

        public static string[] IgnoreList
        {
            get
            {
                return _IniFile.GetValue("general", "ignore_list",".jpg;.jpeg;.png;.bmp;.gif;.javascript;.js;.png;.css;.ico; chatserver.svc;.axd").Split(';');
                
            }
        }
    }
}
