using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using NAudio.Lame;
using NAudio.Wave;

namespace DjSetCutter
{
    public partial class Form1 : Form
    {
        private AudioSeparator separator;
        private CancellationTokenSource token;
        private bool isWorked = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonDirPath_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = !textBoxDirPath.Text.IsNullOrEmpty() && Directory.Exists(textBoxDirPath.Text) ? textBoxDirPath.Text : Directory.GetCurrentDirectory();
                var result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBoxDirPath.Text = fbd.SelectedPath;
                }
            }
        }

        
        private async void ButtonStartStop_Click(object sender, EventArgs e)
        {
            StatusTextLable.Text = "";

            if (!isWorked)
            {
                if (textBoxDirPath.Text.IsNullOrEmpty() && !Directory.Exists(textBoxDirPath.Text))
                {
                    StatusTextLable.Text = @"Directory is empty or not Exist!";
                    return;
                }

                isWorked = true;
                ChangeFormStatus();

                token = new CancellationTokenSource();
                separator = new AudioSeparator(new Regex(textBox1.Text, RegexOptions.Compiled | (checkBox1.Checked ? RegexOptions.Multiline : RegexOptions.Singleline))
                    , new Regex(textBox2.Text, RegexOptions.Compiled | (checkBox2.Checked ? RegexOptions.Multiline : RegexOptions.Singleline))
                    , new Regex(textBox3.Text, RegexOptions.Compiled | (checkBox3.Checked ? RegexOptions.Multiline : RegexOptions.Singleline))
                    , textBoxDirPath.Text
                    , textBoxFormat.Text
                    , deleteSourceCUE.Checked
                    , deleteSourceMP3.Checked);
                separator.ProcessingException += ProcessingException;
                await separator.StartAsync(token);

                isWorked = false;
                ChangeFormStatus();
            }
            else
            {
                token.Cancel();
            }
        }

        private void ProcessingException(object sender, EventArgs e)
        {
            ReturnException(sender.ToString());
        }

        void ChangeFormStatus()
        {
            StatusTextLable.Text = isWorked ? "Working..." : "Finished";
            ButtonStartStop.Text = isWorked ? @"Stop" : @"Start";
            textBox1.Enabled = !isWorked;
            textBox2.Enabled = !isWorked;
            textBox3.Enabled = !isWorked;
            checkBox1.Enabled = !isWorked;
            checkBox2.Enabled = !isWorked;
            checkBox3.Enabled = !isWorked;
            textBoxDirPath.Enabled = !isWorked;
            textBoxFormat.Enabled = !isWorked;
            ButtonDirPath.Enabled = !isWorked;
            deleteSourceCUE.Enabled = !isWorked;
            deleteSourceMP3.Enabled = !isWorked;
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

    }
}