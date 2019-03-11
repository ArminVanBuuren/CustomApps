using System;

namespace Utils.Builds.Updater
{
    public interface IUploadProgress
    {
        /// <summary>
        /// Когда новые версии бильдов скачались на локальный диск в темповую папку. Либо загрузка завершилась неудачей
        /// </summary>
        event UploadBuildHandler OnFetchComplete;
        /// <summary>
        /// Статус скачанных файлов с сервера
        /// </summary>
        bool IsUploaded { get; }
        /// <summary>
        /// Количество скачанных байт
        /// </summary>
        long UploadedBytes { get; }
        /// <summary>
        /// Размер файлов на сервере
        /// </summary>
        long TotalBytes { get; }
        /// <summary>
        /// Прогресс в процентах скачиванных файлов с сервера
        /// </summary>
        int ProgressPercent { get; }
        /// <summary>
        /// Прогресс скачиванных файлов с сервера
        /// </summary>
        /// <returns></returns>
        string GetProgressString();
        /// <summary>
        /// Скачать файлы с сервера
        /// </summary>
        /// <returns>Возвращает возможно ли скачать файлы. Возможно новые версии не были найдены или файлы необходимо удалить с локального диска, поэтому скачивать ничего не надо</returns>
        bool Fetch();
        /// <summary>
        /// Финальная стадия обновления. Когда все файлы успешно скачаны, в методе генерится список комманд на обновление и после успешного завершения програма принудительно закрывается
        /// </summary>
        void Commit();
        /// <summary>
        /// Удаление темполвых файлов скачанных с сервера
        /// </summary>
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

        public virtual bool Fetch()
        {
            return false;
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
