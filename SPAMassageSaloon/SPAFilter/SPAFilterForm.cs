using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using FastColoredTextBoxNS;
using OfficeOpenXml;
using SPAFilter.Properties;
using SPAFilter.SPA;
using SPAFilter.SPA.Components.ROBP;
using SPAFilter.SPA.Components.SRI;
using SPAMassageSaloon.Common;
using Utils;
using Utils.WinForm.DataGridViewHelper;
using Utils.WinForm.Notepad;
using Utils.WinForm.CustomProgressBar;

namespace SPAFilter
{
    public enum SPAProcessFilterType
    {
        Processes = 0,
        ROBPOperations = 1,
        CatalogOperations = 2,
        AddActivators = 4,
        RemoveActivators = 8,
        RefreshActivators = 16,
        ReloadActivators = 32
    }

    [Serializable]
    public partial class SPAFilterForm : Form, ISaloonForm, ISerializable
    {
        private readonly object _syncLoading = new object();
        private readonly object _syncSaveData = new object();
        private readonly object _syncInProgress = new object();

        private ProgressCalculationAsync _progressMonitor;
        private ToolTip _tooltip;
        private string _lastDirPath = string.Empty;
        private bool _IsInProgress = false;

        private Notepad _notepad;
        private bool _notepadWordWrap = true;
        private bool _notepadWordHighlights = true;
        private Size _notepadSize = new Size(1200, 800);
        private FormWindowState _notepadWindowsState = FormWindowState.Normal;

        private ToolStripStatusLabel BPCount;
        private ToolStripStatusLabel OperationsCount;
        private ToolStripStatusLabel ScenariosCount;
        private ToolStripStatusLabel CommandsCount;
        private ToolStripStatusLabel NEElementsCount;

        private SPAProcessFilter Filter { get; set; }

        public static string SavedDataPath { get; }

        private bool IsInititializating { get; set; } = true;

