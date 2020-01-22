using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Utils
{
    public static class ZIP
    {
        //public static void Compress(string fileSource, string fileDestination)
        //{
        //    if (File.Exists(fileDestination))
        //        throw new ArgumentException($"Destination file [{fileDestination}] already exist");

        //    FileInfo fi = new FileInfo(fileSource);
        //    using (FileStream inFile = fi.OpenRead())
        //    {
        //        //if((File.GetAttributes(fi.FullName) | FileAttributes.Hidden) != FileAttributes.Hidden && fi.Extension != ".gz")
        //        using (FileStream outFile = File.Create(fi.FullName))
        //        {
        //            using (GZipStream compress = new GZipStream(outFile, CompressionMode.Compress))
        //            {
        //                inFile.CopyTo(compress);
        //            }
        //        }
        //    }
        //}

        //public static void Decompress(string filePath)
        //{
        //    FileInfo info = new FileInfo(filePath);
        //}

        static void CompressFile(string sDir, string sRelativePath, GZipStream zipStream)
        {
            //Compress file name
            var chars = sRelativePath.ToCharArray();
            zipStream.Write(BitConverter.GetBytes(chars.Length), 0, sizeof(int));
            foreach (var c in chars)
                zipStream.Write(BitConverter.GetBytes(c), 0, sizeof(char));

            //Compress file content
            var bytes = File.ReadAllBytes(Path.Combine(sDir, sRelativePath));
            zipStream.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
            zipStream.Write(bytes, 0, bytes.Length);
        }

        static bool DecompressFile(string sDir, GZipStream zipStream, Action<string> progress = null)
        {
            //Decompress file name
            var bytes = new byte[sizeof(int)];
            var Readed = zipStream.Read(bytes, 0, sizeof(int));
            if (Readed < sizeof(int))
                return false;

            var iNameLen = BitConverter.ToInt32(bytes, 0);
            bytes = new byte[sizeof(char)];
            var sb = new StringBuilder();
            for (var i = 0; i < iNameLen; i++)
            {
                zipStream.Read(bytes, 0, sizeof(char));
                var c = BitConverter.ToChar(bytes, 0);
                sb.Append(c);
            }

            var sFileName = sb.ToString();
            progress?.Invoke(sFileName);

            //Decompress file content
            bytes = new byte[sizeof(int)];
            zipStream.Read(bytes, 0, sizeof(int));
            var iFileLen = BitConverter.ToInt32(bytes, 0);

            bytes = new byte[iFileLen];
            zipStream.Read(bytes, 0, bytes.Length);

            var sFilePath = Path.Combine(sDir, sFileName);
            var sFinalDir = Path.GetDirectoryName(sFilePath);
            if (!Directory.Exists(sFinalDir))
                Directory.CreateDirectory(sFinalDir);

            using (var outFile = new FileStream(sFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                outFile.Write(bytes, 0, iFileLen);

            return true;
        }

        static void CompressDirectory(string sInDir, string sOutFile, Action<string> progress = null)
        {
            var sFiles = Directory.GetFiles(sInDir, "*.*", SearchOption.AllDirectories);
            var iDirLen = sInDir[sInDir.Length - 1] == Path.DirectorySeparatorChar ? sInDir.Length : sInDir.Length + 1;

            using (var outFile = new FileStream(sOutFile, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var str = new GZipStream(outFile, CompressionMode.Compress))
                foreach (var sFilePath in sFiles)
                {
                    var sRelativePath = sFilePath.Substring(iDirLen);
                    progress?.Invoke(sRelativePath);
                    CompressFile(sInDir, sRelativePath, str);
                }
        }

        static void DecompressToDirectory(string sCompressedFile, string sDir, Action<string> progress = null)
        {
            using (var inFile = new FileStream(sCompressedFile, FileMode.Open, FileAccess.Read, FileShare.None))
            using (var zipStream = new GZipStream(inFile, CompressionMode.Decompress, true))
                while (DecompressFile(sDir, zipStream, progress))
                {
                }
        }

        internal static int Test(string[] argv)
        {
            if (argv.Length != 2)
            {
                Console.WriteLine("Usage: CmprDir.exe <in_dir compressed_file> | <compressed_file out_dir>");
                return 1;
            }

            try
            {
                string sDir;
                string sCompressedFile;
                var bCompress = false;
                if (Directory.Exists(argv[0]))
                {
                    sDir = argv[0];
                    sCompressedFile = argv[1];
                    bCompress = true;
                }
                else if (File.Exists(argv[0]))
                {
                    sCompressedFile = argv[0];
                    sDir = argv[1];
                    bCompress = false;
                }
                else
                {
                    Console.Error.WriteLine("Wrong arguments");
                    return 1;
                }

                if (bCompress)
                    CompressDirectory(sDir, sCompressedFile, (fileName) => { Console.WriteLine("Compressing {0}...", fileName); });
                else
                    DecompressToDirectory(sCompressedFile, sDir, (fileName) => { Console.WriteLine("Decompressing {0}...", fileName); });

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}