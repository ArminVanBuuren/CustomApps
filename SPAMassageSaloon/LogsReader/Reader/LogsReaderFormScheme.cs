﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using SPAMassageSaloon.Common;
using Utils;
using Utils.WinForm;

namespace LogsReader.Reader
{
	public sealed class LogsReaderFormScheme : LogsReaderFormBase
    {
	    private readonly Label OrderByLabel;
	    private readonly Label MaxLinesLabel;
	    private readonly TextBox orderByText;
	    private readonly Label MaxThreadsLabel;
	    private readonly TextBox maxLinesStackText;
	    private readonly Label RowsLimitLabel;
	    private readonly TextBox maxThreadsText;
	    private readonly TextBox rowsLimitText;
	    private readonly CustomTreeView TreeMain;

	    /// <summary>
		/// Сохранить изменения в конфиг
		/// </summary>
		public event EventHandler OnSchemeChanged;

	    /// <summary>
		/// Поиск логов начался или завершился
		/// </summary>
        public event EventHandler OnSearchChanged;

        /// <summary>
        /// Текущая схема настроек
        /// </summary>
        public LRSettingsScheme CurrentSettings { get; private set; }

        public LogsReaderPerformerScheme MainReader { get; private set; }

        public TreeViewContainer TreeViewContainer { get; }

        public DataTemplateCollection OverallResultList { get; private set; }

        public override bool HasAnyResult => OverallResultList != null && OverallResultList.Count > 0;

        public bool IsInitialized { get; private set; } = false;

