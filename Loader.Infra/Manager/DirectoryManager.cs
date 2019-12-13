using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Loader.Infra.Manager
{
    public static class DirectoryManager
    {

        public static bool CopyFilesOrFolder(string SourceFileOrFolder, string DestinationFileOrFolder)
        {
            if (File.Exists(SourceFileOrFolder))
            {
                File.Copy(SourceFileOrFolder, DestinationFileOrFolder, true);

                return File.Exists(DestinationFileOrFolder);
            }
            else
            {
                DirectoryCopy(SourceFileOrFolder, DestinationFileOrFolder, true, true);
                return Directory.Exists(DestinationFileOrFolder);
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwiteDestinationFile = false)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, overwiteDestinationFile);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs, overwiteDestinationFile);
                }
            }
        }
        public static bool DeleteDirectory(string Path, List<string> IgnoreExtensions = null)
        {
            if (!Directory.Exists(Path)) return false;

            try
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(Path);

                foreach (FileInfo file in di.GetFiles())
                {
                    if(!(IgnoreExtensions ?? new List<string>()).Contains(file.Extension))
                        file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return !Directory.Exists(Path);
        }

        public static bool RenameDirectory(string SourcePath, string DestinationPath)
        {
            Directory.Move(SourcePath, DestinationPath);

            return Directory.Exists(DestinationPath);
        }
    }
}
