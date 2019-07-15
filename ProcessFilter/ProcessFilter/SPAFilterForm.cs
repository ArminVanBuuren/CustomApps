using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using SPAFilter.SPA;
using SPAFilter.SPA.Collection;
using SPAFilter.SPA.Components;
using Utils;
using Utils.WinForm.DataGridViewHelper;
using Utils.WinForm.Notepad;
using Utils.WinForm.CustomProgressBar;
using Utils.WinForm.Handles;
using static Utils.ASSEMBLY;

namespace SPAFilter
{
    public enum SPAProcessFilterType
    {
        Processes = 0,
        ROBPOperations = 1,
        SCOperations = 2,
        Activators_Add = 4,
        Activators_Remove = 8
    }

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
                    ROBPOperationTextBox.Enabled = !_isInProgress;
                    ServiceCatalogTextBox.Enabled = !_isInProgress;
                    ProcessesButtonOpen.Enabled = !_isInProgress;
                    ROBPOperationButtonOpen.Enabled = !_isInProgress;
                    ServiceCatalogOpenButton.Enabled = !_isInProgress;

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
                ProcessesTextBox.Text = propertyBag.GetString("ADWFFW") ?? string.Empty;
                ROBPOperationTextBox.Text = propertyBag.GetString("AAEERF") ?? string.Empty;
                ServiceCatalogTextBox.Text = propertyBag.GetString("DFWDRT") ?? string.Empty;
                ExportSCPath.Text = propertyBag.GetString("FFFGHJ") ?? string.Empty;
                OpenSCXlsx.Text = propertyBag.GetString("GGHHRR") ?? string.Empty;
                _lastDirPath = propertyBag.GetString("GHDDSD") ?? string.Empty;
                if (propertyBag.GetValue("WWWERT", typeof(List<string>)) is List<string> siConfigsList)
                    AssignServiceInstanes(siConfigsList);
                _notepadWordWrap = propertyBag.GetBoolean("DDCCVV");
                _notepadLocation = (FormLocation) propertyBag.GetValue("RRTTDD", typeof(FormLocation));
                _notepadWindowsState = (FormWindowState) propertyBag.GetValue("SSEEFF", typeof(FormWindowState));
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

        async void AssignServiceInstanes(List<string> siConfigsList)
        {
            foreach (var fileConfig in siConfigsList)
            {
                await _spaFilter.AssignActivatorAsync(fileConfig);
            }

            if (_spaFilter.ServiceInstances != null)
            {
                dataGridServiceInstances.AssignListToDataGrid(_spaFilter.ServiceInstances, new Padding(0, 0, 15, 0));
            }
        }

        void ISerializable.GetObjectData(SerializationInfo propertyBag, StreamingContext context)
        {
            propertyBag.AddValue("ADWFFW", ProcessesTextBox.Text);
            propertyBag.AddValue("AAEERF", ROBPOperationTextBox.Text);
            propertyBag.AddValue("DFWDRT", ServiceCatalogTextBox.Text);
            propertyBag.AddValue("FFFGHJ", ExportSCPath.Text);
            propertyBag.AddValue("GGHHRR", OpenSCXlsx.Text);
            propertyBag.AddValue("GHDDSD", _lastDirPath);
            if (_spaFilter.ServiceInstances != null)
            {
                List<string> filesConfigs = _spaFilter.ServiceInstances.Select(x => x.FilePath).ToList();
                propertyBag.AddValue("WWWERT", filesConfigs);
            }

            propertyBag.AddValue("DDCCVV", _notepadWordWrap);
            propertyBag.AddValue("RRTTDD", _notepadLocation);
            propertyBag.AddValue("SSEEFF", _notepadWindowsState);
        }

