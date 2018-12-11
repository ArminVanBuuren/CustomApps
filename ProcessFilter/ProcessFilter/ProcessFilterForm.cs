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
using System.Runtime.InteropServices;
using Utils.XmlHelper;
using Timer = System.Timers.Timer;
using System.Threading;
using Utils.WinForm.DataGridViewHelper;
using Utils.WinForm.Notepad;

namespace ProcessFilter
{
    [Serializable]
    public partial class ProcessFilterForm : Form, ISerializable
    {
        private string lastPath = string.Empty;
        public static string SerializationDataPath => $"{Customs.ApplicationFilePath}.bin";
        private XmlNotepad notepad;
        public CollectionBusinessProcess Processes { get; private set; }
        public CollectionNetworkElements NetElements { get; private set; }
        public CollectionScenarios Scenarios { get; private set; }
        public CollectionCommands Commands { get; private set; }
        private object sync = new object();
        private Timer _timerGC;

        public ProcessFilterForm()
        {
            MainInit();
        }

        private void ProcessFilterForm_Closing(object sender, CancelEventArgs e)
        {
            SaveFile();
        }
        private void _timerCollect(object sender, System.Timers.ElapsedEventArgs e)
        {
            SaveFile();
        }

        void SaveFile()
        {
            lock (sync)
            {
                using (FileStream stream = new FileStream(SerializationDataPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    new BinaryFormatter().Serialize(stream, this);
                }
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

            //progressBar.Visible = true;
            //progressBar.Value = 100;

            dataGridProcessesResults.KeyDown += DataGridProcessesResults_KeyDown;
            dataGridOperationsResult.KeyDown += DataGridOperationsResult_KeyDown;
            dataGridScenariosResult.KeyDown += DataGridScenariosResult_KeyDown;
            dataGridCommandsResult.KeyDown += DataGridCommandsResult_KeyDown;

            tabControl1.KeyDown += ProcessFilterForm_KeyDown;
            ProcessesTextBox.KeyDown += ProcessFilterForm_KeyDown;
            OperationTextBox.KeyDown += ProcessFilterForm_KeyDown;
            ScenariosTextBox.KeyDown += ProcessFilterForm_KeyDown;
            CommandsTextBox.KeyDown += ProcessFilterForm_KeyDown;
            ProcessesComboBox.KeyDown += ProcessFilterForm_KeyDown;
            NetSettComboBox.KeyDown += ProcessFilterForm_KeyDown;
            OperationComboBox.KeyDown += ProcessFilterForm_KeyDown;
            dataGridProcessesResults.KeyDown += ProcessFilterForm_KeyDown;
            dataGridOperationsResult.KeyDown += ProcessFilterForm_KeyDown;
            dataGridScenariosResult.KeyDown += ProcessFilterForm_KeyDown;
            dataGridCommandsResult.KeyDown += ProcessFilterForm_KeyDown;
            ProcessesButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            OperationButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            ScenariosButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            CommnadsButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            buttonFilter.KeyDown += ProcessFilterForm_KeyDown;
            this.KeyDown += ProcessFilterForm_KeyDown;



            dataGridScenariosResult.CellFormatting += DataGridScenariosResult_CellFormatting;
            this.Closing += ProcessFilterForm_Closing;

            _timerGC = new Timer
                       {
                           Interval = 5 * 1000
                       };
            _timerGC.Elapsed += _timerCollect;
            _timerGC.Start();
        }


        private void ProcessFilterForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                buttonFilterClick(this, EventArgs.Empty);
            }
        }



        bool _filterCompleted = false;
        int _filterProgress = 1;
        int _totalProgress = 10;


