using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils.WinForm.MediaCapture
{
    public interface IMediaCapture
    {
        Thread MainThread { get; }
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
        EncoderMediaDevices CamDevices { get; }
        /// <summary>
        /// Папка для записи файлв результата
        /// </summary>
        string DestinationDir { get; set; }
        /// <summary>
        /// Время записи видео в секундах
        /// </summary>
        int SecondsRecordDuration { get; set; }
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
        void StartCamRecording(string fileName);
        /// <summary>
        /// Начать запись видео с камеры
        /// </summary>
        Task StartCamRecordingAsync(string fileName);
        /// <summary>
        /// Начать запись видео с экрана монитора
        /// </summary>
        void StartScreenRecording(string fileName);
        /// <summary>
        /// Начать запись видео с экрана монитора
        /// </summary>
        Task StartScreenRecordingAsync(string fileName);
        /// <summary>
        /// Включить трансляцию видео
        /// </summary>
        /// <param name="port"></param>
        bool StartBroadcast(int port);
        /// <summary>
        /// Включить трансляцию видео
        /// </summary>
        /// <param name="port"></param>
        Task<bool> StartBroadcastAsync(int port);
        /// <summary>
        /// Остановить все процессы
        /// </summary>
        void Stop();
        /// <summary>
        /// Остановить все процессы
        /// </summary>
        Task StopAsync();
        /// <summary>
        /// Получить картинку с камеры
        /// </summary>
        /// <returns></returns>
        Bitmap GetPicture();
        /// <summary>
        /// Получить картинку с камеры
        /// </summary>
        /// <returns></returns>
        Task<Bitmap> GetPictureAsync();
    }

    public abstract class MediaCapture : IMediaCapture
    {
        

        private string _destinationDir = string.Empty;
        MediaCaptureMode _mode = MediaCaptureMode.None;

        public Thread MainThread { get; private set; }

        public event MediaCaptureEventHandler OnRecordingCompleted;

        public AForgeMediaDevices AForgeDevices { get; }

        public EncoderMediaDevices CamDevices { get; }

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
        public int SecondsRecordDuration { get; set; }

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


        protected MediaCapture(AForgeMediaDevices aDevices, EncoderMediaDevices cDevices, string destinationDir, int secondsRecDuration)
        {
            MainThread = Thread.CurrentThread;
            
            AForgeDevices = aDevices;
            CamDevices = cDevices;

            DestinationDir = destinationDir.IsNullOrEmptyTrim() ? ASSEMBLY.ApplicationDirectory : destinationDir;

            int duration = secondsRecDuration;
            if (duration > 1800)
                duration = 1800;
            else if (duration < 1)
                duration = 1;
            SecondsRecordDuration = duration;
        }

        public virtual void ChangeVideoDevice(string name)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartCamPreview(PictureBox pictureBox)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartCamRecording(string fileName = null)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Task StartCamRecordingAsync(string fileName = null)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartScreenRecording(string fileName = null)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Task StartScreenRecordingAsync(string fileName = null)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual bool StartBroadcast(int port = 8080)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Task<bool> StartBroadcastAsync(int port = 8080)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void Stop()
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Task StopAsync()
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Bitmap GetPicture()
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Task<Bitmap> GetPictureAsync()
        {
            throw new NotSupportedException("Not supported.");
        }

        protected string GetNewVideoFilePath(string fileName, string extension = ".wmv")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return Path.Combine(DestinationDir, STRING.RandomString(15) + extension);
            return Path.Combine(DestinationDir, fileName);
        }

        protected void RecordCompleted(MediaCaptureEventArgs args, bool isAsync = false)
        {
            try
            {
                if (isAsync)
                    OnRecordingCompleted?.BeginInvoke(this, args, null, null);
                else
                    OnRecordingCompleted?.Invoke(this, args);
            }
            catch (Exception)
            {
                // null
            }
        }

    }
}