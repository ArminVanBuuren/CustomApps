using System;
using System.Device.Location;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Utils;
using Utils.AppUpdater;
using Utils.AppUpdater.Pack;

namespace TFSAssist.Remoter
{
    class SystemInfo : IDisposable
    {
        private GeoCoordinateWatcher _watcher;
        private string _locationResult = "Location unknown.";

        public event ProcessingErrorHandler OnProcessingExceptions;

        public SystemInfo()
        {

        }

        public void Initialize()
        {
            try
            {
                _watcher = new GeoCoordinateWatcher();
                _watcher.StatusChanged += GeoWatcher_StatusChanged;
                _watcher.Start();
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }
        }

        void GeoWatcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status != GeoPositionStatus.Ready)
                return;

            if (_watcher.Position.Location.IsUnknown)
                return;

            var coordinate = _watcher.Position.Location;
            _locationResult = $"Latitude=[{coordinate.Latitude.ToString().Replace(",", ".")}] Longitude=[{coordinate.Longitude.ToString().Replace(",", ".")}]";
        }

        public string GetLocationInfo()
        {
            return _locationResult;
        }

        public string GetDrivesInfo()
        {
            var drives = DriveInfo.GetDrives();
            if (drives.Length == 0)
                return null;

            var result = string.Empty;
            foreach (var drive in drives)
            {
                if (drive.IsReady)
                {
                    result += $"Drive=[{drive.Name}] TotalSize=[{IO.FormatBytes(drive.TotalSize, out _)}] FreeSize=[{IO.FormatBytes(drive.TotalFreeSpace, out _)}]\r\n";
                }
            }

            return result.Trim();
        }

        public string GetDiagnosticInfo()
        {
            var proc = Process.GetCurrentProcess();

            var totalCPU = (int)SERVER.GetCpuUsage();
            var totalRAM = SERVER.GetTotalMemoryInMiB();
            var avalRAM = SERVER.GetPhysicalAvailableMemoryInMiB();

            var appCPUUsage = (int)SERVER.GetCpuUsage(proc);
            var appRAMUsage = SERVER.GetMemUsage(proc).ToFileSize();

            var result = $"AppCPU=[{appCPUUsage}%]\r\nAppRAM=[{appRAMUsage}]\r\nCPU=[{totalCPU}%]\r\nRAM=[Free='{avalRAM} MB' Total='{totalRAM} MB']";

            return result;
        }

        public string GetHostInfo()
        {
            try
            {
                return $"Host=[{Dns.GetHostName()}]";
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string GetDetailedHostInfo(string[] additionalPaths = null)
        {
            var detInfo = new StringBuilder();

            var hostIps = HOST.GetIPAddresses();
            var maxLenghtSpace = hostIps.Aggregate("", (max, cur) => max.Length > cur.Interface.Name.Length ? max : cur.Interface.Name).Length + 3;

            foreach (var address in hostIps)
            {
                detInfo.Append($"{address.Interface.Name} {new string('.', maxLenghtSpace - address.Interface.Name.Length)} [{address.IPAddress.Address}] ({address.Interface.Description})\r\n");
            }

            var whiteSpace = "=";
            if (maxLenghtSpace > 10)
                whiteSpace = " " + new string('.', maxLenghtSpace - 10) + " ";

            detInfo.Append($"ExternalIP{whiteSpace}[{HOST.GetExternalIPAddress()}]\r\n");


            var det = HOST.GetDetailedHostInfo();
            if (det?.Count > 0)
            {
                detInfo.Append($"\r\nDetailed:\r\n");
                maxLenghtSpace = det.Keys.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length + 3;

                foreach (var value in det)
                {
                    detInfo.Append($"{value.Key} {new string('.', maxLenghtSpace - value.Key.Length)} [{string.Join("];[", value.Value)}]\r\n");
                }

                try
                {
                    // должен быть именно GetEntryAssembly вместо GetExecutingAssembly. Т.к. GetExecutingAssembly смотрит исполняемую библиотеку, а не испольняемый exe файл
                    var localVersions = BuildPackInfo.GetLocalVersions(AssemblyInfo.ApplicationDirectory, SearchOption.AllDirectories);
                    if (localVersions?.Count > 0)
                    {
                        maxLenghtSpace = localVersions.Aggregate("", (max, cur) => max.Length > cur.Location.Length ? max : cur.Location).Length + 3;

                        detInfo.Append($"\r\nLocation=[{AssemblyInfo.ApplicationDirectory}]\r\n");
                        foreach (var versionLocalFiles in localVersions)
                        {
                            detInfo.Append(
                                $"{versionLocalFiles.Location} {new string('.', maxLenghtSpace - versionLocalFiles.Location.Length)} [{versionLocalFiles.Version}] ({new FileInfo(Path.Combine(AssemblyInfo.ApplicationDirectory, versionLocalFiles.Location)).Length.ToMegabytes()} MB)\r\n");
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (additionalPaths != null && additionalPaths.Length > 0)
            {
                foreach (var path in additionalPaths)
                {
                    detInfo.Append($"\r\nPath=[{path}]\r\n");
                    var tempDir = new DirectoryInfo(path);
                    var tempFilesCollection = tempDir.GetFiles("*", SearchOption.AllDirectories);
                    if (tempFilesCollection.Any())
                    {
                        maxLenghtSpace = tempFilesCollection.Aggregate("", (max, cur) => max.Length > cur.FullName.Length ? max : cur.FullName).Length + 3;

                        foreach (var tempFile in tempFilesCollection)
                        {
                            detInfo.Append($"{tempFile.FullName} {new string('.', maxLenghtSpace - tempFile.FullName.Length)} [{BuildNumber.FromFile(tempFile.FullName)}] ({tempFile.Length.ToMegabytes()} MB)\r\n");
                        }
                    }
                }
            }

            return detInfo.ToString().Trim();
        }

        void WriteExLog(Exception log)
        {
            OnProcessingExceptions?.Invoke(log.ToString());
        }

        public void Dispose()
        {
            _watcher?.Stop();
        }
    }
}
