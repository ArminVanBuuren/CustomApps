using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading.Tasks;
using OfficeOpenXml;
using SPAFilter.SPA;
using SPAFilter.SPA.Collection;
using SPAFilter.SPA.Components;
using SPAFilter.SPA.SC;
using Utils;
using Utils.WinForm.DataGridViewHelper;
using Utils.WinForm.Notepad;
using Utils.WinForm.CustomProgressBar;
using static Utils.ASSEMBLY;

namespace SPAFilter
{
    [Serializable]
    public partial class SPAFilterForm : Form, ISerializable
    {
        private bool _isInProgress = false;
        private string _lastPath = string.Empty;
        private readonly object sync = new object();
        private readonly object sync2 = new object();


        private bool IsInProgress
        {
            get => _isInProgress;
            set
            {
                lock (sync2)
                {
                    _isInProgress = value;

                    if(_isInProgress && notepad != null && !notepad.WindowIsClosed)
                        notepad.Close();

                    buttonFilter.Enabled = !_isInProgress;
                    buttonPrintXML.Enabled = !_isInProgress;
                    ProcessesTextBox.Enabled = !_isInProgress;
                    OperationTextBox.Enabled = !_isInProgress;
                    ScenariosTextBox.Enabled = !_isInProgress;
                    CommandsTextBox.Enabled = !_isInProgress;
                    ProcessesButtonOpen.Enabled = !_isInProgress;
                    OperationButtonOpen.Enabled = !_isInProgress;
                    ScenariosButtonOpen.Enabled = !_isInProgress;
                    CommnadsButtonOpen.Enabled = !_isInProgress;

                    dataGridProcesses.Visible = !_isInProgress;
                    dataGridOperations.Visible = !_isInProgress;
                    dataGridScenarios.Visible = !_isInProgress;
                    dataGridCommands.Visible = !_isInProgress;
                    GenerateSC.Enabled = !_isInProgress;

                    ProcessesComboBox.Enabled = !_isInProgress;
                    NetSettComboBox.Enabled = !_isInProgress;
                    OperationComboBox.Enabled = !_isInProgress;
                }
            }
        }

        public static string SerializationDataPath => $"{ApplicationFilePath}.bin";
        private XmlNotepad notepad;
        public CollectionBusinessProcess Processes { get; private set; }
        public NetworkElementCollection NetElements { get; private set; }
        public CollectionScenarios Scenarios { get; private set; }
        public CollectionCommands Commands { get; private set; }
        private bool IsInitialization { get; } = true;

        public SPAFilterForm()
        {
            IsInitialization = false;
            MainInit();
        }

        private void ProcessFilterForm_Closing(object sender, CancelEventArgs e)
        {
            SaveFile();
        }

