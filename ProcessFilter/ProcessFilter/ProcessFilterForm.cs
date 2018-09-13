using System;
using System.Collections;
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
using ProcessFilter.SPA;
using Utils;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using FormUtils.Notepad;
using FormUtils.DataGridViewHelper;
using Utils.XmlHelper;

namespace ProcessFilter
{
    [Serializable]
    public partial class ProcessFilterForm : Form, ISerializable
    {
        private string lastPath = string.Empty;
        public static string SerializationDataPath => $"{Customs.AccountFilePath}.bin";
        private XmlNotepad notepad;
        public CollectionBusinessProcess Processes { get; private set; }
        public CollectionNetworkElements NetElements { get; private set; }
        public CollectionScenarios Scenarios { get; private set; }
        public CollectionCommands Commands { get; private set; }

        public ProcessFilterForm()
        {
            MainInit();
        }

        private void ProcessFilterForm_Closing(object sender, CancelEventArgs e)
        {
            using (FileStream stream = new FileStream(SerializationDataPath, FileMode.Create, FileAccess.ReadWrite))
            {
                new BinaryFormatter().Serialize(stream, this);
            }
        }

        ProcessFilterForm(SerializationInfo propertyBag, StreamingContext context)
        {
            MainInit();
            ProcessesTextBox.Text = propertyBag.GetString("ADWFFW");
            OperationTextBox.Text = propertyBag.GetString("AAEERF");
            ScenariosTextBox.Text = propertyBag.GetString("DFWDRT");
            CommandsTextBox.Text = propertyBag.GetString("WWWERT");
        }

        void ISerializable.GetObjectData(SerializationInfo propertyBag, StreamingContext context)
        {
            propertyBag.AddValue("ADWFFW", ProcessesTextBox.Text);
            propertyBag.AddValue("AAEERF", OperationTextBox.Text);
            propertyBag.AddValue("DFWDRT", ScenariosTextBox.Text);
            propertyBag.AddValue("WWWERT", CommandsTextBox.Text);
        }

        void MainInit()
        {
            InitializeComponent();
            dataGridScenariosResult.CellFormatting += DataGridScenariosResult_CellFormatting;
            this.Closing += ProcessFilterForm_Closing;
        }

        

        private void buttonFilterClick(object sender, EventArgs e)
        {
            CollectionBusinessProcess bpCollection;
            NetworkElementCollection netElemCollection;
            CollectionScenarios scoCollection = new CollectionScenarios();
            CollectionCommands cmmCollection = new CollectionCommands();
            List<Scenario> subScenarios = new List<Scenario>();

            if (Processes == null || NetElements == null)
            {
                dataGridProcessesResults.DataSource = null;
                dataGridOperationsResult.DataSource = null;
                return;
            }
            


            if (!ProcessesComboBox.Text.IsNullOrEmpty())
            {
                bpCollection = new CollectionBusinessProcess();
                IEnumerable<BusinessProcess> getCollection;
                if (ProcessesComboBox.Text[0] == '%' || ProcessesComboBox.Text[ProcessesComboBox.Text.Length - 1] == '%')
                    getCollection = Processes.Where(p => p.Name.IndexOf(ProcessesComboBox.Text.Replace("%", ""), StringComparison.CurrentCultureIgnoreCase) != -1);
                else
                    getCollection = Processes.Where(p => p.Name.Equals(ProcessesComboBox.Text, StringComparison.CurrentCultureIgnoreCase));

                foreach (var bp in getCollection)
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
                netElemCollection = new NetworkElementCollection();
                foreach (NetworkElement netElem in NetElements.Elements.Where(p => p.Name.Equals(NetSettComboBox.Text, StringComparison.CurrentCultureIgnoreCase)))
                {
                    netElemCollection.Add(netElem.Clone());
                }
            }
            else
            {
                netElemCollection = NetElements.Elements.Clone();
            }

            if (!OperationComboBox.Text.IsNullOrEmpty() && netElemCollection != null)
            {
                NetworkElementCollection netElemCollection2 = new NetworkElementCollection();
                foreach (NetworkElement nec in netElemCollection)
                {
                    List<NetworkElementOpartion> ops = new List<NetworkElementOpartion>();
                    foreach (NetworkElementOpartion neo in nec.Operations)
                    {
                        if (OperationComboBox.Text[0] == '%' || OperationComboBox.Text[OperationComboBox.Text.Length - 1] == '%')
                        {
                            if (neo.Name.IndexOf(OperationComboBox.Text.Replace("%", ""), StringComparison.CurrentCultureIgnoreCase) != -1)
                            {
                                ops.Add(neo);
                            }
                        }
                        else
                        {
                            if (neo.Name.Equals(OperationComboBox.Text, StringComparison.CurrentCultureIgnoreCase))
                            {
                                ops.Add(neo);
                            }
                        }
                    }

                    if (ops.Count > 0)
                    {
                        NetworkElement fileteredElementAndOps = new NetworkElement(nec.Path, nec.ID, ops);
                        netElemCollection2.Add(fileteredElementAndOps);
                    }
                }
                netElemCollection = netElemCollection2;
            }

            int endOfBpCollection = bpCollection.Count;
            for (int i = 0; i < endOfBpCollection; i++)
            {
                XmlDocument document = XmlHelper.LoadXml(bpCollection[i].Path, true);
                if (document != null)
                {
                    bpCollection[i].AddBodyOperations(document);
                    bool hasMatch = bpCollection[i].Operations.Any(x => netElemCollection.AllOperationsName.Any(y => x.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1));

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

            Action<NetworkElementOpartion> getScenario = null;
            if (Scenarios != null)
            {
                getScenario = operation =>
                {
                    List<Scenario> scenarios = Scenarios.Where(p => p.Name.Equals(operation.Name, StringComparison.CurrentCultureIgnoreCase)).ToList();

                    foreach (Scenario scenario in scenarios)
                    {
                        if (scenario.AddBodyCommands())
                        {
                            scoCollection.Add(scenario);
                            subScenarios.AddRange(scenario.SubScenarios);
                        }
                    }
                };
            }

            foreach (var netElem in netElemCollection)
            {
                int endOfOpCollection = netElem.Operations.Count;
                for (int i = 0; i < endOfOpCollection; i++)
                {
                    bool hasMatch = bpCollection.Any(x => x.Operations.Any(y => netElem.Operations[i].Name.Equals(y, StringComparison.CurrentCultureIgnoreCase)));

                    if (hasMatch)
                    {
                        getScenario?.Invoke(netElem.Operations[i]);
                        continue;
                    }

                    netElem.Operations.Remove(netElem.Operations[i]);
                    i--;
                    endOfOpCollection--;
                }
            }

            if (Commands != null && scoCollection.Count > 0)
            {
                foreach (Command command in Commands)
                {
                    bool hasValue = scoCollection.Any(x => x.Commands.Any(y => y.Equals(command.Name, StringComparison.CurrentCultureIgnoreCase)));
                    if (hasValue)
                    {
                        cmmCollection.Add(command);
                    }
                }
            }

            subScenarios = subScenarios.Distinct(new ItemEqualityComparer()).ToList();
            scoCollection.AddRange(subScenarios);

            dataGridProcessesResults.AssignListToDataGrid(bpCollection, new Padding(0, 0, 15, 0));
            dataGridOperationsResult.AssignListToDataGrid(netElemCollection.AllOperations, new Padding(0, 0, 15, 0));
            dataGridScenariosResult.AssignListToDataGrid(scoCollection, new Padding(0, 0, 15, 0));
            dataGridCommandsResult.AssignListToDataGrid(cmmCollection, new Padding(0, 0, 15, 0));
        }

        private void DataGridScenariosResult_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewRow row = dataGridScenariosResult.Rows[e.RowIndex];// get you required index
            // check the cell value under your specific column and then you can toggle your colors
            if (row.Cells[0].Value.ToString() == "-1")
            {
                row.DefaultCellStyle.BackColor = Color.Yellow;
            }

        }

        private void dataGridProcessesResults_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            OpenEditor(dataGridProcessesResults);
        }