        private void buttonFilterClick(object sender, EventArgs e)
        {
            if (Processes == null || NetElements == null)
            {
                dataGridProcessesResults.DataSource = null;
                dataGridOperationsResult.DataSource = null;
                return;
            }

            try
            {
                progressBar.Visible = true;
                _filterCompleted = false;
                _filterProgress = 1;

                dataGridProcessesResults.DataSource = null;
                dataGridOperationsResult.DataSource = null;
                dataGridScenariosResult.DataSource = null;
                dataGridCommandsResult.DataSource = null;
                dataGridProcessesResults.Refresh();
                dataGridOperationsResult.Refresh();
                dataGridScenariosResult.Refresh();
                dataGridCommandsResult.Refresh();

                Progress();
                Action datafilter = new Action(DataFilter);
                IAsyncResult asyncResult = datafilter.BeginInvoke(FilterCompleted, datafilter);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void FilterCompleted(IAsyncResult asyncResult)
        {
            Action dataDilter = (Action) asyncResult.AsyncState;
            dataDilter.EndInvoke(asyncResult);

            _filterCompleted = true;
            progressBar.Invoke(new MethodInvoker(delegate
                                                 {
                                                     progressBar.Visible = false;
                                                 }));
        }


        Task Progress()
        {
            return Task.Run(() =>
                            {
                                while (!_filterCompleted)
                                {
                                    if (_filterProgress == 0 || _totalProgress == 0)
                                        continue;

                                    double calc = (double) _filterProgress / _totalProgress;
                                    int progr = ((int) (calc * 100)) >= 100 ? 100 : ((int)(calc * 100));
                                    progressBar.Invoke(new MethodInvoker(delegate
                                                                         {
                                                                             progressBar.Value = progr;
                                                                             progressBar.SetProgressNoAnimation(progr);
                                                                         }));
                                }
                            });
        }

        void DataFilter()
        {
            try
            {
                CollectionBusinessProcess bpCollection;
                NetworkElementCollection netElemCollection;
                CollectionScenarios scoCollection = new CollectionScenarios();
                CollectionCommands cmmCollection = new CollectionCommands();
                List<Scenario> subScenarios = new List<Scenario>();

                string processFilter = null, netElemFilter = null, operFilter = null;
                progressBar.Invoke(new MethodInvoker(delegate
                                                     {
                                                         processFilter = ProcessesComboBox.Text;
                                                         netElemFilter = NetSettComboBox.Text;
                                                         operFilter = OperationComboBox.Text;
                                                     }));
                _filterProgress = 1;

                if (!processFilter.IsNullOrEmpty())
                {
                    bpCollection = new CollectionBusinessProcess();
                    IEnumerable<BusinessProcess> getCollection;
                    if (processFilter[0] == '%' || processFilter[processFilter.Length - 1] == '%')
                        getCollection = Processes.Where(p => p.Name.IndexOf(processFilter.Replace("%", ""), StringComparison.CurrentCultureIgnoreCase) != -1);
                    else
                        getCollection = Processes.Where(p => p.Name.Equals(processFilter, StringComparison.CurrentCultureIgnoreCase));

                    bpCollection.AddRange(getCollection);
                }
                else
                {
                    bpCollection = Processes.Clone();
                }


                _filterProgress = 2;

                if (!netElemFilter.IsNullOrEmpty())
                {
                    netElemCollection = new NetworkElementCollection();
                    IEnumerable<NetworkElement> getCollection;
                    if (netElemFilter[0] == '%' || netElemFilter[netElemFilter.Length - 1] == '%')
                        getCollection = NetElements.Elements.Where(p => p.Name.IndexOf(netElemFilter.Replace("%", ""), StringComparison.CurrentCultureIgnoreCase) != -1);
                    else
                        getCollection = NetElements.Elements.Where(p => p.Name.Equals(netElemFilter, StringComparison.CurrentCultureIgnoreCase));

                    netElemCollection.AddRange(getCollection.Select(netElem => netElem.Clone()));
                }
                else
                {
                    netElemCollection = NetElements.Elements.Clone();
                }

                if (!operFilter.IsNullOrEmpty() && netElemCollection != null)
                {
                    NetworkElementCollection netElemCollection2 = new NetworkElementCollection();
                    foreach (NetworkElement nec in netElemCollection)
                    {
                        List<NetworkElementOpartion> ops = new List<NetworkElementOpartion>();
                        foreach (NetworkElementOpartion neo in nec.Operations)
                        {
                            if (operFilter[0] == '%' || operFilter[operFilter.Length - 1] == '%')
                            {
                                if (neo.Name.IndexOf(operFilter.Replace("%", ""), StringComparison.CurrentCultureIgnoreCase) != -1)
                                {
                                    ops.Add(neo);
                                }
                            }
                            else
                            {
                                if (neo.Name.Equals(operFilter, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    ops.Add(neo);
                                }
                            }
                        }

                        if (ops.Count > 0)
                        {
                            NetworkElement fileteredElementAndOps = new NetworkElement(nec.FilePath, nec.ID, ops);
                            netElemCollection2.Add(fileteredElementAndOps);
                        }
                    }
                    netElemCollection = netElemCollection2;
                }


                _filterProgress = 3;

                int endOfBpCollection = bpCollection.Count;
                for (int i = 0; i < endOfBpCollection; i++)
                {
                    XmlDocument document = XmlHelper.LoadXml(bpCollection[i].FilePath, true);
                    if (document != null)
                    {
                        bpCollection[i].AddBodyOperations(document);
                        //bool hasMatch = bpCollection[i].Operations.Any(x => netElemCollection.AllOperationsName.Any(y => x.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1));
                        bool hasMatch = bpCollection[i].Operations.Any(x => netElemCollection.AllOperationsName.Any(y => x.Equals(y, StringComparison.CurrentCultureIgnoreCase)));

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


                _filterProgress = 4;

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


                _filterProgress = 5;

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

                _filterProgress = 6;

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


                _filterProgress = 7;

                subScenarios = subScenarios.Distinct(new ItemEqualityComparer()).ToList();
                scoCollection.AddRange(subScenarios);


                progressBar.Invoke(new MethodInvoker(delegate
                                                     {
                                                         _filterProgress = 8;
                                                         dataGridProcessesResults.AssignListToDataGrid(bpCollection, new Padding(0, 0, 15, 0));
                                                         dataGridOperationsResult.AssignListToDataGrid(netElemCollection.AllOperations, new Padding(0, 0, 15, 0));
                                                         _filterProgress = 9;
                                                         dataGridScenariosResult.AssignListToDataGrid(scoCollection, new Padding(0, 0, 15, 0));
                                                         dataGridCommandsResult.AssignListToDataGrid(cmmCollection, new Padding(0, 0, 15, 0));
                                                         _filterProgress = 10;
                                                     }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void DataGridScenariosResult_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewRow row = dataGridScenariosResult.Rows[e.RowIndex];
            if (row.Cells[0].Value.ToString() == "-1")
            {
                row.DefaultCellStyle.BackColor = Color.Yellow;
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.White;
            }
        }

        private void dataGridProcessesResults_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridProcessesResults);
        }
        private void DataGridProcessesResults_KeyDown(object sender, KeyEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridProcessesResults, e);
        }

        private void dataGridOperationsResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridOperationsResult);
        }
        private void DataGridOperationsResult_KeyDown(object sender, KeyEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridOperationsResult, e);
        }

        private void dataGridScenariosResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridScenariosResult);
        }
        private void DataGridScenariosResult_KeyDown(object sender, KeyEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridScenariosResult, e);
        }

