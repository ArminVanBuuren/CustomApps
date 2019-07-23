using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using Utils;
using Utils.AppUpdater.Updater;

namespace TFSAssist
{
    [Serializable]
    public class WindowSaver : IDisposable
    {
        public static string WindowSaverPath = $"{ASSEMBLY.ApplicationFilePath}.update";
        public IUpdater Updater { get; }
        public WindowState WindowState { get; }
        public bool ShowInTaskbar { get; }
        public bool TfsInProgress { get; }
        public List<TraceHighlighter> Traces { get; }



        public WindowSaver(IUpdater updater, WindowState windowState, bool showInTaskbar, List<TraceHighlighter> traces, bool tfsInProgress):this(windowState, showInTaskbar, traces, tfsInProgress)
        {
            Updater = updater;
        }

        public WindowSaver(WindowState windowState, bool showInTaskbar, List<TraceHighlighter> traces, bool tfsInProgress)
        {
            WindowState = windowState;
            ShowInTaskbar = showInTaskbar;
            Traces = traces;
            TfsInProgress = tfsInProgress;
        }

        public static WindowSaver Deserialize()
        {
            if (File.Exists(WindowSaverPath))
            {
                try
                {
                    WindowSaver updater;
                    using (Stream stream = new FileStream(WindowSaverPath, FileMode.Open, FileAccess.Read))
                    {
                        updater = new BinaryFormatter().Deserialize(stream) as WindowSaver;
                    }

                    return updater;
                }
                catch (Exception)
                {
                    DeleteWindowSaverFile();
                }
            }
            return null;
        }

        public void Serialize()
        {
            DeleteWindowSaverFile();

            try
            {
                using (var stream = new FileStream(WindowSaverPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    new BinaryFormatter().Serialize(stream, this);
                }

                File.SetAttributes(WindowSaverPath, File.GetAttributes(WindowSaverPath) | FileAttributes.Hidden);
            }
            catch (Exception)
            {
                DeleteWindowSaverFile();
                throw;
            }
        }


        public void Dispose()
        {
            try
            {
                DeleteWindowSaverFile();
                Updater?.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        static void DeleteWindowSaverFile()
        {
            try
            {
                if (File.Exists(WindowSaverPath))
                {
                    IO.GetAccessToFile(WindowSaverPath);
                    File.Delete(WindowSaverPath);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
