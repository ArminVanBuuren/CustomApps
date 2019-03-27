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
        public static string FileUpdatesPath = $"{ASSEMBLY.ApplicationFilePath}.update";
        public IUpdater Updater { get; }
        public WindowState WindowState { get; }
        public bool ShowInTaskbar { get; }
        public bool TfsInProgress { get; }
        public List<TraceHighlighter> Traces { get; }
        public string PackName => Updater.ProjectBuildPack.Name;

        public WindowSaver(IUpdater updater, WindowState windowState, bool showInTaskbar, List<TraceHighlighter> traces, bool tfsInProgress)
        {
            if (updater == null)
                throw new ArgumentNullException(nameof(updater));

            Updater = updater;
            WindowState = windowState;
            ShowInTaskbar = showInTaskbar;
            Traces = traces;
            TfsInProgress = tfsInProgress;
        }

        public static WindowSaver Deserialize()
        {
            if (File.Exists(FileUpdatesPath))
            {
                try
                {
                    WindowSaver updater;
                    using (Stream stream = new FileStream(FileUpdatesPath, FileMode.Open, FileAccess.Read))
                    {
                        updater = new BinaryFormatter().Deserialize(stream) as WindowSaver;
                    }

                    return updater;
                }
                catch (Exception)
                {
                    DeleteFileUpdates();
                }
            }
            return null;
        }

        public void Serialize()
        {
            DeleteFileUpdates();

            try
            {
                using (FileStream stream = new FileStream(FileUpdatesPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    new BinaryFormatter().Serialize(stream, this);
                }

                File.SetAttributes(FileUpdatesPath, File.GetAttributes(FileUpdatesPath) | FileAttributes.Hidden);
            }
            catch (Exception)
            {
                DeleteFileUpdates();
                throw;
            }
        }


        public void Dispose()
        {
            try
            {
                DeleteFileUpdates();
                Updater.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        static void DeleteFileUpdates()
        {
            try
            {
                if (File.Exists(FileUpdatesPath))
                {
                    IO.AccessToFile(FileUpdatesPath);
                    File.Delete(FileUpdatesPath);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
