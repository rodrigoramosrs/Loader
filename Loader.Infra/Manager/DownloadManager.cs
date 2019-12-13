using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Loader.Infra.Manager
{
    public static class DownloadManager
    {
        public static string DownloadString(string FileOrPath)
        {
            string result = "";

            if (!FileOrPath.StartsWith("http"))
                result = File.ReadAllText(FileOrPath);
            else
                using (WebClient client = new WebClient())
                    result = client.DownloadString(FileOrPath);

            return result;
        }

        public static string DownloadTempData(string FileOrPath)
        {
            string TempFilename = Path.GetTempFileName();

            if (!FileOrPath.StartsWith("http"))
            {
                File.Copy(FileOrPath, TempFilename);
            }
            else
            {
                using (WebClient client = new WebClient())
                {
                    byte[] data = client.DownloadData(FileOrPath);
                    File.WriteAllBytes(TempFilename, data);
                }


            }
            return TempFilename;
        }
    }
}
