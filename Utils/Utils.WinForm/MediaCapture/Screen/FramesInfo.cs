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
        private readonly object sync = new object();
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

        public async void InhibitStop()
        {
            _inhibitMonitor.Stop();

            if (_inhibitMonitor.ElapsedMilliseconds < FrameMSec)
            {
                int diffMSec = FrameMSec - (int)_inhibitMonitor.ElapsedMilliseconds;
                if (diffMSec > 10)
                    await Task.Delay(diffMSec - 10);
            }

            _inhibitMonitor.Reset();
        }

        void ResetPerSec(object ignored)
        {
            lock (sync)
            {
                if (_framesPerSec == 0)
                    return;

                FramesPerSec.Add(DateTime.Now, new KeyValuePair<long, int>(_secondsMonitor.ElapsedMilliseconds, _framesPerSec));
                _framesPerSec = 0;
                _secondsMonitor.Reset();
                _secondsMonitor.Start();
            }
        }

        //public static FramesInfo operator ++(FramesInfo @class)
        //{
        //    AddFrame(@class);
        //    return @class;
        //}

        //static void AddFrame(FramesInfo @class)
        //{
        //    lock (@class.sync)
        //    {
        //        @class._framesPerSec++;
        //        @class.TotalFrames++;
        //    }

        //    //if (@class._watcher.ElapsedMilliseconds > 1000)
        //    //{
        //    //    @class.FramesPerSec.Add(DateTime.Now, new KeyValuePair<long, int>(@class._watcher.ElapsedMilliseconds, @class._framesPerSec));
        //    //    @class._framesPerSec = 0;
        //    //    @class._watcher.Reset();
        //    //    @class._watcher.Start();
        //    //}
        //}

        public void PlusFrame()
        {
            lock (sync)
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

            lock (sync)
            {
                _framesPerSec = 0;
                _secondsMonitor.Stop();
                _secondsMonitor.Reset();
            }
        }
    }
}