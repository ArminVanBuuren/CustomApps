using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Utils;
using Utils.AppUpdater.Updater;

namespace TFSAssist
{
    [Serializable]
    public class TFSAssistUpdater : IDisposable
    {
        public static string FileUpdatesPath = $"{ASSEMBLY.ApplicationFilePath}.update";
        public IUpdater Updater { get; }
        public WindowState WindowState { get; }
        public bool TfsInProgress { get; }

        public TFSAssistUpdater(IUpdater updater, MainWindow window)
        {
            if (updater == null)
                throw new ArgumentNullException("updater");
            if (window == null)
                throw new ArgumentNullException("window");

            Updater = updater;
            WindowState = window.WindowState;
            TfsInProgress = window.TfsControl.InProgress;
        }

        public static TFSAssistUpdater Deserialize()
        {
            if (File.Exists(FileUpdatesPath))
            {
                try
                {
                    TFSAssistUpdater updater;
                    using (Stream stream = new FileStream(FileUpdatesPath, FileMode.Open, FileAccess.Read))
                    {
                        updater = new BinaryFormatter().Deserialize(stream) as TFSAssistUpdater;
                    }

                    return updater;
                }
                catch (Exception)
                {
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
            catch (Exception e)
            {

            }
        }


        public void Dispose()
        {
            try
            {
                DeleteFileUpdates();
                Updater.Dispose();
            }
            catch (Exception e)
            {
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
            catch (Exception e)
            {
            }
        }
    }
}
