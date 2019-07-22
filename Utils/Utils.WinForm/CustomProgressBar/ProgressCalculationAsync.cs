using System;
using System.Threading;
using System.Windows.Forms;

namespace Utils.WinForm.CustomProgressBar
{
    public class ProgressCalculationAsync : IDisposable
    {
        private int _precentComplete = 0;
        private readonly Action<int> _setValue;

        ProgressBar _progressBar { get; }
        XpProgressBar _xpProgressBar { get; }

        private SynchronizationContext _syncContext;
        private int _currentProgressIterator = 0;

        private Thread _calculation;
        //private IAsyncResult _result;
        //private Action _processChecking;

        public bool ProgressCompleted { get; private set; } = false;


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

        public int PercentComplete
        {
            get => _precentComplete;
            private set => _setValue.Invoke(value);
        }

        public ProgressCalculationAsync(ProgressBar progressBar, int totalProgressIterator = 10)
        {
            _progressBar = progressBar ?? throw new ArgumentNullException(nameof(progressBar));
            _setValue = SetValue;
            Initialize(totalProgressIterator);
        }

        public ProgressCalculationAsync(XpProgressBar progressBar, int totalProgressIterator = 10)
        {
            _xpProgressBar = progressBar ?? throw new ArgumentNullException(nameof(progressBar));
            _setValue = SetValueXp;
            Initialize(totalProgressIterator);
        }

        void Initialize(int totalProgressIterator)
        {
            if (totalProgressIterator <= 0)
                throw new ArgumentException($"Parameter {nameof(totalProgressIterator)} must be more then zero");

            _syncContext = SynchronizationContext.Current;
            TotalProgressIterator = totalProgressIterator;

            Reset();
        }

        public void Reset()
        {
            Stop();

            CurrentProgressIterator = 0;
            ProgressCompleted = false;

            if (_progressBar != null)
            {
                _progressBar.Value = 0;
                _progressBar.Visible = true;
            }
            else
            {
                _xpProgressBar.Position = 0;
                _xpProgressBar.Visible = true;
            }

            _calculation = new Thread(ProgressChecking)
            {
                Priority = ThreadPriority.Lowest
            };
            _calculation.Start();

            //_processChecking = ProgressChecking;
            //_result = _processChecking.BeginInvoke(null, null);
        }

        public void Stop()
        {
            ProgressCompleted = true;

            //_processChecking?.EndInvoke(_result);
            //_processChecking = null;
            //_result = null;

            if (_progressBar != null && _progressBar.Visible)
                _progressBar.Visible = false;
            else if (_xpProgressBar != null && _xpProgressBar.Visible)
                _xpProgressBar.Visible = false;
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
                var _prevPercentage = -1;
                while (!ProgressCompleted)
                {
                    if (CurrentProgressIterator == 0 || TotalProgressIterator == 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    var calc = (double) CurrentProgressIterator / TotalProgressIterator;
                    var percentage = ((int) (calc * 100)) >= 100 ? 100 : ((int) (calc * 100));

                    if (_prevPercentage >= percentage)
                        continue;

                    PercentComplete = percentage;
                    _prevPercentage = percentage;
                }
            }
            catch (ObjectDisposedException ex)
            {

            }
            catch (Exception ex)
            {
                // null
            }
        }

        void SetValue(int currentValue)
        {
            _progressBar.SafeInvoke(() =>
            {
                var diff = currentValue - _progressBar.Value;
                if (diff <= 4)
                {
                    _progressBar.SetProgressNoAnimation(currentValue);
                }
                else
                {
                    var iterations = diff / 4;

                    for (var i = 0; i < iterations; i++)
                    {
                        _progressBar.SetProgressNoAnimation(_progressBar.Value + 4);
                        Thread.Sleep(10);
                    }

                    _progressBar.SetProgressNoAnimation(currentValue);
                }
            }, false);
        }

        void SetValueXp(int currentValue)
        {
            _xpProgressBar.SafeInvoke(() =>
            {
                var diff = currentValue - _xpProgressBar.Position;
                if (diff <= 4)
                {
                    _xpProgressBar.Position = currentValue;
                }
                else
                {
                    var iterations = diff / 4;

                    for (var i = 0; i < iterations; i++)
                    {
                        _xpProgressBar.Position = _xpProgressBar.Position + 4;
                        Thread.Sleep(10);
                    }

                    _xpProgressBar.Position = currentValue;
                }
            }, false);
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