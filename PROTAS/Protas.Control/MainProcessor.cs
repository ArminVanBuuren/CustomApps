using System;
using System.IO;
using System.Security.Permissions;
using Protas.Components.PerformanceLog;
using Protas.Components.XPackage;

namespace Protas.Control
{
    public class MainProcessor : IDisposable
    {
        System.Timers.Timer _timerRefreshConfig = new System.Timers.Timer();
        double _DefRefreshMin = 100;
        public string ConfigPath { get; }
        Log3Net MainLog3Net { get; set;}
        ShellLog3Net LogWrite { get; set; }
        XmlTransform XmlTrans { get; set;}
        Dispatcher Dispatcher { get; set; }
        public MainProcessor(string configPath)
        {
            ConfigPath = configPath;
            Initialize();
        }
        void Initialize()
        {
            try
            {
                XmlTrans = new XmlTransform(ConfigPath, null);
                if(!XmlTrans.IsCorrect)
                    throw new Exception("Invalid Config File. Not Initialize");
                MainLog3Net = new Log3Net(XmlTrans);

                if (!Load(XmlTrans.XPackage, MainLog3Net))
                    throw new Exception("Dispatcher Is Not Initialize");
                _timerRefreshConfig.Elapsed += RefreshConfig;
                _timerRefreshConfig.Enabled = true;

                ConfigFileWatcher();
            }
            catch (Exception ex)
            {
                Log3Net.AddEventLog(true, "Exception:{0}\r\n{1}", ex.Message, ex.StackTrace);
                Stop();
            }
        }
        bool Load(XPack parentXpack, Log3Net log)
        {
            try
            {
                LogWrite = new ShellLog3Net(log);
                LogWrite.AddLog(Log3NetSeverity.Debug, "Start");
                Dispatcher?.Stop();
                Dispatcher = new Dispatcher(parentXpack, LogWrite);
                Dispatcher.Start();
                double refreshConfInterval = TimeoutRefreshConfig;
                if (_timerRefreshConfig.Interval != refreshConfInterval)
                    _timerRefreshConfig.Interval = refreshConfInterval;
                LogWrite.AddLog(Log3NetSeverity.Debug, "End");
            }
            catch(Exception ex)
            {
                LogWrite?.AddLogForm(Log3NetSeverity.Error, "Catched Exception:{0}\r\n{1}\r\n{2}", ex.Message, ex.Data, ex.StackTrace);
                Stop();
                return false;
            }
            return true;
        }
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        void ConfigFileWatcher()
        {
            string[] args = Environment.GetCommandLineArgs();
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(ConfigPath);
            watcher.Filter = Path.GetFileName(ConfigPath);
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Changed += ConfigOnChanged;
            watcher.EnableRaisingEvents = true;
            LogWrite.AddLog(Log3NetSeverity.Max, "Initiated");
        }
        void RefreshConfig(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (changedConfig > 0)
                {
                    LogWrite.AddLogForm(Log3NetSeverity.Debug, "Config Changed:{0} times; Starting Refresh Config...", ((changedConfig > 1) ? changedConfig / 2 : 1));
                    if (Reload())
                        LogWrite.AddLog(Log3NetSeverity.Debug, "Config Successfully Refreshed");
                    else
                        LogWrite.AddLog(Log3NetSeverity.Debug, "Config Not Refreshed! Old Configuration In Working");
                    changedConfig = 0;
                }
            }
            catch(Exception ex)
            {
                LogWrite.AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.Data);
            }
        }
        bool Reload()
        {
            XmlTransform newXTrans = new XmlTransform(ConfigPath, LogWrite);
            if (newXTrans.IsCorrect)
            {
                if (!XmlTrans.XPackage.CompareXPack(newXTrans.XPackage))
                { 
                    if (MainLog3Net.ReLoad(ConfigPath))
                    {
                        XmlTrans = newXTrans;
                        if (Load(XmlTrans.XPackage, MainLog3Net))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        int changedConfig = 0;
        void ConfigOnChanged(object source, FileSystemEventArgs e)
        {
            if (e?.ChangeType == WatcherChangeTypes.Changed)
                changedConfig++;
        }

        /// <summary>
        /// Время в минутах с какой частотой обновлять конфиг если изменения в файле конфига были произведенны
        /// </summary>
        public double TimeoutRefreshConfig
        {
            get
            {
                double _refresh;
                string result = XmlTrans.XPackage[@"/config"]?[0].Attributes["refreshmin"];
                double.TryParse(result, out _refresh);
                if (_refresh <= 0)
                    _refresh = _DefRefreshMin;
                LogWrite.AddLogForm(Log3NetSeverity.Debug, "Refresh Time Min:{0}", _refresh);
                _refresh = _refresh * 60 * 1000;
                return _refresh;
            }
        }

        public void Dispose()
        {
            Dispatcher?.Dispose();
            LogWrite.AddLog(Log3NetSeverity.Debug, "Success");
        }
        public void Stop()
        {
            LogWrite.AddLog(Log3NetSeverity.Debug, "Stopping...");
            Dispose();
            System.Environment.Exit(0);
        }
    }
}