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
using System.Windows.Forms;
using System.Xml;
using OfficeOpenXml;
using SPAFilter.SPA;
using SPAFilter.SPA.SC;
using Utils;
using Utils.WinForm.DataGridViewHelper;
using Utils.WinForm.Notepad;
using XmlHelper = Utils.XmlHelper.XmlHelper;
using Utils.WinForm.CustomProgressBar;
using Utils.AssemblyHelper;

namespace SPAFilter
{
    [Serializable]
    public partial class SPAFilterForm : Form, ISerializable
    {
        private string lastPath = string.Empty;
        public static string SerializationDataPath => $"{Customs.ApplicationFilePath}.bin";
        private XmlNotepad notepad;
        public CollectionBusinessProcess Processes { get; private set; }
        public CollectionNetworkElements NetElements { get; private set; }
        public CollectionScenarios Scenarios { get; private set; }
        public CollectionCommands Commands { get; private set; }
        private object sync = new object();
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
                using (FileStream stream = new FileStream(SerializationDataPath, FileMode.Create, FileAccess.ReadWrite))
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
        }


        private void ProcessFilterForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                buttonFilterClick(this, EventArgs.Empty);
            }
        }

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
                dataGridProcessesResults.DataSource = null;
                dataGridOperationsResult.DataSource = null;
                dataGridScenariosResult.DataSource = null;
                dataGridCommandsResult.DataSource = null;
                dataGridProcessesResults.Refresh();
                dataGridOperationsResult.Refresh();
                dataGridScenariosResult.Refresh();
                dataGridCommandsResult.Refresh();

                ProgressBarCompetition<bool> progress = new ProgressBarCompetition<bool>(progressBar, 10);
                progress.StartProgress(DataFilter);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        CollectionBusinessProcess _filteredProcessCollection;
        NetworkElementCollection _filteredNetElemCollection;
        CollectionScenarios _filteredScenarioCollection = new CollectionScenarios();
        List<Scenario> _filteredSubScenarios = new List<Scenario>();
        CollectionCommands _filteredCMMCollection = new CollectionCommands();
        

        bool DataFilter(ProgressBarCompetition<bool> progressComp)
        {
            try
            {
                _filteredScenarioCollection = new CollectionScenarios();
                _filteredCMMCollection = new CollectionCommands();
                _filteredSubScenarios = new List<Scenario>();

                string processFilter = null, netElemFilter = null, operFilter = null;
                progressBar.Invoke(new MethodInvoker(delegate
                                                     {
                                                         processFilter = ProcessesComboBox.Text;
                                                         netElemFilter = NetSettComboBox.Text;
                                                         operFilter = OperationComboBox.Text;
                                                     }));
                progressComp.ProgressValue = 1;

                if (!processFilter.IsNullOrEmpty())
                {
                    _filteredProcessCollection = new CollectionBusinessProcess();
                    IEnumerable<BusinessProcess> getCollection;
                    if (processFilter[0] == '%' || processFilter[processFilter.Length - 1] == '%')
                        getCollection = Processes.Where(p => p.Name.IndexOf(processFilter.Replace("%", ""), StringComparison.CurrentCultureIgnoreCase) != -1);
                    else
                        getCollection = Processes.Where(p => p.Name.Equals(processFilter, StringComparison.CurrentCultureIgnoreCase));

                    _filteredProcessCollection.AddRange(getCollection);
                }
                else
                {
                    _filteredProcessCollection = Processes.Clone();
                }


                progressComp.ProgressValue = 2;

                if (!netElemFilter.IsNullOrEmpty())
                {
                    _filteredNetElemCollection = new NetworkElementCollection();
                    IEnumerable<NetworkElement> getCollection;
                    if (netElemFilter[0] == '%' || netElemFilter[netElemFilter.Length - 1] == '%')
                        getCollection = NetElements.Elements.Where(p => p.Name.IndexOf(netElemFilter.Replace("%", ""), StringComparison.CurrentCultureIgnoreCase) != -1);
                    else
                        getCollection = NetElements.Elements.Where(p => p.Name.Equals(netElemFilter, StringComparison.CurrentCultureIgnoreCase));

                    _filteredNetElemCollection.AddRange(getCollection.Select(netElem => netElem.Clone()));
                }
                else
                {
                    _filteredNetElemCollection = NetElements.Elements.Clone();
                }

                if (!operFilter.IsNullOrEmpty() && _filteredNetElemCollection != null)
                {
                    NetworkElementCollection netElemCollection2 = new NetworkElementCollection();
                    foreach (NetworkElement nec in _filteredNetElemCollection)
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
                    _filteredNetElemCollection = netElemCollection2;
                }


                progressComp.ProgressValue = 3;

                int endOfBpCollection = _filteredProcessCollection.Count;
                for (int i = 0; i < endOfBpCollection; i++)
                {
                    XmlDocument document = XmlHelper.LoadXml(_filteredProcessCollection[i].FilePath, true);
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


                progressComp.ProgressValue = 4;

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
                                              _filteredScenarioCollection.Add(scenario);
                                              _filteredSubScenarios.AddRange(scenario.SubScenarios);
                                          }
                                      }
                                  };
                }


                progressComp.ProgressValue = 5;

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
                }

                progressComp.ProgressValue = 6;

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


                progressComp.ProgressValue = 7;

                _filteredSubScenarios = _filteredSubScenarios.Distinct(new ItemEqualityComparer()).ToList();
                _filteredScenarioCollection.AddRange(_filteredSubScenarios);


                progressBar.Invoke(new MethodInvoker(delegate
                                                     {
                                                         progressComp.ProgressValue = 8;
                                                         dataGridProcessesResults.AssignListToDataGrid(_filteredProcessCollection, new Padding(0, 0, 15, 0));
                                                         ProcessStatRefresh(_filteredProcessCollection);

                                                         dataGridOperationsResult.AssignListToDataGrid(_filteredNetElemCollection.AllOperations, new Padding(0, 0, 15, 0));
                                                         NEStatRefresh(_filteredNetElemCollection);
                                                         OperationsStatRefresh(_filteredNetElemCollection);

                                                         progressComp.ProgressValue = 9;
                                                         dataGridScenariosResult.AssignListToDataGrid(_filteredScenarioCollection, new Padding(0, 0, 15, 0));
                                                         ScenariosStatRefresh(_filteredScenarioCollection);

                                                         dataGridCommandsResult.AssignListToDataGrid(_filteredCMMCollection, new Padding(0, 0, 15, 0));
                                                         CommandsStatRefresh(_filteredCMMCollection);
                                                         progressComp.ProgressValue = 10;
                                                     }));
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
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
                                MessageBox.Show(ex.Message);
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

        void OpenEditor(string path)
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
                NetElements = new CollectionNetworkElements(opPath.Trim(' '));
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

        void NEStatRefresh(NetworkElementCollection netElems)
        {
            NEElementsCount.Text = $"NEs: {(netElems == null ? string.Empty : netElems.AllNetworkElements.Count.ToString())}";
        }

        void OperationsStatRefresh(NetworkElementCollection netElems)
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

        private void ButtonGenerateSC_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(ExportSCPath.Text))
            {
                Directory.CreateDirectory(ExportSCPath.Text);
            }

            GenerateSC.Enabled = false;

            ProgressBarCompetition<string> progress = new ProgressBarCompetition<string>(progressBar, 6, EnableGenerateSCForm);
            progress.StartProgress(GenerateSCMethod);
        }
        void EnableGenerateSCForm(string result)
        {
            GenerateSC.Enabled = true;
            if (!result.IsNullOrEmpty() && File.Exists(result))
            {
                OpenEditor(result);
            }
        }

        string GenerateSCMethod(ProgressBarCompetition<string> progressComp)
        {
            try
            {
                ServiceCatalog sc = null;
                progressComp.ProgressValue = 1;
                DataTable serviceTable = GetServiceXslx();
                progressComp.ProgressValue = 2;

                if (_filteredNetElemCollection != null && _filteredNetElemCollection.Count > 0)
                    sc = new ServiceCatalog(_filteredNetElemCollection, progressComp, serviceTable);
                else if (NetElements?.Elements != null && NetElements?.Elements.Count > 0)
                    sc = new ServiceCatalog(NetElements.Elements, progressComp, serviceTable);

                return sc?.Save(ExportSCPath.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private string[] mandatoryXslxColumns = new string[] { "#", "SPA_SERVICE_CODE", "GLOBAL_SERVICE_CODE", "SERVICE_NAME", "SERVICE_FULL_NAME", "SERVICE_FULL_NAME2", "DESCRIPTION", "SERVICE_CODE", "SERVICE_NAME2", "EXTERNAL_CODE", "EXTERNAL_CODE2" };

        DataTable GetServiceXslx()
        {
            DataTable serviceTable = null;
            if (!OpenSCXlsx.Text.IsNullOrEmptyTrim() && File.Exists(OpenSCXlsx.Text))
            {
                using (ExcelPackage xlPackage = new ExcelPackage(new FileInfo(OpenSCXlsx.Text)))
                {
                    serviceTable = new DataTable();

                    var myWorksheet = xlPackage.Workbook.Worksheets.First(); //sheet
                    var totalRows = myWorksheet.Dimension.End.Row;
                    var totalColumns = mandatoryXslxColumns.Length;

                    var columnsNames = myWorksheet.Cells[1, 1, 1, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());

                    if (!columnsNames.Any())
                        return null;

                    int i = 0;
                    foreach (var columnName in columnsNames)
                    {
                        string columnNameUp = columnName.ToUpper();
                        if (mandatoryXslxColumns[i++] != columnNameUp)
                            throw new Exception($"Wrong column name before \'{columnNameUp}\' from file '{OpenSCXlsx.Text}'.\r\nColumns names should be like:\r\n'{string.Join("','", mandatoryXslxColumns)}'");
                        serviceTable.Columns.Add(columnNameUp, typeof(string));
                        if (i == mandatoryXslxColumns.Length)
                            break;
                    }

                    if(i != mandatoryXslxColumns.Length)
                        throw new Exception($"Wrong file '{OpenSCXlsx.Text}'. Missing some required columns. \r\nColumns names should be like:\r\n'{string.Join("','", mandatoryXslxColumns)}'");

                    for (int rowNum = 2; rowNum <= totalRows; rowNum++)
                    {
                        //var row = myWorksheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());
                        var row = myWorksheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).Take(totalColumns);
                        //IEnumerable<string> ff = row.Take(totalColumns);
                        
                        serviceTable.Rows.Add(values: row.ToArray());

                    }
                }
            }

            return serviceTable;
        }

        void UpdateLastPath(string path)
        {
            if (!path.IsNullOrEmpty())
                lastPath = path;
        }

        string OpenFolder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (!lastPath.IsNullOrEmpty())
                    fbd.SelectedPath = lastPath;
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    return fbd.SelectedPath;
                }
            }
            return null;
        }


    }
}
