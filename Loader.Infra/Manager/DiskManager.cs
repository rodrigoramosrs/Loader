using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Loader.Infra.Manager
{
   public class DiskManager
    {
        public class DiskMetrics
        {
            public string DriveFormat;

            public string DriveLetter;

            public long Total;

            public string TotalFormatted
            {
                get
                {
                    return DiskManager.GetSize(this.Total);
                }

            }




            public long Used;

            public string UsedFormatted
            {
                get
                {
                    return DiskManager.GetSize(this.Used);
                    
                }

            }

            public double UsedInPercent
            {
                get
                {
                    return Math.Round((double)(this.Used * 100) / this.Total, 2);
                }

            }

            public long Free;

            public string FreeFormatted
            {
                get
                {
                    return DiskManager.GetSize(this.Free);
                }

            }

            public double FreeInPercent
            {
                get
                {
                    return Math.Round((double)(this.Free * 100) / this.Total, 2);
                }

            }


            

        }

        public  List<DiskMetrics> CheckAllDisksSpace()
        {
            var diskResult = new List<DiskMetrics>();
            try
            {
                
                var exclusionList = new List<DriveType>() { DriveType.Network, DriveType.CDRom, DriveType.Ram, DriveType.Removable };
                foreach (var driveInfo in DriveInfo.GetDrives())
                {
                    try
                    {
                        if (exclusionList.Contains(driveInfo.DriveType)) continue;

                        //var totalBytes = driveInfo.TotalSize;
                        //var freeBytes = driveInfo.AvailableFreeSpace;

                        // var freePercent = (int)((100 * freeBytes) / totalBytes);
                        diskResult.Add(new DiskMetrics()
                        {
                            DriveLetter = driveInfo.Name,
                            Free = driveInfo.AvailableFreeSpace,
                            Total = driveInfo.TotalSize,
                            Used = driveInfo.TotalSize - driveInfo.AvailableFreeSpace,
                            DriveFormat = driveInfo.DriveFormat
                        });
                    }
                    catch (Exception EX)
                    {

                        //TODO: IMPLMENTAR EXCEPTION
                    }



                }
            }
            catch (Exception)
            {

                //TODO: IMPLMENTAR EXCEPTION
            }

            return diskResult;
        }

        public static string GetSize(long bytes)
        {
            if (bytes > 1073741824)
                return Math.Ceiling(bytes / 1073741824M).ToString("#,### GB");
            else if (bytes > 1048576)
                return Math.Ceiling(bytes / 1048576M).ToString("#,### MB");
            else if (bytes >= 1)
                return Math.Ceiling(bytes / 1024M).ToString("#,### KB");
            else if (bytes < 0)
                return "";
            else
                return bytes.ToString("#,### B");
            /*
            string postfix = "Bytes";
            long result = size;
            if (size >= 1073741824)//more than 1 GB
            {
                result = size / 1073741824;
                postfix = "GB";
            }
            else if (size >= 1048576)//more that 1 MB
            {
                result = size / 1048576;
                postfix = "MB";
            }
            else if (size >= 1024)//more that 1 KB
            {
                result = size / 1024;
                postfix = "KB";
            }

            return result.ToString("F1") + " " + postfix;*/
        }
    }
}
