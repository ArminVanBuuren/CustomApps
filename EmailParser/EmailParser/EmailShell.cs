using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmailParser
{
    public class EmailShell
    {
        int _maxLines;
        int _lineInProgeress;

        public EmailShell(string filePath)
        {
            FilePath = filePath;
        }
        public static Regex RegForAllMails { get; } = new Regex(@"[A-Z0-9._%-]+@[A-Z0-9.-]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static Regex RegForValidEmails { get; } = new Regex(@"[A-Z0-9._%-]{2,}@[A-Z0-9.-]{2,}\.[A-Z]{2,4}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public int Progress => (_maxLines <= 0) ? 0 : (_lineInProgeress / _maxLines) * 100;
        public int StatValidMails { get; private set; } = 0;
        public int StatInvalidMails { get; private set; } = 0;
        string FilePath { get; }
        List<string> CollectionValidMails { get; } = new List<string>();
        List<string> CollectionInvalidMails { get; } = new List<string>();


        public void StartProcess(MainWindow process)
        {
            try
            {
                string[] lines = File.ReadAllLines(FilePath);
                _maxLines = lines.Length;
                _lineInProgeress = 0;
                StatValidMails = 0;
                StatInvalidMails = 0;

                foreach (string line in lines)
                {
                    if (!process.StatusProcess)
                        break;

                    _lineInProgeress++;
                    MatchCollection collection = RegForAllMails.Matches(line);
                    foreach (Match mail in collection)
                    {
                        if (!process.StatusProcess)
                            break;

                        if (RegForValidEmails.IsMatch(mail.Value))
                        {
                            if (CollectionValidMails.Contains(mail.Value, StringComparer.CurrentCultureIgnoreCase))
                                continue;
                            StatValidMails++;
                            CollectionValidMails.Add(mail.Value.ToLower());
                        }
                        else
                        {
                            if (CollectionInvalidMails.Contains(mail.Value, StringComparer.CurrentCultureIgnoreCase))
                                continue;
                            StatInvalidMails++;
                            CollectionInvalidMails.Add(mail.Value.ToLower());
                        }
                    }
                }


                using (StreamWriter file = new StreamWriter(GetFreeDestFile(FilePath, Properties.Resources.COMPLETED_TEXT), false, Encoding.UTF8))
                {
                    foreach (string str in CollectionValidMails)
                    {
                        file.WriteLine(str);
                    }
                }
                CollectionValidMails.Clear();

                using (StreamWriter file = new StreamWriter(GetFreeDestFile(FilePath, Properties.Resources.FAILED_TEXT), false, Encoding.UTF8))
                {
                    foreach (string str in CollectionInvalidMails)
                    {
                        file.WriteLine(str);
                    }
                }
                CollectionInvalidMails.Clear();
            }
            catch (Exception ex)
            {
                process.AddStatusInfo(ex.Message, true);
            }
        }


        string GetFreeDestFile(string filePath, string apos)
        {
            string _pathDirectory = Path.GetDirectoryName(filePath);
            string _fileName = Path.GetFileNameWithoutExtension(filePath);
            return string.Format(@"{0}\{1}_{2}", _pathDirectory, _fileName, apos);

            //int id = 0;
            //while (true)
            //{
            //    id++;
            //    string possilePath = string.Format(@"{0}\{1}_{2}.txt", _pathDirectory, _fileName, id);
            //    if (!File.Exists(possilePath))
            //        return possilePath;

            //}
        }
    }
}
