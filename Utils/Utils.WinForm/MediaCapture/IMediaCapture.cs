using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils.WinForm.MediaCapture
{
    public interface IMediaCapture
    {
        /// <summary>
        /// При завершении процесса записи файла видео или ошибка процессинга
        /// </summary>
        event MediaCaptureEventHandler OnRecordingCompleted;
        /// <summary>
        /// Все видео устройства полученные через библиотеку AForge
        /// </summary>
        AForgeMediaDevices AForgeDevices { get; }
        /// <summary>
        /// Все видео и аудио устройства полученные через библиотеку Expression.Encoder
        /// </summary>
        CamMediaDevices CamDevices { get; }
        /// <summary>
        /// Папка для записи файлв результата
        /// </summary>
        string DestinationDir { get; set; }
        /// <summary>
        /// Время записи видео в секундах
        /// </summary>
        int RecDurationSec { get; set; }
        /// <summary>
        /// Время старта процесса
        /// </summary>
        DateTime? TimeOfStart { get; }
        /// <summary>
        /// Режим работы процесса
        /// </summary>
        MediaCaptureMode Mode { get; }
        /// <summary>
        /// Изменить видео устройство для захвата видео
        /// </summary>
        /// <param name="name"></param>
        void ChangeVideoDevice(string name);
        /// <summary>
        /// Начать захват видео с камеры
        /// </summary>
        /// <param name="pictureBox"></param>
        void StartCamPreview(PictureBox pictureBox);
        /// <summary>
        /// Начать запись видео с камеры
        /// </summary>
        void StartCamRecording();
        /// <summary>
        /// Начать запись видео с экрана монитора
        /// </summary>
        void StartScreenRecording();
        /// <summary>
        /// Включить трансляцию видео
        /// </summary>
        /// <param name="port"></param>
        void StartBroadcast(int port = 8080);
        /// <summary>
        /// Остановить все процессы
        /// </summary>
        void Stop();
        /// <summary>
        /// Получить картинку с камеры
        /// </summary>
        /// <returns></returns>
        Task<Bitmap> GetPicture();
    }

    public abstract class MediaCapture : IMediaCapture
    {
        private string _destinationDir = string.Empty;
        MediaCaptureMode _mode = MediaCaptureMode.None;

        public event MediaCaptureEventHandler OnRecordingCompleted;

        public AForgeMediaDevices AForgeDevices { get; }

        public CamMediaDevices CamDevices { get; }

        public string DestinationDir
        {
            get => _destinationDir;
            set
            {
                if (Mode == MediaCaptureMode.Recording)
                    return;

                _destinationDir = value;
                if (!Directory.Exists(_destinationDir))
                    Directory.CreateDirectory(_destinationDir);
            }
        }
        public int RecDurationSec { get; set; }

        public MediaCaptureMode Mode
        {
            get => _mode;
            protected set
            {
                _mode = value;

                if (_mode == MediaCaptureMode.None)
                    TimeOfStart = null;
                else
                    TimeOfStart = DateTime.Now;
            }
        }

        public DateTime? TimeOfStart { get; private set; }


        protected MediaCapture(AForgeMediaDevices aDevices, CamMediaDevices cDevices, string destinationDir, int durationRecSec)
        {
            AForgeDevices = aDevices;
            CamDevices = cDevices;

            DestinationDir = destinationDir.IsNullOrEmptyTrim() ? ASSEMBLY.ApplicationDirectory : destinationDir;

            int duration = durationRecSec;
            if (duration > 1800)
                duration = 1800;
            else if (duration < 1)
                duration = 1;
            RecDurationSec = duration;
        }

        public virtual void ChangeVideoDevice(string name)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Task<Bitmap> GetPicture()
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartBroadcast(int port = 8080)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartCamPreview(PictureBox pictureBox)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartCamRecording()
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartScreenRecording()
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void Stop()
        {
            throw new NotSupportedException("Not supported.");
        }

        protected string GetNewVideoFilePath()
        {
            return Path.Combine(DestinationDir, STRING.RandomString(15) + ".wmv");
        }

        protected void RecordCompleted(MediaCaptureEventArgs args, bool isAsync = false)
        {
            if(isAsync)
                OnRecordingCompleted?.BeginInvoke(this, args, null,null);
            else
                OnRecordingCompleted?.Invoke(this, args);
        }
    }
}