		public LogsReaderFormScheme(LRSettingsScheme scheme) : base(scheme.Encoding, new UserSettings(scheme.Name))
        {
	        try
	        {
		        CurrentSettings = scheme;
		        CurrentSettings.ReportStatus += ReportStatus;

		        #region Initialize Controls

		        OrderByLabel = new Label
		        {
			        AutoSize = true,
			        Location = new Point(3, 88),
			        Name = "OrderByLabel",
			        Size = new Size(53, 15),
			        TabIndex = 17,
			        Text = Resources.Txt_LogsReaderForm_OrderBy
		        };

		        MaxLinesLabel = new Label
		        {
			        AutoSize = true,
			        Location = new Point(3, 10),
			        Name = "MaxLinesLabel",
			        Size = new Size(59, 15),
			        TabIndex = 13,
			        Text = Resources.Txt_LogsReaderForm_MaxLines
		        };

		        orderByText = new TextBox
		        {
			        Anchor = (AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right,
			        Location = new Point(84, 85),
			        Name = "orderByText",
			        Size = new Size(122, 23),
			        TabIndex = 18
		        };
		        orderByText.Leave += OrderByText_Leave;

		        MaxThreadsLabel = new Label
		        {
			        AutoSize = true,
			        Location = new Point(3, 36),
			        Name = "MaxThreadsLabel",
			        Size = new Size(74, 15),
			        TabIndex = 9,
			        Text = Resources.Txt_LogsReaderForm_MaxThreads
		        };

		        maxLinesStackText = new TextBox
		        {
			        Anchor = (AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right,
			        Location = new Point(84, 7),
			        Name = "maxLinesStackText",
			        Size = new Size(122, 23),
			        TabIndex = 15
		        };
		        maxLinesStackText.TextChanged += MaxLinesStackText_TextChanged;
		        maxLinesStackText.Leave += MaxLinesStackText_Leave;

		        RowsLimitLabel = new Label
		        {
			        AutoSize = true,
			        Location = new Point(3, 62),
			        Name = "RowsLimitLabel",
			        Size = new Size(65, 15),
			        TabIndex = 15,
			        Text = Resources.Txt_LogsReaderForm_RowsLimit
		        };

		        maxThreadsText = new TextBox
		        {
			        Anchor = (AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right,
			        Location = new Point(84, 33),
			        Name = "maxThreadsText",
			        Size = new Size(122, 23),
			        TabIndex = 16
		        };
		        maxThreadsText.TextChanged += MaxThreadsText_TextChanged;
		        maxThreadsText.Leave += MaxThreadsText_Leave;

		        rowsLimitText = new TextBox
		        {
			        Anchor = (AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right,
			        Location = new Point(84, 59),
			        Name = "rowsLimitText",
			        Size = new Size(122, 23),
			        TabIndex = 17
		        };
		        rowsLimitText.TextChanged += RowsLimitText_TextChanged;
		        rowsLimitText.Leave += RowsLimitText_Leave;

		        TreeMain = new CustomTreeView
		        {
			        Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right,
			        Location = new Point(1, 114),
			        Name = "TreeMain",
			        Size = new Size(208, 352),
			        TabIndex = 19
		        };

		        MainSplitContainer.Panel1.Controls.Add(OrderByLabel);
		        MainSplitContainer.Panel1.Controls.Add(MaxLinesLabel);
		        MainSplitContainer.Panel1.Controls.Add(orderByText);
		        MainSplitContainer.Panel1.Controls.Add(MaxThreadsLabel);
		        MainSplitContainer.Panel1.Controls.Add(maxLinesStackText);
		        MainSplitContainer.Panel1.Controls.Add(RowsLimitLabel);
		        MainSplitContainer.Panel1.Controls.Add(maxThreadsText);
		        MainSplitContainer.Panel1.Controls.Add(rowsLimitText);
		        MainSplitContainer.Panel1.Controls.Add(TreeMain);

		        #endregion

		        #region Options

		        maxLinesStackText.AssignValue(CurrentSettings.MaxLines, MaxLinesStackText_TextChanged);
		        maxThreadsText.AssignValue(CurrentSettings.MaxThreads, MaxThreadsText_TextChanged);
		        rowsLimitText.AssignValue(CurrentSettings.RowsLimit, RowsLimitText_TextChanged);
		        orderByText.Text = CurrentSettings.OrderBy;
		        orderByText.GotFocus += OrderByText_GotFocus;

		        #endregion

		        #region TreeView Container

		        TreeViewContainer = new TreeViewContainer(
			        CurrentSettings,
			        TreeMain,
			        CurrentSettings.Servers.Groups,
			        CurrentSettings.FileTypes.Groups,
			        CurrentSettings.LogsFolder.Folders);

		        TreeViewContainer.OnChanged += (clearStatus, onSchemeChanged) =>
		        {
			        ValidationCheck(clearStatus);
			        if (onSchemeChanged)
				        OnSchemeChanged?.Invoke(this, EventArgs.Empty);
		        };
		        TreeViewContainer.OnError += ex => { ReportStatus(ex.Message, ReportStatusType.Error); };

		        var template = UserSettings.Template;
		        if (template != null)
			        TreeViewContainer.UpdateContainerByTemplate(template);

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

        public void Initialize()
        {
	        if (IsInitialized) 
		        return;
	        TreeViewContainer.EnableSync();
	        IsInitialized = true;
        }

        public override void ApplySettings()
        {
	        OrderByLabel.Text = Resources.Txt_LogsReaderForm_OrderBy;
	        RowsLimitLabel.Text = Resources.Txt_LogsReaderForm_RowsLimit;
	        MaxThreadsLabel.Text = Resources.Txt_LogsReaderForm_MaxThreads;
	        MaxLinesLabel.Text = Resources.Txt_LogsReaderForm_MaxLines;

	        Tooltip.SetToolTip(maxThreadsText, Resources.Txt_LRSettingsScheme_MaxThreads);
	        Tooltip.SetToolTip(maxLinesStackText, Resources.Txt_LRSettingsScheme_MaxTraceLines);
	        Tooltip.SetToolTip(rowsLimitText, Resources.Txt_LRSettingsScheme_RowsLimit);
	        Tooltip.SetToolTip(orderByText, Resources.Txt_LRSettingsScheme_OrderBy);
	        Tooltip.SetToolTip(TreeMain, Resources.Txt_Form_trvMainComment);

	        TreeViewContainer.ApplySettings();

            base.ApplySettings();
        }

        public override void SaveData()
        {
	        UserSettings.Template = TreeViewContainer.GetTemplate(TreeMain);
            base.SaveData();
        }

        public override void LogsReaderKeyDown(object sender, KeyEventArgs e)
        {
	        base.LogsReaderKeyDown(sender, e);
	        TreeViewContainer.MainFormKeyDown(TreeMain, e);
        }

        internal override async void BtnSearch_Click(object sender, EventArgs e)
        {
            if (!IsWorking)
            {
                var stop = new Stopwatch();
                try
                {
	                // получение экземплярва фильтра, если необходимо выполнять фильтр во время поиска
	                var filter = alreadyUseFilter.Checked ? GetFilter() : null;

	                // получение серверов по чекбоксам
	                var servers = new List<string>();
	                foreach (TreeNode childTreeNode in TreeMain.Nodes[TreeViewContainer.TRVServers].Nodes)
		                servers.AddRange(childTreeNode.Nodes.OfType<TreeNode>().Where(x => x.Checked).Select(x => x.Text));
	                servers = servers.GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase).Select(x => x.Key).ToList();

	                // получение названия файлов типов  по чекбоксам
	                var fileTypes = new List<string>();
	                foreach (TreeNode childTreeNode in TreeMain.Nodes[TreeViewContainer.TRVTypes].Nodes)
		                fileTypes.AddRange(childTreeNode.Nodes.OfType<TreeNode>().Where(x => x.Checked).Select(x => x.Text));
	                fileTypes = fileTypes.GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase).Select(x => x.Key).ToList();

	                // получение папок по чекбоксам
	                var folders = TreeViewContainer.GetFolders(TreeMain, true);

	                MainReader = new LogsReaderPerformerScheme(CurrentSettings, txtPattern.Text, useRegex.Checked, servers, fileTypes, folders, filter);
	                MainReader.OnProcessReport += ReportProcessStatus;

	                stop.Start();
	                IsWorking = true;

	                ReportStatus(Resources.Txt_LogsReaderForm_LogFilesSearching, ReportStatusType.Success);
	                await MainReader.GetTargetFilesAsync(); // получение файлов логов

	                ReportStatus(Resources.Txt_LogsReaderForm_Working, ReportStatusType.Success);
	                await MainReader.StartAsync(); // вополнение поиска

	                // результат выполнения
	                OverallResultList = new DataTemplateCollection(CurrentSettings, MainReader.ResultsOfSuccess);
	                if (MainReader.ResultsOfError != null)
		                OverallResultList.AddRange(MainReader.ResultsOfError.OrderBy(x => x.Date));

	                // заполняем DataGrid
	                if (await AssignResult(filter))
		                ReportStatus(string.Format(Resources.Txt_LogsReaderForm_FinishedIn, stop.Elapsed.ToReadableString()), ReportStatusType.Success);

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

        protected override IEnumerable<DataTemplate> GetResultTemplates()
        {
	        return OverallResultList == null ? new List<DataTemplate>() : new List<DataTemplate>(OverallResultList);
        }

        protected override void ChangeFormStatus()
        {
	        base.ChangeFormStatus();

            TreeMain.Enabled = !IsWorking;
	        maxThreadsText.Enabled = !IsWorking;
	        rowsLimitText.Enabled = !IsWorking;
	        maxLinesStackText.Enabled = !IsWorking;
	        orderByText.Enabled = !IsWorking;

	        OnSearchChanged?.Invoke(this, EventArgs.Empty);
		}

        internal override bool TryGetTemplate(DataGridViewRow row, out DataTemplate template)
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

        protected override void ValidationCheck(bool clearStatus)
        {
	        try
	        {
		        var settIsCorrect = CurrentSettings?.IsCorrect ?? false;
		        if (settIsCorrect && clearStatus)
			        ClearErrorStatus();

		        BTNSearch.Enabled = settIsCorrect
		                            && !txtPattern.Text.IsNullOrEmpty()
		                            && TreeMain.Nodes[TreeViewContainer.TRVServers].Nodes.OfType<TreeNode>().Any(x => x.Nodes.OfType<TreeNode>().Any(x2 => x2.Checked))
		                            && TreeMain.Nodes[TreeViewContainer.TRVTypes].Nodes.OfType<TreeNode>().Any(x => x.Nodes.OfType<TreeNode>().Any(x2 => x2.Checked))
		                            && TreeMain.Nodes[TreeViewContainer.TRVFolders].Nodes.OfType<TreeNode>().Any(x => x.Checked);

		        base.ValidationCheck(clearStatus);
	        }
	        catch (Exception)
	        {
		        // ignored
	        }
        }

        protected override void ClearForm(bool saveData = true)
        {
            base.ClearForm(saveData);
            OverallResultList?.Clear();
            OverallResultList = null;
        }

        public override string ToString()
        {
            return CurrentSettings?.ToString() ?? base.ToString();
        }
    }
}