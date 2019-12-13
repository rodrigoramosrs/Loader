using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Loader.Infra.Manager
{
    public static class AssemblyManager
    {
        public static Version GetAssemblyVersion(string AssemblyPath)
        {
            if (!File.Exists(AssemblyPath)) return new Version();

            var data = FileVersionInfo.GetVersionInfo(AssemblyPath);
            return new Version(data.FileVersion);
        }

        public static bool IsNewerVersion(Version CurrentVersion, Version NewVersion)
        {
            var result = CurrentVersion.CompareTo(NewVersion);

            //Se result > 0 CurrentVersion  é superior a NewVersion
            //Se result < 0 CurrentVersion  é inferior a NewVersion
            //Se result = 0 ambas versões são iguais

            return result < 0;

            
        }
    }
}
