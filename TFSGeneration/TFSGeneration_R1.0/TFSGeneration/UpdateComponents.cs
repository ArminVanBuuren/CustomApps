using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Utils.AppUpdater.Updater;

namespace TFSAssist
{
    [Serializable]
    public class TFSAssistUpdater
    {
        public IUpdater Updater { get; }
        public WindowState WindowState { get; }
        public bool TfsInProgress { get; }

        public TFSAssistUpdater(IUpdater updater, MainWindow window)
        {
            Updater = updater;
            WindowState = window.WindowState;
            TfsInProgress = window.TfsControl.InProgress;
        }

        public static void Check(string locationApp)
        {

        }

        public void Save()
        {

        }
    }
}
