using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Threading.Tasks;
using SPAFilter.SPA;
using SPAFilter.SPA.Collection;
using Utils;
using Utils.WinForm.DataGridViewHelper;
using Utils.WinForm.Notepad;
using Utils.WinForm.CustomProgressBar;
using Utils.WinForm.Handles;
using static Utils.ASSEMBLY;

namespace SPAFilter
{
    [Serializable]
    public partial class SPAFilterForm : Form, ISerializable
    {
        private bool _isInProgress = false;
        private string _lastDirPath = string.Empty;
        private readonly object sync = new object();
        private readonly object sync2 = new object();
        private Notepad _notepad;
        private bool _notepadWordWrap = true;
        private FormLocation _notepadLocation = FormLocation.Default;
        private FormWindowState _notepadWindowsState = FormWindowState.Maximized;
        private SPAProcessFilter _spaFilter;


        public static string SerializationDataPath => $"{ApplicationFilePath}.bin";
        private bool IsInitialization { get; } = true;

        private bool IsInProgress
        {
            get => _isInProgress;
            set
            {
                lock (sync2)
                {
                    _isInProgress = value;

                    if(_isInProgress && _notepad != null && !_notepad.WindowIsClosed)
                        _notepad.Close();

                    FilterButton.Enabled = !_isInProgress;
                    PrintXMLButton.Enabled = !_isInProgress;
                    ProcessesTextBox.Enabled = !_isInProgress;
                    OperationTextBox.Enabled = !_isInProgress;
                    ServiceCatalogTextBox.Enabled = !_isInProgress;
                    ActivatorList.Enabled = !_isInProgress;
                    ProcessesButtonOpen.Enabled = !_isInProgress;
                    OperationButtonOpen.Enabled = !_isInProgress;
                    ServiceCatalogOpenButton.Enabled = !_isInProgress;
                    AddActivatorButton.Enabled = !_isInProgress;
                    RemoveActivatorButton.Enabled = !_isInProgress;

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


        public SPAFilterForm()
        {
            IsInitialization = false;
            MainInit();
        }



        SPAFilterForm(SerializationInfo propertyBag, StreamingContext context)
        {
            MainInit();
            try
            {
                ProcessesTextBox.Text = propertyBag.GetString("ADWFFW");
                OperationTextBox.Text = propertyBag.GetString("AAEERF");
                ServiceCatalogTextBox.Text = propertyBag.GetString("DFWDRT");
                ActivatorList.Text = propertyBag.GetString("WWWERT");
                ExportSCPath.Text = propertyBag.GetString("FFFGHJ");
                OpenSCXlsx.Text = propertyBag.GetString("GGHHRR");
                _notepadWordWrap = propertyBag.GetBoolean("DDCCVV");
                _notepadLocation = (FormLocation)propertyBag.GetValue("RRTTDD", typeof(FormLocation));
                _notepadWindowsState = (FormWindowState)propertyBag.GetValue("SSEEFF", typeof(FormWindowState));
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
            propertyBag.AddValue("DFWDRT", ServiceCatalogTextBox.Text);
            propertyBag.AddValue("WWWERT", ActivatorList.Text);
            propertyBag.AddValue("FFFGHJ", ExportSCPath.Text);
            propertyBag.AddValue("GGHHRR", OpenSCXlsx.Text);
            propertyBag.AddValue("DDCCVV", _notepadWordWrap);
            propertyBag.AddValue("RRTTDD", _notepadLocation);
            propertyBag.AddValue("SSEEFF", _notepadWindowsState);
        }

        void MainInit()
        {
            ServiceCatalog sc = new ServiceCatalog(@"E:\test.xml");

            InitializeComponent();

            dataGridProcesses.KeyDown += DataGridProcessesResults_KeyDown;
            dataGridOperations.KeyDown += DataGridOperationsResult_KeyDown;
            dataGridScenarios.KeyDown += DataGridScenariosResult_KeyDown;
            dataGridCommands.KeyDown += DataGridCommandsResult_KeyDown;

            tabControl1.KeyDown += ProcessFilterForm_KeyDown;
            ProcessesTextBox.KeyDown += ProcessFilterForm_KeyDown;
            OperationTextBox.KeyDown += ProcessFilterForm_KeyDown;
            //ScenariosTextBox.KeyDown += ProcessFilterForm_KeyDown;
            //CommandsTextBox.KeyDown += ProcessFilterForm_KeyDown;
            ProcessesComboBox.KeyDown += ProcessFilterForm_KeyDown;
            NetSettComboBox.KeyDown += ProcessFilterForm_KeyDown;
            OperationComboBox.KeyDown += ProcessFilterForm_KeyDown;
            dataGridProcesses.KeyDown += ProcessFilterForm_KeyDown;
            dataGridOperations.KeyDown += ProcessFilterForm_KeyDown;
            dataGridScenarios.KeyDown += ProcessFilterForm_KeyDown;
            dataGridCommands.KeyDown += ProcessFilterForm_KeyDown;
            ProcessesButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            OperationButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            //ScenariosButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            //CommnadsButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            FilterButton.KeyDown += ProcessFilterForm_KeyDown;
            KeyDown += ProcessFilterForm_KeyDown;

            dataGridScenarios.CellFormatting += DataGridScenariosResult_CellFormatting;
            Closing += (s, e) => SaveSerializationFile();

            _spaFilter = new SPAProcessFilter();
        }


        private void DataGridScenariosResult_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewRow row = dataGridScenarios.Rows[e.RowIndex];
            row.DefaultCellStyle.BackColor = row.Cells[0].Value.ToString() == "-1" ? Color.Yellow : Color.White;
        }

        private void dataGridProcessesResults_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridProcesses);
        }
        private void DataGridProcessesResults_KeyDown(object sender, KeyEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridProcesses, e);
        }

