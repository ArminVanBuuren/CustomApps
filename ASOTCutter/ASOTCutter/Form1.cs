using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace ASOTCutter
{
    public partial class Form1 : Form
    {
        private static readonly Regex cueTrackSeparator = new Regex(@"TRACK.+?INDEX\s*[0-9 :]+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        //static readonly Regex reg = new Regex(@".+?\s*TRACK\s*(?<TRACK>\d+)\s*.+?PERFORMER\s*\""(?<PERFORMER>.+?)\"".+?TITLE\s*\""(?<TITLE>.+?)\"".+?INDEX\s*01\s*(?<INDEX>\d+:\d+:\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
        
        static readonly string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        private bool isWorked = false;
        private Thread _asyncThread;
        private DeleteSourceFiles deleteSourceFiles;
        delegate void DeleteSourceFiles(string cuePath, string mp3Path);

        public Form1()
        {
            InitializeComponent();
            textBoxDirPath.Text = @"D:\MUSIC\ ASOT\400-499\464";
            textBoxFormat.Text = @"[ASOT %DIR_NAME%] %TRACK%. %PERFORMER% - %TITLE%";
            outputDirectory.Text = @"Result";
        }

        private void ButtonDirPath_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = !textBoxDirPath.Text.IsNullOrEmpty() && Directory.Exists(textBoxDirPath.Text) ? textBoxDirPath.Text : Directory.GetCurrentDirectory();
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBoxDirPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void ButtonStartStop_Click(object sender, EventArgs e)
        {
            StatusTextLable.Text = "";

            isWorked = !isWorked;
            if (isWorked)
            {
                if (textBoxDirPath.Text.IsNullOrEmpty() && !Directory.Exists(textBoxDirPath.Text))
                {
                    StatusTextLable.Text = @"Directory is empty or not Exist!";
                    isWorked = !isWorked;
                    return;
                }

                StatusTextLable.Text = @"Working...";
                ButtonStartStop.Text = @"Stop";
                textBoxDirPath.Enabled = false;
                textBoxFormat.Enabled = false;
                ButtonDirPath.Enabled = false;
                deleteSourceSet.Enabled = false;
                outputDirectory.Enabled = false;
                exceptionMessage.Text = string.Empty;

                if (deleteSourceSet.Checked)
                {
                    deleteSourceFiles = (cuePath, mp3Path) =>
                    {
                        File.Delete(cuePath);
                        File.Delete(mp3Path);
                    };
                }
                else
                {
                    deleteSourceFiles = null;
                }

                _asyncThread = new Thread(() => StartProcess(textBoxDirPath.Text));
                _asyncThread.Start();
                
            }
            else
            {
                _asyncThread?.Abort();
            }
            //FindAllSubDir(textBoxDirPath.Text);
        }

        void StartProcess(string dirPath)
        {
            try
            {
                string[] DirPaths = Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories);
                foreach (var dir in DirPaths)
                {
                    GetCueAndMp3(dir);
                }
                GetCueAndMp3(dirPath);

                StoppedProcessActivateForm("Finished");
            }
            catch (ThreadAbortException)
            {
                StoppedProcessActivateForm("Stopped");
            }
            catch (Exception ex)
            {
                string detailException = string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace);
                ex = ex.InnerException;
                while (ex != null)
                {
                    detailException = detailException + "\r\n" + string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace);
                    ex = ex.InnerException;
                }
                ReturnException(detailException);
                StoppedProcessActivateForm("Catched Exception");
            }
        }

        void StoppedProcessActivateForm(string bottomStr)
        {
            Invoke(new MethodInvoker(delegate
            {
                isWorked = false;
                StatusTextLable.Text = bottomStr;
                ButtonStartStop.Text = @"Start";
                textBoxDirPath.Enabled = true;
                textBoxFormat.Enabled = true;
                ButtonDirPath.Enabled = true;
                deleteSourceSet.Enabled = true;
                outputDirectory.Enabled = true;
            }));
        }

        public void ReturnException(string info)
        {
            if (string.IsNullOrEmpty(info))
                return;

            Invoke(new MethodInvoker(delegate
            {
                exceptionMessage.Text = info + Environment.NewLine + exceptionMessage.Text;
            }));
        }



        void GetCueAndMp3(string path)
        {
            //string dirPath = Path.GetDirectoryName(cuePath); - полный путь к папке без файла

            string pathResult = path;
            if (!outputDirectory.Text.IsNullOrEmpty())
            {
                pathResult = Path.Combine(path, outputDirectory.Text);
                if (Directory.Exists(pathResult)) // пропускаем если уже есть папка Result, не перезаписываем треки
                    return;
            }

            if (pathResult.Length > 250)
                throw new Exception("Length of directory path too high. More than 250");

            string[] files = Directory.GetFiles(path, "*.cue");
            foreach (string sourceASOTCuePath in files)
            {
                string sourceASOTMp3Path = sourceASOTCuePath.Substring(0, sourceASOTCuePath.Length - 4) + ".mp3";
                if (!File.Exists(sourceASOTMp3Path))
                    continue;

                File.SetAttributes(sourceASOTCuePath, FileAttributes.Normal);
                File.SetAttributes(sourceASOTMp3Path, FileAttributes.Normal);

                string dirName = Path.GetFileName(path);
                List<CutterTrack> tracks = ReadCue(pathResult.Length, dirName, sourceASOTCuePath);

                bool newDirIsCreated = false;
                if (!Directory.Exists(pathResult))
                {
                    Directory.CreateDirectory(pathResult);
                    newDirIsCreated = true;
                }

                try
                {
                    using (Stream stream = new FileStream(sourceASOTMp3Path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        using (var reader = new Mp3FileReader(stream))
                        {
                            for (int i = 0; i < tracks.Count; i++)
                            {
                                string outputTrackResult = Path.Combine(pathResult, tracks[i].TrackFileName + ".mp3");
                                TrimMp3(reader, outputTrackResult, tracks[i].Start, i >= tracks.Count - 1 ? reader.TotalTime : tracks[i + 1].Start);
                            }
                        }
                    }

                    deleteSourceFiles?.Invoke(sourceASOTCuePath, sourceASOTMp3Path);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (InvalidOperationException ex)
                {
                    ReturnException(string.Format("'{0}' was skip. Exception:{1}.", sourceASOTMp3Path, ex.Message));
                    if (newDirIsCreated)
                        Directory.Delete(pathResult);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Exception \"{0}\" when read or write track {1}.", ex.GetType(), sourceASOTMp3Path), ex);
                }
            }
        }

        private const string TRACK = "TRACK";
        private const string PERFORMER = "PERFORMER";
        private const string TITLE = "TITLE";
        private const string INDEX = "INDEX";
        private const string DIR_NAME = "DIR_NAME";
        //.+?\s*TRACK\s*(?<TRACK>\d+)\s*.+?PERFORMER\s*\""(?<PERFORMER>.+?)\"".+?TITLE\s*\""(?<TITLE>.+?)\"".+?INDEX\s*01\s*(?<INDEX>\d+:\d+:\d+)
        static readonly Regex cueTrackNum = new Regex(TRACK + @"\s*(?<" + TRACK + @">\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex cueTrackPerf = new Regex(PERFORMER + @"\s*(\""|)(?<" + PERFORMER  + @">.+?)(\""|)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static readonly Regex cueTrackTitle = new Regex(TITLE + @"\s*(\""|)(?<" + TITLE + @">.+?)(\""|)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static readonly Regex cueTrackIndex = new Regex(INDEX + @"\s*01\s*(?<"+ INDEX + @">\d+:\d+:\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex regASOTNum = new Regex(@"^\d*", RegexOptions.Compiled);
        class CutterTrack
        {
            public CutterTrack(string sourceTemplate, string trackData, string parentDirName, int outDirPathLength)
            {
                try
                {
                    var collect = cueTrackPerf.Match(trackData);

                    TrackFileName = sourceTemplate;
                    TrackFileName = ReplacerTrackName(TrackFileName, TRACK, cueTrackNum.Match(trackData).Groups[TRACK].Value);
                    TrackFileName = ReplacerTrackName(TrackFileName, PERFORMER, cueTrackPerf.Match(trackData).Groups[PERFORMER].Value);
                    TrackFileName = ReplacerTrackName(TrackFileName, TITLE, cueTrackTitle.Match(trackData).Groups[TITLE].Value);
                    string indexTrack = cueTrackIndex.Match(trackData).Groups[INDEX].Value;
                    TrackFileName = ReplacerTrackName(TrackFileName, INDEX, indexTrack);
                    TrackFileName = ReplacerTrackName(TrackFileName, DIR_NAME, regASOTNum.Match(parentDirName).Value);
                    TrackFileName = GetCorrectEncoding(TrackFileName);

                    int excess = TrackFileName.Length + outDirPathLength - 254;
                    if (excess > 0)
                        TrackFileName = TrackFileName.Substring(0, TrackFileName.Length - excess);


                    TimeSpan spanMin = TimeSpan.FromMinutes(int.Parse(indexTrack.Split(':')[0]));
                    TimeSpan spanSec = TimeSpan.FromSeconds(int.Parse(indexTrack.Split(':')[1]));
                    TimeSpan spanMilisec = TimeSpan.FromMilliseconds(int.Parse(indexTrack.Split(':')[2] + "0"));
                    Start = spanMin + spanSec + spanMilisec;


                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("CUE File Is Incorrect on Directory={0}", parentDirName), ex);
                }
            }

            public string TrackFileName { get; }
            public TimeSpan Start { get; }

            static string GetCorrectEncoding(string sample)
            {
                byte[] encoded = Encoding.GetEncoding(1251).GetBytes(sample);
                string corrected = Encoding.UTF8.GetString(encoded);

                foreach (char c in invalid)
                {
                    corrected = corrected.Replace(c.ToString(), "");
                }

                return corrected.Replace("�", "O"); // Orjan Nilsen
            }
            static string ReplacerTrackName(string source, string groupName, string parcedValue)
            {
                return new Regex(string.Format(@"%\s*{0}\s*%", groupName), RegexOptions.IgnoreCase).Replace(source, parcedValue);
            }

            public override string ToString()
            {
                return string.Format("{1:G} | {0}", TrackFileName, Start);
            }
        }

        List<CutterTrack> ReadCue(int outDirPathLength, string parentDirName, string cuePath)
        {
            List<CutterTrack> cutterTracks = new List<CutterTrack>();

            using (FileStream inputStream = File.OpenRead(cuePath))
            {
                using (StreamReader inputReader = new StreamReader(inputStream, Encoding.GetEncoding("windows-1251")))
                {
                    string cueData = inputReader.ReadToEnd();

                    foreach (Match track in cueTrackSeparator.Matches(cueData))
                    {
                        CutterTrack newTrack = new CutterTrack(textBoxFormat.Text, track.Value, parentDirName, outDirPathLength);
                        cutterTracks.Add(newTrack);
                    }
                }
            }
            return cutterTracks;
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


            using (WaveFileWriter writer = new WaveFileWriter(outputPath, reader.WaveFormat))
            {
                reader.Position = startPos;
                byte[] buffer = new byte[1024];
                while (reader.Position < endPos)
                {
                    int bytesRequired = (int)(endPos - reader.Position);
                    if (bytesRequired > 0)
                    {
                        int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                        int bytesRead = reader.Read(buffer, 0, bytesToRead);
                        if (bytesRead > 0)
                        {
                            writer.WriteData(buffer, 0, bytesRead);
                        }
                    }
                }

            }
        }

        void TrimMp3(Mp3FileReader reader, string outputPath, TimeSpan? begin, TimeSpan? end)
        {
            if (begin.HasValue && end.HasValue && begin > end)
                throw new ArgumentOutOfRangeException(nameof(end), @"end should be greater than begin");

            if (File.Exists(outputPath))
                File.Delete(outputPath);

            using (var writer = File.Create(outputPath))
            {
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                    if (reader.CurrentTime >= begin || !begin.HasValue)
                    {
                        if (reader.CurrentTime <= end || !end.HasValue)
                            writer.Write(frame.RawData, 0, frame.RawData.Length);
                        else break;
                    }
            }

        }
    }
}