        void MainInit()
        {
            InitializeComponent();

            dataGridProcesses.KeyDown += DataGridProcessesResults_KeyDown;
            dataGridOperations.KeyDown += DataGridOperationsResult_KeyDown;
            dataGridServiceInstances.KeyDown += DataGridServiceInstances_KeyDown;
            dataGridScenarios.KeyDown += DataGridScenariosResult_KeyDown;
            dataGridCommands.KeyDown += DataGridCommandsResult_KeyDown;

            tabControl1.KeyDown += ProcessFilterForm_KeyDown;
            ProcessesTextBox.KeyDown += ProcessFilterForm_KeyDown;
            ROBPOperationTextBox.KeyDown += ProcessFilterForm_KeyDown;
            ProcessesComboBox.KeyDown += ProcessFilterForm_KeyDown;
            NetSettComboBox.KeyDown += ProcessFilterForm_KeyDown;
            OperationComboBox.KeyDown += ProcessFilterForm_KeyDown;
            dataGridProcesses.KeyDown += ProcessFilterForm_KeyDown;
            dataGridOperations.KeyDown += ProcessFilterForm_KeyDown;
            dataGridServiceInstances.KeyDown += ProcessFilterForm_KeyDown;
            dataGridScenarios.KeyDown += ProcessFilterForm_KeyDown;
            dataGridCommands.KeyDown += ProcessFilterForm_KeyDown;
            ProcessesButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            ROBPOperationButtonOpen.KeyDown += ProcessFilterForm_KeyDown;
            FilterButton.KeyDown += ProcessFilterForm_KeyDown;
            KeyDown += ProcessFilterForm_KeyDown;

            dataGridScenarios.CellFormatting += DataGridScenariosResult_CellFormatting;
            Closing += (s, e) => SaveData();

            _spaFilter = new SPAProcessFilter();
        }