        private void dataGridOperationsResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OpenEditor(dataGridOperationsResult);
        }
        private void dataGridScenariosResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OpenEditor(dataGridScenariosResult);
        }

        private void dataGridCommandsResult_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            OpenEditor(dataGridCommandsResult);
        }

        void OpenEditor(DataGridView grid)
        {
            string path = grid.SelectedRows[0].Cells[grid.ColumnCount - 1].Value.ToString();
            if (notepad == null || notepad.WindowIsClosed)
            {
                notepad = new XmlNotepad(path);
                notepad.Show();
            }
            else
            {
                notepad.AddNewDocument(path);
                notepad.Focus();
            }
        }

        private void ProcessesButtonOpen_Click(object sender, EventArgs e)
        {
            ProcessesTextBox.Text = OpenFolder() ?? ProcessesTextBox.Text;
            CheckProcessesPath(ProcessesTextBox.Text);
        }

        private void ProcessesTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckProcessesPath(ProcessesTextBox.Text);
        }

        void CheckProcessesPath(string prcsPath)
        {
            if(Directory.Exists(prcsPath.Trim(' ')))
            {
                UpdateLastPath(prcsPath.Trim(' '));
                Processes = new CollectionBusinessProcess(prcsPath.Trim(' '));
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
            OperationTextBox.Text = OpenFolder() ?? OperationTextBox.Text;
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
                UpdateLastPath(opPath.Trim(' '));
                NetElements = new CollectionNetworkElements(opPath.Trim(' '));
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

        private void ScenariosButtonOpen_Click(object sender, EventArgs e)
        {
            ScenariosTextBox.Text = OpenFolder() ?? ScenariosTextBox.Text;
            CheckScenariosPath(ScenariosTextBox.Text);
        }
        private void ScenariosTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckScenariosPath(ScenariosTextBox.Text);
        }
        void CheckScenariosPath(string scoPath)
        {
            if (Directory.Exists(scoPath.Trim(' ')))
            {
                UpdateLastPath(scoPath.Trim(' '));
                Scenarios = new CollectionScenarios(scoPath.Trim(' '));
                SenariosStat.Text = $"Scenarios:{Scenarios.Count}";
            }
            else
            {
                SenariosStat.Text = "Scenarios:";
            }
        }

        private void CommnadsButtonOpen_Click(object sender, EventArgs e)
        {
            CommandsTextBox.Text = OpenFolder() ?? CommandsTextBox.Text;
            CheckCommandsPath(CommandsTextBox.Text);
        }
        private void CommandsTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckCommandsPath(CommandsTextBox.Text);
        }
        void CheckCommandsPath(string cmmPath)
        {
            if (Directory.Exists(cmmPath.Trim(' ')))
            {
                UpdateLastPath(cmmPath.Trim(' '));
                Commands = new CollectionCommands(cmmPath.Trim(' '));
                CommandsStat.Text = $"Commands:{Commands.Count}";
            }
            else
            {
                CommandsStat.Text = "Commands:";
            }
        }

        string OpenFolder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if(!lastPath.IsNullOrEmpty())
                    fbd.SelectedPath = lastPath;
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    UpdateLastPath(fbd.SelectedPath);
                    return fbd.SelectedPath;
                }
            }
            return null;
        }

        void UpdateLastPath(string path)
        {
            if (!path.IsNullOrEmpty())
                lastPath = path;
        }
    }
}
