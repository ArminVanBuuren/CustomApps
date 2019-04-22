using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.WinForm.MediaCapture.Screen
{
    public class FramesInfo : IDisposable
    {
        private readonly object sync1 = new object();
        private readonly object sync2 = new object();
        private int _framesPerSec = 0;

        private readonly Stopwatch _secondsMonitor;
        private readonly Stopwatch _inhibitMonitor;
        private readonly Timer _timer;

        public Dictionary<DateTime, KeyValuePair<long, int>> FramesPerSec { get; }
        public int AvgFrames => FramesPerSec.Count == 0 ? 0 : TotalFrames / FramesPerSec.Count;
        public int TotalFrames { get; private set; } = 0;
        public int FrameMSec { get; }

        public FramesInfo(int frameRate)
        {
            FrameMSec = frameRate == 0 ? 1000 : 1000 / frameRate;
            _faultMSecPerSec = 1000 - FrameMSec * frameRate;

            FramesPerSec = new Dictionary<DateTime, KeyValuePair<long, int>>();

            _secondsMonitor = new Stopwatch();
            _inhibitMonitor = new Stopwatch();

            _timer = new Timer(ResetPerSec, 0, 0, 1000);
            _secondsMonitor.Start();
        }

        public void InhibitStart()
        {
            _inhibitMonitor.Start();
        }

        public int _faultMSec = 0;
        public int _faultMSecPerSec = 0;

        public void InhibitStop()
        {
            try
            {
                _inhibitMonitor.Stop();

                if (_inhibitMonitor.ElapsedMilliseconds < FrameMSec)
                {
                    var diffMSec = FrameMSec - (int) _inhibitMonitor.ElapsedMilliseconds;

                    lock (sync2)
                    {
                        if (_faultMSec > diffMSec)
                        {
                            _faultMSec = _faultMSec - diffMSec;
                            return;
                        }
                        else
                        {
                            diffMSec = diffMSec - _faultMSec;
                            _faultMSec = 0;
                        }
                    }

                    //await Task.Delay(TimeSpan.FromMilliseconds(diffMSec)); - работает некорректно, иногда в меньшую иногда в большую сторону, все потому что await пытается получить доуступ к выделенному процессу
                    if (diffMSec > 3) // 3 милисекунды на примерную поргешность вычислений после _inhibitMonitor.Stop()
                        Thread.Sleep(TimeSpan.FromMilliseconds(diffMSec - 3));
                }
                else if (_inhibitMonitor.ElapsedMilliseconds > FrameMSec)
                {
                    lock (sync2)
                    {
                        _faultMSec = ((int) _inhibitMonitor.ElapsedMilliseconds) - FrameMSec;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                _inhibitMonitor.Reset();
            }
        }

        void ResetPerSec(object ignored)
        {
            lock (sync1)
            {
                if (_framesPerSec == 0)
                    return;

                FramesPerSec.Add(DateTime.Now, new KeyValuePair<long, int>(_secondsMonitor.ElapsedMilliseconds, _framesPerSec));
                _framesPerSec = 0;

                lock (sync2)
                {
                    _faultMSec += _faultMSecPerSec;
                }

                _secondsMonitor.Reset();
                _secondsMonitor.Start();
            }
        }

        public void PlusFrame()
        {
            lock (sync1)
            {
                _framesPerSec++;
                TotalFrames++;
            }
        }

        public void Dispose()
        {
            PlusFrame();

            _inhibitMonitor.Stop();
            _timer.Dispose();

            lock (sync1)
            {
                _framesPerSec = 0;
                _secondsMonitor.Stop();
                _secondsMonitor.Reset();
            }
        }
    }
}