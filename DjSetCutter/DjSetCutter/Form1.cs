using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace DjSetCutter
{
    public partial class Form1 : Form
    {
        private AudioSeparator separator;
        private bool isWorked = false;

        public Form1()
        {
            InitializeComponent();
            var toolTip = new ToolTip();
            toolTip.SetToolTip(checkBox1, "Multi Line");
            toolTip.SetToolTip(checkBox2, "Multi Line");
            toolTip.SetToolTip(checkBox3, "Multi Line");
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
            if (!isWorked)
            {
                if (textBoxDirPath.Text.IsNullOrEmpty() || !Directory.Exists(textBoxDirPath.Text))
                {
                    StatusTextLable.Text = @"Directory is empty or not Exist!";
                    return;
                }

                StatusTextLable.Text = "";
                exceptionMessage.Text = string.Empty;
                isWorked = true;
                ChangeFormStatus();

                
                separator = new AudioSeparator(new Regex(textBox1.Text, RegexOptions.Compiled | (checkBox1.Checked ? RegexOptions.Multiline : RegexOptions.Singleline))
                    , new Regex(textBox2.Text, RegexOptions.Compiled | (checkBox2.Checked ? RegexOptions.Multiline : RegexOptions.Singleline))
                    , new Regex(textBox3.Text, RegexOptions.Compiled | (checkBox3.Checked ? RegexOptions.Multiline : RegexOptions.Singleline))
                    , textBoxDirPath.Text
                    , textBoxFormat.Text
                    , deleteSourceCUE.Checked
                    , deleteSourceMP3.Checked);
                separator.ProcessingException += ProcessingException;
                await separator.StartAsync();

                isWorked = false;
                ChangeFormStatus();
            }
            else
            {
                separator?.Stop();
                StatusTextLable.Text = @"Stopping...";
            }
        }

        private void ProcessingException(object sender, EventArgs e)
        {
            ReturnException(sender.ToString());
        }

        void ChangeFormStatus()
        {
            StatusTextLable.Text = isWorked ? @"Working..." : @"Finished";
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
                if (exceptionMessage.Text.IsNullOrEmpty())
                    exceptionMessage.Text = info;
                else
                    exceptionMessage.Text = exceptionMessage.Text + Environment.NewLine + new string('=', 50) + Environment.NewLine + info;
            }));
        }
    }
}