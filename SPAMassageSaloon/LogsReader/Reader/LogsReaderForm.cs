using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using LogsReader.Config;
using LogsReader.Properties;
using LogsReader.Reader.Forms;
using SPAMassageSaloon.Common;
using Utils;
using Utils.WinForm;
using Utils.WinForm.DataGridViewHelper;
using Utils.WinForm.Notepad;

namespace LogsReader.Reader
{
    public sealed partial class LogsReaderForm : UserControl, IUserForm
    {
        private readonly Func<DateTime> _getStartDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        private readonly Func<DateTime> _getEndDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

        private bool _oldDateStartChecked;
        private bool _oldDateEndChecked;
        private bool _settingsLoaded;

        private readonly ContextMenuStrip _contextTreeMainMenuStrip;
        private readonly TreeNode treeNodeServersGroup;
        private readonly TreeNode treeNodeTypesGroup;
        private readonly TreeNode treeNodeFolders;
        private readonly ToolStripStatusLabel _statusInfo;
        private readonly ToolStripStatusLabel _findedInfo;
        private readonly ToolStripStatusLabel _completedFilesStatus;
        private readonly ToolStripStatusLabel _totalFilesStatus;
        private readonly ToolStripStatusLabel _filtersCompleted1;
        private readonly ToolStripStatusLabel _filtersCompleted2;
        private readonly ToolStripStatusLabel _overallFound1;
        private readonly ToolStripStatusLabel _overallFound2;
        private readonly ToolTip _tooltip;
        private readonly Editor _message;
        private readonly Editor _traceMessage;


        /// <summary>
        /// Сохранить изменения в конфиг
        /// </summary>
        public event EventHandler OnSchemeChanged;

        /// <summary>
        /// Статус выполнения поиска
        /// </summary>
        public bool IsWorking { get; private set; }

        /// <summary>
        /// Юзерские настройки 
        /// </summary>
        public UserSettings UserSettings { get; private set; }

        /// <summary>
        /// Текущая схема настроек
        /// </summary>
        public LRSettingsScheme CurrentSettings { get; private set; }

        public DataTemplateCollection OverallResultList { get; private set; }

        public LogsReaderPerformer MainReader { get; private set; }

        public int Progress
        {
            get => IsWorking ? progressBar.Value : 100;
            private set => progressBar.Value = value;
        }

