using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.BuildUpdater
{
    public interface IUploadProgress
    {
        event UploadBuildHandler OnFetchComplete;
        bool IsUploaded { get; }
        long UploadedBytes { get; }
        long TotalBytes { get; }
        int ProgressPercent { get; }
        string GetProgressString();
        void Fetch();
        void Commit();
        void RemoveTempFiles();
    }

    [Serializable]
    public abstract class UploadProgress
    {
        public virtual bool IsUploaded { get; protected set; } = false;
        public virtual long UploadedBytes { get; protected set; } = 0l;
        public virtual long TotalBytes { get; protected set; } = 0l;

        public virtual int ProgressPercent
        {
            get
            {
                FormatBytes(UploadedBytes, out double upload);
                FormatBytes(TotalBytes, out double total);
                return int.Parse(((upload / total) * 100).ToString());
            }
        }

        public virtual void Fetch()
        {

        }

        public virtual void Commit()
        {

        }

        public virtual void RemoveTempFiles()
        {

        }

        public virtual string GetProgressString()
        {
            return $"Downloaded {FormatBytes(UploadedBytes, out double result)} of {FormatBytes(TotalBytes, out double result2)}";
        }

        /// <summary>
        /// Formats the byte count to closest byte type
        /// </summary>
        /// <param name="bytes">The amount of bytes</param>
        /// <param name="decimalPlaces">How many decimal places to show</param>
        /// <param name="showByteType">Add the byte type on the end of the string</param>
        /// <returns>The bytes formatted as specified</returns>
        public static string FormatBytes(long bytes, out double newBytes, int decimalPlaces = 1, bool showByteType = true)
        {
            newBytes = bytes;
            string formatString = "{0";
            string byteType = "B";

            // Check if best size in KB
            if (newBytes > 1024 && newBytes < 1048576)
            {
                newBytes /= 1024;
                byteType = "KB";
            }
            else if (newBytes > 1048576 && newBytes < 1073741824)
            {
                // Check if best size in MB
                newBytes /= 1048576;
                byteType = "MB";
            }
            else
            {
                // Best size in GB
                newBytes /= 1073741824;
                byteType = "GB";
            }

            // Show decimals
            if (decimalPlaces > 0)
                formatString += ":0.";

            // Add decimals
            for (int i = 0; i < decimalPlaces; i++)
                formatString += "0";

            // Close placeholder
            formatString += "}";

            // Add byte type
            if (showByteType)
                formatString += byteType;

            return string.Format(formatString, newBytes);
        }
    }
    
}
