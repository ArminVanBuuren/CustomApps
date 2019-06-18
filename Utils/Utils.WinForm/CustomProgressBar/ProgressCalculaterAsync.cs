using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.WinForm.CustomProgressBar
{
    public interface IProgressBar
    {
        bool Visible { get; set; }
        int Maximum { get; set; }
        int Value { get; set; }
    }

    public class ProgressCalculaterAsync : IDisposable
    {
        private readonly SynchronizationContext _syncContext;
        private readonly IAsyncResult _result;
        private readonly Action _processChecking;

        public bool ProgressCompleted { get; private set; } = false;
        public IProgressBar ProgressBar { get; }
        public int CurrentProgressInterator { get; set; } = 0;
        public int TotalProgressInterator { get; private set; }

        public int PercentComplete { get; private set; }

        public ProgressCalculaterAsync(IProgressBar progressBar, int totalProgressIterator = 10)
        {
            if (totalProgressIterator <= 0)
                throw new ArgumentException($"Parameter {nameof(totalProgressIterator)} must be more then zero");

            _syncContext = SynchronizationContext.Current;
            ProgressBar = progressBar ?? throw new ArgumentNullException(nameof(progressBar));
            TotalProgressInterator = totalProgressIterator;

            CurrentProgressInterator = 0;
            ProgressCompleted = false;

            ProgressBar.Visible = true;
            _processChecking = new Action(ProgressChecking);
            _result = _processChecking.BeginInvoke(null, null);
        }

        void Stop()
        {
            ProgressCompleted = true;
            _processChecking?.EndInvoke(_result);
            ProgressBar.Visible = false;
        }

        public static ProgressCalculaterAsync operator ++(ProgressCalculaterAsync first)
        {
            first.CurrentProgressInterator++;
            return first;
        }

        void ProgressChecking()
        {
            try
            {
                int _prevValue = -1;
                while (!ProgressCompleted)
                {
                    if (CurrentProgressInterator == 0 || TotalProgressInterator == 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    double calc = (double)CurrentProgressInterator / TotalProgressInterator;
                    PercentComplete = ((int)(calc * 100)) >= 100 ? 100 : ((int)(calc * 100));

                    if (_prevValue == PercentComplete)
                        continue;
                    _prevValue = PercentComplete;

                    _syncContext.Post(delegate
                    {
                        ProgressBar.Value = PercentComplete;
                        ProgressBar.SetProgressNoAnimation(PercentComplete);
                    }, null);
                }
            }
            catch (Exception)
            {
                // null
            }
        }

        public void Dispose()
        {
            Stop();
        }

        public override string ToString()
        {
            return $"[{PercentComplete}%] Current=[{CurrentProgressInterator}] Total=[{TotalProgressInterator}]";
        }
    }
}
