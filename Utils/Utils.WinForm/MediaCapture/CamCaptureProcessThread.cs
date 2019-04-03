using Microsoft.Expression.Encoder.Live;
using Microsoft.Expression.Encoder.ScreenCapture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.WinForm.MediaCapture
{
    internal class CamCaptureProcessThread
    {
        public Thread ThreadProc { get; }
        public LiveJob Job { get; }
        public ScreenCaptureJob ScreenJob { get; }
        public LiveDeviceSource Device { get; set; }

        public CamCaptureProcessThread(Thread thread, LiveJob job)
        {
            ThreadProc = thread;
            Job = job;
        }
        public CamCaptureProcessThread(Thread thread, ScreenCaptureJob screenJob)
        {
            ThreadProc = thread;
            ScreenJob = screenJob;
        }

        /// <summary>
        /// добавление девайсов почему то зависает, поэтому по таймеру срубается процесс Thread. И поэтому если вдруг обработка в этом методе пойдет дальше, нужно ее принудительно срубать
        /// </summary>
        /// <param name="externalThread">текущий рабочий поток</param>
        /// <returns></returns>
        public bool IfItAbortedThenEndProcess(Thread externalThread)
        {
            if (externalThread != null && externalThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
                return false;

            Terminate();
            return true;
        }

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
                catch (Exception e)
                {
                    // null
                }
            });
        }

        public override string ToString()
        {
            return $"ID=[{ThreadProc?.ManagedThreadId}] IsAlive=[{ThreadProc?.IsAlive}]";
        }
    }
}