        void SaveFile()
        {
            if (IsInitialization)
                return;

            lock (sync)
            {
                using (var stream = new FileStream(SerializationDataPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    new BinaryFormatter().Serialize(stream, this);
                }
            }
        }

        SPAFilterForm(SerializationInfo propertyBag, StreamingContext context)
        {
            MainInit();
            try
            {
                ProcessesTextBox.Text = propertyBag.GetString("ADWFFW");
                OperationTextBox.Text = propertyBag.GetString("AAEERF");
                ScenariosTextBox.Text = propertyBag.GetString("DFWDRT");
                CommandsTextBox.Text = propertyBag.GetString("WWWERT");
                ExportSCPath.Text = propertyBag.GetString("FFFGHJ");
                OpenSCXlsx.Text = propertyBag.GetString("GGHHRR");
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                IsInitialization = false;
            }
        }

        void ISerializable.GetObjectData(SerializationInfo propertyBag, StreamingContext context)
        {
            propertyBag.AddValue("ADWFFW", ProcessesTextBox.Text);
            propertyBag.AddValue("AAEERF", OperationTextBox.Text);
            propertyBag.AddValue("DFWDRT", ScenariosTextBox.Text);
            propertyBag.AddValue("WWWERT", CommandsTextBox.Text);
            propertyBag.AddValue("FFFGHJ", ExportSCPath.Text);
            propertyBag.AddValue("GGHHRR", OpenSCXlsx.Text);
        }

        void MainInit()
        {
            InitializeComponent();

            //progressBar.Visible = true;
            //progressBar.Value = 100;

            dataGridProcesses.KeyDown += DataGridProcessesResults_KeyDown;
            dataGridOperations.KeyDown += DataGridOperationsResult_KeyDown;
            dataGridScenarios.KeyDown += DataGridScenariosResult_KeyDown;
            dataGridCommands.KeyDown += DataGridCommandsResult_KeyDown;

            tabControl1.KeyDown += ProcessFilterForm_KeyDown;
            ProcessesTextBox.KeyDown += ProcessFilterForm_KeyDown;
            OperationTextBox.KeyDown += ProcessFilterForm_KeyDown;
            ScenariosTextBox.KeyDown += ProcessFilterForm_KeyDown;
            CommandsTextBox.KeyDown += ProcessFilterForm_KeyDown;
            ProcessesComboBox.KeyDown += ProcessFilterForm_KeyDown;
            NetSettComboBox.KeyDown += ProcessFilterForm_KeyDown;
            OperationComboBox.KeyDown += ProcessFilterForm_KeyDown;
            dataGridProcesses.KeyDown += ProcessFilterForm_KeyDown;
            dataGridOperations.KeyDown += ProcessFilterForm_KeyDown;
            dataGridScenarios.KeyDown += ProcessFilterForm_KeyDown;
            dataGridCommands.KeyDown += ProcessFilterForm_KeyDown;
            ProcessesButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            OperationButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            ScenariosButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            CommnadsButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            buttonFilter.KeyDown += ProcessFilterForm_KeyDown;
            KeyDown += ProcessFilterForm_KeyDown;

            dataGridScenarios.CellFormatting += DataGridScenariosResult_CellFormatting;
            Closing += ProcessFilterForm_Closing;
        }


        private void ProcessFilterForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    ButtonFilterClick(this, EventArgs.Empty);
                    break;
                //case Keys.F6:
                //    buttonPrintXML_Click(this, EventArgs.Empty);
                //    break;
            }
        }

