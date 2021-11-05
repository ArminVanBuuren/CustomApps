using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Utils.AppUpdater
{
    /// <summary>
    /// The type of hash to create
    /// </summary>
    public enum HashType
    {
        /// <summary>
        /// Возвращает хэш для указанной строки. Это необратимый алгоритм, получить исходную информацию невозможно.
        /// </summary>
        MD5,
        SHA1,
        SHA512
    }

    /// <summary>
    /// Class used to generate hash sums of files
    /// </summary>
    public static class Hasher
    {
        /// <summary>
        /// Generate a hash sum of a file
        /// </summary>
        /// <param name="filePath">The file to hash</param>
        /// <param name="algo">The Type of hash</param>
        /// <returns>The computed hash</returns>
        public static string HashFile(string filePath, HashType algo)
        {
            var fileSource = filePath;
            var res = IO.WhoIsLocking(fileSource);
            if (res.Count > 0)
            {
                fileSource = Path.GetTempFileName();
                File.Copy(filePath, fileSource, true);
            }

            using (var stream = new FileStream(fileSource, FileMode.Open))
            {
                switch (algo)
                {
                    case HashType.MD5:
                        return MakeHashString(MD5.Create().ComputeHash(stream));
                    case HashType.SHA1:
                        return MakeHashString(SHA1.Create().ComputeHash(stream));
                    case HashType.SHA512:
                        return MakeHashString(SHA512.Create().ComputeHash(stream));
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// Converts byte[] to string
        /// </summary>
        /// <param name="hash">The hash to convert</param>
        /// <returns>Hash as string</returns>
        private static string MakeHashString(byte[] hash)
        {
            var s = new StringBuilder(hash.Length);

            foreach (var b in hash)
                s.Append(b.ToString("x2").ToLower());

            return s.ToString();
        }
    }
}
