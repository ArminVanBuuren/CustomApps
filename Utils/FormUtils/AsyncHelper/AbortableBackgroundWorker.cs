﻿using System.ComponentModel;
using System.Threading;

namespace FormUtils.AsyncHelper
{
    public class AbortableBackgroundWorker : BackgroundWorker
    {
        private Thread workerThread;

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            workerThread = Thread.CurrentThread;
            try
            {
                base.OnDoWork(e);
            }
            catch (ThreadAbortException)
            {
                e.Cancel = true; //We must set Cancel property to true!
                Thread.ResetAbort(); //Prevents ThreadAbortException propagation
            }
        }

        public void Abort()
        {
            if (workerThread != null)
            {
                workerThread.Abort();
                //workerThread = null;
            }
        }
    }
}
