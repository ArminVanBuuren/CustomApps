using Microsoft.Expression.Encoder.Live;
using Microsoft.Expression.Encoder.ScreenCapture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.WinForm.MediaCapture
{
    internal class EncoderProcessingThread
    {
        public Thread ThreadProc { get; set; }
        public LiveJob Job { get; set; }
        public ScreenCaptureJob ScreenJob { get; set; }
        public LiveDeviceSource Device { get; set; }
        public int BroadcastPort { get; }
        public string DestinationFilePath { get; }
        public bool IsCanceled { get; private set; } = false;

        public EncoderProcessingThread(string destinationFilePath)
        {
            DestinationFilePath = destinationFilePath;
        }

        public EncoderProcessingThread(int port)
        {
            BroadcastPort = port;
        }

        ///// <summary>
        ///// добавление девайсов почему то зависает, поэтому по таймеру срубается процесс Thread. И поэтому если вдруг обработка в этом методе пойдет дальше, нужно ее принудительно срубать
        ///// </summary>
        ///// <param name="externalThread">текущий рабочий поток</param>
        ///// <returns></returns>
        //public bool IfItAbortedThenEndProcess(Thread externalThread)
        //{
        //    if (externalThread != null && externalThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
        //        return false;

        //    Terminate();
        //    return true;
        //}

        public Task Stop()
        {
            return Task.Run(() =>
            {
                try
                {
                    if (Job != null)
                    {
                        Job.StopEncoding();
                        if (Device != null)
                            Job.RemoveDeviceSource(Device);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                try
                {
                    ScreenJob?.Stop();
                }
                catch (Exception)
                {
                    //null;
                }

                try
                {
                    if (Device == null)
                        return;

                    Device.PreviewWindow = null;
                    Device.Dispose();
                }
                catch (Exception)
                {
                    //null;
                }
            });
        }


        public Task Terminate()
        {
            IsCanceled = true;

            return Task.Run(() =>
            {
                try
                {
                    Job?.Dispose();
                }
                catch (Exception)
                {
                    //null;
                }

                try
                {
                    ScreenJob?.Dispose();
                }
                catch (Exception)
                {
                    //null;
                }

                try
                {
                    Device?.Dispose();
                }
                catch (Exception)
                {
                    //null;
                }

                try
                {
                    ThreadProc?.Abort();
                }
                catch (Exception)
                {
                    // null
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(DestinationFilePath))
                        File.Delete(DestinationFilePath);
                }
                catch (Exception)
                {
                    // null
                }
            });
        }

        public override string ToString()
        {
            return $"ID=[{ThreadProc?.ManagedThreadId}] IsAlive=[{ThreadProc?.IsAlive}] IsCanceled=[{IsCanceled}]";
        }
    }
}
