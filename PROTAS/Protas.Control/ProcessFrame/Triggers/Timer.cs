using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Protas.Components.PerformanceLog;
using Protas.Components.Types;
using Protas.Components.XPackage;
using Protas.Control.Resource;

namespace Protas.Control.ProcessFrame.Triggers
{
    internal sealed class TimerTrigger : Trigger, IProcessor
    {
        public const string ATTR_INTERVAL = "Interval";
        public const string ATTR_COUNT_CALLS = "CountCalls";
        public const string ATTR_DAYS = "Days";
        public const string ATTR_START = "Start";

        System.Timers.Timer BaseTimer { get; } = new System.Timers.Timer();
        public int Interval { get; }
        public Dictionary<string, bool> Days { get; } = new Dictionary<string, bool>();
        public int CountCalls { get; }
        public bool IsCorrect { get; } = false;
        /// <summary>
        /// Стандартный комплекс для класса TimerTrigger, System.Timers.Timer ничего не возвращает, по этому в хендлер всегда будет приходить один тот же комплекс, с пераичными контекстами
        /// (!! это не значит что будут значения одни и теже класс комплекс динамичный сам по себе, все будет зависеть от непосредственных ресурсов)
        /// </summary>
        ResourceComplex Complex { get; }
        public TimerTrigger(int id, XPack pack, Process proces) : base(id, pack, proces)
        {
            Interval = SetIntProperty(ATTR_INTERVAL, 24*60*60);
            if (Interval <= 10)
                Interval = 10;
            Interval = Interval*1000;
            CountCalls = SetIntProperty(ATTR_COUNT_CALLS, -1);

            string _days;
            if (!MainPack.Attributes.TryGetValue(ATTR_DAYS, out _days))
                GetDays("1;2;3;4;5;6;0");
            else if (!GetDays(_days))
            {
                AddLogForm(Log3NetSeverity.Error, "NoOne {0} Satisfy Condition", ATTR_DAYS);
                return;
            }

            double intervalToStart = GetIntervalToStartTimer();
            if (intervalToStart > 0)
            {
                AddLogForm(Log3NetSeverity.Debug, "Timer Will Start {0} Seconds Later", intervalToStart/1000);
                BaseTimer.Interval = intervalToStart;
            }
            else
                BaseTimer.Interval = 100;

            if (CountCalls > 0)
                BaseTimer.Elapsed += TimerOnElapsedWithCountCalls;
            else
                BaseTimer.Elapsed += TimerOnElapsed;

            Complex = GetComplex(null);

            IsCorrect = true;
        }

        int _currentCountCalls = 0;
        void TimerOnElapsedWithCountCalls(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (_currentCountCalls >= CountCalls)
            {
                Stop();
                return;
            }
            _currentCountCalls++;

            TimerOnElapsed(sender, elapsedEventArgs);
        }
        void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (BaseTimer.Interval != Interval)
                BaseTimer.Interval = Interval;
            if (Days[DateTime.Now.DayOfWeek.ToString("d")])
            {
                CallHandlers(Complex);
            }
        }

        double GetIntervalToStartTimer()
        {
            string startInterval;
            if (MainPack.Attributes.TryGetValue(ATTR_START, out startInterval))
            {
                if (GetTypeEx.IsTime(startInterval))
                    startInterval = string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd"), startInterval);
                DateTime dateToStart;
                if (DateTime.TryParse(startInterval, out dateToStart))
                {
                    double seconds = dateToStart.Subtract(DateTime.Now).Seconds;
                    if (seconds < 0)
                        return 0;
                    if (seconds > double.MaxValue/1000)
                        return double.MaxValue;
                    return seconds*1000;
                }
            }
            return 0;
        }

        int SetIntProperty(string findAttribute, int defaultValue)
        {
            string sValue;
            if (!MainPack.Attributes.TryGetValue(findAttribute, out sValue))
                return defaultValue;
            int iValue;
            return (int.TryParse(sValue, out iValue)) ? iValue : defaultValue;
        }
        /// <summary>
        /// Создаем массив с датами типа (string, bool), где проаписана каждая дата в поле string, и возможностью выполнения в определенный день bool
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        bool GetDays(string days)
        {
            string[] daysStr = days.Split(';');
            int countTrue = 0;
            for (int i = 0; i < 7; i++)
            {
                if (daysStr.Any(c => c.Equals(i.ToString())))
                {
                    countTrue++;
                    Days.Add(i.ToString(), true);
                }
                else
                    Days.Add(i.ToString(), false);
            }
            AddLogForm(Log3NetSeverity.Debug, "{0}:{1}", ATTR_DAYS, string.Join<KeyValuePair<string, bool>>(",", Days));
            return (countTrue > 0);
        }
        public bool Start()
        {
            if (!IsCorrect)
                return false;
            BaseTimer.Enabled = true;
            AddLog(Log3NetSeverity.Debug, "Started...");
            return true;
        }
        public bool Stop()
        {
            if (!IsCorrect)
                return false;
            BaseTimer.Enabled = false;
            AddLog(Log3NetSeverity.Debug, "Stopped...");
            return true;
        }
        public override void Dispose()
        {
            Stop();
            BaseTimer.Dispose();
        }
    }
}
