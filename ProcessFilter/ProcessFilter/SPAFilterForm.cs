using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Threading.Tasks;
using FastColoredTextBoxNS;
using SPAFilter.Properties;
using SPAFilter.SPA;
using SPAFilter.SPA.Components.ROBP;
using SPAFilter.SPA.Components.SRI;
using Utils;
using Utils.AppUpdater;
using Utils.AppUpdater.Updater;
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
        AddActivators = 4,
        RemoveActivators = 8,
        ReloadActivators = 16
    }

    [Serializable]
    public partial class SPAFilterForm : Form, ISerializable
    {
        readonly object _sync = new object();

        private bool _IsInProgress = false;

        private string _lastDirPath = string.Empty;
        private readonly object sync = new object();
        private readonly object sync2 = new object();
        private Notepad _notepad;
        private bool _notepadWordWrap = true;
        private bool _notepadWordHighlights = true;
        private FormLocation _notepadLocation = FormLocation.Default;
        private FormWindowState _notepadWindowsState = FormWindowState.Maximized;
        private SPAProcessFilter _spaFilter;

        ApplicationUpdater AppUpdater { get; set; }
        IUpdater Updater { get; set; }

        public static string SavedDataPath => $"{ApplicationFilePath}.bin";

        private bool IsInititializating { get; set; } = true;

        private bool IsInProgress
        {
            get => _IsInProgress;
            set
            {
                lock (sync2)
                {
                    _IsInProgress = value;

                    if (_IsInProgress && _notepad != null && !_notepad.WindowIsClosed)
                        _notepad.Close();

                    FilterButton.Enabled = !_IsInProgress;
                    PrintXMLButton.Enabled = !_IsInProgress;

                    ProcessesTextBox.Enabled = !_IsInProgress;
                    ProcessesButtonOpen.Enabled = !_IsInProgress;

                    ROBPOperationsRadioButton.Enabled = !_IsInProgress;
                    ServiceCatalogRadioButton.Enabled = !_IsInProgress;

                    if (ROBPOperationsRadioButton.Checked)
                    {
                        ROBPOperationTextBox.Enabled = !_IsInProgress;
                        ROBPOperationButtonOpen.Enabled = !_IsInProgress;
                        ServiceCatalogTextBox.Enabled = false;
                        ServiceCatalogOpenButton.Enabled = false;
                    }
                    else
                    {
                        ROBPOperationTextBox.Enabled = false;
                        ROBPOperationButtonOpen.Enabled = false;
                        ServiceCatalogTextBox.Enabled = !_IsInProgress;
                        ServiceCatalogOpenButton.Enabled = !_IsInProgress;
                    }

                    addServiceInstancesButton.Enabled = !_IsInProgress;
                    removeServiceInstancesButton.Enabled = !_IsInProgress;
                    refreshServiceInstancesButton.Enabled = !_IsInProgress;

                    dataGridProcesses.Visible = !_IsInProgress;
                    dataGridOperations.Visible = !_IsInProgress;
                    dataGridScenarios.Visible = !_IsInProgress;
                    dataGridCommands.Visible = !_IsInProgress;
                    GenerateSC.Enabled = !_IsInProgress;

                    ProcessesComboBox.Enabled = !_IsInProgress;
                    NetSettComboBox.Enabled = !_IsInProgress;
                    OperationComboBox.Enabled = !_IsInProgress;
                }
            }
        }

        private bool IsFiltered { get; set; } = false;

        private int _loadIterator = 0;
        private bool IsLoading
        {
            get => _loadIterator > 0;
            set
            {
                lock (_sync)
                {
                    if (value)
                        _loadIterator++;
                    else
                        _loadIterator--;

                    switch (_loadIterator)
                    {
                        case 1 when value:
                            IsInProgress = true;

                            progressBar.Visible = true;
                            progressBar.MarqueeAnimationSpeed = 10;
                            progressBar.Style = ProgressBarStyle.Marquee;
                            break;
                        case 0:
                            IsInProgress = false;

                            progressBar.Visible = false;
                            progressBar.Style = ProgressBarStyle.Blocks;
                            break;
                    }
                }
            }
        }

        public SPAFilterForm()
        {
            IsInititializating = false;
            PreInit();
            PostInit();
        }

        SPAFilterForm(SerializationInfo propertyBag, StreamingContext context)
        {
            var allSavedParams = new Dictionary<string, object>();
            try
            {
                foreach (var entry in propertyBag)
                {
                    allSavedParams.Add(entry.Name, entry.Value);
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
            finally
            {
                LoadFromFile(allSavedParams);
            }
        }

        async void LoadFromFile(IReadOnlyDictionary<string, object> allSavedParams)
        {
            try
            {
                PreInit();
                IsLoading = true;

                ProcessesTextBox.Text = (string) TryGetSerializationValue(allSavedParams, "ADWFFW", string.Empty);
                if (!ProcessesTextBox.Text.IsNullOrEmptyTrim())
                {
                    // RunSynchronously - не подходит, т.к. образуется дедлок, потому что асинхронный поток не пытается вернуться назад
                    //var task = Task.Run(() => AssignAsync(SPAProcessFilterType.Processes, false));
                    //task.Wait();
                    await AssignAsync(SPAProcessFilterType.Processes, false);
                }

                var robpIsChecked = (bool) TryGetSerializationValue(allSavedParams, "GGHHTTDD", true);
                if (robpIsChecked != ROBPOperationsRadioButton.Checked)
                {
                    ROBPOperationsRadioButton.Checked = robpIsChecked;
                    ServiceCatalogRadioButton.Checked = !robpIsChecked;
                }
                else
                {
                    ROBPOperationsRadioButton_CheckedChanged(ServiceCatalogRadioButton, EventArgs.Empty);
                }

                ROBPOperationTextBox.Text = (string) TryGetSerializationValue(allSavedParams, "AAEERF", string.Empty);
                ServiceCatalogTextBox.Text = (string) TryGetSerializationValue(allSavedParams, "DFWDRT", string.Empty);

                if (ROBPOperationsRadioButton.Checked)
                {
                    if (!ROBPOperationTextBox.Text.IsNullOrEmptyTrim())
                    {
                        //var taskROBP = Task.Run(() => AssignAsync(SPAProcessFilterType.ROBPOperations, false));
                        //taskROBP.Wait();
                        await AssignAsync(SPAProcessFilterType.ROBPOperations, false);
                    }
                }
                else if (!ServiceCatalogTextBox.Text.IsNullOrEmptyTrim())
                {
                    //var taskSC = Task.Run(() => AssignAsync(SPAProcessFilterType.SCOperations, false));
                    //taskSC.Wait();
                    await AssignAsync(SPAProcessFilterType.SCOperations, false);
                }

                ExportSCPath.Text = (string) TryGetSerializationValue(allSavedParams, "FFFGHJ", string.Empty);
                OpenSCXlsx.Text = (string) TryGetSerializationValue(allSavedParams, "GGHHRR", string.Empty);
                _lastDirPath = (string) TryGetSerializationValue(allSavedParams, "GHDDSD", string.Empty);

                //var taskInstances = Task.Run(() => AssignServiceInstanes((List<string>)TryGetSerializationValue(allSavedParams, "WWWERT", null)));
                //taskInstances.Wait();
                await AssignServiceInstanes((List<string>) TryGetSerializationValue(allSavedParams, "WWWERT", null));

                _notepadWordWrap = (bool) TryGetSerializationValue(allSavedParams, "DDCCVV", true);
                _notepadWordHighlights = (bool) TryGetSerializationValue(allSavedParams, "RRTTGGBB", true);
                _notepadLocation = (FormLocation) TryGetSerializationValue(allSavedParams, "RRTTDD", FormLocation.Default);
                _notepadWindowsState = (FormWindowState) TryGetSerializationValue(allSavedParams, "SSEEFF", FormWindowState.Maximized);

                Updater = (IUpdater)TryGetSerializationValue(allSavedParams, "UUPPDD", null);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                IsLoading = false;
                PostInit();
            }
        }

        static object TryGetSerializationValue(IReadOnlyDictionary<string, object> allSavedParams, string key, object defaultResult)
        {
            return allSavedParams.TryGetValue(key, out var res) ? res : defaultResult;
        }

        async Task AssignServiceInstanes(IEnumerable<string> configurationApplicationList)
        {
            if(configurationApplicationList == null)
                return;

            await AssignServiceInstances(_spaFilter.AssignActivatorAsync(configurationApplicationList));
        }

        void ISerializable.GetObjectData(SerializationInfo propertyBag, StreamingContext context)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => SerializeData(propertyBag)));
            }
            else
            {
                SerializeData(propertyBag);
            }
        }

        void SerializeData(SerializationInfo propertyBag)
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
                var filesConfigs = _spaFilter.ServiceInstances.Select(x => x.FilePath).Distinct().ToList();
                propertyBag.AddValue("WWWERT", filesConfigs);
            }

            propertyBag.AddValue("DDCCVV", _notepadWordWrap);
            propertyBag.AddValue("RRTTGGBB", _notepadWordHighlights);
            propertyBag.AddValue("RRTTDD", _notepadLocation);
            propertyBag.AddValue("SSEEFF", _notepadWindowsState);

            propertyBag.AddValue("UUPPDD", Updater);
        }

        void PreInit()
        {
            _spaFilter = new SPAProcessFilter();

            InitializeComponent();
            KeyPreview = true; // для того чтобы работали горячие клавиши по всей форме и всем контролам
            var tooltipPrintXML = new ToolTip
            {
                InitialDelay = 50
            };
            tooltipPrintXML.SetToolTip(PrintXMLButton, Resources.Form_PrintXMLFiles_ToolTip);
            tooltipPrintXML.SetToolTip(ProcessesComboBox, Resources.Form_ToolTip_SearchPattern);
            tooltipPrintXML.SetToolTip(OperationComboBox, Resources.Form_ToolTip_SearchPattern);
            tooltipPrintXML.SetToolTip(NetSettComboBox, Resources.Form_ToolTip_SearchPattern);

            ROBPOperationsRadioButton.CheckedChanged += ROBPOperationsRadioButton_CheckedChanged;
            ServiceCatalogRadioButton.CheckedChanged += ServiceCatalogRadioButton_CheckedChanged;
        }

        void PostInit()
        {
            try
            {
                var currentPackUpdaterName = Updater?.ProjectBuildPack.Name;
                AppUpdater = new ApplicationUpdater(Assembly.GetExecutingAssembly(), currentPackUpdaterName, 900);
                AppUpdater.OnFetch += AppUpdater_OnFetch;
                AppUpdater.OnUpdate += AppUpdater_OnUpdate;
                AppUpdater.OnProcessingError += AppUpdater_OnProcessingError;
                AppUpdater.Start();
                AppUpdater.CheckUpdates();
            }
            catch (Exception ex)
            {
                // ignored
            }
            

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

            IsInititializating = false;

            RefreshStatus();
        }

        #region Application Update

        private static void AppUpdater_OnFetch(object sender, ApplicationFetchingArgs args)
        {
            if (args == null)
                return;

            if (args.Control == null)
            {
                args.Result = UpdateBuildResult.Cancel;
                return;
            }
        }

        private void AppUpdater_OnUpdate(object sender, ApplicationUpdatingArgs args)
        {
            try
            {
                if (args?.Control?.ProjectBuildPack == null)
                {
                    AppUpdater.Refresh();
                    return;
                }

                Updater = args.Control;
                Updater.SecondsMoveDelay = 1;
                Updater.SecondsRunDelay = 3;

                if (Updater.ProjectBuildPack.NeedRestartApplication)
                {
                    SaveData();
                    AppUpdater.DoUpdate(Updater);
                }
                else
                {
                    AppUpdater.DoUpdate(Updater);
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private static void AppUpdater_OnProcessingError(object sender, ApplicationUpdatingArgs args)
        {

        }

        #endregion

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
                    cell2.ToolTipText = Resources.Form_GridView_IncorrectConfig;
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
                        cell2.ToolTipText = Resources.Form_GridView_NotFoundSomeOPs;
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
                        cell2.ToolTipText = Resources.Form_GridView_NotFoundServiceCatalogCall;
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
                    cell2.ToolTipText = Resources.Form_GridView_NotFoundSomeScenarios;
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
                    cell3.ToolTipText = Resources.Form_GridView_NotFoundSomeCommandsInSub;
                }
            }
            else if (!allCommandsExist)
            {
                row.DefaultCellStyle.BackColor = Color.LightPink;
                foreach (DataGridViewCell cell3 in row.Cells)
                {
                    cell3.ToolTipText = Resources.Form_GridView_NotFoundSomeCommands;
                }
            }
            else if (isSubScenario)
            {
                row.DefaultCellStyle.BackColor = Color.Aqua;
                foreach (DataGridViewCell cell3 in row.Cells)
                {
                    cell3.ToolTipText = Resources.Form_GridView_IsSubScenario;
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
            try
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
            catch (Exception ex)
            {
                ShowError(ex.Message);
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

        async void CallAndCheckDataGridKey(DataGridView grid, KeyEventArgs e = null)
        {
            try
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

                                if (filesPath.Count == 0)
                                    return;

                                var userResult = MessageBox.Show(string.Format(Resources.Form_GridView_DeleteSelected, (filesPath.Count == 1 ? $"file" : $"{filesPath.Count} files")), @"Question", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                                if (userResult != DialogResult.OK)
                                    return;

                                foreach (var filePath in filesPath)
                                {
                                    if (File.Exists(filePath))
                                        File.Delete(filePath);
                                }

                                if (IsFiltered)
                                {
                                    FilterButton_Click(this, EventArgs.Empty);
                                }
                                else if (grid == dataGridServiceInstances)
                                {
                                    await AssignServiceInstances(_spaFilter.ReloadActivatorsAsync());
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
            catch (Exception ex)
            {
                ShowError(ex.Message);
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
            ProcessesTextBox.BackColor = Color.White;
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
            ROBPOperationTextBox.BackColor = Color.White;
            _robpOperationTextBoxChanged = true;
        }

        private async void ROBPOperationsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ServiceCatalogRadioButton.CheckedChanged -= ServiceCatalogRadioButton_CheckedChanged;

            ServiceCatalogRadioButton.Checked = !ROBPOperationsRadioButton.Checked;

            if (!IsInProgress)
            {
                ServiceCatalogTextBox.Enabled = ServiceCatalogRadioButton.Checked;
                ServiceCatalogOpenButton.Enabled = ServiceCatalogRadioButton.Checked;
                ROBPOperationTextBox.Enabled = ROBPOperationsRadioButton.Checked;
                ROBPOperationButtonOpen.Enabled = ROBPOperationsRadioButton.Checked;
            }

            await AssignOperations();

            ServiceCatalogRadioButton.CheckedChanged += ServiceCatalogRadioButton_CheckedChanged;
        }

        private async void ServiceCatalogRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ROBPOperationsRadioButton.CheckedChanged -= ROBPOperationsRadioButton_CheckedChanged;

            ROBPOperationsRadioButton.Checked = !ServiceCatalogRadioButton.Checked;

            if (!IsInProgress)
            {
                ROBPOperationTextBox.Enabled = ROBPOperationsRadioButton.Checked;
                ROBPOperationButtonOpen.Enabled = ROBPOperationsRadioButton.Checked;
                ServiceCatalogTextBox.Enabled = ServiceCatalogRadioButton.Checked;
                ServiceCatalogOpenButton.Enabled = ServiceCatalogRadioButton.Checked;
            }

            await AssignOperations();

            ROBPOperationsRadioButton.CheckedChanged += ROBPOperationsRadioButton_CheckedChanged;
        }

        async Task AssignOperations()
        {
            var type = ROBPOperationsRadioButton.Checked ? SPAProcessFilterType.ROBPOperations : SPAProcessFilterType.SCOperations;
            switch (type)
            {
                case SPAProcessFilterType.ROBPOperations:
                case SPAProcessFilterType.SCOperations:
                    await AssignAsync(type);
                    break;
            }
        }

        private bool _serviceCatalogTextBoxChanged = false;
        private async void ServiceCatalogOpenButton_Click(object sender, EventArgs e)
        {
            if (!OpenFile(@"(*.xml) | *.xml", false,out var res))
                return;

            ServiceCatalogTextBox.Text = res.First();
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
            ServiceCatalogTextBox.BackColor = Color.White;
            _serviceCatalogTextBoxChanged = true;
        }

        private async void AddActivatorButton_Click(object sender, EventArgs e)
        {
            await AssignAsync(SPAProcessFilterType.AddActivators);
        }

        private async void RemoveActivatorButton_Click(object sender, EventArgs e)
        {
            await AssignAsync(SPAProcessFilterType.RemoveActivators);
        }

        private async void RefreshActivatorButton_Click(object sender, EventArgs e)
        {
            await AssignAsync(SPAProcessFilterType.ReloadActivators);
        }

        async Task AssignAsync(SPAProcessFilterType type, bool saveSettings = true)
        {
            try
            {
                switch (type)
                {
                    case SPAProcessFilterType.Processes:
                        await _spaFilter.AssignProcessesAsync(ProcessesTextBox.Text);

                        ProcessesComboBox.DataSource = _spaFilter.Processes.Select(p => p.Name).ToList();
                        ProcessesComboBox.Text = null;
                        ProcessesComboBox.DisplayMember = null;

                        UpdateLastPath(ProcessesTextBox.Text);
                        ProcessesTextBox.BackColor = Color.White;

                        ClearDataGrid();
                        
                        break;
                    case SPAProcessFilterType.SCOperations:
                    case SPAProcessFilterType.ROBPOperations:

                        try
                        {
                            IsLoading = true;
                            switch (type)
                            {
                                case SPAProcessFilterType.SCOperations:
                                    await _spaFilter.AssignSCOperationsAsync(ServiceCatalogTextBox.Text);
                                    UpdateLastPath(ServiceCatalogTextBox.Text);
                                    ServiceCatalogTextBox.BackColor = Color.White;
                                    break;
                                case SPAProcessFilterType.ROBPOperations:
                                    await _spaFilter.AssignROBPOperationsAsync(ROBPOperationTextBox.Text);
                                    UpdateLastPath(ROBPOperationTextBox.Text);
                                    ROBPOperationTextBox.BackColor = Color.White;
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            IsLoading = false;
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
                    case SPAProcessFilterType.AddActivators when OpenFile(@"(configuration.application.xml) | *.xml", true, out var fileConfig):
                        await AssignServiceInstances(_spaFilter.AssignActivatorAsync(fileConfig.ToList()));
                        UpdateLastPath(fileConfig.Last());
                        break;
                    case SPAProcessFilterType.RemoveActivators when GetCellItemSelectedRows(dataGridServiceInstances, out var listFiles):
                        await AssignServiceInstances(_spaFilter.RemoveActivatorAsync(listFiles));
                        break;
                    case SPAProcessFilterType.ReloadActivators:
                        await AssignServiceInstances(_spaFilter.ReloadActivatorsAsync());
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);

                ClearDataGrid();

                switch (type)
                {
                    case SPAProcessFilterType.Processes:
                        //ProcessesTextBox.Text = string.Empty;
                        //ProcessesTextBox.BackColor = Color.PaleVioletRed;
                        ProcessesTextBox.BackColor = Color.LightPink;
                        ProcessesComboBox.DataSource = null;
                        ProcessesComboBox.Text = null;
                        ProcessesComboBox.DisplayMember = null;
                        break;
                    case SPAProcessFilterType.ROBPOperations:
                        //ROBPOperationTextBox.Text = string.Empty;
                        ROBPOperationTextBox.BackColor = Color.LightPink;
                        ClearOperationsComboBox();
                        break;
                    case SPAProcessFilterType.SCOperations:
                        //ServiceCatalogTextBox.Text = string.Empty;
                        ServiceCatalogTextBox.BackColor = Color.LightPink;
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

        async Task AssignServiceInstances(Task getInstances)
        {
            try
            {
                IsLoading = true;

                addServiceInstancesButton.Enabled = false;
                removeServiceInstancesButton.Enabled = false;
                refreshServiceInstancesButton.Enabled = false;

                await getInstances;
                await AssignServiceInstances();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                IsLoading = false;

                addServiceInstancesButton.Enabled = true;
                removeServiceInstancesButton.Enabled = true;
                refreshServiceInstancesButton.Enabled = true;

                ClearDataGrid(true);
                RefreshStatus();
            }
        }

        async Task AssignServiceInstances()
        {
            //dataGridServiceInstances.Enabled = !_IsInProgress; - из за ебучего Enabled дисеблился scrollbar. Не дисейблить.
            dataGridServiceInstances.DataSource = null;
            dataGridServiceInstances.Refresh();

            if (_spaFilter.ServiceInstances != null)
            {
                await dataGridServiceInstances.AssignCollectionAsync(_spaFilter.ServiceInstances, new Padding(0, 0, 15, 0), true);
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
                case Keys.F5 when FilterButton.Enabled:
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
                var filterHT = NetSettComboBox.Text;
                var filterOp = OperationComboBox.Text;

                ClearDataGrid();

                using (var progressCalc = new ProgressCalculationAsync(progressBar, 9))
                {
                    await _spaFilter.DataFilterAsync(filterProcess, filterHT, filterOp, progressCalc);

                    await AssignServiceInstances();

                    await dataGridProcesses.AssignCollectionAsync(_spaFilter.Processes, null, true);

                    if (ROBPOperationsRadioButton.Checked)
                        await dataGridOperations.AssignCollectionAsync(_spaFilter.HostTypes.Operations.OfType<ROBPOperation>(), null, true);
                    else
                        await dataGridOperations.AssignCollectionAsync(_spaFilter.HostTypes.Operations.OfType<CatalogOperation>(), null, true);

                    progressCalc.Append(1);

                    if (_spaFilter.Scenarios != null)
                    {
                        await dataGridScenarios.AssignCollectionAsync(_spaFilter.Scenarios, null, true);
                    }
                    else
                    {
                        dataGridScenarios.DataSource = null;
                        dataGridScenarios.Refresh();
                    }

                    if (_spaFilter.Commands != null)
                    {
                        await dataGridCommands.AssignCollectionAsync(_spaFilter.Commands, null, true);
                    }
                    else
                    {
                        dataGridCommands.DataSource = null;
                        dataGridCommands.Refresh();
                    }
                }

                IsFiltered = true;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
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
            if(!OpenFile(@"(*.xlsx) | *.xlsx", false, out var res))
                return;

            OpenSCXlsx.Text = res.First();
        }

        private async void ButtonGenerateSC_Click(object sender, EventArgs e)
        {
            if (IsInProgress)
                return;

            var fileOperationsCount = _spaFilter.HostTypes.DriveOperationsCount;

            if (fileOperationsCount == 0)
            {
                MessageBox.Show(Resources.Form_GenerateSC_NotFoundAnyOperations, @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        var rdServices = await _spaFilter.GetRDServicesFromXslxAsync(file, progrAsync);
                        fileResult = await _spaFilter.GetServiceCatalogAsync(rdServices, ExportSCPath.Text, progrAsync);
                    }
                }
                else
                {
                    using (var progrAsync = new CustomProgressCalculation(progressBar, fileOperationsCount))
                    {
                        fileResult = await _spaFilter.GetServiceCatalogAsync(null, ExportSCPath.Text, progrAsync);
                    }
                }

                if (!fileResult.IsNullOrEmpty() && File.Exists(fileResult))
                {
                    OpenEditor(fileResult);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
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
                MessageBox.Show(Resources.Form_GenerateSC_NotFileredROBPOps, @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        MessageBox.Show(string.Format(Resources.Form_PrintXMLFiles_Error, stringErrors.Lines, stringErrors.ToString(2)), @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show(Resources.Form_PrintXMLFiles_Successfully, @"OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
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
                        WordWrap = _notepadWordWrap,
                        WordHighlights = _notepadWordHighlights
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
                ShowError(ex.Message);
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
                _notepadWordHighlights = notepad.WordHighlights;
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
            BPCount.Text = string.Format(Resources.Form_Status_Processes, (_spaFilter.Processes?.Count ?? 0 ).ToString());
            NEElementsCount.Text = string.Format(Resources.Form_Status_HostTypes, (_spaFilter.HostTypes?.HostTypeNames?.Count ?? 0).ToString());
            OperationsCount.Text = string.Format(Resources.Form_Status_Operations, (_spaFilter.HostTypes?.OperationsCount ?? 0).ToString());
            ScenariosCount.Text = string.Format(Resources.Form_Status_Scenarios, (_spaFilter.Scenarios?.Count ?? 0).ToString());
            CommandsCount.Text = string.Format(Resources.Form_Status_Commands, (_spaFilter.Commands?.Count ?? 0).ToString());

            FilterButton.Enabled = _spaFilter.IsEnabledFilter;
            PrintXMLButton.Enabled = _spaFilter.WholeDriveItemsCount > 0 && IsFiltered;
            ButtonGenerateSC.Enabled = _spaFilter.CanGenerateSC && !ExportSCPath.Text.IsNullOrEmptyTrim() && ROBPOperationsRadioButton.Checked && IsFiltered;
        }

        void UpdateLastPath(string path)
        {
            if (path.IsNullOrEmpty())
                return;
            _lastDirPath = Directory.Exists(path) ? path : File.Exists(path) ? Path.GetDirectoryName(path) : _lastDirPath;
        }

        void SaveData()
        {
            if (IsInititializating)
                return;

            lock (sync)
            {
                using (var stream = new FileStream(SavedDataPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    new BinaryFormatter().Serialize(stream, this);
                }
            }
        }

        static bool OpenFile(string extension, bool multipleSelect, out string[] result)
        {
            result = null;
            try
            {
                using (var fbd = new OpenFileDialog())
                {
                    fbd.Filter = extension;
                    fbd.Multiselect = multipleSelect;
                    if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName) && File.Exists(fbd.FileName))
                    {
                        result = multipleSelect ? fbd.FileNames : new[] { fbd.FileName };
                        return true;
                    }
                }
                return false;
            }
            catch(Exception ex)
            {
                ShowError(ex.Message);
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
                ShowError(ex.Message);
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

                result = result.Distinct().ToList();
            }
            else
            {
                MessageBox.Show(Resources.Form_GridView_NotSelectedAnyRows, @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return result.Count > 0;
        }

        static void ShowError(string message, string caption = @"Error")
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
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