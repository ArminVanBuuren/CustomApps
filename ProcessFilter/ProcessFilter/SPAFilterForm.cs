using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using FastColoredTextBoxNS;
using SPAFilter.SPA;
using SPAFilter.SPA.Components.ROBP;
using SPAFilter.SPA.Components.SC;
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
        readonly object _sync = new object();

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
        private bool IsFiltered { get; set; } = false;
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
                    ProcessesButtonOpen.Enabled = !_isInProgress;

                    ROBPOperationsRadioButton.Enabled = !_isInProgress;
                    ServiceCatalogRadioButton.Enabled = !_isInProgress;

                    if (ROBPOperationsRadioButton.Checked)
                    {
                        ROBPOperationTextBox.Enabled = !_isInProgress;
                        ROBPOperationButtonOpen.Enabled = !_isInProgress;
                    }
                    else
                    {
                        ServiceCatalogTextBox.Enabled = !_isInProgress;
                        ServiceCatalogOpenButton.Enabled = !_isInProgress;
                    }

                    addServiceInstancesButton.Enabled = !_isInProgress;
                    removeServiceInstancesButton.Enabled = !_isInProgress;
                    dataGridServiceInstances.Enabled = !_isInProgress;

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
            PreInit();
            PostInit();
        }

        SPAFilterForm(SerializationInfo propertyBag, StreamingContext context)
        {
            PreInit();
            try
            {
                var allSavedParams = new Dictionary<string, object>();
                foreach (var entry in propertyBag)
                {
                    allSavedParams.Add(entry.Name, entry.Value);
                }

                ProcessesTextBox.Text = (string)TryGetSerializationValue(allSavedParams, "ADWFFW", string.Empty);
                if (!ProcessesTextBox.Text.IsNullOrEmptyTrim())
                {
                    // RunSynchronously - не подходит, т.к. образуется дедлок, потому что асинхронный поток не пытается вернуться назад
                    var task = Task.Run(() => AssignAsync(SPAProcessFilterType.Processes, false));
                    task.Wait();
                }

                ROBPOperationsRadioButton.Checked = (bool)TryGetSerializationValue(allSavedParams, "GGHHTTDD", true);
                ServiceCatalogRadioButton.Checked = !ROBPOperationsRadioButton.Checked;

                ROBPOperationTextBox.Text = (string)TryGetSerializationValue(allSavedParams, "AAEERF", string.Empty);
                ServiceCatalogTextBox.Text = (string)TryGetSerializationValue(allSavedParams, "DFWDRT", string.Empty);

                if (ROBPOperationsRadioButton.Checked)
                {
                    if (!ROBPOperationTextBox.Text.IsNullOrEmptyTrim())
                    {
                        var task = Task.Run(() => AssignAsync(SPAProcessFilterType.ROBPOperations, false));
                        task.Wait();
                    }
                }
                else if(!ServiceCatalogTextBox.Text.IsNullOrEmptyTrim())
                {
                    var task = Task.Run(() => AssignAsync(SPAProcessFilterType.SCOperations, false));
                    task.Wait();
                }

                ExportSCPath.Text = (string)TryGetSerializationValue(allSavedParams, "FFFGHJ", string.Empty);
                OpenSCXlsx.Text = (string)TryGetSerializationValue(allSavedParams, "GGHHRR", string.Empty);
                _lastDirPath = (string)TryGetSerializationValue(allSavedParams, "GHDDSD", string.Empty);

                AssignServiceInstanes((List<string>)TryGetSerializationValue(allSavedParams, "WWWERT", null));

                _notepadWordWrap = (bool)TryGetSerializationValue(allSavedParams, "DDCCVV", true);
                _notepadLocation = (FormLocation)TryGetSerializationValue(allSavedParams, "RRTTDD", FormLocation.Default);
                _notepadWindowsState = (FormWindowState)TryGetSerializationValue(allSavedParams, "SSEEFF", FormWindowState.Maximized);
            }
            catch (Exception ex)
            {
                // ignored
            }
            finally
            {
                IsInitialization = false;
                PostInit();
            }
        }

        static object TryGetSerializationValue(IReadOnlyDictionary<string, object> allSavedParams, string key, object defaultResult)
        {
            return allSavedParams.TryGetValue(key, out var res) ? res : defaultResult;
        }

        async void AssignServiceInstanes(IEnumerable<string> siConfigsList)
        {
            if(siConfigsList == null)
                return;

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

            propertyBag.AddValue("GGHHTTDD", ROBPOperationsRadioButton.Checked);

            if (_spaFilter.ServiceInstances != null)
            {
                var filesConfigs = _spaFilter.ServiceInstances.Select(x => x.FilePath).ToList();
                propertyBag.AddValue("WWWERT", filesConfigs);
            }

            propertyBag.AddValue("DDCCVV", _notepadWordWrap);
            propertyBag.AddValue("RRTTDD", _notepadLocation);
            propertyBag.AddValue("SSEEFF", _notepadWindowsState);
        }

        void PreInit()
        {
            InitializeComponent();
            KeyPreview = true; // для того чтобы работали горячие клавиши по всей форме и всем контролам
            new ToolTip().SetToolTip(PrintXMLButton, "Format all filtered xml files");
            
            _spaFilter = new SPAProcessFilter();

            ROBPOperationsRadioButton.CheckedChanged += ROBPOperationsRadioButton_CheckedChanged;
            ServiceCatalogRadioButton.CheckedChanged += ServiceCatalogRadioButton_CheckedChanged;
        }

        void PostInit()
        {
            dataGridServiceInstances.KeyDown += DataGridServiceInstances_KeyDown;
            dataGridProcesses.KeyDown += DataGridProcessesResults_KeyDown;
            dataGridOperations.KeyDown += DataGridOperationsResult_KeyDown;
            dataGridScenarios.KeyDown += DataGridScenariosResult_KeyDown;
            dataGridCommands.KeyDown += DataGridCommandsResult_KeyDown;

            KeyDown += ProcessFilterForm_KeyDown;

            dataGridServiceInstances.CellFormatting += DataGridServiceInstances_CellFormatting;
            dataGridProcesses.CellFormatting += DataGridProcesses_CellFormatting;
            dataGridOperations.CellFormatting += DataGridOperations_CellFormatting;
            dataGridScenarios.CellFormatting += DataGridScenariosResult_CellFormatting;

            ProcessesButtonOpen.Click += ProcessesButtonOpen_Click;
            ProcessesTextBox.TextChanged += ProcessesTextBox_TextChanged;
            ProcessesTextBox.LostFocus += ProcessesTextBox_LostFocus;

            ROBPOperationButtonOpen.Click += ROBPOperationButtonOpen_Click;
            ROBPOperationTextBox.TextChanged += ROBPOperationTextBox_TextChanged;
            ROBPOperationTextBox.LostFocus += ROBPOperationTextBox_LostFocus;

            ServiceCatalogOpenButton.Click += ServiceCatalogOpenButton_Click;
            ServiceCatalogTextBox.TextChanged += ServiceCatalogTextBox_TextChanged;
            ServiceCatalogTextBox.LostFocus += ServiceCatalogTextBox_LostFocus;

            Closing += (s, e) => SaveData();
        }

        #region Check warning rows

        private static void DataGridServiceInstances_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var row = ((DataGridView)sender).Rows[e.RowIndex];
            var cell = row?.Cells["IsCorrect"];
            if (cell != null && cell.Value is bool cellValue && cellValue)
            {
                row.DefaultCellStyle.BackColor = Color.White;
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.LightPink;
                foreach (DataGridViewCell cell2 in row.Cells)
                {
                    cell2.ToolTipText = "Configuration application is incorrect";
                }
            }
        }

        private void DataGridProcesses_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var row = ((DataGridView)sender).Rows[e.RowIndex];
            if (ROBPOperationsRadioButton.Checked)
            {
                var cell = row?.Cells["AllOperationsExist"];
                if (cell != null && cell.Value is bool cellValue && cellValue)
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.LightPink;
                    foreach (DataGridViewCell cell2 in row.Cells)
                    {
                        cell2.ToolTipText = "Business process has operations that don't exist";
                    }
                }
            }
            else
            {
                var cell = row?.Cells["HasCatalogCall"];
                if (cell != null && cell.Value is bool cellValue && cellValue)
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.LightPink;
                    foreach (DataGridViewCell cell2 in row.Cells)
                    {
                        cell2.ToolTipText = "Business process doesn't have service catalog call";
                    }
                }
            }
        }


        private static void DataGridOperations_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var row = ((DataGridView)sender).Rows[e.RowIndex];
            var cell = row?.Cells["IsScenarioExist"];
            if (cell != null && cell.Value is bool cellValue && cellValue)
            {
                row.DefaultCellStyle.BackColor = Color.White;
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.LightPink;
                foreach (DataGridViewCell cell2 in row.Cells)
                {
                    cell2.ToolTipText = "Scenario for this operation doesn't exist";
                }
            }
        }

        private static void DataGridScenariosResult_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var row = ((DataGridView)sender).Rows[e.RowIndex];
            var cellIsSubScenario = row?.Cells["IsSubScenario"];
            var cellAllCommandsExist = row?.Cells["AllCommandsExist"];
            var isSubScenario = cellIsSubScenario != null && cellIsSubScenario.Value is bool cellValue && cellValue;
            var allCommandsExist = cellAllCommandsExist != null && cellAllCommandsExist.Value is bool cellValue2 && cellValue2;
            if (isSubScenario && !allCommandsExist)
            {
                row.DefaultCellStyle.BackColor = Color.LightPink;
                foreach (DataGridViewCell cell3 in row.Cells)
                {
                    cell3.ToolTipText = "Subscenario has commands that don't exist";
                }
            }
            else if (!allCommandsExist)
            {
                row.DefaultCellStyle.BackColor = Color.LightPink;
                foreach (DataGridViewCell cell3 in row.Cells)
                {
                    cell3.ToolTipText = "Scenario has commands that don't exist";
                }
            }
            else if (isSubScenario)
            {
                row.DefaultCellStyle.BackColor = Color.Aqua;
                foreach (DataGridViewCell cell3 in row.Cells)
                {
                    cell3.ToolTipText = "Subscenario";
                }
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.White;
            }
        }

        #endregion

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
            if (ROBPOperationsRadioButton.Checked)
                CallAndCheckDataGridKey(dataGridOperations);
            else
                OpenServiceCatalogOperation();
        }

        private void DataGridOperationsResult_KeyDown(object sender, KeyEventArgs e)
        {
            if (ROBPOperationsRadioButton.Checked)
                CallAndCheckDataGridKey(dataGridOperations, e);
            else if (e.KeyCode == Keys.Enter)
                OpenServiceCatalogOperation();
        }

        void OpenServiceCatalogOperation()
        {
            if (!GetCellItemSelectedRows(dataGridOperations, out var scOperationNames, "Operation"))
                return;

            var scOperations = _spaFilter.HostTypes.Operations.Where(p => scOperationNames.Any(x => x.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase)));
            foreach (var operation in scOperations)
            {
                if (operation is CatalogOperation catalogOperation)
                    OpenEditor(catalogOperation.Body, catalogOperation.Name);
            }
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
                if ((ModifierKeys & Keys.Alt) != 0 && KeyIsDown(Keys.F4))
                {
                    Close();
                    return;
                }

                switch (e.KeyCode)
                {
                    case Keys.Enter:
                    {
                        if (!GetCellItemSelectedRows(grid, out var filePath1))
                            return;

                        foreach (var filePath in filePath1)
                        {
                            OpenEditor(filePath);
                        }

                        break;
                    }
                    case Keys.Delete:
                    {
                        if (!GetCellItemSelectedRows(grid, out var filesPath))
                            return;
                        if(filesPath.Count == 0)
                            return;
                    
                        var userResult = MessageBox.Show($"Do you want delete selected {(filesPath.Count == 1 ? $"file" : $"{filesPath.Count} files")} ?", @"Question", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

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

                        break;
                    }
                }
            }
            else
            {
                if (!GetCellItemSelectedRows(grid, out var filesPath))
                    return;

                foreach (var filePath in filesPath)
                {
                    OpenEditor(filePath);
                }
            }
        }

        #endregion

        private bool _processesTextBoxChanged = false;

        private async void ProcessesButtonOpen_Click(object sender, EventArgs e)
        {
            if (!OpenFolder(_lastDirPath, out var filePath))
                return;

            ProcessesTextBox.Text = filePath;
            await AssignAsync(SPAProcessFilterType.Processes);
            _processesTextBoxChanged = false;
        }

        private async void ProcessesTextBox_LostFocus(object sender, EventArgs e)
        {
            if (!_processesTextBoxChanged)
                return;

            await AssignAsync(SPAProcessFilterType.Processes);
            _processesTextBoxChanged = false;
        }

        private void ProcessesTextBox_TextChanged(object sender, EventArgs e)
        {
            _processesTextBoxChanged = true;
        }


        private bool _robpOperationTextBoxChanged = false;
        private async void ROBPOperationButtonOpen_Click(object sender, EventArgs e)
        {
            if (!OpenFolder(_lastDirPath, out var filePath))
                return;

            ROBPOperationTextBox.Text = filePath;
            await AssignAsync(SPAProcessFilterType.ROBPOperations);
            _robpOperationTextBoxChanged = false;
        }

        private async void ROBPOperationTextBox_LostFocus(object sender, EventArgs e)
        {
            if (!_robpOperationTextBoxChanged)
                return;

            await AssignAsync(SPAProcessFilterType.ROBPOperations);
            _robpOperationTextBoxChanged = false;
        }

        private void ROBPOperationTextBox_TextChanged(object sender, EventArgs e)
        {
            _robpOperationTextBoxChanged = true;
        }

        private async void ROBPOperationsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ServiceCatalogRadioButton.CheckedChanged -= ServiceCatalogRadioButton_CheckedChanged;

            ServiceCatalogRadioButton.Checked = !ROBPOperationsRadioButton.Checked;
            ServiceCatalogTextBox.Enabled = ServiceCatalogRadioButton.Checked;
            ServiceCatalogOpenButton.Enabled = ServiceCatalogRadioButton.Checked;

            ROBPOperationTextBox.Enabled = ROBPOperationsRadioButton.Checked;
            ROBPOperationButtonOpen.Enabled = ROBPOperationsRadioButton.Checked;

            await AssignOperations();

            ServiceCatalogRadioButton.CheckedChanged += ServiceCatalogRadioButton_CheckedChanged;
        }

        private async void ServiceCatalogRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ROBPOperationsRadioButton.CheckedChanged -= ROBPOperationsRadioButton_CheckedChanged;

            ROBPOperationsRadioButton.Checked = !ServiceCatalogRadioButton.Checked;
            ROBPOperationTextBox.Enabled = ROBPOperationsRadioButton.Checked;
            ROBPOperationButtonOpen.Enabled = ROBPOperationsRadioButton.Checked;

            ServiceCatalogTextBox.Enabled = ServiceCatalogRadioButton.Checked;
            ServiceCatalogOpenButton.Enabled = ServiceCatalogRadioButton.Checked;

            await AssignOperations();

            ROBPOperationsRadioButton.CheckedChanged += ROBPOperationsRadioButton_CheckedChanged;
        }

        async Task AssignOperations()
        {
            var type = ROBPOperationsRadioButton.Checked ? SPAProcessFilterType.ROBPOperations : SPAProcessFilterType.SCOperations;
            switch (type)
            {
                case SPAProcessFilterType.ROBPOperations when !ROBPOperationTextBox.Text.IsNullOrEmptyTrim():
                case SPAProcessFilterType.SCOperations when !ServiceCatalogTextBox.Text.IsNullOrEmptyTrim():
                    await AssignAsync(type);
                    break;
            }
        }

        private bool _serviceCatalogTextBoxChanged = false;
        private async void ServiceCatalogOpenButton_Click(object sender, EventArgs e)
        {
            if (!OpenFile(@"(*.xml) | *.xml", out var res))
                return;

            ServiceCatalogTextBox.Text = res;
            await AssignAsync(SPAProcessFilterType.SCOperations);
            _serviceCatalogTextBoxChanged = false;
        }

        private async void ServiceCatalogTextBox_LostFocus(object sender, System.EventArgs e)
        {
            if (!_serviceCatalogTextBoxChanged)
                return;

            await AssignAsync(SPAProcessFilterType.SCOperations);
            _serviceCatalogTextBoxChanged = false;
        }

        private void ServiceCatalogTextBox_TextChanged(object sender, EventArgs e)
        {
            _serviceCatalogTextBoxChanged = true;
        }

        private async void AddActivatorButton_Click(object sender, EventArgs e)
        {
            await AssignAsync(SPAProcessFilterType.Activators_Add);
        }

        private async void RemoveActivatorButton_Click_1(object sender, EventArgs e)
        {
            await AssignAsync(SPAProcessFilterType.Activators_Remove);
        }

        async Task AssignAsync(SPAProcessFilterType type, bool saveSettings = true)
        {
            try
            {
                switch (type)
                {
                    case SPAProcessFilterType.Processes when !ProcessesTextBox.Text.IsNullOrEmptyTrim():
                        await _spaFilter.AssignProcessesAsync(ProcessesTextBox.Text);

                        ProcessesComboBox.DataSource = _spaFilter.Processes.Select(p => p.Name).ToList();
                        ProcessesComboBox.Text = null;
                        ProcessesComboBox.DisplayMember = null;

                        UpdateLastPath(ProcessesTextBox.Text);
                        ClearDataGrid();

                        break;
                    case SPAProcessFilterType.SCOperations:
                    case SPAProcessFilterType.ROBPOperations:

                        switch (type)
                        {
                            case SPAProcessFilterType.SCOperations:
                                await _spaFilter.AssignSCOperationsAsync(ServiceCatalogTextBox.Text);
                                UpdateLastPath(Path.GetDirectoryName(ServiceCatalogTextBox.Text));
                                break;
                            case SPAProcessFilterType.ROBPOperations:
                                await _spaFilter.AssignROBPOperationsAsync(ROBPOperationTextBox.Text);
                                UpdateLastPath(ROBPOperationTextBox.Text);
                                break;
                        }

                        if (_spaFilter.HostTypes != null)
                        {
                            NetSettComboBox.DataSource = _spaFilter.HostTypes.HostTypeNames;
                            NetSettComboBox.Text = null;
                            NetSettComboBox.DisplayMember = null;
                            OperationComboBox.DataSource = _spaFilter.HostTypes.OperationNames;
                            OperationComboBox.Text = null;
                            OperationComboBox.DisplayMember = null;
                        }
                        else
                        {
                            ClearOperationsComboBox();
                        }

                        ClearDataGrid(true);

                        break;
                    case SPAProcessFilterType.Activators_Add:
                        if (!OpenFile(@"(configuration.application.xml) | *.xml", out var fileConfig))
                            return;

                        await _spaFilter.AssignActivatorAsync(fileConfig);
                        AssignActivator();
                        UpdateLastPath(fileConfig);
                        break;
                    case SPAProcessFilterType.Activators_Remove:
                        if (GetCellItemSelectedRows(dataGridServiceInstances, out var listFiles))
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
                if (saveSettings)
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
                case Keys.F5 when _spaFilter.IsEnabledFilter:
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
                var syncContext = SynchronizationContext.Current;

                ClearDataGrid();

                using (var progressCalc = new ProgressCalculationAsync(progressBar, 12))
                {
                    await Task.Factory.StartNew(() => _spaFilter.DataFilter(filterProcess, filterNE, filterOp, progressCalc));

                    await Task.Factory.StartNew(() => syncContext.Post(delegate
                    {
                        AssignActivator();

                        dataGridProcesses.AssignListToDataGrid(_spaFilter.Processes, new Padding(0, 0, 15, 0));
                        progressCalc.Append(1);

                        if(ROBPOperationsRadioButton.Checked)
                            dataGridOperations.AssignListToDataGrid<ROBPOperation>(_spaFilter.HostTypes.Operations.OfType<ROBPOperation>().ToList(), new Padding(0, 0, 15, 0));
                        else
                            dataGridOperations.AssignListToDataGrid<CatalogOperation>(_spaFilter.HostTypes.Operations.OfType<CatalogOperation>().ToList(), new Padding(0, 0, 15, 0));

                        progressCalc.Append(1);

                        if (_spaFilter.Scenarios != null)
                        {
                            dataGridScenarios.AssignListToDataGrid(_spaFilter.Scenarios, new Padding(0, 0, 15, 0));
                        }
                        else
                        {
                            dataGridScenarios.DataSource = null;
                            dataGridScenarios.Refresh();
                        }

                        progressCalc.Append(1);

                        if (_spaFilter.Commands != null)
                        {
                            dataGridCommands.AssignListToDataGrid(_spaFilter.Commands, new Padding(0, 0, 15, 0));
                        }
                        else
                        {
                            dataGridCommands.DataSource = null;
                            dataGridCommands.Refresh();
                        }
                    }, null));
                }

                IsFiltered = true;
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
            if(!OpenFile(@"(*.xlsx) | *.xlsx", out var res))
                return;

            OpenSCXlsx.Text = res;
        }

        private async void ButtonGenerateSC_Click(object sender, EventArgs e)
        {
            if (IsInProgress)
                return;

            var fileOperationsCount = _spaFilter.HostTypes.DriveOperationsCount;

            if (fileOperationsCount == 0)
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
                    using (var progrAsync = new CustomProgressCalculation(progressBar, fileOperationsCount, file))
                    {
                        var rdServices = await Task<DataTable>.Factory.StartNew(() => _spaFilter.GetRDServicesFromXslx(file, progrAsync));
                        fileResult = await Task.Factory.StartNew(() => _spaFilter.GetServiceCatalog(rdServices, ExportSCPath.Text, progrAsync));
                    }
                }
                else
                {
                    using (var progrAsync = new CustomProgressCalculation(progressBar, fileOperationsCount))
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
            var filesNumber = _spaFilter.WholeDriveItemsCount;
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

        void OpenEditor(string source, string name = null)
        {
            try
            {
                if (_notepad == null || _notepad.WindowIsClosed)
                {
                    if (_notepadLocation == FormLocation.Default)
                        _notepadLocation = new FormLocation(this);

                    _notepad = new Notepad()
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
                    _notepad.Focus();
                    _notepad.Activate();
                }

                if (name == null)
                    _notepad.AddFileDocument(source);
                else
                    _notepad.AddDocument(name, source, Language.XML);
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

        void ClearDataGrid(bool onlyOperations = false)
        {
            if (onlyOperations)
            {
                dataGridProcesses.DataSource = null;
                dataGridProcesses.Refresh();
            }
            dataGridOperations.DataSource = null;
            dataGridScenarios.DataSource = null;
            dataGridCommands.DataSource = null;
            dataGridOperations.Refresh();
            dataGridScenarios.Refresh();
            dataGridCommands.Refresh();

            IsFiltered = false;
        }

        void RefreshStatus()
        {
            BPCount.Text = $"Processes: {(_spaFilter.Processes?.Count ?? 0 ).ToString()}";
            NEElementsCount.Text = $"HostTypes: {(_spaFilter.HostTypes?.HostTypeNames?.Count ?? 0).ToString()}";
            OperationsCount.Text = $"Operations: {(_spaFilter.HostTypes?.OperationsCount ?? 0).ToString()}";
            ScenariosCount.Text = $"Scenarios: {(_spaFilter.Scenarios?.Count ?? 0).ToString()}";
            CommandsCount.Text = $"Commands: {(_spaFilter.Commands?.Count ?? 0).ToString()}";

            FilterButton.Enabled = _spaFilter.IsEnabledFilter;
            PrintXMLButton.Enabled = _spaFilter.WholeDriveItemsCount > 0 && IsFiltered;
            ButtonGenerateSC.Enabled = _spaFilter.CanGenerateSC && !ExportSCPath.Text.IsNullOrEmptyTrim() && ROBPOperationsRadioButton.Checked && IsFiltered;
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

        static bool OpenFile(string extension, out string result)
        {
            result = null;
            try
            {
                using (var fbd = new OpenFileDialog())
                {
                    fbd.Filter = extension;

                    if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName) && File.Exists(fbd.FileName))
                    {
                        result = fbd.FileName;
                        return true;
                    }
                }
                return false;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
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

        static bool GetCellItemSelectedRows(DataGridView grid, out List<string> result, string name = "File Path")
        {
            result = new List<string>();
            if (grid.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in grid.SelectedRows)
                {
                    var cell = row.Cells[name]?.Value;
                    if (cell != null)
                        result.Add(cell.ToString());
                }
            }
            else
            {
                MessageBox.Show(@"You must select a row.", @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return result.Count > 0;
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