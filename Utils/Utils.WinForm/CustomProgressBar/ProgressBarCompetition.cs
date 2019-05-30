using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils.WinForm.CustomProgressBar
{
    public class ProgressBarCompetition<T>
    {
        private IAsyncResult _asyncResult;
        private Func<ProgressBarCompetition<T>, T> _datafilter;
        private ProgressBar _progressBar;

        public bool ProgressCompleted { get; private set; } = false;
        public int ProgressValue { get; set; } = 0;
        public int TotalProgress { get; }
        private Action<T> _someMethodForComplete;
        public ProgressBarCompetition(ProgressBar progressBar, int totalProgress, Action<T> someMethodForComplete = null)
        {
            _progressBar = progressBar;
            TotalProgress = totalProgress;
            _someMethodForComplete = someMethodForComplete;
        }

        public void StartProgress(Func<ProgressBarCompetition<T>, T> method)
        {
            ProgressValue = 0;
            _progressBar.Visible = true;
            ProgressCompleted = false;

            var precentCalculate = new Action(ProgressAsync);
            precentCalculate.BeginInvoke(null, null);

            _datafilter = new Func<ProgressBarCompetition<T>, T>(method);
            _asyncResult = _datafilter.BeginInvoke(this, IsCompleted, _datafilter);
        }

        public void TerminateProgress()
        {
            try
            {
                if (!ProgressCompleted)
                {
                    ProgressCompleted = true;
                    _datafilter?.EndInvoke(_asyncResult);
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(@"Operation was canceled.");
            }
        }

        void IsCompleted(IAsyncResult asyncResult)
        {
            ProgressCompleted = true;
            AsyncResult ar = asyncResult as AsyncResult;
            Func<ProgressBarCompetition<T>, T> caller = (Func<ProgressBarCompetition<T>, T>)ar.AsyncDelegate;
            T result = caller.EndInvoke(asyncResult);

            _progressBar.Invoke(new MethodInvoker(delegate
            {
                _progressBar.Visible = false;
                _someMethodForComplete?.Invoke(result);
            }));
        }

        void ProgressAsync()
        {
            try
            {
                int _prevValue = -1;
                while (!ProgressCompleted)
                {
                    if (ProgressValue == 0 || TotalProgress == 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    double calc = (double)ProgressValue / TotalProgress;
                    int progr = ((int)(calc * 100)) >= 100 ? 100 : ((int)(calc * 100));

                    if (_prevValue == progr)
                        continue;
                    _prevValue = progr;

                    _progressBar.Invoke(new MethodInvoker(delegate
                    {
                        _progressBar.Value = progr;
                        _progressBar.SetProgressNoAnimation(progr);
                    }));
                }
            }
            catch (Exception)
            {
                // null
            }
        }
    }
}
