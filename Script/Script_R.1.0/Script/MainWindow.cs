using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Script.Control;
using XPackage;

namespace Script
{
    public partial class MainWindow : Form
    {
        private string configPath;
        public static string LocalPath => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        ScriptTemplate st = null;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                configPath = Path.Combine(LocalPath, "Script.Config.sxml");
                if (!File.Exists(configPath))
                {
                    using (StreamWriter writer = new StreamWriter(configPath, false, Functions.Enc))
                    {
                        //writer.Write(Properties.Resources.Script_Config);
                        writer.Write(ScriptTemplate.GetExampleOfConfig());
                        writer.Close();
                    }
                }
                SXML_Config.Text = configPath.LoadFileByPath();
            }
            catch (Exception ex)
            {
                StatusBarLable.Text = @"Exception! ";
                StatusBarDesc.Text = ex.Message;
            }
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
                    openFileDialog.Filter = @"Value(*.sxml) | *.sxml";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        configPath = openFileDialog.FileName;
                        SXML_Config.Text = configPath.LoadFileByPath();
                    }
                }
            }
            catch (Exception ex)
            {
                StatusBarLable.Text = @"Exception! ";
                StatusBarDesc.Text = ex.Message;
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(SXML_Config.Text);
                st = new ScriptTemplate(xdoc);
            }
            catch (Exception ex)
            {
                StatusBarLable.Text = @"Exception! ";
                StatusBarDesc.Text = ex.Message;
            }
        }

    }
}