        private async void ButtonPrintXML_Click(object sender, EventArgs e)
        {
            int filesNumber = (_filteredProcessCollection?.Count ?? 0) +
                              ((_filteredNetElemCollection == null || _filteredNetElemCollection.Count == 0) ? 0 : _filteredNetElemCollection.Sum(x => x.Operations.Count)) +
                              (_filteredScenarioCollection?.Count ?? 0) +
                              (_filteredCMMCollection?.Count ?? 0);

            if (filesNumber <= 0)
            {
                MessageBox.Show(@"You must filter files first.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (IsInProgress)
                return;

            try
            {
                IsInProgress = true;

                using (var stringErrors = new CustomStringBuilder())
                {
                    using (var progrAsync = new ProgressCalculationAsync(progressBar, filesNumber))
                    {
                        await Task.Factory.StartNew(() =>
                        {
                            GetFiles(_filteredProcessCollection, stringErrors, progrAsync);
                            if (_filteredNetElemCollection != null)
                                GetFiles(_filteredNetElemCollection.AllOperations, stringErrors, progrAsync);
                            GetFiles(_filteredScenarioCollection, stringErrors, progrAsync);
                            GetFiles(_filteredCMMCollection, stringErrors, progrAsync);
                        });
                    }

                    if (stringErrors.Length > 0)
                    {
                        MessageBox.Show($"Several ({stringErrors.Lines}) errors found:\r\n\r\n{stringErrors.ToString(2)}", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show($"Successfully completed.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                IsInProgress = false;
            }
        }

        static void GetFiles(IEnumerable<ObjectTemplate> fileObj, CustomStringBuilder stringErrors, ProgressCalculationAsync progressCalc)
        {
            if (fileObj == null || !fileObj.Any())
                return;

            foreach (var file in fileObj)
            {
                FormattingXML(file.FilePath, stringErrors);
                progressCalc++;
            }
        }

        static void FormattingXML(string filePath, CustomStringBuilder stringErrors)
        {
            try
            {
                string fileString = IO.SafeReadFile(filePath);
                if (XML.IsXml(fileString, out XmlDocument document))
                {
                    string formatting = document.PrintXml();

                    int attempts = 0;
                    while (!IO.IsFileReady(filePath))
                    {
                        System.Threading.Thread.Sleep(500);
                        attempts++;
                        if (attempts > 3)
                            return;
                    }

                    IO.WriteFile(filePath, formatting, Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                stringErrors.AppendLine($"Message=\"{e.Message}\" File=\"{filePath}]\"");
            }
        }

       
        private async void ButtonFilterClick(object sender, EventArgs e)
        {
            if (IsInProgress)
                return;

            if (Processes == null || NetElements == null)
            {
                dataGridProcesses.DataSource = null;
                dataGridOperations.DataSource = null;
                return;
            }

            try
            {
                IsInProgress = true;
                dataGridProcesses.DataSource = null;
                dataGridOperations.DataSource = null;
                dataGridScenarios.DataSource = null;
                dataGridCommands.DataSource = null;
                dataGridProcesses.Refresh();
                dataGridOperations.Refresh();
                dataGridScenarios.Refresh();
                dataGridCommands.Refresh();

                using (var progressCalc = new ProgressCalculationAsync(progressBar, 20))
                {
                    var filterProcess = ProcessesComboBox.Text;
                    var filterNE = NetSettComboBox.Text;
                    var filterOp = OperationComboBox.Text;

                    await Task.Factory.StartNew(() => DataFilter(filterProcess, filterNE, filterOp, progressCalc));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                IsInProgress = false;
            }
        }

        CollectionBusinessProcess _filteredProcessCollection;
        CollectionNetworkElement _filteredNetElemCollection;
        CollectionScenarios _filteredScenarioCollection = new CollectionScenarios();
        List<Scenario> _filteredSubScenarios = new List<Scenario>();
        CollectionCommands _filteredCMMCollection = new CollectionCommands();
        

        void DataFilter(string filterProcess, string filterNE, string filterOp, ProgressCalculationAsync progressCalc)
        {
            try
            {
                _filteredScenarioCollection = new CollectionScenarios();
                _filteredCMMCollection = new CollectionCommands();
                _filteredSubScenarios = new List<Scenario>();

                if (!filterProcess.IsNullOrEmpty())
                {
                    _filteredProcessCollection = new CollectionBusinessProcess();
                    IEnumerable<BusinessProcess> getCollection;
                    if (filterProcess[0] == '%' || filterProcess[filterProcess.Length - 1] == '%')
                        getCollection = Processes.Where(p => p.Name.IndexOf(filterProcess.Replace("%", ""), StringComparison.CurrentCultureIgnoreCase) != -1);
                    else
                        getCollection = Processes.Where(p => p.Name.Equals(filterProcess, StringComparison.CurrentCultureIgnoreCase));

                    _filteredProcessCollection.AddRange(getCollection);
                }
                else
                {
                    _filteredProcessCollection = Processes.Clone();
                }

                _filteredProcessCollection.Sort((x, y) => x.ID - y.ID);

                if (!filterNE.IsNullOrEmpty())
                {
                    _filteredNetElemCollection = new CollectionNetworkElement();
                    IEnumerable<NetworkElement> getCollection;
                    if (filterNE[0] == '%' || filterNE[filterNE.Length - 1] == '%')
                        getCollection = NetElements.Elements.Where(p => p.Name.IndexOf(filterNE.Replace("%", ""), StringComparison.CurrentCultureIgnoreCase) != -1);
                    else
                        getCollection = NetElements.Elements.Where(p => p.Name.Equals(filterNE, StringComparison.CurrentCultureIgnoreCase));

                    _filteredNetElemCollection.AddRange(getCollection.Select(netElem => netElem.Clone()));
                }
                else
                {
                    _filteredNetElemCollection = NetElements.Elements.Clone();
                }

                if (!filterOp.IsNullOrEmpty() && _filteredNetElemCollection != null)
                {
                    var netElemCollection2 = new CollectionNetworkElement();
                    foreach (NetworkElement nec in _filteredNetElemCollection)
                    {
                        List<Operation> ops = new List<Operation>();
                        foreach (Operation neo in nec.Operations)
                        {
                            if (filterOp[0] == '%' || filterOp[filterOp.Length - 1] == '%')
                            {
                                if (neo.Name.IndexOf(filterOp.Replace("%", ""), StringComparison.CurrentCultureIgnoreCase) != -1)
                                {
                                    ops.Add(neo);
                                }
                            }
                            else
                            {
                                if (neo.Name.Equals(filterOp, StringComparison.CurrentCultureIgnoreCase))
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
                    _filteredNetElemCollection = netElemCollection2;
                }

                int endOfBpCollection = _filteredProcessCollection.Count;
                for (int i = 0; i < endOfBpCollection; i++)
                {
                    var document = XML.LoadXml(_filteredProcessCollection[i].FilePath, true);
                    if (document != null)
                    {
                        _filteredProcessCollection[i].AddBodyOperations(document);
                        //bool hasMatch = bpCollection[i].Operations.Any(x => netElemCollection.AllOperationsName.Any(y => x.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1));
                        bool hasMatch = _filteredProcessCollection[i].Operations.Any(x => _filteredNetElemCollection.AllOperationsName.Any(y => x.Equals(y, StringComparison.CurrentCultureIgnoreCase)));

                        if (!hasMatch)
                        {
                            _filteredProcessCollection.Remove(_filteredProcessCollection[i]);
                            i--;
                            endOfBpCollection--;
                        }
                    }
                    else
                    {
                        _filteredProcessCollection.Remove(_filteredProcessCollection[i]);
                        i--;
                        endOfBpCollection--;
                    }
                }

                progressCalc.Append(2);

                Action<Operation> getScenario = null;
                if (Scenarios != null)
                {
                    getScenario = operation =>
                                  {
                                      var scenarios = Scenarios.Where(p => p.Name.Equals(operation.Name, StringComparison.CurrentCultureIgnoreCase)).ToList();

                                      foreach (Scenario scenario in scenarios)
                                      {
                                          if (scenario.AddBodyCommands())
                                          {
                                              _filteredScenarioCollection.Add(scenario);
                                              _filteredSubScenarios.AddRange(scenario.SubScenarios);
                                          }
                                      }
                                  };
                }

                int maxForCurrentIterator = progressCalc.TotalProgressIterator / 2;
                if (_filteredNetElemCollection != null && _filteredNetElemCollection.Count > 0)
                {
                    int progressIterator = 0;

                    Action calcProgress = null;
                    if (_filteredNetElemCollection.Count > maxForCurrentIterator)
                    {
                        int numberOfIter = 0;
                        progressIterator = _filteredNetElemCollection.Count / maxForCurrentIterator;
                        calcProgress = () =>
                        {
                            numberOfIter++;
                            if (numberOfIter < progressIterator)
                                return;
                            progressCalc++;
                            numberOfIter = 0;
                        };
                    }
                    else
                    {
                        progressIterator = maxForCurrentIterator / _filteredNetElemCollection.Count;
                        calcProgress = () =>
                        {
                            progressCalc.Append(progressIterator);
                        };
                    }

                    foreach (var netElem in _filteredNetElemCollection)
                    {
                        int endOfOpCollection = netElem.Operations.Count;
                        for (int i = 0; i < endOfOpCollection; i++)
                        {
                            bool hasMatch = _filteredProcessCollection.Any(x => x.Operations.Any(y => netElem.Operations[i].Name.Equals(y, StringComparison.CurrentCultureIgnoreCase)));

                            if (hasMatch)
                            {
                                getScenario?.Invoke(netElem.Operations[i]);
                                continue;
                            }

                            netElem.Operations.Remove(netElem.Operations[i]);
                            i--;
                            endOfOpCollection--;
                        }

                        calcProgress.Invoke();
                    }
                }
                else
                {
                    progressCalc.Append(maxForCurrentIterator);
                }



                if (Commands != null && _filteredScenarioCollection.Count > 0)
                {
                    foreach (Command command in Commands)
                    {
                        bool hasValue = _filteredScenarioCollection.Any(x => x.Commands.Any(y => y.Equals(command.Name, StringComparison.CurrentCultureIgnoreCase)));
                        if (hasValue)
                        {
                            _filteredCMMCollection.Add(command);
                        }
                    }
                }


                progressCalc.Append(2);

                _filteredSubScenarios = _filteredSubScenarios.Distinct(new ItemEqualityComparer<Scenario>()).ToList();
                _filteredScenarioCollection.AddRange(_filteredSubScenarios);


                progressBar.Invoke(new MethodInvoker(delegate
                                                     {
                                                         dataGridProcesses.AssignListToDataGrid(_filteredProcessCollection, new Padding(0, 0, 15, 0));
                                                         ProcessStatRefresh(_filteredProcessCollection);

                                                         progressCalc.Append(2);

                                                         dataGridOperations.AssignListToDataGrid(_filteredNetElemCollection.AllOperations, new Padding(0, 0, 15, 0));
                                                         NEStatRefresh(_filteredNetElemCollection);
                                                         OperationsStatRefresh(_filteredNetElemCollection);

                                                         progressCalc.Append(2);

                                                         dataGridScenarios.AssignListToDataGrid(_filteredScenarioCollection, new Padding(0, 0, 15, 0));
                                                         ScenariosStatRefresh(_filteredScenarioCollection);

                                                         dataGridCommands.AssignListToDataGrid(_filteredCMMCollection, new Padding(0, 0, 15, 0));
                                                         CommandsStatRefresh(_filteredCMMCollection);

                                                         progressCalc.Append(2);
                                                     }));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void DataGridScenariosResult_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewRow row = dataGridScenarios.Rows[e.RowIndex];
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
            CallAndCheckDataGridKey(dataGridProcesses);
        }
        private void DataGridProcessesResults_KeyDown(object sender, KeyEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridProcesses, e);
        }

        private void dataGridOperationsResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridOperations);
        }
        private void DataGridOperationsResult_KeyDown(object sender, KeyEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridOperations, e);
        }

        private void dataGridScenariosResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridScenarios);
        }
        private void DataGridScenariosResult_KeyDown(object sender, KeyEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridScenarios, e);
        }

        private void dataGridCommandsResult_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridCommands);
        }

        private void DataGridCommandsResult_KeyDown(object sender, KeyEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridCommands, e);
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
                        OpenEditor(filePath1);
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
                                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                
            }
            else
            {
                if (GetFilePathCurrentRow(grid, out var path))
                    OpenEditor(path);
            }
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
        private static bool KeyIsDown(Keys key)
        {
            return (GetAsyncKeyState(key) < 0);
        }

        static bool GetFilePathCurrentRow(DataGridView grid, out string filePath)
        {
            filePath = null;
            if (grid.SelectedRows.Count > 0)
            {
                filePath = grid.SelectedRows[0].Cells[grid.ColumnCount - 1].Value.ToString();
            }
            else
            {
                MessageBox.Show(@"You must select a row!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            ButtonFilterClick(this, EventArgs.Empty);
        }

        void OpenEditor(string path)
        {
            if (notepad == null || notepad.WindowIsClosed)
            {
                notepad = new XmlNotepad(path)
                {
                    Location = Location,
                    WindowState = FormWindowState.Maximized
                };
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
            SaveFile();
        }

        void CheckProcessesPath(string prcsPath, bool saveLastSett = false)
        {
            string lastSettProcess = saveLastSett ? ProcessesComboBox.Text : null;
            Processes?.Clear();
            _filteredProcessCollection?.Clear();

            if (Directory.Exists(prcsPath.Trim(' ')))
            {
                UpdateLastPath(prcsPath.Trim(' '));
                Processes = new CollectionBusinessProcess(prcsPath.Trim(' '));
                ProcessesComboBox.DataSource = Processes.Select(p => p.Name).ToList();
                ProcessStatRefresh(Processes);
            }
            else
            {
                ProcessesComboBox.DataSource = null;
                ProcessStatRefresh(null);
            }
            ProcessesComboBox.Text = lastSettProcess;
            ProcessesComboBox.DisplayMember = lastSettProcess;
            //ProcessesComboBox.ValueMember = lastSettProcess;
        }

        void ProcessStatRefresh(CollectionBusinessProcess processes)
        {
            BPCount.Text = $"Processes: {(processes == null ? string.Empty : processes.Count.ToString())}";
        }

        private void OperationButtonOpen_Click(object sender, EventArgs e)
        {
            OperationTextBox.Text = OpenFolder() ?? OperationTextBox.Text;
            CheckOperationsPath(OperationTextBox.Text);
        }
        private void OperationTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckOperationsPath(OperationTextBox.Text);
            SaveFile();
        }

        void CheckOperationsPath(string opPath, bool saveLastSett = false)
        {
            string lastSettNetSett = saveLastSett ? NetSettComboBox.Text : null;
            string lastSettOper = saveLastSett ? OperationComboBox.Text : null;
            NetElements?.Elements?.Clear();
            _filteredNetElemCollection?.Clear();

            if (Directory.Exists(opPath.Trim(' ')))
            {   
                UpdateLastPath(opPath.Trim(' '));
                NetElements = new NetworkElementCollection(opPath.Trim(' '));
                NetSettComboBox.DataSource = NetElements.Elements.AllNetworkElements;
                OperationComboBox.DataSource = NetElements.Elements.AllOperationsName;
                NEStatRefresh(NetElements.Elements);
                OperationsStatRefresh(NetElements.Elements);
            }
            else
            {
                NetSettComboBox.DataSource = null;
                OperationComboBox.DataSource = null;
                NEStatRefresh(null);
                OperationsStatRefresh(null);
            }

            CheckButtonGenerateSC();

            NetSettComboBox.Text = lastSettNetSett;
            NetSettComboBox.DisplayMember = lastSettNetSett;
            //NetSettComboBox.ValueMember = lastSettNetSett;
            OperationComboBox.Text = lastSettOper;
            OperationComboBox.DisplayMember = lastSettOper;
            //OperationComboBox.ValueMember = lastSettOper;
        }

        void NEStatRefresh(CollectionNetworkElement netElems)
        {
            NEElementsCount.Text = $"NEs: {(netElems == null ? string.Empty : netElems.AllNetworkElements.Count.ToString())}";
        }

        void OperationsStatRefresh(CollectionNetworkElement netElems)
        {
            OperationsCount.Text = $"Operations: {(netElems == null ? string.Empty : netElems.AllOperationsName.Count.ToString())}";
        }

        private void ScenariosButtonOpen_Click(object sender, EventArgs e)
        {
            ScenariosTextBox.Text = OpenFolder() ?? ScenariosTextBox.Text;
            CheckScenariosPath(ScenariosTextBox.Text);
        }
        private void ScenariosTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckScenariosPath(ScenariosTextBox.Text);
            SaveFile();
        }
        void CheckScenariosPath(string scoPath)
        {
            Scenarios?.Clear();
            _filteredScenarioCollection?.Clear();
            _filteredSubScenarios?.Clear();

            if (Directory.Exists(scoPath.Trim(' ')))
            {
                UpdateLastPath(scoPath.Trim(' '));
                Scenarios = new CollectionScenarios(scoPath.Trim(' '));
                ScenariosStatRefresh(Scenarios);
            }
            else
            {
                ScenariosStatRefresh(null);
            }
        }

        void ScenariosStatRefresh(CollectionScenarios scenarios)
        {
            ScenariosCount.Text = $"Scenarios: {(scenarios == null ? string.Empty : scenarios.Count.ToString())}";
        }

        private void CommnadsButtonOpen_Click(object sender, EventArgs e)
        {
            CommandsTextBox.Text = OpenFolder() ?? CommandsTextBox.Text;
            CheckCommandsPath(CommandsTextBox.Text);
        }
        private void CommandsTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckCommandsPath(CommandsTextBox.Text);
            SaveFile();
        }
        void CheckCommandsPath(string cmmPath)
        {
            Commands?.Clear();
            _filteredCMMCollection?.Clear();

            if (Directory.Exists(cmmPath.Trim(' ')))
            {
                UpdateLastPath(cmmPath.Trim(' '));
                Commands = new CollectionCommands(cmmPath.Trim(' '));
                CommandsStatRefresh(Commands);
            }
            else
            {
                CommandsStatRefresh(null);
            }
        }

        void CommandsStatRefresh(CollectionCommands commands)
        {
            CommandsCount.Text = $"Commands: {(commands == null ? string.Empty : commands.Count.ToString())}";
        }

        void CheckButtonGenerateSC()
        {
            if (NetElements?.Elements != null && NetElements?.Elements.Count > 0 && ExportSCPath.Text != null && ExportSCPath.Text.Length > 3)
            {
                ButtonGenerateSC.Enabled = true;
                return;
            }

            ButtonGenerateSC.Enabled = false;
        }

        private void ExportSCPath_TextChanged(object sender, EventArgs e)
        {
            CheckButtonGenerateSC();
            SaveFile();
        }

        private void OpenSCXlsx_TextChanged(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void RootSCExportPathButton_Click(object sender, EventArgs e)
        {
            ExportSCPath.Text = OpenFolder() ?? ExportSCPath.Text;
        }

        private void OpenSevExelButton_Click(object sender, EventArgs e)
        {
            using (var fbd = new OpenFileDialog())
            {
                fbd.Filter = @"(*.xlsx) | *.xlsx";
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                {
                    OpenSCXlsx.Text = fbd.FileName;
                }
            }
        }

        private async void ButtonGenerateSC_Click(object sender, EventArgs e)
        {
            if (IsInProgress)
                return;

            CollectionNetworkElement collectionElements = null;

            if (_filteredNetElemCollection != null && _filteredNetElemCollection.Count > 0)
                collectionElements = _filteredNetElemCollection;
            else if (NetElements?.Elements != null && NetElements?.Elements.Count > 0)
                collectionElements = NetElements.Elements;


            int generateSCIterators = (collectionElements == null || collectionElements.Count == 0) ? 0 : collectionElements.Sum(x => x.Operations.Count);

            if (generateSCIterators == 0)
            {
                MessageBox.Show($"Not found any operations.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                IsInProgress = true;

                if (!Directory.Exists(ExportSCPath.Text))
                    Directory.CreateDirectory(ExportSCPath.Text);

                string fileResult = null;

                if (!OpenSCXlsx.Text.IsNullOrEmptyTrim() && File.Exists(OpenSCXlsx.Text))
                {
                    DataTable rdServices = null;
                    var file = new FileInfo(OpenSCXlsx.Text);
                    
                    using (var progrAsync = new CustomProgressCalculation(progressBar, generateSCIterators, file))
                    {
                        rdServices = await Task<DataTable>.Factory.StartNew(() => GetRDServicesFromXslx(file, progrAsync));

                        fileResult = await Task.Factory.StartNew(() => GetServiceCatalog(collectionElements, rdServices, ExportSCPath.Text, progrAsync));
                    }
                }
                else
                {
                    using (var progrAsync = new CustomProgressCalculation(progressBar, generateSCIterators))
                    {
                        fileResult = await Task.Factory.StartNew(() => GetServiceCatalog(collectionElements, null, ExportSCPath.Text, progrAsync));
                    }
                }

                if (!fileResult.IsNullOrEmpty() && File.Exists(fileResult))
                {
                    OpenEditor(fileResult);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                IsInProgress = false;
            }
        }

        private readonly string[] mandatoryXslxColumns = new string[] { "#", "SPA_SERVICE_CODE", "GLOBAL_SERVICE_CODE", "SERVICE_NAME", "SERVICE_FULL_NAME", "SERVICE_FULL_NAME2", "DESCRIPTION", "SERVICE_CODE", "SERVICE_NAME2", "EXTERNAL_CODE", "EXTERNAL_CODE2" };

        DataTable GetRDServicesFromXslx(FileInfo file, CustomProgressCalculation progressCalc)
        {
            var serviceTable = new DataTable();

            progressCalc.BeginOpenXslxFile();

            using (var xslPackage = new ExcelPackage(file))
            {
                progressCalc.BeginReadXslxFile();

                var myWorksheet = xslPackage.Workbook.Worksheets.First(); //sheet
                var totalRows = myWorksheet.Dimension.End.Row;
                var totalColumns = mandatoryXslxColumns.Length;

                progressCalc.EndReadXslxFile(totalRows);

                var columnsNames = myWorksheet.Cells[1, 1, 1, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());

                if (!columnsNames.Any())
                    return null;

                int i = 0;
                foreach (var columnName in columnsNames)
                {
                    string columnNameUp = columnName.ToUpper();
                    if (mandatoryXslxColumns[i++] != columnNameUp)
                    {
                        throw new Exception($"Wrong column name before \'{columnNameUp}\' from file '{OpenSCXlsx.Text}'.\r\n\r\nColumns names and orders must be like:\r\n'{string.Join("','", mandatoryXslxColumns)}'");
                    }

                    serviceTable.Columns.Add(columnNameUp, typeof(string));
                    if (i == mandatoryXslxColumns.Length)
                        break;
                }

                if (i != mandatoryXslxColumns.Length)
                    throw new Exception($"Wrong file '{OpenSCXlsx.Text}'. Missing some required columns. \r\nColumns names should be like:\r\n'{string.Join("','", mandatoryXslxColumns)}'");

                for (int rowNum = 2; rowNum <= totalRows; rowNum++)
                {
                    var row = myWorksheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).Take(totalColumns);
                    serviceTable.Rows.Add(values: row.ToArray());
                    progressCalc.ReadXslxFileLine();
                }
            }

            progressCalc.EndOpenXslxFile();

            return serviceTable;
        }

        static string GetServiceCatalog(CollectionNetworkElement elements, DataTable rdServices, string exportFilePath, CustomProgressCalculation progressCalc)
        {
            try
            {
                var sc = new ServiceCatalog(elements, rdServices, progressCalc);
                if (progressCalc.CurrentProgressIterator < progressCalc.TotalProgressIterator)
                    progressCalc.Append(progressCalc.TotalProgressIterator - progressCalc.CurrentProgressIterator);
                return sc?.Save(exportFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        

        void UpdateLastPath(string path)
        {
            if (!path.IsNullOrEmpty())
                _lastPath = path;
        }

        string OpenFolder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (!_lastPath.IsNullOrEmpty())
                    fbd.SelectedPath = _lastPath;

                var result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    return fbd.SelectedPath;
                }
            }
            return null;
        }
    }
}
