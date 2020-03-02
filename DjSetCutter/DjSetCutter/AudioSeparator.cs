using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Lame;
using NAudio.Wave;

namespace DjSetCutter
{
    public class AudioSeparator
    {
        public EventHandler ProcessingException;
        private readonly Regex trackSeparator;
        private readonly Regex trackInfo;
        private readonly Regex dirParser;
        private readonly string sourcePath;
        private readonly string destinatonFormat;
        private readonly bool removeSourceCUE = false;
        private readonly bool removeSourceMp3 = false;
        private SemaphoreHelper<string> process;

        public AudioSeparator(Regex trackSeparator, Regex trackInfo, Regex dirParser, string sourcePath, string destinatonFormat, bool removeSourceCUE = false, bool removeSourceMp3 = false)
        {
            this.trackSeparator = trackSeparator;
            this.trackInfo = trackInfo;
            this.dirParser = dirParser;
            this.sourcePath = sourcePath;
            this.destinatonFormat = destinatonFormat;
            this.removeSourceCUE = removeSourceCUE;
            this.removeSourceMp3 = removeSourceMp3;
        }

        public async Task StartAsync()
        {
            var listOfDirs = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories).ToList();
            listOfDirs.Add(sourcePath);

            process = new SemaphoreHelper<string>((string dirPath) =>
            {
                try
                {
                    PerformSet(dirPath);
                }
                catch (Exception ex)
                {
                    ProcessingException?.Invoke($"{ex.GetType()} when process on folder '{dirPath}'. {ex}", EventArgs.Empty);
                }
            }, 5);

            await process.ExecuteAsync(listOfDirs);
        }

        public void Stop()
        {
            process?.Abort();
        }

