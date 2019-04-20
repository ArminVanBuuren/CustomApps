﻿using System;
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
        /// <summary>
        /// При завершении процесса записи файла видео или ошибка процессинга
        /// </summary>
        event MediaCaptureEventHandler OnRecordingCompleted;

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
}