        private bool IsInProgress
        {
            get => _IsInProgress;
            set
            {
                lock (_syncInProgress)
                {
                    _IsInProgress = value;

                    buttonFilter.Enabled = !_IsInProgress;
                    buttonReset.Enabled = !_IsInProgress;

                    if(PrintXMLButton.Text != Resources.Form_PrintXMLFiles_Cancel)
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

                    bindWithFilter.Enabled = !_IsInProgress;

                    addServiceInstancesButton.Enabled = !_IsInProgress;
                    removeServiceInstancesButton.Enabled = !_IsInProgress;
                    refreshServiceInstancesButton.Enabled = !_IsInProgress;
                    reloadServiceInstancesButton.Enabled = !_IsInProgress;

                    ChangeEnablingDGV(dataGridServiceInstances, !_IsInProgress);
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

        void ChangeEnablingDGV(DataGridView grid, bool enabled)
        {
            foreach (var dgvChild in grid.Controls.OfType<Control>()) // решает баг с задисейбленным скролл баром DataGridView
                dgvChild.Enabled = enabled;
            grid.Enabled = enabled;
        }

        private bool IsFiltered { get; set; } = false;

        private int _loadIterator = 0;
        private bool IsLoading
        {
            get => _loadIterator > 0;
            set
            {
                lock (_syncLoading)
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

        public int ActiveProcessesCount => _progressMonitor != null ? 1 : 0;

        public int ActiveTotalProgress => _progressMonitor?.PercentComplete ?? 0;

        static SPAFilterForm()
        {
            SavedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{nameof(SPAFilter)}.bin");
        }

        public static SPAFilterForm GetControl()
        {
            SPAFilterForm mainControl = null;
            if (File.Exists(SPAFilterForm.SavedDataPath))
            {
                try
                {
                    using (Stream stream = new FileStream(SPAFilterForm.SavedDataPath, FileMode.Open, FileAccess.Read))
                    {
                        mainControl = new BinaryFormatter().Deserialize(stream) as SPAFilterForm;
                    }
                }
                catch (Exception)
                {
                    File.Delete(SPAFilterForm.SavedDataPath);
                }
            }

            if (mainControl == null)
                mainControl = new SPAFilterForm();

            return mainControl;
        }

        SPAFilterForm()
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

                ROBPOperationTextBox.Text = (string)TryGetSerializationValue(allSavedParams, "AAEERF", string.Empty);
                ServiceCatalogTextBox.Text = (string)TryGetSerializationValue(allSavedParams, "DFWDRT", string.Empty);

                var robpIsChecked = (bool) TryGetSerializationValue(allSavedParams, "GGHHTTDD", true);
                ROBPOperationsRadioButton.Checked = robpIsChecked;
                ServiceCatalogRadioButton.Checked = !robpIsChecked;

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
                    await AssignAsync(SPAProcessFilterType.CatalogOperations, false);
                }

                ExportSCPath.Text = (string) TryGetSerializationValue(allSavedParams, "FFFGHJ", string.Empty);
                OpenSCXlsx.Text = (string) TryGetSerializationValue(allSavedParams, "GGHHRR", string.Empty);
                _lastDirPath = (string) TryGetSerializationValue(allSavedParams, "GHDDSD", string.Empty);

                //var taskInstances = Task.Run(() => AssignServiceInstanes((List<string>)TryGetSerializationValue(allSavedParams, "WWWERT", null)));
                //taskInstances.Wait();
                await AssignServiceInstanes((List<string>) TryGetSerializationValue(allSavedParams, "WWWERT", null));

                _notepadWordWrap = (bool) TryGetSerializationValue(allSavedParams, "DDCCVV", true);
                _notepadWordHighlights = (bool) TryGetSerializationValue(allSavedParams, "RRTTGGBB", true);
                _notepadSize = (Size) TryGetSerializationValue(allSavedParams, "SSEETT", _notepadSize);
                if(_notepadSize.Width <= 300 || _notepadSize.Height <= 300)
                    _notepadSize = new Size(1200, 800);
                _notepadWindowsState = (FormWindowState) TryGetSerializationValue(allSavedParams, "SSEEFF", FormWindowState.Normal);

                ProcessesComboBox.Text = (string)TryGetSerializationValue(allSavedParams, "GGGRRTT", string.Empty);
                NetSettComboBox.Text = (string)TryGetSerializationValue(allSavedParams, "GGGRRQQ", string.Empty);
                OperationComboBox.Text = (string)TryGetSerializationValue(allSavedParams, "GGGRRWW", string.Empty);
                bindWithFilter.Checked = (bool)TryGetSerializationValue(allSavedParams, "GGGRRSS", false);
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex);
            }
            finally
            {
                IsLoading = false;
                PostInit();
            }
        }

        static object TryGetSerializationValue(IReadOnlyDictionary<string, object> allSavedParams, string key, object defaultResult)
        {
            return allSavedParams.TryGetValue(key, out var res) && res != null ? res : defaultResult;
        }

        async Task AssignServiceInstanes(IEnumerable<string> configurationApplicationList)
        {
            if(configurationApplicationList == null)
                return;

            await AssignServiceInstances(Filter.AssignActivatorAsync(configurationApplicationList));
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

            propertyBag.AddValue("GGGRRTT", ProcessesComboBox.Text);
            propertyBag.AddValue("GGGRRQQ", NetSettComboBox.Text);
            propertyBag.AddValue("GGGRRWW", OperationComboBox.Text);
            propertyBag.AddValue("GGGRRSS", bindWithFilter.Checked);

            propertyBag.AddValue("GGHHTTDD", ROBPOperationsRadioButton.Checked);

            if (Filter.ServiceInstances != null)
            {
                var filesConfigs = Filter.ServiceInstances.Select(x => x.FilePath).Distinct().ToList();
                propertyBag.AddValue("WWWERT", filesConfigs);
            }

            propertyBag.AddValue("DDCCVV", _notepadWordWrap);
            propertyBag.AddValue("RRTTGGBB", _notepadWordHighlights);
            propertyBag.AddValue("SSEETT", _notepadSize);
            propertyBag.AddValue("SSEEFF", _notepadWindowsState);
        }

        void PreInit()
        {
            Filter = new SPAProcessFilter();

            InitializeComponent();

            // Gets or sets a value indicating whether to catch calls on the wrong thread that access a control's Handle property when an application is being debugged.
            Control.CheckForIllegalCrossThreadCalls = false;

            base.Text = $"{base.Text} {this.GetAssemblyInfo().CurrentVersion}";

            var statusStripItemsPaddingStart = new Padding(0, 2, 0, 2);
            var statusStripItemsPaddingEnd = new Padding(-3, 2, 1, 2);

            statusStrip.Items.Add(new ToolStripStatusLabel("Processes:") { Margin = statusStripItemsPaddingStart });
            BPCount = new ToolStripStatusLabel() { Margin = statusStripItemsPaddingEnd };
            statusStrip.Items.Add(BPCount);

            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(new ToolStripStatusLabel("HostTypes:") { Margin = statusStripItemsPaddingStart });
            NEElementsCount = new ToolStripStatusLabel() { Margin = statusStripItemsPaddingEnd };
            statusStrip.Items.Add(NEElementsCount);

            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(new ToolStripStatusLabel("Operations:") { Margin = statusStripItemsPaddingStart });
            OperationsCount = new ToolStripStatusLabel() { Margin = statusStripItemsPaddingEnd };
            statusStrip.Items.Add(OperationsCount);

            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(new ToolStripStatusLabel("Scenarios:") { Margin = statusStripItemsPaddingStart });
            ScenariosCount = new ToolStripStatusLabel() { Margin = statusStripItemsPaddingEnd };
            statusStrip.Items.Add(ScenariosCount);

            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(new ToolStripStatusLabel("Commands:") { Margin = statusStripItemsPaddingStart });
            CommandsCount = new ToolStripStatusLabel() { Margin = statusStripItemsPaddingEnd };
            statusStrip.Items.Add(CommandsCount);


            _tooltip = new ToolTip { InitialDelay = 50 };
            ApplySettings();
        }

        void PostInit()
        {
            KeyPreview = true; // для того чтобы работали горячие клавиши по всей форме и всем контролам
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

            ROBPOperationsRadioButton.CheckedChanged += ROBPOperationsRadioButton_CheckedChanged;
            ServiceCatalogRadioButton.CheckedChanged += ServiceCatalogRadioButton_CheckedChanged;

            Closing += (s, e) => SaveData();

            IsInititializating = false;

            RefreshStatus();

            ApplySettings();
            CenterToScreen();
        }

        public void ApplySettings()
        {
            _tooltip.RemoveAll();
            _tooltip.SetToolTip(ProcessesButtonOpen, Resources.Form_ToolTip_ProcessesButtonOpen);
            _tooltip.SetToolTip(ROBPOperationButtonOpen, Resources.Form_ToolTip_ROBPOperationButtonOpen);
            _tooltip.SetToolTip(ServiceCatalogOpenButton, Resources.Form_ToolTip_ServiceCatalogOpenButton);

            _tooltip.SetToolTip(ProcessesComboBox, Resources.Form_ToolTip_SearchPattern);
            _tooltip.SetToolTip(OperationComboBox, Resources.Form_ToolTip_SearchPattern);
            _tooltip.SetToolTip(NetSettComboBox, Resources.Form_ToolTip_SearchPattern);

            _tooltip.SetToolTip(bindWithFilter, Resources.Form_BindWithFilterToolbox);
            _tooltip.SetToolTip(buttonFilter, Resources.Form_ToolTip_FilterButton);
            _tooltip.SetToolTip(buttonReset, Resources.Form_ToolTip_buttonReset);
            _tooltip.SetToolTip(PrintXMLButton, Resources.Form_PrintXMLFiles_ToolTip);
            
            _tooltip.SetToolTip(ExportSCPath, Resources.Form_ToolTip_ExportSCPath);
            _tooltip.SetToolTip(RootSCExportPathButton, Resources.Form_ToolTip_RootSCExportPathButton);
            _tooltip.SetToolTip(OpenSCXlsx, string.Format(Resources.Form_ToolTip_OpenSCXlsx, string.Join("\",\"", MandatoryXslxColumns)));
            _tooltip.SetToolTip(OpenSevExelButton, string.Format(Resources.Form_ToolTip_OpenSevExelButton, string.Join("\",\"", MandatoryXslxColumns)));
            _tooltip.SetToolTip(ButtonGenerateSC, Resources.Form_ToolTip_ButtonGenerateSC);

            bindWithFilter.Text = Resources.Form_BindWithFilter;
            buttonFilter.Text = Resources.Form_Get;
            buttonReset.Text = Resources.Form_Reset;
            PrintXMLButton.Text = Resources.Form_PrintXMLFiles_Button;

            addServiceInstancesButton.Text = Resources.Form_AddActivator;
            removeServiceInstancesButton.Text = Resources.Form_RemoveInstance;
            refreshServiceInstancesButton.Text = Resources.Form_Refresh;
            reloadServiceInstancesButton.Text = Resources.Form_Reload;

            groupBox1.Text = Resources.Form_Filter;

            RootSCExportPathButton.Text = Resources.Form_Root;
            OpenSevExelButton.Text = Resources.Form_OpenXksx;
            GenerateSC.Text = Resources.Form_GenerateSC;
            ButtonGenerateSC.Text = Resources.Form_GenerateSC2;

            ExportPathLabel.Text = Resources.Form_ExportPath;
            RDServicesLabel.Text = Resources.Form_RDServices;
        }

        #region Check warning rows

        private void DataGridServiceInstances_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (!GetUniqueName(sender, e.RowIndex, out var row, out var uniqueName))
                    return;

                var template = Filter.ServiceInstances[uniqueName];
                if (template == null || template.IsCorrect)
                    return;

                row.DefaultCellStyle.BackColor = Color.Yellow;
                foreach (DataGridViewCell cell2 in row.Cells)
                {
                    cell2.ToolTipText = Resources.Form_GridView_IncorrectConfig;
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private void DataGridProcesses_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (!GetUniqueName(sender, e.RowIndex, out var row, out var uniqueName))
                    return;

                var template = Filter.Processes[uniqueName];
                if (template == null)
                    return;

                if (ROBPOperationsRadioButton.Checked && !template.AllOperationsExist)
                {
                    SetFailedRow(row, Resources.Form_GridView_NotFoundSomeOPs);
                }
                else if (!ROBPOperationsRadioButton.Checked && !template.HasCatalogCall)
                {
                    SetFailedRow(row, Resources.Form_GridView_NotFoundServiceCatalogCall);
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private void DataGridOperations_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (!GetUniqueName(sender, e.RowIndex, out var row, out var uniqueName))
                    return;

                var template = Filter.HostTypes.Operations[uniqueName];
                if (template == null)
                    return;

                if (!template.IsScenarioExist)
                {
                    SetFailedRow(row, Resources.Form_GridView_NotFoundSomeScenarios);
                }
                else if (ROBPOperationsRadioButton.Checked && template is ROBPOperation robpOperation && robpOperation.IsFailed)
                {
                    SetFailedRow(row, Resources.Form_GridView_IncorrectROBPOperation);
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }
        private void DataGridScenariosResult_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (!GetUniqueName(sender, e.RowIndex, out var row, out var uniqueName))
                    return;

                var template = Filter.Scenarios[uniqueName];
                if (template == null)
                    return;

                if (!template.IsCorrectXML)
                {
                    SetFailedRow(row, Resources.Form_GridView_XMLFileIsIncorrect);
                }
                else if (template.IsSubScenario && !template.AllCommandsExist)
                {
                    SetFailedRow(row, Resources.Form_GridView_NotFoundSomeCommandsInSub);
                }
                else if (!template.AllCommandsExist)
                {
                    SetFailedRow(row, Resources.Form_GridView_NotFoundSomeCommands);
                }
                else if (template.IsSubScenario)
                {
                    SetFailedRow(row, Resources.Form_GridView_IsSubScenario, Color.Aqua);
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        bool GetUniqueName(object sender, int rowIndex, out DataGridViewRow row, out string uniqueName)
        {
            if (Filter == null)
            {
                row = null;
                uniqueName = null;
                return false;
            }

            row = ((DataGridView)sender).Rows[rowIndex];
            uniqueName = (string)row.Cells["UniqueName"]?.Value;
            return uniqueName != null;
        }

        //public void FormattingProcess()
        //{
        //    if (Filter == null)
        //        return;

        //    CurrencyManager currencyManager = (CurrencyManager) BindingContext[dataGridProcesses.DataSource];
        //    if (showFailed.Checked)
        //    {
        //        dataGridProcesses.CurrentCell = null;
        //        currencyManager.SuspendBinding();
        //    }

        //    foreach (DataGridViewRow row in dataGridProcesses.Rows)
        //    {
        //        else
        //        {
        //            HideNotFailed(row);
        //        }
        //    }

        //    if (showFailed.Checked)
        //        currencyManager.ResumeBinding();
        //}

        //void HideNotFailed(DataGridViewRow row)
        //{
        //    if (showFailed.Checked)
        //        row.Visible = false;
        //}

        void SetFailedRow(DataGridViewRow row, string toolTipMessage, Color? color = null)
        {
            row.DefaultCellStyle.BackColor = color ?? Color.LightPink;
            foreach (DataGridViewCell cell in row.Cells)
                cell.ToolTipText = toolTipMessage;
        }

        #endregion

        #region DataGrid hot keys and Row double click

        private void DataGridServiceInstances_DoubleClick(object sender, EventArgs e)
        {
            CallAndCheckDataGridKey(dataGridServiceInstances);
        }

        private void DataGridProcessesResults_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridProcesses);
        }

        private void DataGridOperationsResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridOperations);
        }

