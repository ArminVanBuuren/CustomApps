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
using Script.Control;
using XPackage;

namespace Script
{
    public partial class MainWindow : Form
    {
        public static string LocalPath => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public MainWindow()
        {
            InitializeComponent();

            ScriptTemplate st = null;
            Console.WriteLine("Start...");
            string configPath = Path.Combine(LocalPath, "Script.Config.xml");
            try
            {
                if (!File.Exists(configPath))
                {
                    using (StreamWriter writer = new StreamWriter(configPath, false, Functions.Enc))
                    {
                        //writer.Write(Properties.Resources.Script_Config);
                        writer.Write(ScriptTemplate.GetExampleOfConfig());
                    }
                }
                else
                {
                    st = new ScriptTemplate(configPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:{0}", ex);
            }


            string configPath = Path.Combine(LocalPath, "Script.Config.xml");

        }
    }
}
