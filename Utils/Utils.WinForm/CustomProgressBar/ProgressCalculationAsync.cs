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

    public class ProgressCalculationAsync : IDisposable
    {
        private int _currentProgressIterator = 0;
        private readonly SynchronizationContext _syncContext;

        private IAsyncResult _result;
        private Action _processChecking;

        public bool ProgressCompleted { get; private set; } = false;
        public IProgressBar ProgressBar { get; }

        public int CurrentProgressIterator
        {
            get => _currentProgressIterator;
            private set
            {
                if (_currentProgressIterator >= value)
                    return;

                _currentProgressIterator = value >= TotalProgressIterator ? TotalProgressIterator : value;
            }
        }

        public int TotalProgressIterator { get; private set; }

        public int PercentComplete { get; private set; }

        public ProgressCalculationAsync(IProgressBar progressBar, int totalProgressIterator = 10)
        {
            if (totalProgressIterator <= 0)
                throw new ArgumentException($"Parameter {nameof(totalProgressIterator)} must be more then zero");

            _syncContext = SynchronizationContext.Current;
            ProgressBar = progressBar ?? throw new ArgumentNullException(nameof(progressBar));
            TotalProgressIterator = totalProgressIterator;

            Reset();
        }

        public void Reset()
        {
            Stop();

            CurrentProgressIterator = 0;
            ProgressCompleted = false;

            ProgressBar.Value = 0;
            ProgressBar.Visible = true;

            _processChecking = new Action(ProgressChecking);
            _result = _processChecking.BeginInvoke(null, null);
        }

        public void Stop()
        {
            ProgressCompleted = true;

            _processChecking?.EndInvoke(_result);
            _processChecking = null;
            _result = null;

            if (ProgressBar.Visible)
                ProgressBar.Visible = false;
        }

        public static ProgressCalculationAsync operator ++(ProgressCalculationAsync first)
        {
            first.CurrentProgressIterator++;
            return first;
        }

        public void Append()
        {
            CurrentProgressIterator++;
        }

        public void Append(int value)
        {
            CurrentProgressIterator += value;
        }

        public void AddBootPercent(int percent)
        {
            if (percent <= 0)
                throw new ArgumentException(nameof(percent));

            TotalProgressIterator += (percent * TotalProgressIterator) / 100;
        }

        void ProgressChecking()
        {
            try
            {
                int _prevValue = -1;
                while (!ProgressCompleted)
                {
                    if (CurrentProgressIterator == 0 || TotalProgressIterator == 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    double calc = (double) CurrentProgressIterator / TotalProgressIterator;
                    int percent = ((int) (calc * 100)) >= 100 ? 100 : ((int) (calc * 100));

                    if (_prevValue >= percent)
                        continue;

                    PercentComplete = percent;
                    _prevValue = percent;


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
            return $"Complete={PercentComplete}%";
        }
    }
}