        public LogsReaderForm(LRSettingsScheme scheme)
        {
            if (scheme == null)
                throw new ArgumentNullException(nameof(scheme));

            InitializeComponent();

            #region Initialize StripStatus

            _tooltip = new ToolTip { InitialDelay = 50 };

            var statusStripItemsPaddingStart = new Padding(0, 2, 0, 2);
            var statusStripItemsPaddingMiddle = new Padding(-3, 2, 0, 2);
            var statusStripItemsPaddingEnd = new Padding(-3, 2, 1, 2);

            _filtersCompleted1 = new ToolStripStatusLabel { Font = Font, Margin = statusStripItemsPaddingStart };
            _completedFilesStatus = new ToolStripStatusLabel("0") { Font = Font, Margin = statusStripItemsPaddingMiddle };
            _filtersCompleted2 = new ToolStripStatusLabel { Font = Font, Margin = statusStripItemsPaddingMiddle };
            _totalFilesStatus = new ToolStripStatusLabel("0") { Font = Font, Margin = statusStripItemsPaddingEnd };
            statusStrip.Items.Add(_filtersCompleted1);
            statusStrip.Items.Add(_completedFilesStatus);
            statusStrip.Items.Add(_filtersCompleted2);
            statusStrip.Items.Add(_totalFilesStatus);

            _overallFound1 = new ToolStripStatusLabel { Font = Font, Margin = statusStripItemsPaddingStart };
            _findedInfo = new ToolStripStatusLabel("0") { Font = Font, Margin = statusStripItemsPaddingMiddle };
            _overallFound2 = new ToolStripStatusLabel { Font = Font, Margin = statusStripItemsPaddingEnd };
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(_overallFound1);
            statusStrip.Items.Add(_findedInfo);
            statusStrip.Items.Add(_overallFound2);

            statusStrip.Items.Add(new ToolStripSeparator());
            _statusInfo = new ToolStripStatusLabel("") { Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Margin = statusStripItemsPaddingStart };
            statusStrip.Items.Add(_statusInfo);

            #endregion

            try
            {
                CurrentSettings = scheme;
                CurrentSettings.ReportStatus += ReportStatus;
                UserSettings = new UserSettings(CurrentSettings.Name);

                dgvFiles.AutoGenerateColumns = false;
                dgvFiles.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
                dgvFiles.CellFormatting += DgvFiles_CellFormatting;
                orderByText.GotFocus += OrderByText_GotFocus;

                #region Initialize Controls

                _message = notepad.AddDocument(new BlankDocument {HeaderName = "Message", Language = Language.XML});
                _message.BackBrush = null;
                _message.BorderStyle = BorderStyle.FixedSingle;
                _message.Cursor = Cursors.IBeam;
                _message.DelayedEventsInterval = 1000;
                _message.DisabledColor = Color.FromArgb(100, 171, 171, 171);
                _message.IsReplaceMode = false;
                _message.SelectionColor = Color.FromArgb(50, 0, 0, 255);
                _message.LanguageChanged += Message_LanguageChanged;

                _traceMessage = notepad.AddDocument(new BlankDocument {HeaderName = "Trace"});
                _traceMessage.BackBrush = null;
                _traceMessage.BorderStyle = BorderStyle.FixedSingle;
                _traceMessage.Cursor = Cursors.IBeam;
                _traceMessage.DelayedEventsInterval = 1000;
                _traceMessage.DisabledColor = Color.FromArgb(100, 171, 171, 171);
                _traceMessage.IsReplaceMode = false;
                _traceMessage.SelectionColor = Color.FromArgb(50, 0, 0, 255);
                _traceMessage.LanguageChanged += TraceMessage_LanguageChanged;

                notepad.SelectEditor(0);
                notepad.DefaultEncoding = CurrentSettings.Encoding;
                notepad.WordWrapStateChanged += Notepad_WordWrapStateChanged;
                notepad.WordHighlightsStateChanged += Notepad_WordHighlightsStateChanged;

                dateStartFilter.ValueChanged += (sender, args) =>
                {
                    if (UserSettings != null)
                        UserSettings.DateStartChecked = dateStartFilter.Checked;

                    if (_oldDateStartChecked || !dateStartFilter.Checked)
                        return;
                    _oldDateStartChecked = true;
                    dateStartFilter.Value = _getStartDate.Invoke();
                };
                dateEndFilter.ValueChanged += (sender, args) =>
                {
                    if (UserSettings != null)
                        UserSettings.DateEndChecked = dateEndFilter.Checked;

                    if (_oldDateEndChecked || !dateEndFilter.Checked)
                        return;
                    _oldDateEndChecked = true;
                    dateEndFilter.Value = _getEndDate.Invoke();
                };

                #endregion

                #region Apply All Settings

                txtPattern.AssignValue(UserSettings.PreviousSearch, TxtPattern_TextChanged);
                useRegex.Checked = UserSettings.UseRegex;
                dateStartFilter.Checked = UserSettings.DateStartChecked;
                if (dateStartFilter.Checked)
                    dateStartFilter.Value = _getStartDate.Invoke();
                dateEndFilter.Checked = UserSettings.DateEndChecked;
                if (dateEndFilter.Checked)
                    dateEndFilter.Value = _getEndDate.Invoke();
                traceNameFilter.AssignValue(UserSettings.TraceNameFilter, TraceNameFilter_TextChanged);
                traceMessageFilter.AssignValue(UserSettings.TraceMessageFilter, TraceMessageFilter_TextChanged);

                var langMessage = UserSettings.MessageLanguage;
                var langTrace = UserSettings.TraceLanguage;
                if (_message.Language != langMessage)
                    _message.ChangeLanguage(langMessage);
                if (_traceMessage.Language != langTrace)
                    _traceMessage.ChangeLanguage(langTrace);

                _message.WordWrap = UserSettings.MessageWordWrap;
                _message.Highlights = UserSettings.MessageHighlights;
                _traceMessage.WordWrap = UserSettings.TraceWordWrap;
                _traceMessage.Highlights = UserSettings.TraceHighlights;

                #endregion

                #region Options

                maxLinesStackText.AssignValue(CurrentSettings.MaxLines, MaxLinesStackText_TextChanged);
                maxThreadsText.AssignValue(CurrentSettings.MaxThreads, MaxThreadsText_TextChanged);
                rowsLimitText.AssignValue(CurrentSettings.RowsLimit, RowsLimitText_TextChanged);
                orderByText.Text = CurrentSettings.OrderBy;

                #endregion

                #region TreeNode - Servers; FileTypes; Folders

                treeNodeServersGroup = GetGroupNodes(Resources.Txt_LogsReaderForm_Servers, CurrentSettings.ServerGroups);
                treeNodeServersGroup.Name = "trvServers";
                treeNodeServersGroup.Checked = false;
                CheckTreeViewNode(treeNodeServersGroup, false);

                treeNodeTypesGroup = GetGroupNodes(Resources.Txt_LogsReaderForm_Types, CurrentSettings.FileTypesGroups);
                treeNodeTypesGroup.Name = "trvTypes";
                treeNodeTypesGroup.Checked = false;
                CheckTreeViewNode(treeNodeTypesGroup, false);

                treeNodeFolders = new TreeNode(Resources.Txt_LogsReaderForm_LogsFolder)
                {
	                Name = "trvFolders", 
	                Text = Resources.Txt_LogsReaderForm_LogsFolder, 
	                Checked = true
                };
                treeNodeFolders.Expand();
                AddFolder(TreeMain.SelectedNode.Text, CurrentSettings.Folders, false);
                CheckTreeViewNode(treeNodeFolders, true);

                TreeMain.Nodes.AddRange(new[] { treeNodeServersGroup, treeNodeTypesGroup, treeNodeFolders });
                TreeMain.MouseDown += TreeMain_MouseDown;
                TreeMain.AfterCheck += TrvMain_AfterCheck;

                _contextTreeMainMenuStrip = new ContextMenuStrip
                {
	                Tag = TreeMain
                };
                _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddServerGroup, Resources.server_group, AddServerGroup);
                _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddServer, Resources.server, AddServer);
                _contextTreeMainMenuStrip.Items.Add(new ToolStripSeparator());
                _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddFileTypeGroup, Resources.types_group, AddFileTypeGroup);
                _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddFileType, Resources.type, AddFileType);
                _contextTreeMainMenuStrip.Items.Add(new ToolStripSeparator());
                _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddFolder, Resources.folder, AddFolder);
                _contextTreeMainMenuStrip.Items.Add(new ToolStripSeparator());
                _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_Properties, Resources.properies, OpenProperties);

                #endregion
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, Resources.Txt_Initialization);
            }
            finally
            {
                ClearForm(false);
                ValidationCheck(false);
            }
        }

        #region TreeNode - Servers; FileTypes; Folders

        void AddServerGroup(object sender, EventArgs args)
        {
	        SetTreeNodes(GroupType.Server, true);
        }

        void AddServer(object sender, EventArgs args)
        {
	        SetTreeNodes(GroupType.Server, false);
        }

        void AddFileTypeGroup(object sender, EventArgs args)
        {
	        SetTreeNodes(GroupType.FileType, true);
        }

        void AddFileType(object sender, EventArgs args)
        {
	        SetTreeNodes(GroupType.FileType, false);
        }

        void OpenProperties(object sender, EventArgs args)
        {
	        if (Compare(TreeMain.SelectedNode, treeNodeServersGroup))
		        SetTreeNodes(GroupType.Server, false);
	        else if (Compare(TreeMain.SelectedNode, treeNodeTypesGroup))
		        SetTreeNodes(GroupType.FileType, false);
	        else if (TreeMain.SelectedNode == treeNodeFolders)
		        AddFolder(this, EventArgs.Empty);
	        else
		        AddFolder(TreeMain.SelectedNode.Text, GetFolders(false), true);
        }

        static bool Compare(TreeNode treeNode, TreeNode toFind)
        {
	        var isCurrent = treeNode == toFind;
	        return isCurrent || (treeNode?.Parent != null && Compare(treeNode.Parent, toFind));
        }

        void SetTreeNodes(GroupType _groupType, bool isNewGroup)
        {
	        var treeNode = _groupType == GroupType.Server ? treeNodeServersGroup : treeNodeTypesGroup;

	        var treeGroups = treeNode
                .Nodes.OfType<TreeNode>()
		        .ToDictionary(x => x.Text, 
	                x => new List<string>(x.Nodes.OfType<TreeNode>().Select(p => p.Text)), StringComparer.InvariantCultureIgnoreCase);
	        var clone = new Dictionary<string, List<string>>(treeGroups, treeGroups.Comparer);

	        if (isNewGroup || treeNode.Nodes.Count == 0)
	        {
		        new AddGroupForm(treeGroups, _groupType).ShowDialog();
	        }
	        else
	        {
		        var groupName = treeGroups.First().Key;
		        if (TreeMain.SelectedNode?.Parent == treeNode)
			        groupName = TreeMain.SelectedNode.Text;
		        else if (TreeMain.SelectedNode?.Parent?.Parent == treeNode)
		            groupName = TreeMain.SelectedNode.Parent.Text;
		        
                AddGroupForm.ShowGroupItemsForm(groupName, treeGroups, _groupType);
            }

	        treeNode.Nodes.Clear();
	        var groupItems = new List<LRGroupItem>(treeGroups.Count);
            foreach (var newGroup in treeGroups)
	        {
		        var childTreeNode = GetGroupItems(newGroup.Key, newGroup.Value);
		        treeNode.Nodes.Add(childTreeNode);
		        groupItems.Add(new LRGroupItem(newGroup.Key, string.Join(", ", newGroup.Value)));

                if (!clone.TryGetValue(newGroup.Key, out var existGroup)
		            || existGroup.Except(newGroup.Value).Any()
		            || newGroup.Value.Except(existGroup).Any())
		        {
			        childTreeNode.Expand();
                    childTreeNode.Checked = true;
                    CheckTreeViewNode(childTreeNode, true);
                    TreeMain.SelectedNode = childTreeNode;
                }
	        }

            if (_groupType == GroupType.Server)
		        CurrentSettings.Servers = groupItems.ToArray();
	        else
		        CurrentSettings.FileTypes = groupItems.ToArray();

            ValidationCheck();
	        OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        void AddFolder(object sender, EventArgs e)
        {
	        AddFolder(null, GetFolders(false), true);
        }

        void AddFolder(string folderPath, Dictionary<string, bool> items, bool showForm)
        {
            var clone = new Dictionary<string, bool>(items, items.Comparer);
	        if (showForm)
	        {
		        var folderForm = new AddFolder(folderPath);
		        folderForm.ShowDialog();
		        if (folderForm.FolderPath.IsNullOrEmptyTrim())
			        return;

                if(items.TryGetValue(folderForm.FolderPath, out var allDirSearching))
                {
	                if (allDirSearching != folderForm.AllDirectoriesSearching)
		                items[folderForm.FolderPath] = folderForm.AllDirectoriesSearching;
                    else
                        return;
                }
                else
                {
	                items.Add(folderForm.FolderPath, folderForm.AllDirectoriesSearching);
                }
	        }


	        treeNodeFolders.Nodes.Clear();
            var foldersList = new List<LRFolder>();
            foreach (var folder in items.OrderBy(x => x.Key))
            {
	            var folderType = folder.Value ? @"[All]" : @"[Top]";
	            var childFolder = treeNodeFolders.Nodes.Add($"{folderType} {folder.Key}");

	            foldersList.Add(new LRFolder(folder.Key, folder.Value));

                if (!clone.TryGetValue(folder.Key, out var _))
                {
	                childFolder.Checked = true;
                    TreeMain.SelectedNode = childFolder;
                }
            }

            treeNodeFolders.Expand();

	        CurrentSettings.LogsFolder = foldersList.ToArray();

            ValidationCheck();
	        OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        Dictionary<string, bool> GetFolders(bool getOnlyChecked)
        {
	        var folders = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
	        foreach (var folderWithType in treeNodeFolders.Nodes.Cast<TreeNode>().Where(x => !getOnlyChecked || x.Checked).Select(x => x.Text))
	        {
		        var folder = folderWithType.Substring(5, folderWithType.Length - 5).Trim();
		        if (folders.TryGetValue(folder, out var type))
		        {
			        if (type)
				        folders[folder] = true;
		        }
		        else
		        {
			        folders.Add(folder, folder.Substring(0, 5).Equals("[All]", StringComparison.InvariantCultureIgnoreCase));
		        }
	        }

	        return folders;
        }

        void DeleteTreeItem()
        {
	        if (!TreeMain.Enabled
	            || TreeMain.SelectedNode == treeNodeServersGroup
		        || TreeMain.SelectedNode == treeNodeTypesGroup
		        || TreeMain.SelectedNode == treeNodeFolders)
		        return;

            if(TreeMain.SelectedNode.Parent == treeNodeFolders)
            {
	            treeNodeFolders.Nodes.Remove(TreeMain.SelectedNode);
            }

	        ValidationCheck();
	        OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        static TreeNode GetGroupNodes(string name, Dictionary<string, IEnumerable<string>> groups)
        {
	        var treeNode = new TreeNode(name);
            foreach (var groupItem in groups)
		        treeNode.Nodes.Add(GetGroupItems(groupItem.Key, groupItem.Value));
	        treeNode.Expand();
	        return treeNode;
        }

        static TreeNode GetGroupItems(string name, IEnumerable<string> items)
        {
	        var treeNode = new TreeNode(name);
            foreach (var item in items.Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(p => p))
	            treeNode.Nodes.Add(item.Trim());
            return treeNode;
        }

        private void TreeMain_MouseDown(object sender, MouseEventArgs e)
        {
	        if (e.Button != MouseButtons.Right)
                return;

	        //var selectedNode = TreeMain.SelectedNode;
	        //var nodeAt = TreeMain.GetNodeAt(e.X, e.Y);

            _contextTreeMainMenuStrip?.Show(TreeMain, e.Location);
        }

        #endregion

        public void ApplyFormSettings()
        {
            try
            {
                if(UserSettings == null)
                    return;

                for (var i = 0; i < dgvFiles.Columns.Count; i++)
                {
                    var valueStr = UserSettings.GetValue("COL" + i);
                    if (!valueStr.IsNullOrEmptyTrim() && int.TryParse(valueStr, out var value) && value > 1 && value <= 1000)
                        dgvFiles.Columns[i].Width = value;
                }

                ParentSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(ParentSplitContainer), 25, 1000, ParentSplitContainer.SplitterDistance);
                MainSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(MainSplitContainer), 25, 1000, MainSplitContainer.SplitterDistance);
                EnumSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(EnumSplitContainer), 25, 1000, EnumSplitContainer.SplitterDistance);
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        public void ApplySettings()
        {
            try
            { 
	            #region Change Language

                treeNodeServersGroup.Text = Resources.Txt_LogsReaderForm_Servers;
                treeNodeTypesGroup.Text = Resources.Txt_LogsReaderForm_Types;
                treeNodeFolders.Text = Resources.Txt_LogsReaderForm_LogsFolder;

                _filtersCompleted1.Text = Resources.Txt_LogsReaderForm_FilesCompleted_1;
                _filtersCompleted2.Text = Resources.Txt_LogsReaderForm_FilesCompleted_2;
                _overallFound1.Text = Resources.Txt_LogsReaderForm_OverallFound_1;
                _overallFound2.Text = Resources.Txt_LogsReaderForm_OverallFound_2;

                traceNameFilterComboBox.Items.Clear();
                traceNameFilterComboBox.Items.Add(Resources.Txt_LogsReaderForm_Contains);
                traceNameFilterComboBox.Items.Add(Resources.Txt_LogsReaderForm_NotContains);
                if (UserSettings != null)
                    traceNameFilterComboBox.AssignValue(UserSettings.TraceNameFilterContains ? Resources.Txt_LogsReaderForm_Contains : Resources.Txt_LogsReaderForm_NotContains,
                        traceNameFilterComboBox_SelectedIndexChanged);

                traceMessageFilterComboBox.Items.Clear();
                traceMessageFilterComboBox.Items.Add(Resources.Txt_LogsReaderForm_Contains);
                traceMessageFilterComboBox.Items.Add(Resources.Txt_LogsReaderForm_NotContains);
                if (UserSettings != null)
                    traceMessageFilterComboBox.AssignValue(
                        UserSettings.TraceMessageFilterContains ? Resources.Txt_LogsReaderForm_Contains : Resources.Txt_LogsReaderForm_NotContains,
                        traceMessageFilterComboBox_SelectedIndexChanged);

                _tooltip.RemoveAll();
                _tooltip.SetToolTip(txtPattern, Resources.Txt_Form_SearchComment);
                _tooltip.SetToolTip(useRegex, Resources.Txt_LRSettings_UseRegexComment);
                _tooltip.SetToolTip(maxThreadsText, Resources.Txt_LRSettingsScheme_MaxThreads);
                _tooltip.SetToolTip(maxLinesStackText, Resources.Txt_LRSettingsScheme_MaxTraceLines);
                _tooltip.SetToolTip(rowsLimitText, Resources.Txt_LRSettingsScheme_RowsLimit);
                _tooltip.SetToolTip(orderByText, Resources.Txt_LRSettingsScheme_OrderBy);
                _tooltip.SetToolTip(TreeMain, Resources.Txt_Form_trvMainComment);
                _tooltip.SetToolTip(dateStartFilter, Resources.Txt_Form_DateFilterComment);
                _tooltip.SetToolTip(dateEndFilter, Resources.Txt_Form_DateFilterComment);
                _tooltip.SetToolTip(traceNameFilter, Resources.Txt_Form_TraceNameFilterComment);
                _tooltip.SetToolTip(traceMessageFilter, Resources.Txt_Form_TraceFilterComment);
                _tooltip.SetToolTip(alreadyUseFilter, Resources.Txt_Form_AlreadyUseFilterComment);
                _tooltip.SetToolTip(buttonExport, Resources.Txt_LogsReaderForm_ExportComment);

                OrderByLabel.Text = Resources.Txt_LogsReaderForm_OrderBy;
                RowsLimitLabel.Text = Resources.Txt_LogsReaderForm_RowsLimit;
                MaxThreadsLabel.Text = Resources.Txt_LogsReaderForm_MaxThreads;
                MaxLinesLabel.Text = Resources.Txt_LogsReaderForm_MaxLines;
                useRegex.Text = Resources.Txt_LogsReaderForm_UseRegex;

                btnSearch.Text = Resources.Txt_LogsReaderForm_Search;
                btnClear.Text = Resources.Txt_LogsReaderForm_Clear;
                btnClear.Size = new Size(Convert.ToInt32(Resources.LogsReaderForm_btnClear_Width), btnClear.Height);
                buttonFilter.Text = Resources.Txt_LogsReaderForm_Filter;
                buttonFilter.Padding = new Padding(3, 0, Convert.ToInt32(Resources.LogsReaderForm_buttonFilter_rightPadding), 0);
                buttonReset.Text = Resources.Txt_LogsReaderForm_Reset;
                buttonReset.Padding = new Padding(2, 0, Convert.ToInt32(Resources.LogsReaderForm_buttonReset_rightPadding), 0);
                buttonExport.Text = Resources.Txt_LogsReaderForm_Export;
                buttonExport.Size = new Size(Convert.ToInt32(Resources.LogsReaderForm_buttonExport_Width), buttonExport.Height);
                alreadyUseFilter.Text = Resources.Txt_LogsReaderForm_UseFilterWhenSearching;
                alreadyUseFilter.Padding = new Padding(0, 0, Convert.ToInt32(Resources.LogsReaderForm_alreadyUseFilter_rightPadding), 0);

                #endregion

                ReportStatus(string.Empty, ReportStatusType.Success);
                ValidationCheck(false);
            }
            catch (Exception ex)
            {
                ValidationCheck(false);
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
            finally
            {
                _settingsLoaded = true;
            }
        }

        public void SaveData()
        {
            try
            {
                if (!_settingsLoaded || UserSettings == null)
                    return;

                for (var i = 0; i < dgvFiles.Columns.Count; i++)
                {
                    UserSettings.SetValue("COL" + i, dgvFiles.Columns[i].Width);
                }

                UserSettings.SetValue(nameof(ParentSplitContainer), ParentSplitContainer.SplitterDistance);
                UserSettings.SetValue(nameof(MainSplitContainer), MainSplitContainer.SplitterDistance);
                UserSettings.SetValue(nameof(EnumSplitContainer), EnumSplitContainer.SplitterDistance);
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        public void LogsReaderKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.F5 when btnSearch.Enabled && !IsWorking:
                        BtnSearch_Click(this, EventArgs.Empty);
                        break;
                    case Keys.Escape when btnSearch.Enabled && IsWorking:
                        BtnSearch_Click(this, EventArgs.Empty);
                        break;
                    case Keys.F6 when btnClear.Enabled:
                        ClearForm();
                        break;
                    case Keys.F7 when buttonFilter.Enabled:
                        buttonFilter_Click(this, EventArgs.Empty);
                        break;
                    case Keys.F8 when buttonReset.Enabled:
                        buttonReset_Click(this, EventArgs.Empty);
                        break;
                    case Keys.S when e.Control && buttonExport.Enabled:
                        ButtonExport_Click(this, EventArgs.Empty);
                        break;


                    case Keys.Delete:
	                    DeleteTreeItem();
                        break;
                    case Keys.G when e.Control && TreeMain.Enabled:
	                    AddServerGroup(this, EventArgs.Empty);
	                    break;
                    case Keys.R when e.Control && TreeMain.Enabled:
	                    AddServer(this, EventArgs.Empty);
                        break;
                    case Keys.H when e.Control && TreeMain.Enabled:
	                    AddFileTypeGroup(this, EventArgs.Empty);
                        break;
                    case Keys.T when e.Control && TreeMain.Enabled:
	                    AddFileType(this, EventArgs.Empty);
                        break;
                    case Keys.F when e.Control && TreeMain.Enabled:
	                    AddFolder(this, EventArgs.Empty);
                        break;
                    case Keys.P when e.Control && TreeMain.Enabled:
	                    OpenProperties(this, EventArgs.Empty);
	                    break;
                    case Keys.C when e.Control && dgvFiles.SelectedRows.Count > 0 && OverallResultList != null:
                        var templateList = new List<DataTemplate>();
                        foreach (DataGridViewRow row in dgvFiles.SelectedRows)
                        {
                            if (TryGetTemplate(row, out var template))
                                templateList.Insert(0, template);
                        }

                        var clipboardText = new StringBuilder(templateList.Count);
                        foreach (var template in templateList)
                        {
                            clipboardText.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\r\n",
                                template.ID,
                                template.ParentReader.FilePath,
                                template.DateOfTrace,
                                template.TraceName,
                                template.Description,
                                notepad.SelectedIndex <= 0 ? template.Message.Trim() : template.TraceMessage.Trim());
                        }

                        Clipboard.SetText(clipboardText.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            if (!IsWorking)
            {
                var stop = new Stopwatch();
                try
                {
                    var filter = alreadyUseFilter.Checked ? GetFilter() : null;

                    var servers = new List<string>();
                    foreach (TreeNode childTreeNode in treeNodeServersGroup.Nodes)
	                    servers.AddRange(childTreeNode.Nodes.Cast<TreeNode>().Where(x => x.Checked).Select(x => x.Text));
                    servers = servers.GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase).Select(x => x.Key).ToList();

                    var fileTypes = new List<string>();
                    foreach (TreeNode childTreeNode in treeNodeTypesGroup.Nodes)
	                    fileTypes.AddRange(childTreeNode.Nodes.Cast<TreeNode>().Where(x => x.Checked).Select(x => x.Text));
                    fileTypes = fileTypes.GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase).Select(x => x.Key).ToList();

                    var folders = GetFolders(true);

                    MainReader = new LogsReaderPerformer(CurrentSettings, servers, fileTypes, folders, txtPattern.Text, useRegex.Checked, filter);
                    MainReader.OnProcessReport += ReportProcessStatus;

                    stop.Start();
                    IsWorking = true;
                    ChangeFormStatus();
                    ReportStatus(Resources.Txt_LogsReaderForm_Working, ReportStatusType.Success);

                    await MainReader.StartAsync();

                    OverallResultList = new DataTemplateCollection(CurrentSettings, MainReader.ResultsOfSuccess);
                    OverallResultList.AddRange(MainReader.ResultsOfError.OrderBy(x => x.Date));

                    if (await AssignResult(filter))
                    {
                        ReportStatus(string.Format(Resources.Txt_LogsReaderForm_FinishedIn, stop.Elapsed.ToReadableString()), ReportStatusType.Success);
                    }

                    stop.Stop();
                }
                catch (Exception ex)
                {
                    ReportStatus(ex.Message, ReportStatusType.Error);
                }
                finally
                {
                    if (MainReader != null)
                    {
                        MainReader.OnProcessReport -= ReportProcessStatus;
                        MainReader.Dispose();
                        MainReader = null;
                    }

                    IsWorking = false;
                    ChangeFormStatus();
                    if (stop.IsRunning)
                        stop.Stop();
                }
            }
            else
            {
                MainReader?.Stop();
                ReportStatus(Resources.Txt_LogsReaderForm_Stopping, ReportStatusType.Success);
            }
        }

        void ReportProcessStatus(int countMatches, int percentOfProgeress, int filesCompleted, int totalFiles)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    _findedInfo.Text = countMatches.ToString();
                    Progress = percentOfProgeress;
                    _completedFilesStatus.Text = filesCompleted.ToString();
                    _totalFilesStatus.Text = totalFiles.ToString();
                }));
            }
            else
            {
                _findedInfo.Text = countMatches.ToString();
                Progress = percentOfProgeress;
                _completedFilesStatus.Text = filesCompleted.ToString();
                _totalFilesStatus.Text = totalFiles.ToString();
            }
        }

        private async void ButtonExport_Click(object sender, EventArgs e)
        {
            if (IsWorking)
                return;

            var fileName = string.Empty;
            try
            { 
                string desctination;
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = @"CSV files (*.csv)|*.csv";
                    if (sfd.ShowDialog() != DialogResult.OK)
                        return;
                    desctination = sfd.FileName;
                }

                btnSearch.Enabled = btnClear.Enabled = buttonExport.Enabled = buttonFilter.Enabled = buttonReset.Enabled = false;
                ReportStatus(Resources.Txt_LogsReaderForm_Exporting, ReportStatusType.Success);
                fileName = Path.GetFileName(desctination);

                int i = 0;
                Progress = 0;
                using (var writer = new StreamWriter(desctination, false, new UTF8Encoding(false)))
                {
                    await writer.WriteLineAsync(GetCSVRow(new[] {"ID", "File", "Date", "Trace name", "Description", notepad.SelectedIndex <= 0 ? "Message" : "Trace Message" }));
                    foreach (DataGridViewRow row in dgvFiles.Rows)
                    {
                        if(!TryGetTemplate(row, out var template))
                            continue;

                        await writer.WriteLineAsync(GetCSVRow(new[]
                        {
                            template.ID.ToString(),
                            template.ParentReader.FilePath,
                            template.DateOfTrace,
                            template.TraceName,
                            $"\"{template.Description}\"",
                            notepad.SelectedIndex <= 0 ? $"\"{template.Message.Trim()}\"" : $"\"{template.TraceMessage.Trim()}\""
                        }));
                        writer.Flush();

                        Progress = (int)Math.Round((double)(100 * ++i) / dgvFiles.RowCount);
                    }
                    writer.Close();
                }

                ReportStatus(string.Format(Resources.Txt_LogsReaderForm_SuccessExport, fileName), ReportStatusType.Success);
            }
            catch (Exception ex)
            {
                ReportStatus(string.Format(Resources.Txt_LogsReaderForm_ErrExport, fileName, ex.Message), ReportStatusType.Error);
            }
            finally
            {
                Progress = 100;
                btnClear.Enabled = true;
                ValidationCheck(false);
                dgvFiles.Focus();
            }
        }

        static string GetCSVRow(IReadOnlyCollection<string> @params)
        {
            var builder = new StringBuilder(@params.Count);
            foreach (var param in @params)
            {
                if (param.IsNullOrEmpty())
                {
                    builder.Append(";");
                }
                else if (param.StartsWith("\"") && param.EndsWith("\""))
                {
                    if (param.IndexOf("\"", 1, param.Length - 2, StringComparison.Ordinal) != -1)
                    {
                        var test = param.Substring(1, param.Length - 2).Replace("\"", "\"\"");
                        builder.Append($"\"{test}\";");
                    }
                    else
                    {
                        builder.Append($"{param};");
                    }
                }
                else if (param.Contains("\""))
                {
                    builder.Append($"\"{param.Replace("\"", "\"\"")}\";");
                }
                else
                {
                    builder.Append($"{param};");
                }
            }

            return builder.ToString();
        }

        private async void buttonFilter_Click(object sender, EventArgs e)
        {
            try
            {
                await AssignResult(GetFilter());
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        DataFilter GetFilter()
        {
            return new DataFilter(dateStartFilter.Checked ? dateStartFilter.Value : DateTime.MinValue,
                dateEndFilter.Checked ? dateEndFilter.Value : DateTime.MaxValue,
                traceNameFilter.Text,
                traceNameFilterComboBox.Text.Like(Resources.Txt_LogsReaderForm_Contains),
                traceMessageFilter.Text,
                traceMessageFilterComboBox.Text.Like(Resources.Txt_LogsReaderForm_Contains));
        }

        private async void buttonReset_Click(object sender, EventArgs e)
        {
            await AssignResult(null);
        }

        async Task<bool> AssignResult(DataFilter filter)
        {
            ClearDGV();
            ClearErrorStatus();

            if (OverallResultList == null)
                return false;

            IEnumerable<DataTemplate> result = new List<DataTemplate>(OverallResultList);

            if (!result.Any())
            {
                ReportStatus(Resources.Txt_LogsReaderForm_NoLogsFound, ReportStatusType.Warning);
                return false;
            }

            if (filter != null)
            {
                result = filter.FilterCollection(result);

                if (!result.Any())
                {
                    ReportStatus(Resources.Txt_LogsReaderForm_NoFilterResultsFound, ReportStatusType.Warning);
                    return false;
                }
            }

            await dgvFiles.AssignCollectionAsync(result, null);

            buttonExport.Enabled = dgvFiles.RowCount > 0;
            return true;
        }

        void ChangeFormStatus()
        {
            btnSearch.Text = IsWorking ? Resources.Txt_LogsReaderForm_Stop : Resources.Txt_LogsReaderForm_Search;
            btnClear.Enabled = !IsWorking;
            TreeMain.Enabled = !IsWorking;
            txtPattern.Enabled = !IsWorking;

            foreach (var dgvChild in dgvFiles.Controls.OfType<Control>()) // решает баг с задисейбленным скролл баром DataGridView
                dgvChild.Enabled = !IsWorking;
            dgvFiles.Enabled = !IsWorking;

            notepad.Enabled = !IsWorking;
            descriptionText.Enabled = !IsWorking;
            useRegex.Enabled = !IsWorking;
            maxThreadsText.Enabled = !IsWorking;
            rowsLimitText.Enabled = !IsWorking;
            maxLinesStackText.Enabled = !IsWorking;
            dateStartFilter.Enabled = !IsWorking;
            dateEndFilter.Enabled = !IsWorking;
            traceNameFilterComboBox.Enabled = !IsWorking;
            traceNameFilter.Enabled = !IsWorking;
            traceMessageFilterComboBox.Enabled = !IsWorking;
            traceMessageFilter.Enabled = !IsWorking;
            alreadyUseFilter.Enabled = !IsWorking;
            orderByText.Enabled = !IsWorking;
            buttonExport.Enabled = dgvFiles.RowCount > 0;
            buttonFilter.Enabled = buttonReset.Enabled = OverallResultList != null && OverallResultList.Count > 0;

            if (IsWorking)
            {
                ParentSplitContainer.Cursor = Cursors.WaitCursor;
                ClearForm();
                Focus();
            }
            else
            {
                //foreach (var pb in ParentSplitContainer.Controls.OfType<Control>())
                //    pb.Enabled = true;

                ParentSplitContainer.Cursor = Cursors.Default;
                dgvFiles.Focus();
            }
        }

        private void DgvFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                var row = ((DataGridView) sender).Rows[e.RowIndex];
                if(!TryGetTemplate(row, out var template))
                    return;

                if (template.IsMatched)
                {
                    if (template.Date == null)
                    {
                        row.DefaultCellStyle.BackColor = Color.Yellow;
                        foreach (DataGridViewCell cell2 in row.Cells)
                            cell2.ToolTipText = Resources.Txt_LogsReaderForm_DateValueIsIncorrect;
                        return;
                    }

                    row.DefaultCellStyle.BackColor = Color.White;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.LightPink;
                    foreach (DataGridViewCell cell2 in row.Cells)
                        cell2.ToolTipText = Resources.Txt_LogsReaderForm_DoesntMatchByPattern;
                }
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private void DgvFiles_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                _message.Text = string.Empty;
                _traceMessage.Text = string.Empty;

                if (dgvFiles.CurrentRow == null || dgvFiles.SelectedRows.Count == 0)
                    return;

                if(!TryGetTemplate(dgvFiles.SelectedRows[0], out var template))
                    return;

                descriptionText.Text = $"FoundLineID:{template.FoundLineID}\r\n{template.Description}";

                if (_message.Language == Language.XML || _message.Language == Language.HTML)
                {
                    var messageXML = XML.RemoveUnallowable(template.Message, " ");
                    _message.Text = messageXML.IsXml(out var xmlDoc) ? xmlDoc.PrintXml() : messageXML.TrimWhiteSpaces();
                }
                else
                {
                    _message.Text = template.Message.TrimWhiteSpaces();
                }
                _message.DelayedEventsInterval = 10;

                _traceMessage.Text = template.TraceMessage;
                _traceMessage.DelayedEventsInterval = 10;
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
            finally
            {
                dgvFiles.Focus();
            }
        }

        bool TryGetTemplate(DataGridViewRow row, out DataTemplate template)
        {
            template = null;
            if (OverallResultList == null)
                return false;
            var privateID = (int)(row?.Cells["PrivateID"]?.Value ?? -1);
            if (privateID <= -1)
                return false;
            template = OverallResultList[privateID];
            return template != null;
        }

        private void DgvFiles_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button != MouseButtons.Right)
                    return;
                var hti = dgvFiles.HitTest(e.X, e.Y);
                if (hti.RowIndex == -1)
                    return;
                dgvFiles.ClearSelection();
                dgvFiles.Rows[hti.RowIndex].Selected = true;
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private void TrvMain_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 0)
                CheckTreeViewNode(e.Node, e.Node.Checked);
            ValidationCheck();
        }

        private void MaxLinesStackText_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(maxLinesStackText.Text, out var value))
                MaxLinesStackTextSave(value);
        }

        private void MaxLinesStackText_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(maxLinesStackText.Text, out var value))
                value = -1;
            MaxLinesStackTextSave(value);
        }

        void MaxLinesStackTextSave(int value)
        {
            CurrentSettings.MaxLines = value;
            maxLinesStackText.AssignValue(CurrentSettings.MaxLines, MaxLinesStackText_TextChanged);
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MaxThreadsText_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(maxThreadsText.Text, out var value))
                MaxThreadsTextSave(value);
        }

        private void MaxThreadsText_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(maxThreadsText.Text, out var value))
                value = -1;
            MaxThreadsTextSave(value);
        }

        void MaxThreadsTextSave(int value)
        {
            CurrentSettings.MaxThreads = value;
            maxThreadsText.AssignValue(CurrentSettings.MaxThreads, MaxThreadsText_TextChanged);
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RowsLimitText_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(rowsLimitText.Text, out var value))
                RowsLimitTextSave(value);
        }

        private void RowsLimitText_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(rowsLimitText.Text, out var value))
                value = -1;
            RowsLimitTextSave(value);
        }

        void RowsLimitTextSave(int value)
        {
            CurrentSettings.RowsLimit = value;
            rowsLimitText.AssignValue(CurrentSettings.RowsLimit, RowsLimitText_TextChanged);
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OrderByText_Leave(object sender, EventArgs e)
        {
            CurrentSettings.OrderBy = orderByText.Text;
            ValidationCheck(CurrentSettings.OrderBy.Equals(orderByText.Text, StringComparison.InvariantCultureIgnoreCase));
            orderByText.Text = CurrentSettings.OrderBy;
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OrderByText_GotFocus(object sender, EventArgs e)
        {
            LRSettingsScheme.CheckOrderByItem(orderByText.Text);
            ValidationCheck(CurrentSettings.OrderBy.Equals(orderByText.Text, StringComparison.InvariantCultureIgnoreCase));
        }

        private static void CheckTreeViewNode(TreeNode node, bool isChecked)
        {
            foreach (TreeNode item in node.Nodes)
            {
                item.Checked = isChecked;
                if (item.Nodes.Count > 0)
                    CheckTreeViewNode(item, isChecked);
            }
        }

        private void TxtPattern_TextChanged(object sender, EventArgs e)
        {
            UserSettings.PreviousSearch = txtPattern.Text;
            ValidationCheck();
        }

        private void UseRegex_CheckedChanged(object sender, EventArgs e)
        {
            UserSettings.UseRegex = useRegex.Checked;
        }

        private void ComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void TraceNameFilter_TextChanged(object sender, EventArgs e)
        {
            UserSettings.TraceNameFilter = traceNameFilter.Text;
        }

        private void traceNameFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UserSettings.TraceNameFilterContains = traceNameFilterComboBox.Text.Like(Resources.Txt_LogsReaderForm_Contains);
        }

        private void TraceMessageFilter_TextChanged(object sender, EventArgs e)
        {
            UserSettings.TraceMessageFilter = traceMessageFilter.Text;
        }

        private void traceMessageFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UserSettings.TraceMessageFilterContains = traceMessageFilterComboBox.Text.Like(Resources.Txt_LogsReaderForm_Contains);
        }

        private void Message_LanguageChanged(object sender, EventArgs e)
        {
            var prev = UserSettings.MessageLanguage;
            UserSettings.MessageLanguage = _message.Language;
            if((prev == Language.HTML || prev == Language.XML) && (_message.Language == Language.XML || _message.Language == Language.HTML))
                return;
            DgvFiles_SelectionChanged(this, EventArgs.Empty);
        }

        private void TraceMessage_LanguageChanged(object sender, EventArgs e)
        {
            UserSettings.TraceLanguage = _traceMessage.Language;
        }

        private void Notepad_WordWrapStateChanged(object sender, EventArgs e)
        {
            if (!(sender is Editor editor))
                return;

            if (editor == _message)
                UserSettings.MessageWordWrap = editor.WordWrap;
            else if (editor == _traceMessage)
                UserSettings.TraceWordWrap = editor.WordWrap;
        }

        private void Notepad_WordHighlightsStateChanged(object sender, EventArgs e)
        {
            if (!(sender is Editor editor))
                return;

            if (editor == _message)
                UserSettings.MessageHighlights = editor.Highlights;
            else if (editor == _traceMessage)
                UserSettings.TraceHighlights = editor.Highlights;
        }

        void ValidationCheck(bool clearStatus = true)
        {
            try
            {
	            var settIsCorrect = CurrentSettings?.IsCorrect ?? false;
                if (settIsCorrect && clearStatus)
                    ClearErrorStatus();

                btnSearch.Enabled = settIsCorrect
                                    && !txtPattern.Text.IsNullOrEmpty()
                                    && treeNodeFolders.Nodes.OfType<TreeNode>().Any(x => x.Checked)
                                    && treeNodeServersGroup.Nodes.Cast<TreeNode>().Any(x => x.Nodes.OfType<TreeNode>().Any(x2 => x2.Checked))
                                    && treeNodeTypesGroup.Nodes.Cast<TreeNode>().Any(x => x.Nodes.OfType<TreeNode>().Any(x2 => x2.Checked));

                buttonExport.Enabled = dgvFiles.RowCount > 0;
                buttonFilter.Enabled = buttonReset.Enabled = OverallResultList != null && OverallResultList.Count > 0;
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        void ClearForm(bool saveData = true)
        {
            try
            {
                if (saveData)
                    SaveData();

                OverallResultList?.Clear();
                OverallResultList = null;

                ClearDGV();

                Progress = 0;

                _completedFilesStatus.Text = @"0";
                _totalFilesStatus.Text = @"0";
                _findedInfo.Text = @"0";

                ReportStatus(string.Empty, ReportStatusType.Success);

                STREAM.GarbageCollect();
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        void ClearDGV()
        {
            try
            {
                dgvFiles.DataSource = null;
                dgvFiles.Rows.Clear();
                dgvFiles.Refresh();
                descriptionText.Text = string.Empty;
                if (_message != null)
                    _message.Text = string.Empty;
                if (_traceMessage != null)
                    _traceMessage.Text = string.Empty;
                buttonExport.Enabled = false;
                buttonFilter.Enabled = buttonReset.Enabled = OverallResultList != null && OverallResultList.Count > 0;
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private bool _isLastWasError;

        void ReportStatus(string message, ReportStatusType type)
        {
            if (!message.IsNullOrEmpty())
            {
                _statusInfo.BackColor = type == ReportStatusType.Error ? Color.Red : type == ReportStatusType.Warning ? Color.Yellow : Color.Green;
                _statusInfo.ForeColor = type == ReportStatusType.Warning ? Color.Black : Color.White;
                _statusInfo.Text = message.Replace("\r", "").Replace("\n", " ");
            }
            else
            {
                _statusInfo.BackColor = SystemColors.Control;
                _statusInfo.ForeColor = Color.Black;
                _statusInfo.Text = string.Empty;
            }
            _isLastWasError = type == ReportStatusType.Error || type == ReportStatusType.Warning;
        }

        void ClearErrorStatus()
        {
            if (!_isLastWasError)
                return;
            _statusInfo.BackColor = SystemColors.Control;
            _statusInfo.ForeColor = Color.Black;
            _statusInfo.Text = string.Empty;
        }

        public override string ToString()
        {
            return CurrentSettings?.ToString() ?? base.ToString();
        }
    }

    public enum ReportStatusType
    {
        Success = 0,
        Warning = 1,
        Error = 2
    }
}