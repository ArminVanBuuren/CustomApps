using System;
using System.Threading;
using System.Windows.Forms;

namespace Utils.WinForm.CustomProgressBar
{
    internal class InnerProgressBar : IProgressBar
    {
        private readonly ProgressBar _progressBar;
        public Control ProgressBar => _progressBar;

        public InnerProgressBar(ProgressBar progressBar)
        {
            _progressBar = progressBar;
        }

        public bool Visible
        {
            get => ProgressBar.Visible;
            set => ProgressBar.Visible = value;
        }

        public int Maximum
        {
            get => _progressBar.Maximum;
            set => _progressBar.Maximum = value;
        }

        public int Value
        {
            get => _progressBar.Value;
            set => _progressBar.Value = value;
        }
    }

    public class ProgressCalculationAsync : IDisposable
    {
        readonly object syncRoot = new object();

        private SynchronizationContext _syncContext;
        private int _currentProgressIterator = 0;
        private int _totalProgressIterator = 0;
        private int _precentComplete = 0;
        private Thread _calculation;

        IProgressBar _progressBar { get; }

        public int CurrentProgressIterator
        {
            get => _currentProgressIterator;
            private set
            {
                if (_currentProgressIterator >= value || value < 0)
                    return;

                _currentProgressIterator = value >= TotalProgressIterator ? TotalProgressIterator : value;
            }
        }

        public int TotalProgressIterator
        {
            get => _totalProgressIterator;
            private set
            {
                if(value < 0)
                    return;

                _totalProgressIterator = value;
            }
        }

        public int PercentComplete
        {
            get => _precentComplete;
            private set
            {
                _precentComplete = value;
                SetValue(_precentComplete);
            }
        }

        public bool ProgressCompleted { get; private set; } = false;

        public ProgressCalculationAsync(ProgressBar progressBar, int totalProgressIterator = 10)
        {
            if (progressBar == null)
                throw new ArgumentNullException(nameof(progressBar));

            _progressBar = new InnerProgressBar(progressBar);
            Initialize(totalProgressIterator);
        }

        public ProgressCalculationAsync(XpProgressBar progressBar, int totalProgressIterator = 10)
        {
            _progressBar = progressBar ?? throw new ArgumentNullException(nameof(progressBar));
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


        public static ProgressCalculationAsync operator ++(ProgressCalculationAsync first)
        {
            first.CurrentProgressIterator++;
            return first;
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
                _progressBar.ProgressBar.SafeInvoke(() =>
                {
                    _progressBar.Value = 0;
                    _progressBar.Visible = true;
                });

                while (!ProgressCompleted)
                {
                    if (CurrentProgressIterator == 0 || TotalProgressIterator == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    if (CurrentProgressIterator == TotalProgressIterator)
                    {
                        ProgressCompleted = true;
                        lock (syncRoot)
                            PercentComplete = 100;
                        break;
                    }

                    var calc = (double) CurrentProgressIterator / TotalProgressIterator;
                    var percentage = ((int) (calc * 100)) >= 100 ? 100 : ((int) (calc * 100));

                    if (_precentComplete >= percentage)
                        continue;

                    lock (syncRoot)
                        PercentComplete = percentage;
                }

                _progressBar.ProgressBar.SafeInvoke(() =>
                {
                    _progressBar.Value = 100;
                    _progressBar.Visible = false;
                });
            }
            catch (ObjectDisposedException ex)
            {

            }
            catch (Exception ex)
            {
                // null
            }
        }

        /// <summary>
        /// обязательно синхронно!!!!
        /// </summary>
        /// <param name="currentValue"></param>
        void SetValue(int currentValue)
        {
            try
            {
                if (currentValue == 100)
                {
                    _progressBar.ProgressBar.SafeInvoke(() => _progressBar.SetProgressNoAnimation(currentValue));
                    return;
                }

                var progressBarValue = -1;
                _progressBar.ProgressBar.SafeInvoke(() => progressBarValue = _progressBar.Value);

                var gap = currentValue - progressBarValue;
                if (gap == 1)
                {
                    _progressBar.ProgressBar.SafeInvoke(() => _progressBar.SetProgressNoAnimation(_progressBar.Value + 1));
                }
                else
                {
                    for (var i = 0; i < gap; i++)
                    {
                        _progressBar.ProgressBar.SafeInvoke(() => _progressBar.SetProgressNoAnimation(_progressBar.Value + 1));
                        Thread.Sleep(3);
                    }

                    _progressBar.ProgressBar.SafeInvoke(() => _progressBar.SetProgressNoAnimation(currentValue));
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void Reset()
        {
            Stop();

            CurrentProgressIterator = 0;

            try
            {
                if (_calculation != null)
                {
                    int countOfCheckThread = 0;
                    ProgressCompleted = true;
                    while (_calculation.IsAlive)
                    {
                        Thread.Sleep(10);
                        countOfCheckThread++;
                        if (countOfCheckThread >= 200)
                        {
                            _calculation.Abort();
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            ProgressCompleted = false;
            _calculation = new Thread(ProgressChecking)
            {
                Priority = ThreadPriority.Lowest
            };
            _calculation.Start();
        }

        public void Stop()
        {
            ProgressCompleted = true;
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