        private void dataGridCommandsResult_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridCommandsResult);
        }

        private void DataGridCommandsResult_KeyDown(object sender, KeyEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridCommandsResult, e);
        }

        void CallAndCheckDataGridKey(DataGridView grid, KeyEventArgs e = null)
        {
            if (e != null)
            {
                bool altIsDown = (Control.ModifierKeys & Keys.Alt) != 0;
                bool f4IsDown = KeyIsDown(Keys.F4);
                if (altIsDown && f4IsDown)
                {
                    Close();
                }
                else if (e.KeyCode == Keys.Enter || (!altIsDown && f4IsDown))
                {
                    if (GetFilePathCurrentRow(grid, out var filePath1))
                        OpenEditor(grid, filePath1);
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    if (GetFilePathCurrentRow(grid, out var filePath2))
                    {
                        if (File.Exists(filePath2))
                        {
                            try
                            {
                                File.Delete(filePath2);
                                UpdateFilteredData();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                    }
                }
                
            }
            else
            {
                if (GetFilePathCurrentRow(grid, out var path))
                    OpenEditor(grid, path);
            }
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
        private bool KeyIsDown(Keys key)
        {
            return (GetAsyncKeyState(key) < 0);
        }

        bool GetFilePathCurrentRow(DataGridView grid, out string filePath)
        {
            filePath = null;
            if (grid.SelectedRows.Count > 0)
                filePath = grid.SelectedRows[0].Cells[grid.ColumnCount - 1].Value.ToString();
            else
            {
                MessageBox.Show(@"Please select a row!");
                return false;
            }
            return true;
        }

        void UpdateFilteredData()
        {
            CheckProcessesPath(ProcessesTextBox.Text, true);
            CheckOperationsPath(OperationTextBox.Text, true);
            CheckScenariosPath(ScenariosTextBox.Text);
            CheckCommandsPath(CommandsTextBox.Text);
            buttonFilterClick(this, EventArgs.Empty);
        }

        void OpenEditor(DataGridView grid, string path)
        {
            if (notepad == null || notepad.WindowIsClosed)
            {
                notepad = new XmlNotepad(path);
                notepad.Location = this.Location;
                notepad.WindowState = FormWindowState.Maximized;
                //notepad.StartPosition = FormStartPosition.CenterScreen;
                //notepad.TopMost = true;
                //notepad.Show(this);
                notepad.Show();
            }
            else
            {
                notepad.AddNewDocument(path);
                notepad.Focus();
                notepad.Activate();
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

        void CheckProcessesPath(string prcsPath, bool saveLastSett = false)
        {
            string lastSettProcess = saveLastSett ? ProcessesComboBox.Text : null;
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
            ProcessesComboBox.Text = lastSettProcess;
            ProcessesComboBox.DisplayMember = lastSettProcess;
            //ProcessesComboBox.ValueMember = lastSettProcess;
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

        void CheckOperationsPath(string opPath, bool saveLastSett = false)
        {
            string lastSettNetSett = saveLastSett ? NetSettComboBox.Text : null;
            string lastSettOper = saveLastSett ? OperationComboBox.Text : null;

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
            NetSettComboBox.Text = lastSettNetSett;
            NetSettComboBox.DisplayMember = lastSettNetSett;
            //NetSettComboBox.ValueMember = lastSettNetSett;
            OperationComboBox.Text = lastSettOper;
            OperationComboBox.DisplayMember = lastSettOper;
            //OperationComboBox.ValueMember = lastSettOper;
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