        private void DataGridOperationsResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridOperations);
        }
        private void DataGridOperationsResult_KeyDown(object sender, KeyEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridOperations, e);
        }

        private void DataGridScenariosResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridScenarios);
        }
        private void DataGridScenariosResult_KeyDown(object sender, KeyEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridScenarios, e);
        }

        private void DataGridCommandsResult_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
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
                var altIsDown = (ModifierKeys & Keys.Alt) != 0;
                var f4IsDown = KeyIsDown(Keys.F4);

                if (altIsDown && f4IsDown)
                {
                    Close();
                }
                else if (e.KeyCode == Keys.Enter || (!altIsDown && f4IsDown))
                {
                    if (!GetFilePathCurrentRow(grid, out var filePath1))
                        return;

                    foreach (var filePath in filePath1)
                    {
                        OpenEditor(filePath);
                    }
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    if (!GetFilePathCurrentRow(grid, out var filesPath))
                        return;
                    if(filesPath.Count == 0)
                        return;
                    
                    var userResult = MessageBox.Show($"Do you really want to delete {(filesPath.Count == 1 ? $"the file" : $"{filesPath.Count} files")} ?", @"Question", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                    if (userResult != DialogResult.OK)
                        return;

                    try
                    {
                        foreach (var filePath in filesPath)
                        {
                            if (File.Exists(filePath))
                                File.Delete(filePath);
                        }

                        StatusRefresh();
                        FilterButton_Click(this, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                
            }
            else
            {
                if (!GetFilePathCurrentRow(grid, out var filesPath))
                    return;

                foreach (var filePath in filesPath)
                {
                    OpenEditor(filePath);
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
        private static bool KeyIsDown(Keys key)
        {
            return (GetAsyncKeyState(key) < 0);
        }

        static bool GetFilePathCurrentRow(DataGridView grid, out List<string> filesPath)
        {
            filesPath = new List<string>();
            if (grid.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in grid.SelectedRows)
                {
                    filesPath.Add(row.Cells[grid.ColumnCount - 1].Value.ToString());
                }
            }
            else
            {
                MessageBox.Show(@"You must select a row.", @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }


        private void ProcessesButtonOpen_Click(object sender, EventArgs e)
        {
            ProcessesTextBox.Text = OpenFolder(_lastDirPath) ?? ProcessesTextBox.Text;
            //AssignProcesses(ProcessesTextBox.Text);
        }

        private void ProcessesTextBox_TextChanged(object sender, EventArgs e)
        {
            AssignProcesses(ProcessesTextBox.Text);
            SaveSerializationFile();
        }

        void AssignProcesses(string dirPath, bool saveLastSett = false)
        {
            try
            {
                var lastSettProcess = saveLastSett ? ProcessesComboBox.Text : null;

                if (_spaFilter.Assign(dirPath, SPAProcessFilterType.Processes))
                {
                    UpdateLastPath(dirPath);
                    ProcessesComboBox.DataSource = _spaFilter.Processes.Select(p => p.Name).ToList();
                    ProcessesComboBox.Text = lastSettProcess;
                    ProcessesComboBox.DisplayMember = lastSettProcess;
                }
                else
                {
                    dataGridProcesses.DataSource = null;
                    dataGridProcesses.Refresh();
                    dataGridOperations.DataSource = null;
                    dataGridOperations.Refresh();
                    dataGridSCOperations.DataSource = null;
                    dataGridSCOperations.Refresh();
                    dataGridScenarios.DataSource = null;
                    dataGridScenarios.Refresh();
                    dataGridCommands.DataSource = null;
                    dataGridCommands.Refresh();

                    ProcessesComboBox.DataSource = null;
                    ProcessesComboBox.Text = null;
                    ProcessesComboBox.DisplayMember = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                StatusRefresh();
            }
        }

        private void OperationButtonOpen_Click(object sender, EventArgs e)
        {
            OperationTextBox.Text = OpenFolder(_lastDirPath) ?? OperationTextBox.Text;
            //AssignOperations(OperationTextBox.Text);
        }
        private void OperationTextBox_TextChanged(object sender, EventArgs e)
        {
            AssignOperations(OperationTextBox.Text, SPAProcessFilterType.NetElements);
            SaveSerializationFile();
        }

        private bool _serviceCatalogTextBoxChanged = false;
        private void ServiceCatalogOpenButton_Click(object sender, EventArgs e)
        {
            ServiceCatalogTextBox.Text = OpenFile(@"(*.xml) | *.xml") ?? ServiceCatalogTextBox.Text;
            AssignOperations(ServiceCatalogTextBox.Text, SPAProcessFilterType.ServiceCatalogOperations);
            _serviceCatalogTextBoxChanged = false;
        }

        private void ServiceCatalogTextBox_LostFocus(object sender, System.EventArgs e)
        {
            if (!_serviceCatalogTextBoxChanged)
                return;

            AssignOperations(ServiceCatalogTextBox.Text, SPAProcessFilterType.ServiceCatalogOperations);
            SaveSerializationFile();
        }

        private void ServiceCatalogTextBox_TextChanged(object sender, EventArgs e)
        {
            _serviceCatalogTextBoxChanged = true;
        }

        void AssignOperations(string dirPath, SPAProcessFilterType type, bool saveLastSett = false)
        {
            try
            {
                var lastSettNetSett = saveLastSett ? NetSettComboBox.Text : null;
                var lastSettOper = saveLastSett ? OperationComboBox.Text : null;

                if (_spaFilter.Assign(dirPath, type))
                {
                    UpdateLastPath(dirPath);
                    NetSettComboBox.DataSource = _spaFilter.NetElements.AllNetworkElements;
                    NetSettComboBox.Text = lastSettNetSett;
                    NetSettComboBox.DisplayMember = lastSettNetSett;
                    OperationComboBox.DataSource = _spaFilter.NetElements.AllOperationsName;
                    OperationComboBox.Text = lastSettOper;
                    OperationComboBox.DisplayMember = lastSettOper;
                }
                else
                {
                    dataGridOperations.DataSource = null;
                    dataGridOperations.Refresh();
                    dataGridScenarios.DataSource = null;
                    dataGridScenarios.Refresh();
                    dataGridCommands.DataSource = null;
                    dataGridCommands.Refresh();

                    NetSettComboBox.Text = null;
                    NetSettComboBox.DisplayMember = null;
                    NetSettComboBox.DataSource = null;
                    OperationComboBox.DataSource = null;
                    OperationComboBox.DisplayMember = null;
                    OperationComboBox.Text = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                StatusRefresh();
            }
        }


        private void AddActivatorButton_Click(object sender, EventArgs e)
        {

        }

        private void RemoveActivatorButton_Click(object sender, EventArgs e)
        {

        }
        
        private void ProcessFilterForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    FilterButton_Click(this, EventArgs.Empty);
                    break;
            }
        }

        private async void PrintXMLButton_Click(object sender, EventArgs e)
        {
            var filesNumber = _spaFilter.WholeItemsCount;

            if (filesNumber <= 0)
            {
                MessageBox.Show(@"You must filter files.", @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        await Task.Factory.StartNew(() => _spaFilter.PrintXML(stringErrors, progrAsync));
                    }

                    if (stringErrors.Length > 0)
                    {
                        MessageBox.Show($"Several ({stringErrors.Lines}) errors found:\r\n\r\n{stringErrors.ToString(2)}", @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show(@"Successfully completed.", @"OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                IsInProgress = false;
            }
        }

        private async void FilterButton_Click(object sender, EventArgs e)
        {
            if (IsInProgress)
                return;

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

                    await Task.Factory.StartNew(() => _spaFilter.DataFilter(filterProcess, filterNE, filterOp, progressCalc));

                    await Task.Factory.StartNew(() =>
                    {
                        progressBar.Invoke(new MethodInvoker(delegate
                        {
                            dataGridProcesses.AssignListToDataGrid(_spaFilter.Processes, new Padding(0, 0, 15, 0));
                            progressCalc.Append(2);

                            dataGridOperations.AssignListToDataGrid(_spaFilter.NetElements, new Padding(0, 0, 15, 0));
                            progressCalc.Append(2);

                            dataGridScenarios.AssignListToDataGrid(_spaFilter.Scenarios, new Padding(0, 0, 15, 0));
                            dataGridCommands.AssignListToDataGrid(_spaFilter.Commands, new Padding(0, 0, 15, 0));
                            progressCalc.Append(2);
                        }));
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                IsInProgress = false;
                StatusRefresh();
            }
        }

        private void ExportSCPath_TextChanged(object sender, EventArgs e)
        {
            StatusRefresh();
            SaveSerializationFile();
        }

        private void OpenSCXlsx_TextChanged(object sender, EventArgs e)
        {
            SaveSerializationFile();
        }

        private void RootSCExportPathButton_Click(object sender, EventArgs e)
        {
            ExportSCPath.Text = OpenFolder(_lastDirPath) ?? ExportSCPath.Text;
        }

        private void OpenRDServiceExelButton_Click(object sender, EventArgs e)
        {
            OpenSCXlsx.Text = OpenFile(@"(*.xlsx) | *.xlsx") ?? OpenSCXlsx.Text;
        }

        private async void ButtonGenerateSC_Click(object sender, EventArgs e)
        {
            if (IsInProgress)
                return;

            if (_spaFilter.NetElements.Count == 0)
            {
                MessageBox.Show(@"Not found any operations.", @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                IsInProgress = true;

                if (!Directory.Exists(ExportSCPath.Text))
                    Directory.CreateDirectory(ExportSCPath.Text);

                string fileResult;

                if (!OpenSCXlsx.Text.IsNullOrEmptyTrim() && File.Exists(OpenSCXlsx.Text))
                {
                    var file = new FileInfo(OpenSCXlsx.Text);
                    
                    using (var progrAsync = new CustomProgressCalculation(progressBar, _spaFilter.NetElements.Count, file))
                    {
                        var rdServices = await Task<DataTable>.Factory.StartNew(() => _spaFilter.GetRDServicesFromXslx(file, progrAsync));

                        fileResult = await Task.Factory.StartNew(() => _spaFilter.GetServiceCatalog(rdServices, ExportSCPath.Text, progrAsync));
                    }
                }
                else
                {
                    using (var progrAsync = new CustomProgressCalculation(progressBar, _spaFilter.NetElements.Count))
                    {
                        fileResult = await Task.Factory.StartNew(() => _spaFilter.GetServiceCatalog( null, ExportSCPath.Text, progrAsync));
                    }
                }

                if (!fileResult.IsNullOrEmpty() && File.Exists(fileResult))
                {
                    OpenEditor(fileResult);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                IsInProgress = false;
            }
        }


        void OpenEditor(string path)
        {
            try
            {
                if (_notepad == null || _notepad.WindowIsClosed)
                {
                    if (_notepadLocation == FormLocation.Default)
                        _notepadLocation = new FormLocation(this);

                    _notepad = new Notepad(path)
                    {
                        Location = _notepadLocation.Location,
                        WindowState = _notepadWindowsState,
                        WordWrap = _notepadWordWrap
                    };
                    _notepad.Closed += _notepad_Closed;
                    _notepad.Show();
                }
                else
                {
                    _notepad.AddDocument(path);
                    _notepad.Focus();
                    _notepad.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void _notepad_Closed(object sender, EventArgs e)
        {
            if (!(sender is Notepad notepad))
                return;

            try
            {
                _notepadLocation = new FormLocation(notepad);
                _notepadWordWrap = notepad.WordWrap;
                _notepadWindowsState = notepad.WindowState;
            }
            catch (Exception)
            {
                // null
            }
        }

        void StatusRefresh()
        {
            BPCount.Text = $"Processes: {(_spaFilter.Processes == null ? 0 : _spaFilter.Processes.Count).ToString()}";
            NEElementsCount.Text = $"NEs: {(_spaFilter.NetElements == null ? 0 : _spaFilter.NetElements.AllNetworkElements.Count).ToString()}";
            OperationsCount.Text = $"Operations: {(_spaFilter.NetElements == null ? 0 : _spaFilter.NetElements.AllOperationsName.Count).ToString()}";
            ScenariosCount.Text = $"Scenarios: {(_spaFilter.Scenarios == null ? 0 : _spaFilter.Scenarios.Count).ToString()}";
            CommandsCount.Text = $"Commands: {(_spaFilter.Commands == null ? 0 : _spaFilter.Commands.Count).ToString()}";

            FilterButton.Enabled = _spaFilter.IsEnabledFilter;
            ButtonGenerateSC.Enabled = _spaFilter.CanGenerateSC && !ExportSCPath.Text.IsNullOrEmpty() && ExportSCPath.Text.Length > 3;
            PrintXMLButton.Enabled = _spaFilter.WholeItemsCount > 0;
        }

        void UpdateLastPath(string path)
        {
            if (!path.IsNullOrEmpty())
                _lastDirPath = path;
        }

        void SaveSerializationFile()
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

        static string OpenFile(string extension)
        {
            try
            {
                using (var fbd = new OpenFileDialog())
                {
                    fbd.Filter = extension;
                    var result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName) && File.Exists(fbd.FileName))
                    {
                        return fbd.FileName;
                    }
                }
                return null;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        static string OpenFolder(string lastDir)
        {
            try
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    if (!lastDir.IsNullOrEmpty())
                        fbd.SelectedPath = lastDir;

                    var result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath) && Directory.Exists(fbd.SelectedPath))
                    {
                        return fbd.SelectedPath;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}