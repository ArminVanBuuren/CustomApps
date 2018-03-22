using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace ASOTCutter
{
    public partial class Form1 : Form
    {
        static Regex reg = new Regex(@".+?\s*TRACK\s*(?<TRACK>\d+)\s*.+?PERFORMER\s*\""(?<PERFORMER>.+?)\"".+?TITLE\s*\""(?<TITLE>.+?)\"".+?INDEX\s*01\s*(?<INDEX>\d+:\d+:\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
        static Regex regNum = new Regex(@"\d*", RegexOptions.Compiled);

        public Form1()
        {
            InitializeComponent();
            textBoxDirPath.Text = @"D:\MUSIC\ ASOT\700-799\725";
            textBoxFormat.Text = @"[ASOT %DIR_NAME%] %TRACK%. %PERFORMER% - %TITLE%";
        }

        private void ButtonDirPath_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBoxDirPath.Text = fbd.SelectedPath;
                    //string[] files = Directory.GetFiles(fbd.SelectedPath);
                    //System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
                }
            }
        }

        private void ButtonStartStop_Click(object sender, EventArgs e)
        {
            StatusTextLable.Text = "";
            if (textBoxDirPath.Text.IsNullOrEmpty() && !Directory.Exists(textBoxDirPath.Text))
            {
                StatusTextLable.Text = @"Directory Is Null Or Not Exist!";
                return;
            }

            FindAllSubDir(textBoxDirPath.Text);
        }

        void FindAllSubDir(string dirPath)
        {
            string[] DirPaths = Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories);
            foreach (var dir in DirPaths)
            {
                FindAllSubDir(dir);
            }
            GetCueAndMp3(dirPath);
        }

        void GetCueAndMp3(string path)
        {
            string[] files = Directory.GetFiles(path, "*.cue");
            foreach (string fileCuePath in files)
            {
                string fileName = fileCuePath.Substring(0, fileCuePath.Length - 4);
                if (File.Exists(fileName + ".mp3"))
                {
                    string dirName = Path.GetFileName(path);
                    List<Track>  tracks = ReadCue(dirName, fileCuePath);


                    string pathResult = Path.Combine(path, "Result");

                    using (var reader = new Mp3FileReader(fileName + ".mp3"))
                    {
                        if (!Directory.Exists(pathResult))
                            Directory.CreateDirectory(pathResult);

                        for (int i = 0; i < tracks.Count; i++)
                        {
                            string outputTrackResult = Path.Combine(pathResult, tracks[i].TrackFileName + ".mp3");
                            TrimMp3(reader, outputTrackResult, tracks[i].Start, i >= tracks.Count -1 ? TimeSpan.Zero : tracks[i + 1].Start);
                        }
                    }
                }
            }
        }

        class Track
        {
            public string TrackFileName { get; set; }
            public TimeSpan Start { get; set; }
        }
        List<Track> ReadCue(string parentDirName, string cuePath)
        {
            List<Track> SetTracks = new List<Track>();
            File.SetAttributes(cuePath, FileAttributes.Normal);
            using (FileStream inputStream = File.OpenRead(cuePath))
            {
                using (StreamReader inputReader = new StreamReader(inputStream))
                {
                    string cueData = inputReader.ReadToEnd();
                    MatchCollection collection = reg.Matches(cueData);
                    foreach (Match track in collection)
                    {
                        if (track.Groups.Count > 0)
                        {
                            Track newTrack = new Track();
                            newTrack.TrackFileName = textBoxFormat.Text;

                            foreach (string match in reg.GetGroupNames())
                            {
                                if(match == "0")
                                    continue;

                                newTrack.TrackFileName =  new Regex(string.Format(@"%\s*{0}\s*%", match), RegexOptions.IgnoreCase).Replace(newTrack.TrackFileName, track.Groups[match].Value);
                            }

                            newTrack.TrackFileName = new Regex(string.Format(@"%\s*{0}\s*%", "DIR_NAME"), RegexOptions.IgnoreCase).Replace(newTrack.TrackFileName, regNum.Match(parentDirName).Value);

                            TimeSpan spanMin = TimeSpan.FromMinutes(int.Parse(track.Groups["INDEX"].Value.Split(':')[0]));
                            TimeSpan spanSec = TimeSpan.FromSeconds(int.Parse(track.Groups["INDEX"].Value.Split(':')[1]));
                            //TimeSpan spanMilisec = TimeSpan.FromMilliseconds(int.Parse(track.Groups["INDEX"].Value.Split(':')[2]));
                            newTrack.Start = spanMin + spanSec;

                            SetTracks.Add(newTrack);
                        }
                    }
                }
            }
            return SetTracks;
        }

        void TrimMp3(Mp3FileReader reader, string outputPath, TimeSpan? begin, TimeSpan? end)
        {
           
                if (end == TimeSpan.Zero)
                    end = reader.TotalTime;

                if (begin.HasValue && end.HasValue && begin > end)
                    throw new ArgumentOutOfRangeException(@"end", @"end should be greater than begin");


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