        private void DataGridScenariosResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridScenarios);
        }

        private void DataGridCommandsResult_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            CallAndCheckDataGridKey(dataGridCommands);
        }

        //[DllImport("user32.dll")]
        //private static extern short GetAsyncKeyState(Keys vKey);
        //private static bool KeyIsDown(Keys key)
        //{
        //    return (GetAsyncKeyState(key) < 0);
        //}

        private async void ProcessFilterForm_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.F6 when buttonReset.Enabled:
                        buttonReset_Click(this, EventArgs.Empty);
                        break;
                    case Keys.F5 when buttonFilter.Enabled:
                        FilterButton_Click(this, EventArgs.Empty);
                        break;
                    case Keys.F4 when (ModifierKeys & Keys.Alt) != 0:
                        Close();
                        break;
                    case Keys.Enter:
                    {
                        if (!GetCurrentDataGridView(out var grid))
                            return;
                        CallAndCheckDataGridKey(grid);
                        break;
                    }
                    case Keys.Delete:
                    {
                        if (!GetCurrentDataGridView(out var grid) || !grid.Focused || (grid == dataGridOperations && !ROBPOperationsRadioButton.Checked) ||
                            !GetCellItemSelectedRows(grid, out var filesPath))
                            return;

                        if (filesPath.Count == 0)
                            return;

                        var userResult = MessageBox.Show(string.Format(Resources.Form_GridView_DeleteSelected), @"Question", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                        if (userResult != DialogResult.OK)
                            return;

                        await DoLongExecutionTasksAsync(MultiTasking.RunAsync((filePath) =>
                        {
                            if (!File.Exists(filePath))
                                return;
                            try
                            {
                                // удаляет и перемещает в корзину
                                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(filePath,
                                    Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                                    Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin,
                                    Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException);
                            }
                            catch (Exception ex)
                            {
                                // ignored
                            }
                        }, filesPath, new MultiTaskingTemplate(filesPath.Count, ThreadPriority.Lowest)));

                        
                        if (grid == dataGridServiceInstances)
                        {
                            await AssignServiceInstances(Filter.RemoveActivatorAsync(filesPath));
                            RefreshStatus();
                        }
                        else if (IsFiltered)
                        {
                            FilterButton_Click(this, EventArgs.Empty);
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex);
            }
        }

        bool GetCurrentDataGridView(out DataGridView focusedGrid)
        {
            focusedGrid = null;
            if (mainTabControl.SelectedTab.HasChildren)
            {
                focusedGrid = mainTabControl.SelectedTab.Controls.OfType<DataGridView>().FirstOrDefault();
            }

            return focusedGrid != null;
        }

        void CallAndCheckDataGridKey(DataGridView grid)
        {
            try
            {
                if(grid == null)
                    return;

                if (grid == dataGridOperations && !ROBPOperationsRadioButton.Checked)
                {
                    if (!GetCellItemSelectedRows(dataGridOperations, out var scOperationNames, "Operation"))
                        return;

                    var scOperations = Filter.HostTypes.Operations.Where(p => scOperationNames.Any(x => x.Like(p.Name)));
                    OpenEditor(scOperations.OfType<CatalogOperation>().Select(x => new BlankDocument() { HeaderName = x.Name, BodyText = x.Body, Language = Language.XML}), null);
                }
                else
                {
                    if (!GetCellItemSelectedRows(grid, out var filePathList))
                        return;

                    OpenEditor(null, filePathList);
                }
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex);
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

        private bool _serviceCatalogTextBoxChanged = false;
        private async void ServiceCatalogOpenButton_Click(object sender, EventArgs e)
        {
            if (!OpenFile(@"(*.xml) | *.xml", false, out var res))
                return;

            ServiceCatalogTextBox.Text = res.First();
            await AssignAsync(SPAProcessFilterType.CatalogOperations);
            _serviceCatalogTextBoxChanged = false;
        }

        private async void ServiceCatalogTextBox_LostFocus(object sender, System.EventArgs e)
        {
            if (!_serviceCatalogTextBoxChanged)
                return;

            await AssignAsync(SPAProcessFilterType.CatalogOperations);
            _serviceCatalogTextBoxChanged = false;
        }

        private void ServiceCatalogTextBox_TextChanged(object sender, EventArgs e)
        {
            ServiceCatalogTextBox.BackColor = Color.White;
            _serviceCatalogTextBoxChanged = true;
        }

        private async void ROBPOperationsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ServiceCatalogRadioButton.CheckedChanged -= ServiceCatalogRadioButton_CheckedChanged;

            ServiceCatalogRadioButton.Checked = !ROBPOperationsRadioButton.Checked;

            ROBP_SC_Check();

            await AssignOperations();

            ServiceCatalogRadioButton.CheckedChanged += ServiceCatalogRadioButton_CheckedChanged;
        }

        private async void ServiceCatalogRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ROBPOperationsRadioButton.CheckedChanged -= ROBPOperationsRadioButton_CheckedChanged;

            ROBPOperationsRadioButton.Checked = !ServiceCatalogRadioButton.Checked;

            ROBP_SC_Check();

            await AssignOperations();

            ROBPOperationsRadioButton.CheckedChanged += ROBPOperationsRadioButton_CheckedChanged;
        }

        void ROBP_SC_Check()
        {
            if (IsInProgress)
                return;

            ROBPOperationTextBox.Enabled = ROBPOperationsRadioButton.Checked;
            ROBPOperationButtonOpen.Enabled = ROBPOperationsRadioButton.Checked;
            ServiceCatalogTextBox.Enabled = ServiceCatalogRadioButton.Checked;
            ServiceCatalogOpenButton.Enabled = ServiceCatalogRadioButton.Checked;
        }

        async Task AssignOperations()
        {
            var type = ROBPOperationsRadioButton.Checked ? SPAProcessFilterType.ROBPOperations : SPAProcessFilterType.CatalogOperations;
            switch (type)
            {
                case SPAProcessFilterType.ROBPOperations:
                case SPAProcessFilterType.CatalogOperations:
                    await AssignAsync(type);
                    break;
            }
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
            await AssignAsync(SPAProcessFilterType.RefreshActivators);
        }

        private async void ReloadActivatorButton_Click(object sender, EventArgs e)
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
                        await Filter.AssignProcessesAsync(ProcessesTextBox.Text);

                        ProcessesComboBox.DataSource = Filter.Processes.Select(p => p.Name).ToList();
                        ProcessesComboBox.Text = null;
                        ProcessesComboBox.DisplayMember = null;

                        UpdateLastPath(ProcessesTextBox.Text);
                        ProcessesTextBox.BackColor = Color.White;

                        ClearDataGrid();

                        break;
                    case SPAProcessFilterType.ROBPOperations:
                    case SPAProcessFilterType.CatalogOperations:

                        try
                        {
                            IsLoading = true;
                            switch (type)
                            {
                                case SPAProcessFilterType.ROBPOperations:
                                    await Filter.AssignROBPOperationsAsync(ROBPOperationTextBox.Text);
                                    UpdateLastPath(ROBPOperationTextBox.Text);
                                    ROBPOperationTextBox.BackColor = Color.White;
                                    break;
                                case SPAProcessFilterType.CatalogOperations:
                                    await Filter.AssignSCOperationsAsync(ServiceCatalogTextBox.Text);
                                    UpdateLastPath(ServiceCatalogTextBox.Text);
                                    ServiceCatalogTextBox.BackColor = Color.White;
                                    break;
                            }
                            await AssignServiceInstances(Filter.RefreshActivatorsAsync());
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            IsLoading = false;
                        }


                        if (Filter.HostTypes != null)
                        {
                            NetSettComboBox.DataSource = Filter.HostTypes.HostTypeNames;
                            NetSettComboBox.Text = null;
                            NetSettComboBox.DisplayMember = null;
                            OperationComboBox.DataSource = Filter.HostTypes.OperationNames;
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
                        await AssignServiceInstances(Filter.AssignActivatorAsync(fileConfig.ToList()));
                        UpdateLastPath(fileConfig.Last());
                        break;
                    case SPAProcessFilterType.RemoveActivators when GetCellItemSelectedRows(dataGridServiceInstances, out var listOfUniqueNames, "UniqueName"):
                        await AssignServiceInstances(Filter.RemoveInstanceAsync(listOfUniqueNames.Where(x => x != null)));
                        break;
                    case SPAProcessFilterType.RefreshActivators:
                        await AssignServiceInstances(Filter.RefreshActivatorsAsync());
                        break;
                    case SPAProcessFilterType.ReloadActivators:
                        await AssignServiceInstances(Filter.ReloadActivatorsAsync());
                        break;
                }
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex.Message);

                ClearDataGrid();

                switch (type)
                {
                    case SPAProcessFilterType.Processes:
                        ProcessesTextBox.BackColor = Color.LightPink;
                        ProcessesComboBox.DataSource = null;
                        ProcessesComboBox.Text = null;
                        ProcessesComboBox.DisplayMember = null;
                        break;
                    case SPAProcessFilterType.ROBPOperations:
                        ROBPOperationTextBox.BackColor = Color.LightPink;
                        ClearOperationsComboBox();
                        break;
                    case SPAProcessFilterType.CatalogOperations:
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
                reloadServiceInstancesButton.Enabled = false;

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
                reloadServiceInstancesButton.Enabled = true;

                ClearDataGrid(true);
                RefreshStatus();
            }
        }

        async Task DoLongExecutionTasksAsync(Task longExecutionTask)
        {
            try
            {
                IsLoading = true;

                addServiceInstancesButton.Enabled = false;
                removeServiceInstancesButton.Enabled = false;
                refreshServiceInstancesButton.Enabled = false;
                reloadServiceInstancesButton.Enabled = false;

                await longExecutionTask;
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
                reloadServiceInstancesButton.Enabled = true;

                ClearDataGrid(true);
                RefreshStatus();
            }
        }

        async Task AssignServiceInstances()
        {
            dataGridServiceInstances.DataSource = null;
            dataGridServiceInstances.Refresh();

            if (Filter.ServiceInstances != null)
            {
                await dataGridServiceInstances.AssignCollectionAsync(Filter.ServiceInstances, new Padding(0, 0, 15, 0), true);
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

        private async void FilterButton_Click(object sender, EventArgs e)
        {
            await DoFilter(ProcessesComboBox.Text, NetSettComboBox.Text, OperationComboBox.Text);
        }

        private async void buttonReset_Click(object sender, EventArgs e)
        {
            await DoFilter(string.Empty, string.Empty, string.Empty);
        }

        async Task DoFilter(string filterProcess, string filterHT, string filterOp)
        {
            if (IsInProgress)
                return;

            try
            {
                SaveData();

                IsInProgress = true;

                ClearDataGrid(true);

                using (_progressMonitor = new ProgressCalculationAsync(progressBar, 9))
                {
                    await Filter.DataFilterAsync(filterProcess, filterHT, filterOp, bindWithFilter.Checked, _progressMonitor);

                    await AssignServiceInstances();

                    await dataGridProcesses.AssignCollectionAsync(Filter.Processes, null, true);
                    var prevProcessText = ProcessesComboBox.Text;
                    ProcessesComboBox.DataSource = Filter.Processes.BusinessProcessNames;
                    ProcessesComboBox.Text = prevProcessText;
                    ProcessesComboBox.DisplayMember = prevProcessText;

                    if (ROBPOperationsRadioButton.Checked)
                        await dataGridOperations.AssignCollectionAsync(Filter.HostTypes.Operations.OfType<ROBPOperation>(), null, true);
                    else
                        await dataGridOperations.AssignCollectionAsync(Filter.HostTypes.Operations.OfType<CatalogOperation>(), null, true);

                    var prevNetText = NetSettComboBox.Text;
                    NetSettComboBox.DataSource = Filter.HostTypes.HostTypeNames;
                    NetSettComboBox.Text = prevNetText;
                    NetSettComboBox.DisplayMember = prevNetText;
                    var prevOpText = OperationComboBox.Text;
                    OperationComboBox.DataSource = Filter.HostTypes.OperationNames;
                    OperationComboBox.Text = prevOpText;
                    OperationComboBox.DisplayMember = prevOpText;

                    _progressMonitor.Append(1);

                    if (Filter.Scenarios != null)
                    {
                        await dataGridScenarios.AssignCollectionAsync(Filter.Scenarios, null, true);
                    }
                    else
                    {
                        dataGridScenarios.DataSource = null;
                        dataGridScenarios.Refresh();
                    }

                    if (Filter.Commands != null)
                    {
                        await dataGridCommands.AssignCollectionAsync(Filter.Commands, null, true);
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
                ReportMessage.Show(ex);
            }
            finally
            {
                _progressMonitor = null;
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

            var fileOperationsCount = Filter.HostTypes.DriveOperationsCount;

            if (fileOperationsCount == 0)
            {
                ReportMessage.Show(Resources.Form_GenerateSC_NotFoundAnyOperations, MessageBoxIcon.Warning);
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
                        var rdServices = await GetRDServicesFromXslxAsync(file, progrAsync);
                        fileResult = await Filter.GetServiceCatalogAsync(rdServices, ExportSCPath.Text, progrAsync);
                    }
                }
                else
                {
                    using (var progrAsync = new CustomProgressCalculation(progressBar, fileOperationsCount))
                    {
                        fileResult = await Filter.GetServiceCatalogAsync(null, ExportSCPath.Text, progrAsync);
                    }
                }

                if (!fileResult.IsNullOrEmpty() && File.Exists(fileResult))
                {
                    OpenEditor(null, new List<string>(){ fileResult });
                }
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex);
            }
            finally
            {
                IsInProgress = false;
            }
        }

        private async void PrintXMLButton_Click(object sender, EventArgs e)
        {
            var filesNumber = Filter.WholeDriveItemsCount;
            if (filesNumber <= 0)
            {
                ReportMessage.Show(Resources.Form_GenerateSC_NotFileredROBPOps, MessageBoxIcon.Warning);
                return;
            }

            if (IsInProgress)
                return;

            var userResult = MessageBox.Show(string.Format(Resources.Form_PrintXMLFiles_Question, filesNumber), @"Question", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (userResult != DialogResult.OK)
                return;

            try
            {
                PrintXMLButton.Text = Resources.Form_PrintXMLFiles_Cancel;
                PrintXMLButton.Click -= PrintXMLButton_Click;
                PrintXMLButton.Click += PrintXMLButton_Click_Cancel;
                IsInProgress = true;

                using (var stringErrors = new CustomStringBuilder())
                {
                    using (_progressMonitor = new ProgressCalculationAsync(progressBar, filesNumber))
                    {
                        await Filter.PrintXMLAsync(_progressMonitor, stringErrors);

                        var result = string.Format(Resources.Form_PrintXMLFiles_Result, _progressMonitor.CurrentProgressIterator, filesNumber);
                        if (stringErrors.Length > 0)
                        {
                            var warning = string.Format(Resources.Form_PrintXMLFiles_Error, stringErrors.Lines, stringErrors.ToString(2));
                            ReportMessage.Show($"{result}\r\n{warning}", MessageBoxIcon.Warning);
                        }
                        else
                        {
                            ReportMessage.Show(result, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex);
            }
            finally
            {
                _progressMonitor = null;
                PrintXMLButton.Text = Resources.Form_PrintXMLFiles_Button;
                PrintXMLButton.Click += PrintXMLButton_Click;
                PrintXMLButton.Click -= PrintXMLButton_Click_Cancel;
                IsInProgress = false;
            }
        }

        private void PrintXMLButton_Click_Cancel(object sender, EventArgs e)
        {
            Filter.PrintXMLAbort();
        }

        async void OpenEditor(IEnumerable<BlankDocument> documentList, IEnumerable<string> filesList)
        {
            if ((documentList != null && !documentList.Any()) || (filesList != null && !filesList.Any()))
                return;

            try
            {
                if (_notepad == null || _notepad.WindowIsClosed)
                {
                    _notepad = new Notepad
                    {
                        WordWrap = _notepadWordWrap,
                        Highlights = _notepadWordHighlights,
                        SizingGrip = true,
                        AllowUserCloseItems = true,
                        Size = _notepadSize,
                        WindowState = _notepadWindowsState
                    };
                    _notepad.CenterToScreen();
                    _notepad.Closing += _notepad_Closed;
                    _notepad.Show();
                }

                if(documentList != null)
                    await _notepad.AddDocumentListAsync(documentList);
                else
                    await _notepad.AddFileDocumentListAsync(filesList);
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex);
            }
            finally
            {
                _notepad?.Activate();
                _notepad?.Focus();
            }
        }

        private void _notepad_Closed(object sender, EventArgs e)
        {
            if (!(sender is Notepad notepad))
                return;

            try
            {
                _notepadWordWrap = notepad.CurrentEditor?.WordWrap ?? true;
                _notepadWordHighlights = notepad.CurrentEditor?.Highlights ?? true;
                _notepadSize = notepad.Size;
                _notepadWindowsState = notepad.WindowState;
            }
            catch (Exception)
            {
                // null
            }
        }


        public static readonly string[] MandatoryXslxColumns = new string[] { "#", "SPA_SERVICE_CODE", "GLOBAL_SERVICE_CODE", "SERVICE_NAME", "SERVICE_FULL_NAME", "SERVICE_FULL_NAME2", "DESCRIPTION", "SERVICE_CODE", "SERVICE_NAME2", "EXTERNAL_CODE", "EXTERNAL_CODE2" };

        public async Task<DataTable> GetRDServicesFromXslxAsync(FileInfo file, CustomProgressCalculation progressCalc)
        {
            return await Task<DataTable>.Factory.StartNew(() => GetRDServicesFromXslx(file, progressCalc));
        }

        static DataTable GetRDServicesFromXslx(FileInfo file, CustomProgressCalculation progressCalc)
        {
            var serviceTable = new DataTable();

            progressCalc.BeginOpenXslxFile();

            using (var xslPackage = new ExcelPackage(file))
            {
                progressCalc.BeginReadXslxFile();

                if (xslPackage.Workbook.Worksheets.Count == 0)
                    throw new Exception(Resources.Filter_NoWorksheetFound);

                var myWorksheet = xslPackage.Workbook.Worksheets.First();
                var totalRows = myWorksheet.Dimension.End.Row;
                var totalColumns = MandatoryXslxColumns.Length;

                progressCalc.EndReadXslxFile(totalRows);

                var columnsNames = myWorksheet.Cells[1, 1, 1, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());

                if (!columnsNames.Any())
                    return null;

                var i = 0;
                foreach (var columnName in columnsNames)
                {
                    var columnNameUp = columnName.ToUpper();
                    if (MandatoryXslxColumns[i++] != columnNameUp)
                    {
                        throw new Exception(string.Format(Resources.Filter_WrongColumnStatement, columnNameUp, file.Name, string.Join("','", MandatoryXslxColumns)));
                    }

                    serviceTable.Columns.Add(columnNameUp, typeof(string));
                    if (i == MandatoryXslxColumns.Length)
                        break;
                }

                if (i != MandatoryXslxColumns.Length)
                    throw new Exception(string.Format(Resources.Filter_MissingColumn, file.Name, string.Join("\",\"", MandatoryXslxColumns)));

                for (var rowNum = 2; rowNum <= totalRows; rowNum++)
                {
                    var row = myWorksheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).Take(totalColumns);
                    serviceTable.Rows.Add(values: row.ToArray());
                    progressCalc.ReadXslxFileLine();
                }
            }

            progressCalc.EndOpenXslxFile();

            return serviceTable;
        }

        void ClearDataGrid(bool allData = false)
        {
            if (allData)
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
            try
            {
                BPCount.Text = (Filter.Processes?.Count ?? 0).ToString();
                NEElementsCount.Text = (Filter.HostTypes?.HostTypeNames?.Count() ?? 0).ToString();
                OperationsCount.Text = (Filter.HostTypes?.Operations.Count ?? 0).ToString();
                ScenariosCount.Text = (Filter.Scenarios?.Count ?? 0).ToString();
                CommandsCount.Text = (Filter.Commands?.Count ?? 0).ToString();

                buttonFilter.Enabled = Filter.IsEnabledFilter;
                buttonReset.Enabled = Filter.IsEnabledFilter;
                PrintXMLButton.Enabled = Filter.WholeDriveItemsCount > 0 && IsFiltered;
                ButtonGenerateSC.Enabled = Filter.CanGenerateSC && !ExportSCPath.Text.IsNullOrEmptyTrim() && ROBPOperationsRadioButton.Checked && IsFiltered;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        void UpdateLastPath(string path)
        {
            if (path.IsNullOrEmpty())
                return;

            _lastDirPath = Directory.Exists(path) ? path : File.Exists(path) ? Path.GetDirectoryName(path) : _lastDirPath;
        }

        public void SaveData()
        {
            if (IsInititializating)
                return;

            lock (_syncSaveData)
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
                ReportMessage.Show(ex);
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
                ReportMessage.Show(ex);
                return false;
            }
        }

        static bool GetCellItemSelectedRows(DataGridView grid, out List<string> result, string name = "FilePath")
        {
            result = new List<string>();
            if (grid.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in grid.SelectedRows)
                {
                    if (TryGetCellValue(row, name, out var cellValue))
                        result.Add(cellValue.ToString());
                }

                result = result.Distinct().ToList();
            }
            else
            {
                ReportMessage.Show(Resources.Form_GridView_NotSelectedAnyRows, MessageBoxIcon.Warning);
                return false;
            }
            return result.Count > 0;
        }

        static bool TryGetCellValue(DataGridViewRow row, string cellName, out object result)
        {
            result = null;

            if (row == null)
                return false;

            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.OwningColumn.Name.Like(cellName) || cell.OwningColumn.HeaderText.Like(cellName))
                {
                    result = cell.Value;
                    return true;
                }
            }

            return false;
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