        private void DataGridScenariosResult_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var row = dataGridScenarios.Rows[e.RowIndex];
            row.DefaultCellStyle.BackColor = row.Cells[0].Value.ToString() == "-1" ? Color.Yellow : Color.White;
        }

        #region DataGrid hot keys and Row double click

        private void DataGridProcessesResults_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
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

        private void DataGridServiceInstances_DoubleClick(object sender, EventArgs e)
        {
            CallAndCheckDataGridKey(dataGridServiceInstances);
        }
        private void DataGridServiceInstances_KeyDown(object sender, KeyEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridServiceInstances, e);
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

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
        private static bool KeyIsDown(Keys key)
        {
            return (GetAsyncKeyState(key) < 0);
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
                    
                    var userResult = MessageBox.Show($"Do you want delete {(filesPath.Count == 1 ? $"the file" : $"{filesPath.Count} files")} ?", @"Question", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                    if (userResult != DialogResult.OK)
                        return;

                    try
                    {
                        foreach (var filePath in filesPath)
                        {
                            if (File.Exists(filePath))
                                File.Delete(filePath);
                        }

                        RefreshStatus();
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

        #endregion

        private void ProcessesButtonOpen_Click(object sender, EventArgs e)
        {
            if (OpenFolder(_lastDirPath, out var filePath))
                ProcessesTextBox.Text = filePath;
        }

        private void ProcessesTextBox_TextChanged(object sender, EventArgs e)
        {
            Assign(SPAProcessFilterType.Processes);
        }


        private void ROBPOperationButtonOpen_Click(object sender, EventArgs e)
        {
            if (OpenFolder(_lastDirPath, out var filePath))
                ROBPOperationTextBox.Text = filePath;
        }

        private void ROBPOperationTextBox_TextChanged(object sender, EventArgs e)
        {
            Assign(SPAProcessFilterType.ROBPOperations);
        }

        private void ROBPOperationsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ServiceCatalogRadioButton.Checked = !ROBPOperationsRadioButton.Checked;
            ServiceCatalogTextBox.Enabled = ServiceCatalogRadioButton.Checked;
            ServiceCatalogOpenButton.Enabled = ServiceCatalogRadioButton.Checked;
            ROBPOperationTextBox.Enabled = ROBPOperationsRadioButton.Checked;
            ROBPOperationButtonOpen.Enabled = ROBPOperationsRadioButton.Checked;
            Assign(ROBPOperationsRadioButton.Checked ? SPAProcessFilterType.ROBPOperations : SPAProcessFilterType.SCOperations);
        }

        private void ServiceCatalogRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ROBPOperationsRadioButton.Checked = !ServiceCatalogRadioButton.Checked;
            ROBPOperationTextBox.Enabled = ROBPOperationsRadioButton.Checked;
            ROBPOperationButtonOpen.Enabled = ROBPOperationsRadioButton.Checked;
            ServiceCatalogTextBox.Enabled = ServiceCatalogRadioButton.Checked;
            ServiceCatalogOpenButton.Enabled = ServiceCatalogRadioButton.Checked;
            Assign(ROBPOperationsRadioButton.Checked ? SPAProcessFilterType.ROBPOperations : SPAProcessFilterType.SCOperations);
        }

        private bool _serviceCatalogTextBoxChanged = false;
        private void ServiceCatalogOpenButton_Click(object sender, EventArgs e)
        {
            ServiceCatalogTextBox.Text = OpenFile(@"(*.xml) | *.xml") ?? ServiceCatalogTextBox.Text;
            Assign(SPAProcessFilterType.SCOperations);
            _serviceCatalogTextBoxChanged = false;
        }

        private void ServiceCatalogTextBox_LostFocus(object sender, System.EventArgs e)
        {
            if (!_serviceCatalogTextBoxChanged)
                return;

            Assign(SPAProcessFilterType.SCOperations);
        }

        private void ServiceCatalogTextBox_TextChanged(object sender, EventArgs e)
        {
            _serviceCatalogTextBoxChanged = true;
        }

        private void AddActivatorButton_Click(object sender, EventArgs e)
        {
            Assign(SPAProcessFilterType.Activators_Add);
        }

        private void RemoveActivatorButton_Click_1(object sender, EventArgs e)
        {
            Assign(SPAProcessFilterType.Activators_Remove);
        }

        async void Assign(SPAProcessFilterType type, bool saveLastSett = false)
        {
            try
            {
                switch (type)
                {
                    case SPAProcessFilterType.Processes when !ProcessesTextBox.Text.IsNullOrEmptyTrim():
                        await _spaFilter.AssignProcessesAsync(ProcessesTextBox.Text);

                        ProcessesComboBox.DataSource = _spaFilter.Processes.Select(p => p.Name).ToList();
                        ProcessesComboBox.Text = saveLastSett ? ProcessesComboBox.Text : null;
                        ProcessesComboBox.DisplayMember = saveLastSett ? ProcessesComboBox.Text : null;

                        UpdateLastPath(ProcessesTextBox.Text);
                        break;
                    case SPAProcessFilterType.SCOperations:
                    case SPAProcessFilterType.ROBPOperations:

                        if (type == SPAProcessFilterType.SCOperations && !ServiceCatalogTextBox.Text.IsNullOrEmptyTrim())
                        {
                            await _spaFilter.AssignSCOperationsAsync(ServiceCatalogTextBox.Text);
                            UpdateLastPath(Path.GetDirectoryName(ServiceCatalogTextBox.Text));
                        }
                        else if (!ROBPOperationTextBox.Text.IsNullOrEmptyTrim())
                        {
                            await _spaFilter.AssignROBPOperationsAsync(ROBPOperationTextBox.Text);
                            UpdateLastPath(ROBPOperationTextBox.Text);
                        }

                        if (_spaFilter.Operations != null)
                        {
                            NetSettComboBox.DataSource = _spaFilter.Operations.AllHostTypes;
                            NetSettComboBox.Text = saveLastSett ? NetSettComboBox.Text : null;
                            NetSettComboBox.DisplayMember = saveLastSett ? NetSettComboBox.Text : null;
                            OperationComboBox.DataSource = _spaFilter.Operations.AllOperationsName;
                            OperationComboBox.Text = saveLastSett ? OperationComboBox.Text : null;
                            OperationComboBox.DisplayMember = saveLastSett ? OperationComboBox.Text : null;
                        }
                        else
                        {
                            ClearOperationsComboBox();
                        }
                        break;
                    case SPAProcessFilterType.Activators_Add:
                        var fileConfig = OpenFile(@"(configuration.application.xml) | *.xml");
                        await _spaFilter.AssignActivatorAsync(fileConfig);
                        AssignActivator();
                        UpdateLastPath(fileConfig);
                        break;
                    case SPAProcessFilterType.Activators_Remove:
                        if (GetFilePathCurrentRow(dataGridServiceInstances, out var listFiles))
                        {
                            await _spaFilter.RemoveActivatorAsync(listFiles);
                            AssignActivator();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                ClearDataGrid();

                switch (type)
                {
                    case SPAProcessFilterType.Processes:
                        ProcessesTextBox.Text = string.Empty;
                        ProcessesComboBox.DataSource = null;
                        ProcessesComboBox.Text = null;
                        ProcessesComboBox.DisplayMember = null;
                        break;
                    case SPAProcessFilterType.ROBPOperations:
                        ROBPOperationTextBox.Text = string.Empty;
                        ClearOperationsComboBox();
                        break;
                    case SPAProcessFilterType.SCOperations:
                        ServiceCatalogTextBox.Text = string.Empty;
                        ClearOperationsComboBox();
                        break;
                }
            }
            finally
            {
                RefreshStatus();
                SaveData();
            }
        }

        void AssignActivator()
        {
            if (_spaFilter.ServiceInstances != null)
            {
                dataGridServiceInstances.AssignListToDataGrid(_spaFilter.ServiceInstances, new Padding(0, 0, 15, 0));
            }
            else
            {
                dataGridServiceInstances.DataSource = null;
                dataGridServiceInstances.Refresh();
            }
        }

        void ClearOperationsComboBox()
        {
            NetSettComboBox.Text = null;
            NetSettComboBox.DisplayMember = null;
            NetSettComboBox.DataSource = null;
            OperationComboBox.DataSource = null;
            OperationComboBox.DisplayMember = null;
            OperationComboBox.Text = null;
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

        private async void FilterButton_Click(object sender, EventArgs e)
        {
            if (IsInProgress)
                return;

            try
            {
                IsInProgress = true;

                var filterProcess = ProcessesComboBox.Text;
                var filterNE = NetSettComboBox.Text;
                var filterOp = OperationComboBox.Text;
                var filterInROBP = ROBPOperationsRadioButton.Checked;
                var syncContext = SynchronizationContext.Current;

                ClearDataGrid();

                using (var progressCalc = new ProgressCalculationAsync(progressBar, 20))
                {
                    await Task.Factory.StartNew(() => _spaFilter.DataFilter(filterProcess, filterNE, filterOp, filterInROBP, progressCalc));

                    await Task.Factory.StartNew(() => syncContext.Post(delegate
                    {
                        dataGridProcesses.AssignListToDataGrid(_spaFilter.Processes, new Padding(0, 0, 15, 0));
                        progressCalc.Append(2);

                        dataGridOperations.AssignListToDataGrid(_spaFilter.Operations.AllOperations, new Padding(0, 0, 15, 0));
                        progressCalc.Append(2);

                        dataGridServiceInstances.AssignListToDataGrid(_spaFilter.ServiceInstances, new Padding(0, 0, 15, 0));

                        dataGridScenarios.AssignListToDataGrid(_spaFilter.Scenarios, new Padding(0, 0, 15, 0));
                        progressCalc.Append(2);

                        dataGridCommands.AssignListToDataGrid(_spaFilter.Commands, new Padding(0, 0, 15, 0));
                        progressCalc.Append(2);
                    }, null));

                    //await Task.Factory.StartNew(() =>
                    //{
                    //    progressBar.Invoke(new MethodInvoker(delegate
                    //    {
                            
                    //    }));
                    //});
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                IsInProgress = false;
                RefreshStatus();
            }
        }

        private void ExportSCPath_TextChanged(object sender, EventArgs e)
        {
            RefreshStatus();
            SaveData();
        }

        private void OpenSCXlsx_TextChanged(object sender, EventArgs e)
        {
            SaveData();
        }

        private void RootSCExportPathButton_Click(object sender, EventArgs e)
        {
            if (OpenFolder(_lastDirPath, out var filePath))
                ExportSCPath.Text = filePath;
        }

        private void OpenRDServiceExelButton_Click(object sender, EventArgs e)
        {
            OpenSCXlsx.Text = OpenFile(@"(*.xlsx) | *.xlsx") ?? OpenSCXlsx.Text;
        }

        private async void ButtonGenerateSC_Click(object sender, EventArgs e)
        {
            if (IsInProgress)
                return;

            if (_spaFilter.Operations.Count == 0)
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
                    using (var progrAsync = new CustomProgressCalculation(progressBar, _spaFilter.Operations.Count, file))
                    {
                        var rdServices = await Task<DataTable>.Factory.StartNew(() => _spaFilter.GetRDServicesFromXslx(file, progrAsync));
                        fileResult = await Task.Factory.StartNew(() => _spaFilter.GetServiceCatalog(rdServices, ExportSCPath.Text, progrAsync));
                    }
                }
                else
                {
                    using (var progrAsync = new CustomProgressCalculation(progressBar, _spaFilter.Operations.Count))
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

        void ClearDataGrid()
        {
            dataGridProcesses.DataSource = null;
            dataGridOperations.DataSource = null;
            dataGridScenarios.DataSource = null;
            dataGridCommands.DataSource = null;
            dataGridProcesses.Refresh();
            dataGridOperations.Refresh();
            dataGridScenarios.Refresh();
            dataGridCommands.Refresh();
        }

        void RefreshStatus()
        {
            BPCount.Text = $"Processes: {(_spaFilter.Processes == null ? 0 : _spaFilter.Processes.Count).ToString()}";
            NEElementsCount.Text = $"HostTypes: {(_spaFilter.Operations == null ? 0 : _spaFilter.Operations.AllHostTypes.Count).ToString()}";
            OperationsCount.Text = $"Operations: {(_spaFilter.Operations == null ? 0 : _spaFilter.Operations.AllOperationsName.Count).ToString()}";
            ScenariosCount.Text = $"Scenarios: {(_spaFilter.Scenarios == null ? 0 : _spaFilter.Scenarios.Count).ToString()}";
            CommandsCount.Text = $"Commands: {(_spaFilter.Commands == null ? 0 : _spaFilter.Commands.Count).ToString()}";

            FilterButton.Enabled = _spaFilter.IsEnabledFilter;
            PrintXMLButton.Enabled = _spaFilter.WholeItemsCount > 0;
            ButtonGenerateSC.Enabled = _spaFilter.CanGenerateSC && !ExportSCPath.Text.IsNullOrEmpty() && ExportSCPath.Text.Length > 3 && ROBPOperationsRadioButton.Checked;
        }

        void UpdateLastPath(string path)
        {
            if (!path.IsNullOrEmpty())
                _lastDirPath = path;
        }

        void SaveData()
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

        static bool OpenFolder(string lastDir, out string result)
        {
            result = null;
            try
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    if (!lastDir.IsNullOrEmpty())
                        fbd.SelectedPath = lastDir;

                    if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath) && Directory.Exists(fbd.SelectedPath))
                    {
                        result = fbd.SelectedPath;
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
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







        //private void AddActivatorButton_Paint(object sender, PaintEventArgs e)
        //{
        //    DrawButton(AddActivatorButton, Properties.Resources.icons8_plus_20, e);
        //}

        //private void RemoveActivatorButton_Paint_1(object sender, PaintEventArgs e)
        //{
        //    DrawButton(RemoveActivatorButton, Properties.Resources.icons8_minus_20, e);
        //}

        //void DrawButton(Button button, Bitmap image, PaintEventArgs e)
        //{
        //    image.MakeTransparent(Color.White);
        //    int x = (button.Width - image.Width) / 2;
        //    int y = (button.Height - image.Height) / 2;
        //    e.Graphics.DrawImage(image, x, y);
        //    button.TabStop = false;
        //    button.FlatStyle = FlatStyle.Flat;
        //    button.FlatAppearance.BorderSize = 0;
        //}

    }
}