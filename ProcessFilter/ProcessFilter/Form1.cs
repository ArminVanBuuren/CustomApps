using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using Utils;

namespace ProcessFilter
{
    public partial class ProcessFilterForm : Form
    {
        public BusinessProcesses Processes { get; private set; }
        public NetworkElements NetElements { get; private set; }

        public ProcessFilterForm()
        {
            InitializeComponent();
            ProcessesTextBox.Text = @"C:\Users\vhovanskij\Desktop\2018.09.06\ROBP\Processes";
            OperationTextBox.Text = @"C:\Users\vhovanskij\Desktop\2018.09.06\ROBP\Operations";
            ProcessesTextBox_TextChanged(null, null);
            OperationTextBox_TextChanged(null, null);
        }

        private void buttonFilterClick(object sender, EventArgs e)
        {
            BusinessProcesses bpCollection;
            NetworkElementCollection opCollection;

            if (Processes == null || NetElements == null)
            {
                dataGridProcessesResults.DataSource = null;
                dataGridOperationsResult.DataSource = null;
                return;
            }

            if (!ProcessesComboBox.Text.IsNullOrEmpty())
            {
                bpCollection = new BusinessProcesses();
                foreach (var bp in Processes.Where(p => p.Name.IndexOf(ProcessesComboBox.Text, StringComparison.CurrentCultureIgnoreCase) != -1))
                {
                    bpCollection.Add(bp);
                }
            }
            else
            {
                bpCollection = Processes.Clone();
            }

            if (!NetSettComboBox.Text.IsNullOrEmpty())
            {
                opCollection = new NetworkElementCollection();
                foreach (NetworkElement netElem in NetElements.Elements.Where(p => p.Name.Equals(NetSettComboBox.Text, StringComparison.CurrentCultureIgnoreCase)))
                {
                    opCollection.Add(netElem.Clone());
                }
            }
            else
            {
                opCollection = NetElements.Elements.Clone();
            }

            if (!OperationComboBox.Text.IsNullOrEmpty() && opCollection != null)
            {
                NetworkElementCollection opCollection2 = new NetworkElementCollection();
                foreach (var nec in opCollection)
                {
                    List<NetworkElementOpartion> ops = new List<NetworkElementOpartion>();
                    foreach (NetworkElementOpartion neo in nec.Operations)
                    {
                        if (neo.Name.IndexOf(OperationComboBox.Text, StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            ops.Add(neo);
                        }
                    }
                    if (ops.Count > 0)
                        opCollection2.Add(nec);
                }
                opCollection = opCollection2;
            }

            int endOfBpCollection = bpCollection.Count;
            for (int i = 0; i < endOfBpCollection; i++)
            {
                XmlDocument document = LoadXml(bpCollection[i].Path);
                if (document != null)
                {
                    bpCollection[i].AddBodyOperations(document);
                    bool hasMatch = bpCollection[i].Operations.Any(x => opCollection.AllOperationsName.Any(y => x.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1));

                    if (!hasMatch)
                    {
                        bpCollection.Remove(bpCollection[i]);
                        i--;
                        endOfBpCollection--;
                    }
                }
                else
                {
                    bpCollection.Remove(bpCollection[i]);
                    i--;
                    endOfBpCollection--;
                }
            }

            foreach (var netElem in opCollection)
            {
                int endOfOpCollection = netElem.Operations.Count;
                for (int i = 0; i < endOfOpCollection; i++)
                {
                    bool hasMatch = bpCollection.Any(x => x.Operations.Any(y => netElem.Operations[i].Name.Equals(y, StringComparison.CurrentCultureIgnoreCase)));

                    if (!hasMatch)
                    {
                        netElem.Operations.Remove(netElem.Operations[i]);
                        i--;
                        endOfOpCollection--;
                    }
                }
            }

            dataGridProcessesResults.DataSource = bpCollection;
            foreach (DataGridViewRow row in dataGridProcessesResults.Rows)
            {
                row.DefaultCellStyle.Padding = new Padding(0, 0, 15, 0);
            }
            dataGridOperationsResult.DataSource = opCollection.AllOperations;
            foreach (DataGridViewRow row in dataGridOperationsResult.Rows)
            {
                row.DefaultCellStyle.Padding = new Padding(0, 0, 15, 0);
            }
        }

        XmlDocument LoadXml(string path)
        {
            if (File.Exists(path))
            {
                string context;
                using (StreamReader sr = new StreamReader(path))
                {
                    context = sr.ReadToEnd().ToLower();
                }

                if (!string.IsNullOrEmpty(context) && context.TrimStart().StartsWith("<"))
                {
                    try
                    {
                        XmlDocument xmlSetting = new XmlDocument();
                        xmlSetting.LoadXml(context);
                        return xmlSetting;
                    }
                    catch (Exception ex)
                    {
                        //null
                    }
                }
            }
            return null;
        }

        private void ProcessesButtonOpen_Click(object sender, EventArgs e)
        {
            ProcessesTextBox.Text = OpenFoilder();
        }

        private void ProcessesTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckProcessesPath(ProcessesTextBox.Text);
        }

        void CheckProcessesPath(string prcsPath)
        {
            if(Directory.Exists(prcsPath.Trim(' ')))
            {
                Processes = new BusinessProcesses(prcsPath);
                ProcessesComboBox.DataSource = Processes.Select(p => p.Name).ToList();
            }
            else
            {
                ProcessesComboBox.DataSource = null;
            }
            ProcessesComboBox.Text = null;
            ProcessesComboBox.DisplayMember = null;
            ProcessesComboBox.ValueMember = null;
        }

        private void OperationButtonOpen_Click(object sender, EventArgs e)
        {
            OperationTextBox.Text = OpenFoilder();
            CheckOperationsPath(OperationTextBox.Text);
        }
        private void OperationTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckOperationsPath(OperationTextBox.Text);
        }

        void CheckOperationsPath(string opPath)
        {
            if (Directory.Exists(opPath.Trim(' ')))
            {
                NetElements = new NetworkElements(opPath);
                NetSettComboBox.DataSource = NetElements.Elements.AllNetworkElements;
                OperationComboBox.DataSource = NetElements.Elements.AllOperationsName;
            }
            else
            {
                NetSettComboBox.DataSource = null;
                OperationComboBox.DataSource = null;
            }
            NetSettComboBox.Text = null;
            NetSettComboBox.DisplayMember = null;
            NetSettComboBox.ValueMember = null;
            OperationComboBox.Text = null;
            OperationComboBox.DisplayMember = null;
            OperationComboBox.ValueMember = null;
        }

        string OpenFoilder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    return fbd.SelectedPath;
                    //string[] files = Directory.GetFiles(fbd.SelectedPath);
                    //System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
                }
            }
            return null;
        }
    }
}