        void PerformSet(string dirPath)
        {
            var dirName = Path.GetFileName(dirPath);
            var destFormat = Path.IsPathRooted(destinatonFormat) ? destinatonFormat : Path.Combine(dirPath, destinatonFormat);

            var files = Directory.GetFiles(dirPath, "*.cue");
            var mp3File = string.Empty;
            foreach (var cueFile in files)
            {
                try
                {
                    mp3File = cueFile.Substring(0, cueFile.Length - 4) + ".mp3";
                    if (!File.Exists(mp3File))
                        continue;

                    File.SetAttributes(cueFile, FileAttributes.Normal);
                    File.SetAttributes(mp3File, FileAttributes.Normal);

                    string cueData;
                    using (var inputStream = File.OpenRead(cueFile))
                    {
                        using (var inputReader = new StreamReader(inputStream, Encoding.GetEncoding("windows-1251")))
                        {
                            cueData = inputReader.ReadToEnd();
                        }
                    }

                    var tracks = new List<CueTrack>();
                    foreach (Match track in trackSeparator.Matches(cueData))
                    {
                        var trackDest = Replacer(dirParser, dirPath, destFormat, out _);
                        trackDest = Replacer(trackInfo, track.Value, trackDest, out var index);

                        var indexSplit = index.Split(':');
                        var spanMin = TimeSpan.FromMinutes(int.Parse(indexSplit[0]));
                        var spanSec = TimeSpan.FromSeconds(int.Parse(indexSplit[1]));
                        var spanMilisec = TimeSpan.FromMilliseconds(int.Parse(indexSplit[2] + "0"));
                        var start = spanMin + spanSec + spanMilisec;

                        tracks.Add(new CueTrack(trackDest, start));
                    }

                    if (tracks.Count == 0)
                        continue;

                    if (!Directory.Exists(tracks[0].Folder))
                        Directory.CreateDirectory(tracks[0].Folder);

                    using (Stream stream = new FileStream(mp3File, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        using (var reader = new Mp3FileReader(stream))
                        {
                            for (var i = 0; i < tracks.Count; i++)
                            {
                                if(process.IsCancellationRequested)
                                    return;

                                if (File.Exists(tracks[i].Destination))
                                    File.Delete(tracks[i].Destination);

                                TrimMp3(reader, tracks[i].Destination, tracks[i].Start, i + 1 >= tracks.Count ? reader.TotalTime : tracks[i + 1].Start);
                            }
                        }
                    }

                    if (process.IsCancellationRequested)
                        return;

                    if (removeSourceCUE)
                        File.Delete(cueFile);

                    if (removeSourceMp3)
                        File.Delete(mp3File);

                }
                catch (InvalidOperationException ex)
                {
                    ProcessingException?.Invoke($"Mp3 file '{mp3File}' was skipped. InvalidOperationException:{ex}", EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    ProcessingException?.Invoke($"Mp3 file '{mp3File}' was skipped. {ex.GetType()}:{ex}", EventArgs.Empty);
                }
            }
        }

        struct CueTrack
        {
            public string File { get; }
            public string Folder { get; }
            public string Destination { get; }
            public TimeSpan Start { get; }

            public CueTrack(string destination, TimeSpan start)
            {
                Destination = destination;
                Start = start;
                File = Destination.Substring(Destination.LastIndexOf('\\') + 1, Destination.Length - Destination.LastIndexOf('\\') - 1);
                Folder = Destination.Substring(0, Destination.LastIndexOf('\\'));
            }

            public override string ToString()
            {
                return $"{File} [{Start}]";
            }
        }

        static string Replacer(Regex pattern, string source, string format, out string indexStart)
        {
            indexStart = string.Empty;
            var result = format;
            var groups = pattern.Match(source).Groups;
            foreach (var groupName in pattern.GetGroupNames())
            {
                if(int.TryParse(groupName, out _))
                    continue;

                var value = groups[groupName].Value.Trim();
                if (groupName.Equals("INDEX", StringComparison.CurrentCultureIgnoreCase))
                {
                    indexStart = value;
                }
                result = new Regex($@"%\s*{groupName}\s*%", RegexOptions.IgnoreCase).Replace(result, GetCorrectEncoding(value));
            }

            return result;
        }


        static readonly string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        static string GetCorrectEncoding(string sample)
        {
            var encoded = Encoding.GetEncoding(1251).GetBytes(sample);
            var corrected = Encoding.GetEncoding(1250).GetString(encoded);

            foreach (var c in invalid)
            {
                corrected = corrected.Replace(c.ToString(), "");
            }

            return corrected;
        }

        private static void TrimWavFile(WaveFileReader reader, string outputPath, int startPos, int endPos)
        {
            //Mp3Frame frame = Mp3Frame.LoadFromStream(stream, false);
            //if (frame.SampleRate == 48000)
            //{
            //    ReturnException(string.Format("'{0}' was skip. Because need frame 44100.", sourceASOTMp3Path));
            //    using (var reader = new WaveFileReader(stream))
            //    {
            //        for (int i = 0; i < tracks.Count; i++)
            //        {
            //            string outputTrackResult = Path.Combine(pathResult, tracks[i].TrackFileName + ".mp3");

            //            int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

            //            int startPos = (int)tracks[i].Start.TotalMilliseconds * bytesPerMillisecond;
            //            startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

            //            int endBytes = (int)(i >= tracks.Count - 1 ? reader.TotalTime : tracks[i + 1].Start).TotalMilliseconds * bytesPerMillisecond;
            //            endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;
            //            int endPos = (int)reader.Length - endBytes;
            //        }
            //    }
            //}


            using (var writer = new WaveFileWriter(outputPath, reader.WaveFormat))
            {
                reader.Position = startPos;
                var buffer = new byte[1024];
                while (reader.Position < endPos)
                {
                    var bytesRequired = (int)(endPos - reader.Position);
                    if (bytesRequired > 0)
                    {
                        var bytesToRead = Math.Min(bytesRequired, buffer.Length);
                        var bytesRead = reader.Read(buffer, 0, bytesToRead);
                        if (bytesRead > 0)
                        {
                            writer.Write(buffer, 0, bytesRead);
                        }
                    }
                }

            }
        }

        void TrimMp3(Mp3FileReader reader, string outputPath, TimeSpan? begin, TimeSpan? end)
        {
            // удаляем существующий файл для перезаписи
            if (File.Exists(outputPath))
                File.Delete(outputPath);

            // если начало и конец совпадают или фрейм начала больше фактического конца аудио файла
            if (begin.HasValue && end.HasValue && (begin == end || begin >= reader.TotalTime))
                return;

            if (begin.HasValue && end.HasValue && begin > end)
                throw new ArgumentOutOfRangeException(nameof(end), @"end should be greater than begin");

            using (var writer = File.Create(outputPath))
            {
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    if (reader.CurrentTime >= begin || !begin.HasValue)
                    {
                        if (reader.CurrentTime <= end || !end.HasValue)
                            writer.Write(frame.RawData, 0, frame.RawData.Length);
                        else
                            break;
                    }
                }
            }
        }


        public static byte[] ConvertWavToMp3(string sourcePath)
        {
            using (var destination = new MemoryStream())
            using (var file = new FileStream(sourcePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var retMs = new MemoryStream())
            {
                file.CopyTo(destination);
                using (var rdr = new WaveFileReader(destination))
                using (var wtr = new LameMP3FileWriter(retMs, rdr.WaveFormat, 320))
                {
                    rdr.CopyTo(wtr);
                    wtr.Flush();
                    return retMs.ToArray();
                }
            }
        }

        static void ConvertToMP3(string sourceFilename, string targetFilename)
        {
            //using (var reader = new NAudio.Wave.AudioFileReader(sourceFilename))
            using (var reader = new WaveFileReader(sourceFilename))
            using (var writer = new LameMP3FileWriter(targetFilename, reader.WaveFormat, NAudio.Lame.LAMEPreset.STANDARD))
            {
                reader.CopyTo(writer);
            }
        }
    }
}
