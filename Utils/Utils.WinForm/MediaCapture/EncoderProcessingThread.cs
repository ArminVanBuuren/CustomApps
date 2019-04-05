﻿using Microsoft.Expression.Encoder.Live;
using Microsoft.Expression.Encoder.ScreenCapture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AForge.Video.DirectShow;
using AForge.Video.VFW;

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
        public bool IsCanceled { get; set; } = false;

        public EncoderProcessingThread(string destinationFilePath)
        {
            DestinationFilePath = destinationFilePath;
        }

        public EncoderProcessingThread(int port)
        {
            BroadcastPort = port;
        }

        public Task StopAsync(bool thenDispose = false)
        {
            return Task.Run(() => Stop(thenDispose));
        }

        public void Stop(bool thenDispose = false)
        {
            try
            {
                if (Job != null)
                {
                    Job.StopEncoding();
                    if (Device != null)
                        Job.RemoveDeviceSource(Device);
                    if (thenDispose)
                        Job?.Dispose();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                ScreenJob?.Stop();
                if (thenDispose)
                    ScreenJob?.Dispose();
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
                if (thenDispose)
                    Device.Dispose();
            }
            catch (Exception)
            {
                //null;
            }
        }

        public Task DisposeAsync()
        {
            return Task.Run(() => Dispose() );
        }

        public void Dispose()
        {
            try
            {
                Job?.Dispose();
            }
            catch (Exception)
            {
                // ignored
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
                Device.Dispose();
            }
            catch (Exception)
            {
                //null;
            }
        }

        public void DeleteFile(bool whileAccessing = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(DestinationFilePath) || !File.Exists(DestinationFilePath))
                    return;

                if (whileAccessing)
                    IO.DeleteFileAfterAccessClose(DestinationFilePath);
                else
                    File.Delete(DestinationFilePath);
            }
            catch (Exception)
            {
                // null
            }
        }

        public override string ToString()
        {
            return $"ID=[{ThreadProc?.ManagedThreadId}] IsAlive=[{ThreadProc?.IsAlive}] IsCanceled=[{IsCanceled}]";
        }
    }
}
