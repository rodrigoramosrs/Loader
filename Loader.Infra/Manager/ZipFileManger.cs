using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Loader.Infra.Manager
{
    public static class ZipFileManger
    {
        public static void CompressDirectory(string SourceDirectoryPath, string DestinationFileName)
        {

            System.IO.Compression.ZipFile.CreateFromDirectory(SourceDirectoryPath,DestinationFileName);

            /*
            DirectoryInfo directorySelected = new DirectoryInfo(DirectoryPath);
            foreach (FileInfo fileToCompress in directorySelected.GetFiles())
            {
                using (FileStream originalFileStream = fileToCompress.OpenRead())
                {
                    if ((File.GetAttributes(fileToCompress.FullName) &
                       FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                    {
                        using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
                        {
                            using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                               CompressionMode.Compress))
                            {
                                originalFileStream.CopyTo(compressionStream);

                            }
                        }
                        FileInfo info = new FileInfo(OutputPath + Path.DirectorySeparatorChar + fileToCompress.Name + ".gz");
                        Console.WriteLine($"Compressed {fileToCompress.Name} from {fileToCompress.Length.ToString()} to {info.Length.ToString()} bytes.");
                    }

                }
            }*/
        }

        public static bool Decompress(string SourceArchiveFileOrURL, string destinatinoDirectoryName, bool overwriteFiles = true)
        {
            string sourceFileName = SourceArchiveFileOrURL;
            if (SourceArchiveFileOrURL.StartsWith("http") || SourceArchiveFileOrURL.StartsWith("ftp"))
                sourceFileName = DownloadManager.DownloadTempData(SourceArchiveFileOrURL);

            System.IO.Compression.ZipFile.ExtractToDirectory(sourceFileName, destinatinoDirectoryName, overwriteFiles);
            return true;
            /*
            FileInfo fileToDecompress = new FileInfo(ZipFileName);
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.WriteLine($"Decompressed: {fileToDecompress.Name}");
                    }
                }
            }*/
        }
    }
}
