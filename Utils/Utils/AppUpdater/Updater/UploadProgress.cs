using System;
using System.Globalization;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    public abstract class UploadProgress
    {
        public virtual long UploadedBytes { get; protected set; } = 0;
        public string UploadedString => IO.FormatBytes(UploadedBytes, out _);

        public virtual long TotalBytes { get; protected set; } = 0;
        public string TotalString => IO.FormatBytes(TotalBytes, out _);

        public virtual int ProgressPercent
        {
            get
            {
                IO.FormatBytes(UploadedBytes, out double upload);
                IO.FormatBytes(TotalBytes, out double total);
                if (total == 0)
                    return 0;
                return int.Parse(((upload / total) * 100).ToString(CultureInfo.InvariantCulture));
            }
        }

        public virtual string GetProgressString()
        {
            return $"{UploadedString} of {TotalString}";
        }
    }
}
