﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using SPAMassageSaloon.Common;
using Utils;
using Utils.WinForm;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader.Reader
{
	public abstract partial class LogsReaderFormBase : UserControl, IUserForm
	{
		private readonly Func<DateTime> _getStartDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        private readonly Func<DateTime> _getEndDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

        private bool _settingIsApplied = false;
        private bool _oldDateStartChecked;
        private bool _oldDateEndChecked;
        private bool _settingsLoaded;
        private bool _isWorking;

        private int _countMatches = 0;
        private int _countErrorMatches = 0;
        private int _filesCompleted = 0;
        private int _totalFiles = 0;

        private IEnumerable<DataTemplate> _currentDGVResult;
        private DataGridViewColumn _oldSortedColumn;
        private bool _byAscending = true;

        private Color _formBackColor;
        private Color _formForeColor;

        private readonly ToolStripStatusLabel _statusInfo;
        private readonly ToolStripStatusLabel _findedInfo;
        private readonly ToolStripStatusLabel _completedFilesStatus;
        private readonly ToolStripStatusLabel _totalFilesStatus;
        private readonly ToolStripStatusLabel _filtersCompleted1;
        private readonly ToolStripStatusLabel _filtersCompleted2;
        private readonly ToolStripStatusLabel _overallFound;
        private readonly ToolStripStatusLabel _errorFound;
        private readonly ToolStripStatusLabel _errorFoundValue;

        /// <summary>
        /// Юзерские настройки 
        /// </summary>
        protected UserSettings UserSettings { get; }

        protected Stopwatch TimeWatcher { get; set; }= new Stopwatch();

        protected ToolTip Tooltip { get; }

        protected bool ButtonHighlightEnabled
        {
	        get => buttonHighlightOn.Enabled;
	        set
	        {
		        buttonHighlightOn.Enabled = buttonHighlightOff.Enabled = buttonHghlBack.Enabled = buttonHghlFore.Enabled = value;
		        if (buttonHighlightOn.Enabled)
		        {
			        buttonHghlBack.BackColor = UserSettings.HghtBackColor;
			        buttonHghlFore.BackColor = UserSettings.HghtForeColor;
		        }
		        else
		        {
			        buttonHghlBack.BackColor = Color.LightGray;
			        buttonHghlFore.BackColor = Color.LightGray;
		        }
	        }
        }

        /// <summary>
        /// Поиск логов начался или завершился
        /// </summary>
        public event EventHandler OnProcessStatusChanged;

        /// <summary>
        /// При смене языка
        /// </summary>
        public event EventHandler OnAppliedSettings;

        /// <summary>
        /// Статус выполнения поиска
        /// </summary>
        public bool IsWorking
        {
	        get => _isWorking;
	        protected set
	        {
		        _isWorking = value;
		        ChangeFormStatus();
            }
        }

        public Encoding DefaultEncoding { get; }

        public Color FormBackColor
        {
	        get => _formBackColor;
	        set => _formBackColor = UserSettings.BackColor = value;
        }

        public Color FormForeColor
        {
	        get => _formForeColor;
	        set => _formForeColor = UserSettings.ForeColor = value;
        }

        public int CountMatches
        {
	        get => _countMatches;
	        protected set
	        {
		        _countMatches = value;
		        _findedInfo.Text = _countMatches.ToString();
	        }
        }

        public int CountErrorMatches
        {
	        get => _countErrorMatches;
	        protected set
	        {
		        _countErrorMatches = value;
		        _errorFoundValue.Text = _countErrorMatches.ToString();
	        }
        }

        public int Progress
        {
	        get => IsWorking ? progressBar.Value : 100;
	        protected set => progressBar.Value = value;
        }

        public int FilesCompleted
        {
	        get => _filesCompleted;
	        protected set
	        {
		        _filesCompleted = value;
		        _completedFilesStatus.Text = _filesCompleted.ToString();
	        }
        }

        public int TotalFiles
        {
	        get => _totalFiles;
	        protected set
	        {
		        _totalFiles = value;
		        _totalFilesStatus.Text = _totalFiles.ToString();
	        }
        }

        public int CustomPanelMinSize
        {
	        get => MainSplitContainer.Panel1MinSize;
            set => MainSplitContainer.Panel1MinSize = value;
        }

        public abstract bool HasAnyResult { get; }

        public TraceItemView MainViewer { get; }

        protected LogsReaderFormBase(Encoding defaultEncoding, UserSettings userSettings)
        {
	        InitializeComponent();

            DefaultEncoding = defaultEncoding;
            UserSettings = userSettings;

            _formBackColor = UserSettings.BackColor;
            _formForeColor = UserSettings.ForeColor;

            base.Font = LogsReaderMainForm.DefFont;
	        ChbxAlreadyUseFilter.Font = LogsReaderMainForm.DefFont;
	        tabControlViewer.Font = LogsReaderMainForm.DgvFont;

            #region Initialize StripStatus

            Tooltip = new ToolTip { InitialDelay = 50 };

            var statusStripItemsPaddingStart = new Padding(0, 2, 0, 2);
            var statusStripItemsPaddingMiddle = new Padding(-3, 2, 0, 2);
            var statusStripItemsPaddingEnd = new Padding(-3, 2, 1, 2);

            _filtersCompleted1 = new ToolStripStatusLabel { Font = base.Font, Margin = statusStripItemsPaddingStart };
            _completedFilesStatus = new ToolStripStatusLabel("0") { Font = base.Font, Margin = statusStripItemsPaddingMiddle };
            _filtersCompleted2 = new ToolStripStatusLabel { Font = base.Font, Margin = statusStripItemsPaddingMiddle };
            _totalFilesStatus = new ToolStripStatusLabel("0") { Font = base.Font, Margin = statusStripItemsPaddingEnd };
            statusStrip.Items.Add(_filtersCompleted1);
            statusStrip.Items.Add(_completedFilesStatus);
            statusStrip.Items.Add(_filtersCompleted2);
            statusStrip.Items.Add(_totalFilesStatus);

            _overallFound = new ToolStripStatusLabel { Font = base.Font, Margin = statusStripItemsPaddingStart };
            _findedInfo = new ToolStripStatusLabel("0") { Font = base.Font, Margin = statusStripItemsPaddingMiddle };
            _errorFound = new ToolStripStatusLabel { Font = base.Font, Margin = statusStripItemsPaddingEnd };
            _errorFoundValue = new ToolStripStatusLabel("0") { Font = base.Font, Margin = statusStripItemsPaddingMiddle };
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(_overallFound);
            statusStrip.Items.Add(_findedInfo);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(_errorFound);
            statusStrip.Items.Add(_errorFoundValue);

            statusStrip.Items.Add(new ToolStripSeparator());
            _statusInfo = new ToolStripStatusLabel("") { Font = new Font(LogsReaderMainForm.MainFontFamily, 8.5F, FontStyle.Bold), Margin = statusStripItemsPaddingStart };
            statusStrip.Items.Add(_statusInfo);

            #endregion

            try
            {
                #region Initialize DgvData

                DgvData.AutoGenerateColumns = false;
	            DgvData.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
	            DgvData.DefaultCellStyle.Font = LogsReaderMainForm.DgvFont;
	            DgvData.Font = LogsReaderMainForm.DgvFont;
	            DgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
	            DgvData.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
	            foreach (DataGridViewColumn c in DgvData.Columns)
		            c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
	            DgvData.DoubleBuffered(true);
                DgvData.CellFormatting += DgvDataOnCellFormatting;
                DgvData.ColumnHeaderMouseClick += DgvDataOnColumnHeaderMouseClick;

                SchemeName.DataPropertyName = nameof(DataTemplate.Tmp.SchemeName);
                SchemeName.Name = nameof(DataTemplate.Tmp.SchemeName);

                PrivateID.DataPropertyName = nameof(DataTemplate.Tmp.PrivateID);
                PrivateID.Name = nameof(DataTemplate.Tmp.PrivateID);

                IsSuccess.DataPropertyName = nameof(DataTemplate.Tmp.IsSuccess);
                IsSuccess.Name = nameof(DataTemplate.Tmp.IsSuccess);

                ID.DataPropertyName = nameof(DataTemplate.Tmp.ID);
                ID.Name = nameof(DataTemplate.Tmp.ID);
                ID.HeaderText = nameof(DataTemplate.Tmp.ID);

                Server.DataPropertyName = nameof(DataTemplate.Tmp.Server);
                Server.Name = nameof(DataTemplate.Tmp.Server);
                Server.HeaderText = nameof(DataTemplate.Tmp.Server);

                TraceName.DataPropertyName = nameof(DataTemplate.Tmp.TraceName);
                TraceName.Name = nameof(DataTemplate.Tmp.TraceName);
                TraceName.HeaderText = nameof(DataTemplate.Tmp.TraceName);

                DateOfTrace.DataPropertyName = nameof(DataTemplate.Tmp.DateString);
                DateOfTrace.Name = nameof(DataTemplate.Tmp.DateString);
                DateOfTrace.HeaderText = nameof(DataTemplate.Tmp.Date);

                ElapsedSec.DataPropertyName = nameof(DataTemplate.Tmp.ElapsedSecString);
                ElapsedSec.Name = nameof(DataTemplate.Tmp.ElapsedSecString);
                ElapsedSec.HeaderText = nameof(DataTemplate.Tmp.ElapsedSec);

                File.DataPropertyName = nameof(DataTemplate.Tmp.FileNamePartial);
                File.Name = nameof(DataTemplate.Tmp.FileNamePartial);
                File.HeaderText = nameof(DataTemplate.Tmp.File);

	            label7.Text = nameof(DataTemplate.Tmp.TraceName);
                label11.Text = DataTemplate.HeaderTraceMessage;

                #endregion

                #region Apply All Settings

                DateStartFilter.ValueChanged += DateStartFilterOnValueChanged;
                DateEndFilter.ValueChanged += DateEndFilterOnValueChanged;

                TbxPattern.AssignValue(UserSettings.PreviousSearch, TxtPatternOnTextChanged);
                ChbxUseRegex.Checked = UserSettings.UseRegex;
                DateStartFilter.Checked = UserSettings.DateStartChecked;
                if (DateStartFilter.Checked)
                    DateStartFilter.Value = _getStartDate.Invoke();
                DateEndFilter.Checked = UserSettings.DateEndChecked;
                if (DateEndFilter.Checked)
                    DateEndFilter.Value = _getEndDate.Invoke();
                TbxTraceNameFilter.AssignValue(UserSettings.TraceNameFilter, TbxTraceNameFilterOnTextChanged);
                TbxTraceMessageFilter.AssignValue(UserSettings.TraceMessageFilter, TbxTraceMessageFilterOnTextChanged);

                checkBoxShowTrns.Checked = UserSettings.ShowTransactions;

                var colorDialog = new ColorDialog
                {
	                AllowFullOpen = true,
	                FullOpen = true,
	                CustomColors = new[]
	                {
		                ColorTranslator.ToOle(LogsReaderMainForm.ERR_COLOR_BACK),
		                ColorTranslator.ToOle(LogsReaderMainForm.TRN_COLOR_BACK),
		                ColorTranslator.ToOle(LogsReaderMainForm.HGT_COLOR_BACK)
	                }
                };

                void ChangeColor(object sender, EventArgs e)
                {
	                if (!(sender is Button button))
		                return;

	                colorDialog.Color = button.BackColor;

	                if (colorDialog.ShowDialog() != DialogResult.OK)
		                return;

	                button.BackColor = colorDialog.Color;

	                if (button == buttonErrorBack)
		                UserSettings.ErrBackColor = colorDialog.Color;
                    else if (button == buttonErrorFore)
		                UserSettings.ErrForeColor = colorDialog.Color;
	                else if (button == buttonTrnBack)
		                UserSettings.TrnBackColor = colorDialog.Color;
	                else if (button == buttonTrnFore)
		                UserSettings.TrnForeColor = colorDialog.Color;
	                else if (button == buttonHghlBack)
		                UserSettings.HghtBackColor = colorDialog.Color;
	                else if (button == buttonHghlFore)
		                UserSettings.HghtForeColor = colorDialog.Color;

	                DgvData.Refresh();
                }

                buttonErrorBack.BackColor = UserSettings.ErrBackColor;
                buttonErrorFore.BackColor = UserSettings.ErrForeColor;
                buttonTrnBack.BackColor = UserSettings.TrnBackColor;
                buttonTrnFore.BackColor = UserSettings.TrnForeColor;
                buttonHghlBack.BackColor = UserSettings.HghtBackColor;
                buttonHghlFore.BackColor = UserSettings.HghtForeColor;
                
                buttonErrorBack.Click += ChangeColor;
                buttonErrorFore.Click += ChangeColor;
                buttonTrnBack.Click += ChangeColor;
                buttonTrnFore.Click += ChangeColor;
                buttonHghlBack.Click += ChangeColor;
                buttonHghlFore.Click += ChangeColor;


                MainViewer = new TraceItemView(defaultEncoding, userSettings, true);
                tabControlViewer.DrawMode = TabDrawMode.Normal;
                tabControlViewer.BackColor = Color.White;
                AddViewer(MainViewer, null);

                #endregion
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, Resources.Txt_Initialization);
            }
        }

        void AddViewer(TraceItemView traceViewer, DataTemplate template)
        {
	        var tabPage = new CustomTabPage(traceViewer)
            {
	            UseVisualStyleBackColor = true,
		        ForeColor = Color.Black,
		        Margin = new Padding(0),
		        Padding = new Padding(0),
		        BorderStyle = BorderStyle.None,
		        CanClose = !traceViewer.IsMain
            };

	        if (!traceViewer.IsMain && MainViewer != null)
		        traceViewer.SplitterDistance = MainViewer.SplitterDistance;
            traceViewer.Dock = DockStyle.Fill;
	        traceViewer.ChangeTemplate(template, checkBoxShowTrns.Checked, out var _);

	        tabControlViewer.TabPages.Add(tabPage);
        }

        public void ApplyFormSettings()
        {
	        try
	        {
		        if (UserSettings == null)
			        return;

		        for (var i = 0; i < DgvData.Columns.Count; i++)
		        {
			        var valueStr = UserSettings.GetValue("COL" + i);
			        if (!valueStr.IsNullOrEmptyTrim() && int.TryParse(valueStr, out var value) && value > 1 && value <= 1000)
				        DgvData.Columns[i].Width = value;
		        }

		        ParentSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(ParentSplitContainer), 25, 2000, ParentSplitContainer.SplitterDistance);
		        MainSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(MainSplitContainer), 25, 2000, MainSplitContainer.SplitterDistance);
		        MainViewer.SplitterDistance = UserSettings.GetValue(nameof(MainViewer), 25, 2000, MainViewer.SplitterDistance);

		        ParentSplitContainer.SplitterMoved += (sender, args) => { SaveData(); };
		        MainSplitContainer.SplitterMoved += (sender, args) => { SaveData(); };
		        MainViewer.SplitterMoved += (sender, args) => { SaveData(); };

		        _settingIsApplied = true;
            }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        }
        }

        public virtual void ApplySettings()
        {
            try
            {
                #region Change Language

                _filtersCompleted1.Text = Resources.Txt_LogsReaderForm_FilesCompleted_1;
                _filtersCompleted2.Text = Resources.Txt_LogsReaderForm_FilesCompleted_2;
                _overallFound.Text = Resources.Txt_LogsReaderForm_OverallFound;
                _errorFound.Text = Resources.Txt_LogsReaderForm_Error;

                CobxTraceNameFilter.Items.Clear();
                CobxTraceNameFilter.Items.Add(Resources.Txt_LogsReaderForm_Contains);
                CobxTraceNameFilter.Items.Add(Resources.Txt_LogsReaderForm_NotContains);
                if (UserSettings != null)
                    CobxTraceNameFilter.AssignValue(UserSettings.TraceNameFilterContains ? Resources.Txt_LogsReaderForm_Contains : Resources.Txt_LogsReaderForm_NotContains,
                        CobxTraceNameFilter_SelectedIndexChanged);

                CobxTraceMessageFilter.Items.Clear();
                CobxTraceMessageFilter.Items.Add(Resources.Txt_LogsReaderForm_Contains);
                CobxTraceMessageFilter.Items.Add(Resources.Txt_LogsReaderForm_NotContains);
                if (UserSettings != null)
                    CobxTraceMessageFilter.AssignValue(UserSettings.TraceMessageFilterContains ? Resources.Txt_LogsReaderForm_Contains : Resources.Txt_LogsReaderForm_NotContains,
                        CobxTraceMessageFilter_SelectedIndexChanged);

                Tooltip.RemoveAll();
                Tooltip.SetToolTip(TbxPattern, Resources.Txt_Form_SearchComment);
                Tooltip.SetToolTip(ChbxUseRegex, Resources.Txt_LRSettings_UseRegexComment);
                Tooltip.SetToolTip(DateStartFilter, Resources.Txt_Form_DateFilterComment);
                Tooltip.SetToolTip(DateEndFilter, Resources.Txt_Form_DateFilterComment);
                Tooltip.SetToolTip(TbxTraceNameFilter, Resources.Txt_Form_TraceNameFilterComment);
                Tooltip.SetToolTip(TbxTraceMessageFilter, Resources.Txt_Form_TraceFilterComment);
                Tooltip.SetToolTip(ChbxAlreadyUseFilter, Resources.Txt_Form_AlreadyUseFilterComment);
                Tooltip.SetToolTip(btnExport, Resources.Txt_LogsReaderForm_ExportComment);
                Tooltip.SetToolTip(buttonPrev, Resources.Txt_LogsReaderForm_PrevErrButt);
                Tooltip.SetToolTip(buttonNext, Resources.Txt_LogsReaderForm_NextErrButt);
                Tooltip.SetToolTip(checkBoxShowTrns, Resources.Txt_Forms_ShowTransactions);
                Tooltip.SetToolTip(buttonHighlightOn, Resources.Txt_LogsReaderForm_HighlightTxt);
                Tooltip.SetToolTip(buttonHighlightOff, Resources.Txt_LogsReaderForm_HighlightTxtOff);

                Tooltip.SetToolTip(buttonErrorBack, Resources.Txt_LogsReaderForm_ColorBack);
                Tooltip.SetToolTip(buttonTrnBack, Resources.Txt_LogsReaderForm_ColorBack);
                Tooltip.SetToolTip(buttonHghlBack, Resources.Txt_LogsReaderForm_ColorBack);
                Tooltip.SetToolTip(buttonErrorFore, Resources.Txt_LogsReaderForm_ColorFore);
                Tooltip.SetToolTip(buttonTrnFore, Resources.Txt_LogsReaderForm_ColorFore);
                Tooltip.SetToolTip(buttonHghlFore, Resources.Txt_LogsReaderForm_ColorFore);

                ChbxUseRegex.Text = Resources.Txt_LogsReaderForm_UseRegex;
                BtnSearch.Text = IsWorking ? Resources.Txt_LogsReaderForm_Stop : Resources.Txt_LogsReaderForm_Search;
                btnClear.Text = Resources.Txt_LogsReaderForm_Clear;
                btnClear.Size = new Size(Convert.ToInt32(Resources.LogsReaderForm_btnClear_Width), btnClear.Height);
                btnFilter.Text = Resources.Txt_LogsReaderForm_Filter;
                btnReset.Text = Resources.Txt_LogsReaderForm_Reset;
                ChbxAlreadyUseFilter.Text = Resources.Txt_LogsReaderForm_UseFilterWhenSearching;

                #endregion
            }
            catch (Exception ex)
            {
	            ReportStatus(ex.Message, ReportStatusType.Error);
            }
            finally
            {
                _settingsLoaded = true;
                OnAppliedSettings?.Invoke(this, EventArgs.Empty);
            }
        }

        public virtual void SaveData()
        {
            try
            {
                if (!_settingsLoaded || UserSettings == null || !_settingIsApplied)
                    return;

                for (var i = 0; i < DgvData.Columns.Count; i++)
                {
                    UserSettings.SetValue("COL" + i, DgvData.Columns[i].Width);
                }

                UserSettings.SetValue(nameof(ParentSplitContainer), ParentSplitContainer.SplitterDistance);
                UserSettings.SetValue(nameof(MainSplitContainer), MainSplitContainer.SplitterDistance);
                UserSettings.SetValue(nameof(MainViewer), MainViewer.SplitterDistance);
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        public virtual void LogsReaderKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
	                case Keys.F3 when e.Shift:
		                buttonPrev_Click(this, EventArgs.Empty);
		                break;
	                case Keys.F3:
		                buttonNext_Click(this, EventArgs.Empty);
		                break;
	                case Keys.F5 when BtnSearch.Enabled && !IsWorking:
                        BtnSearch_Click(this, EventArgs.Empty);
                        break;
                    case Keys.Escape when BtnSearch.Enabled && IsWorking:
                        BtnSearch_Click(this, EventArgs.Empty);
                        break;
                    case Keys.F6 when btnClear.Enabled:
	                    BtnClear_Click(this, EventArgs.Empty);
                        break;
                    case Keys.F7 when btnFilter.Enabled:
                        BtnFilter_Click(this, EventArgs.Empty);
                        break;
	                case Keys.F8 when btnReset.Enabled:
		                BtnReset_Click(this, EventArgs.Empty);
		                break;
                    case Keys.F1 when ButtonHighlightEnabled:
		                buttonHighlight_Click(this, EventArgs.Empty);
		                break;
	                case Keys.F2 when ButtonHighlightEnabled:
		                buttonHighlightOff_Click(this, EventArgs.Empty);
		                break;
	                case Keys.S when e.Control && btnExport.Enabled:
                        BtnExport_Click(this, EventArgs.Empty);
                        break;
                    case Keys.C when e.Control && DgvData.SelectedRows.Count > 0:
                        var templateList = new List<DataTemplate>();
                        foreach (DataGridViewRow row in DgvData.SelectedRows)
                        {
                            if (TryGetTemplate(row, out var template))
                                templateList.Insert(0, template);
                        }

                        var clipboardText = new StringBuilder();
                        if (MainViewer.CurrentEditor == MainViewer.EditorMessage)
                        {
	                        foreach (var template in templateList)
	                        {
		                        clipboardText.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\r\n",
			                        template.ID,
			                        template.ParentReader.FilePath,
			                        template.DateString,
			                        template.TraceName,
			                        template.ElapsedSecString,
			                        template.Description,
			                        template.Message.Trim());
                            }
                        }
                        else
                        {
	                        foreach (var template in templateList)
		                        clipboardText.Append($"{template.ParentReader.FilePath}\t{template.TraceMessage.Trim()}\r\n");
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

        internal virtual void BtnSearch_Click(object sender, EventArgs e)
        {
	        if (TbxPattern.Text.IsNullOrEmpty())
                throw new Exception(Resources.Txt_LogsReaderForm_SearchPatternIsNull);
        }

        protected void ReportProcessStatus(int countMatches, int countErrorMatches, int percentOfProgeress, int filesCompleted, int totalFiles)
        {
	        if (!IsWorking)
		        return;

	        this.SafeInvoke(() =>
	        {
		        if (!IsWorking)
			        return;

		        CountMatches = countMatches;
		        CountErrorMatches = countErrorMatches;
		        Progress = percentOfProgeress;
		        FilesCompleted = filesCompleted;
		        TotalFiles = totalFiles;
		        ReportStatus(string.Format(Resources.Txt_LogsReaderForm_Working, $" ({TimeWatcher.Elapsed.ToReadableString()})"), ReportStatusType.Success);
	        });

	        OnProcessStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        void ClearStatus()
        {
	        this.SafeInvoke(() =>
	        {
		        CountMatches = 0;
		        CountErrorMatches = 0;
		        Progress = 0;
		        FilesCompleted = 0;
		        TotalFiles = 0;
		        ReportStatus(string.Empty, ReportStatusType.Success);
            });

	        OnProcessStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private async void BtnExport_Click(object sender, EventArgs e)
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

                BtnSearch.Enabled = btnClear.Enabled = btnExport.Enabled = btnFilter.Enabled = ButtonHighlightEnabled = btnReset.Enabled = false;
                ReportStatus(Resources.Txt_LogsReaderForm_Exporting, ReportStatusType.Success);
                fileName = Path.GetFileName(desctination);

                var i = 0;
                Progress = 0;
                using (var writer = new StreamWriter(desctination, false, new UTF8Encoding(false)))
                {
	                if (MainViewer.CurrentEditor == MainViewer.EditorMessage)
	                {
		                await writer.WriteLineAsync(GetCSVRow(new[]
		                {
			                nameof(DataTemplate.Tmp.ID),
			                nameof(DataTemplate.Tmp.File),
			                nameof(DataTemplate.Tmp.Date),
			                nameof(DataTemplate.Tmp.TraceName),
			                DataTemplate.HeaderDescription,
			                DataTemplate.HeaderMessage
                        }));

                        foreach (DataGridViewRow row in DgvData.Rows)
		                {
			                if (!TryGetTemplate(row, out var template))
				                continue;

			                await writer.WriteLineAsync(
				                GetCSVRow(new[]
				                {
					                template.ID.ToString(),
					                template.ParentReader.FilePath,
					                template.DateString,
					                template.TraceName,
					                $"\"{template.Description}\"",
					                $"\"{template.Message.Trim()}\""
				                }));
			                writer.Flush();

			                Progress = (int) Math.Round((double) (100 * ++i) / DgvData.RowCount);
		                }
	                }
	                else
	                {
		                await writer.WriteLineAsync(GetCSVRow(new[]
		                {
			                nameof(DataTemplate.Tmp.File), 
			                DataTemplate.HeaderTraceMessage
		                }));

                        foreach (DataGridViewRow row in DgvData.Rows)
		                {
			                if (!TryGetTemplate(row, out var template))
				                continue;

			                await writer.WriteLineAsync(
				                GetCSVRow(new[]
				                {
					                template.ParentReader.FilePath,
					                $"\"{template.TraceMessage.Trim()}\""
				                }));
			                writer.Flush();

			                Progress = (int) Math.Round((double) (100 * ++i) / DgvData.RowCount);
		                }
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
                DgvData.Focus();
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

        private async void BtnFilter_Click(object sender, EventArgs e)
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

        protected DataFilter GetFilter()
        {
            return new DataFilter(DateStartFilter.Checked ? DateStartFilter.Value : DateTime.MinValue,
                DateEndFilter.Checked ? DateEndFilter.Value : DateTime.MaxValue,
                TbxTraceNameFilter.Text,
                CobxTraceNameFilter.Text.Like(Resources.Txt_LogsReaderForm_Contains),
                TbxTraceMessageFilter.Text,
                CobxTraceMessageFilter.Text.Like(Resources.Txt_LogsReaderForm_Contains));
        }

        private async void BtnReset_Click(object sender, EventArgs e)
        {
	        try
	        {
		        await AssignResult(null);
            }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private void buttonHighlight_Click(object sender, EventArgs e)
        {
	        try
	        {
		        if (_currentDGVResult == null || !_currentDGVResult.Any())
                    return;

		        foreach (var template in _currentDGVResult.Where(x => x.IsFiltered))
			        template.IsFiltered = false;

                var filter = GetFilter();
		        if (filter == null)
			        return;

		        foreach (var template in filter.FilterCollection(_currentDGVResult))
			        template.IsFiltered = true;

		        DgvData.Refresh();
            }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private void buttonHighlightOff_Click(object sender, EventArgs e)
        {
	        try
	        {
		        if (_currentDGVResult == null || !_currentDGVResult.Any())
			        return;

		        foreach (var template in _currentDGVResult.Where(x => x.IsFiltered))
			        template.IsFiltered = false;

		        DgvData.Refresh();
	        }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        protected async Task<bool> AssignResult(DataFilter filter)
        {
	        try
	        {
		        // сохраняем предыдущую позицию в гриде
		        var minIndex = -1;
		        var maxIndex = -1;
		        var countVisible = -1;
		        var firstVisible = -1;
		        if (DgvData.SelectedRows.Count > 0)
		        {
			        var selCol = DgvData.SelectedRows.OfType<DataGridViewRow>();
			        minIndex = selCol.First().Index;
			        maxIndex = selCol.Last().Index;
			        countVisible = DgvData.DisplayedRowCount(false);
			        firstVisible = DgvData.FirstDisplayedScrollingRowIndex;
		        }

		        Clear();
		        ClearErrorStatus();

		        _currentDGVResult = GetResultTemplates();

		        if (_currentDGVResult == null || !_currentDGVResult.Any())
		        {
			        ReportStatus(Resources.Txt_LogsReaderForm_NoLogsFound, ReportStatusType.Warning);
			        return false;
		        }

		        foreach (var template in _currentDGVResult.Where(x => x.IsFiltered))
			        template.IsFiltered = false;

                if (filter != null)
		        {
			        _currentDGVResult = filter.FilterCollection(_currentDGVResult);

			        if (!_currentDGVResult.Any())
			        {
				        ReportStatus(Resources.Txt_LogsReaderForm_NoFilterResultsFound, ReportStatusType.Warning);
				        return false;
			        }
		        }

		        await DgvData.AssignCollectionAsync(_currentDGVResult, null);

		        btnExport.Enabled = DgvData.RowCount > 0;

                // возвращаем позицию в гриде
		        if (DgvData.RowCount > 0 && (minIndex > -1 || maxIndex > -1))
		        {
			        DgvData.ClearSelection();
			        if (DgvData.RowCount > maxIndex && maxIndex > -1)
			        {
				        EnsureVisibleRow(DgvData, maxIndex, firstVisible, countVisible);
			        }
			        else if (DgvData.RowCount > minIndex && minIndex > -1)
			        {
				        EnsureVisibleRow(DgvData, minIndex, firstVisible, countVisible);
			        }
			        else
			        {
				        EnsureVisibleRow(DgvData, DgvData.RowCount - 1, firstVisible, countVisible);
			        }
		        }

		        return true;
            }
	        catch (Exception ex)
	        {
		        DgvData.Focus();
		        ReportStatus(ex.Message, ReportStatusType.Error);
                return false;
	        }
        }

        static void EnsureVisibleRow(DataGridView view, int rowToShow, int firstVisible, int countVisible)
        {
	        if (rowToShow < 0 || rowToShow >= view.RowCount)
		        return;

	        view.Rows[rowToShow].Selected = true;
	        if (firstVisible <= -1 || countVisible <= -1)
	        {
		        view.FirstDisplayedScrollingRowIndex = rowToShow;
                return;
	        }

	        if (rowToShow < firstVisible)
	        {
		        view.FirstDisplayedScrollingRowIndex = rowToShow;
	        }
	        else if (rowToShow >= firstVisible + countVisible)
	        {
		        view.FirstDisplayedScrollingRowIndex = rowToShow - countVisible + 1;
	        }
	        else
	        {
		        view.FirstDisplayedScrollingRowIndex = firstVisible;
	        }
        }

        protected abstract IEnumerable<DataTemplate> GetResultTemplates();

        protected virtual void ChangeFormStatus()
        {
	        try
	        {
		        BtnSearch.Text = IsWorking ? Resources.Txt_LogsReaderForm_Stop : Resources.Txt_LogsReaderForm_Search;
		        btnClear.Enabled = !IsWorking;
		        TbxPattern.Enabled = !IsWorking;

		        foreach (var dgvChild in DgvData.Controls.OfType<Control>()) // решает баг с задисейбленным скролл баром DataGridView
			        dgvChild.Enabled = !IsWorking;
		        DgvData.Enabled = !IsWorking;

		        tabControlViewer.Enabled = !IsWorking;
		        ChbxUseRegex.Enabled = !IsWorking;
		        DateStartFilter.Enabled = !IsWorking;
		        DateEndFilter.Enabled = !IsWorking;
		        CobxTraceNameFilter.Enabled = !IsWorking;
		        TbxTraceNameFilter.Enabled = !IsWorking;
		        CobxTraceMessageFilter.Enabled = !IsWorking;
		        TbxTraceMessageFilter.Enabled = !IsWorking;
		        ChbxAlreadyUseFilter.Enabled = !IsWorking;

		        if (IsWorking)
		        {
			        ParentSplitContainer.Cursor = Cursors.WaitCursor;
			        ClearForm(true);
		        }
		        else
		        {
			        ParentSplitContainer.Cursor = Cursors.Default;
		        }

		        btnExport.Enabled = DgvData.RowCount > 0;
		        btnFilter.Enabled = ButtonHighlightEnabled = btnReset.Enabled = HasAnyResult;
            }
	        finally
	        {
		        if (IsWorking)
			        Focus();
		        else
			        DgvData.Focus();
            }
        }

        private void DgvDataOnCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
	        if (e.RowIndex < 0 || e.ColumnIndex < 0) 
		        return;

	        try
	        {
		        var row = ((DataGridView)sender).Rows[e.RowIndex];
		        if (!TryGetTemplate(row, out var template))
			        return;

		        var cellFile = row.Cells[nameof(DataTemplate.Tmp.FileNamePartial)];
		        if (cellFile != null)
			        cellFile.ToolTipText = template.File;

		        if (template.IsSuccess)
		        {
			        if (template.Date == null)
				        foreach (DataGridViewCell cell2 in row.Cells)
					        cell2.ToolTipText = Resources.Txt_LogsReaderForm_DateValueIsIncorrect;

			        if (checkBoxShowTrns.Checked && template.IsSelected)
			        {
				        row.DefaultCellStyle.BackColor = buttonTrnBack.BackColor;
				        row.DefaultCellStyle.ForeColor = buttonTrnFore.BackColor;
                    }
                    else if (template.IsFiltered)
			        {
				        row.DefaultCellStyle.BackColor = buttonHghlBack.BackColor;
				        row.DefaultCellStyle.ForeColor = buttonHghlFore.BackColor;
                    }
			        else
			        {
				        СolorizationDGV(row, template);
			        }
		        }
		        else
		        {
			        row.DefaultCellStyle.BackColor = buttonErrorBack.BackColor;
			        row.DefaultCellStyle.ForeColor = buttonErrorFore.BackColor;
			        row.DefaultCellStyle.Font = LogsReaderMainForm.ErrFont;

			        if (template.IsMatched) 
				        return;

			        foreach (DataGridViewCell cell2 in row.Cells)
				        cell2.ToolTipText = Resources.Txt_LogsReaderForm_DoesntMatchByPattern;
		        }
	        }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        }
        }

        protected virtual void СolorizationDGV(DataGridViewRow row, DataTemplate template)
        {
	        row.DefaultCellStyle.BackColor = row.Index.IsParity() ? Color.White : Color.FromArgb(245, 245, 245);
        }

        //DgvData.RowPostPaint += DgvData_RowPostPaint;
        //private void DgvData_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        //{
        // if (e.RowIndex < 0)
        //  return;

        // try
        // {
        //  var row = ((DataGridView)sender).Rows[e.RowIndex];
        //  if (!TryGetTemplate(row, out var template))
        //   return;
        //        //Color.Red

        //        //((DataGridView)sender).GridColor 
        //        using (Pen pen = new Pen(Color.Red))
        //        {
        //         int penWidth = 1;

        //         pen.Width = penWidth;

        //         int x = e.RowBounds.Left + (penWidth / 2);
        //         int y = e.RowBounds.Top + (penWidth / 2);
        //         int width = e.RowBounds.Width - penWidth;
        //         int height = e.RowBounds.Height - penWidth;

        //         e.Graphics.DrawRectangle(pen, x, y, width, height);
        //        }
        //    }
        // catch (Exception ex)
        // {
        //  ReportStatus(ex.Message, ReportStatusType.Error);
        // }
        //}

        //void SetBorderRowColor(Color color, DataGridViewCellPaintingEventArgs e)
        //{
        //    e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);
        //    using (var p = new Pen(color, 2))
        //    {
        //        var rect = e.CellBounds;
        //        rect.Y = rect.Top + 1;
        //        rect.Height -= 2;
        //        e.Graphics.DrawRectangle(p, rect);
        //    }

        //    e.Handled = true;
        //}

        private void buttonPrev_Click(object sender, EventArgs e)
        {
	        try
	        {
		        if (DgvData.SelectedRows.Count == 0)
			        return;

		        var countVisible = DgvData.DisplayedRowCount(false);
		        var firstVisible = DgvData.FirstDisplayedScrollingRowIndex;

		        var selected = DgvData.SelectedRows[0];
		        foreach (var row in DgvData.Rows.OfType<DataGridViewRow>()
			        .Where(x => x.Index < selected.Index
			                    && !bool.Parse(x.Cells[nameof(DataTemplate.Tmp.IsSuccess)].Value.ToString())).Reverse())
		        {
			        DgvData.ClearSelection();
			        DgvData.FirstDisplayedScrollingRowIndex = row.Index >= firstVisible && row.Index < firstVisible + countVisible
				        ? firstVisible
				        : row.Index;
			        row.Selected = true;
			        DgvData.CurrentCell = DgvData.Rows[row.Index].Cells[DgvData.CurrentCell.ColumnIndex];
			        return;
		        }
            }
	        catch (Exception)
	        {
		        // ignored
	        }
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
	        try
	        {
		        if (DgvData.SelectedRows.Count == 0)
			        return;

		        var countVisible = DgvData.DisplayedRowCount(false);
		        var firstVisible = DgvData.FirstDisplayedScrollingRowIndex;

		        var selected = DgvData.SelectedRows[0];
		        foreach (var row in DgvData.Rows.OfType<DataGridViewRow>()
			        .Where(x => x.Index > selected.Index
			                    && !bool.Parse(x.Cells[nameof(DataTemplate.Tmp.IsSuccess)].Value.ToString())))
		        {
			        DgvData.ClearSelection();
			        DgvData.FirstDisplayedScrollingRowIndex = row.Index >= firstVisible && row.Index < firstVisible + countVisible
				        ? firstVisible
				        : row.Index - countVisible >= 0 ? row.Index - countVisible : row.Index;
			        row.Selected = true;
			        DgvData.CurrentCell = DgvData.Rows[row.Index].Cells[DgvData.CurrentCell.ColumnIndex];
			        return;
                }
            }
	        catch (Exception)
	        {
		        // ignored
            }
        }

        private async void DgvDataOnColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
	        try
	        {
		        if (!HasAnyResult || _currentDGVResult == null)
			        return;

		        var oldSortedColumn = _oldSortedColumn; // поле очищается в методе ClearDGV
		        var newColumn = DgvData.Columns[e.ColumnIndex];

		        var byAscending = _oldSortedColumn?.Index != e.ColumnIndex || _oldSortedColumn?.Index == e.ColumnIndex && !_byAscending;
		        var source = _currentDGVResult;

		        Clear();
		        ClearErrorStatus();

		        var columnName = DgvData.Columns[e.ColumnIndex].HeaderText;
		        var orderByOption = byAscending
			        ? new Dictionary<string, bool>() {{columnName, false}}
			        : new Dictionary<string, bool>() {{columnName, true}};
		        if (!orderByOption.ContainsKey(nameof(DataTemplate.Tmp.File)))
			        orderByOption.Add(nameof(DataTemplate.Tmp.File), !byAscending);
		        if (!orderByOption.ContainsKey(nameof(DataTemplate.Tmp.FoundLineID)))
			        orderByOption.Add(nameof(DataTemplate.Tmp.FoundLineID), !byAscending);

		        _currentDGVResult = DataTemplateCollection.DoOrdering(source, orderByOption);

		        await DgvData.AssignCollectionAsync(_currentDGVResult, null);

		        if (oldSortedColumn != null)
			        oldSortedColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
		        if (newColumn != null)
			        newColumn.HeaderCell.SortGlyphDirection = byAscending ? SortOrder.Ascending : SortOrder.Descending;

		        _oldSortedColumn = newColumn;
		        _byAscending = byAscending;
	        }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        }
        }

        private void DgvData_SelectionChanged(object sender, EventArgs e)
        {
	        try
	        {
		        if (DgvData.SelectedRows.Count == 0 || !TryGetTemplate(DgvData.SelectedRows[0], out var template))
			        return;

		        MainViewer.ChangeTemplate(template, checkBoxShowTrns.Checked, out var noChanged);

		        if (checkBoxShowTrns.Checked && !noChanged)
			        DgvData.Refresh();

                if (MainViewer.Parent is TabPage page)
			        tabControlViewer.SelectTab(page);
	        }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        }
	        finally
	        {
		        DgvData.Focus();
	        }
        }

        private void DgvData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
	        try
	        {
		        if (DgvData.SelectedRows.Count == 0 || !TryGetTemplate(DgvData.SelectedRows[0], out var template))
			        return;

                AddViewer(new TraceItemView(DefaultEncoding, UserSettings, false), template);
	        }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        }
	        finally
	        {
		        DgvData.Focus();
	        }
        }

        internal abstract bool TryGetTemplate(DataGridViewRow row, out DataTemplate template);

        private void DgvData_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button != MouseButtons.Right)
                    return;
                var hti = DgvData.HitTest(e.X, e.Y);
                if (hti.RowIndex == -1)
                    return;
                DgvData.ClearSelection();
                DgvData.Rows[hti.RowIndex].Selected = true;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        internal virtual void TxtPatternOnTextChanged(object sender, EventArgs e)
        {
            UserSettings.PreviousSearch = ((TextBox)sender).Text;
            ValidationCheck(true);
        }

        internal virtual void ChbxUseRegex_CheckedChanged(object sender, EventArgs e)
        {
            UserSettings.UseRegex = ((CheckBox)sender).Checked;
        }

        internal virtual void DateStartFilterOnValueChanged(object sender, EventArgs e)
        {
	        if (UserSettings != null)
		        UserSettings.DateStartChecked = DateStartFilter.Checked;

	        if (_oldDateStartChecked || !DateStartFilter.Checked)
		        return;

	        _oldDateStartChecked = true;
	        DateStartFilter.Value = _getStartDate.Invoke();
        }

        internal virtual void DateEndFilterOnValueChanged(object sender, EventArgs e)
        {
	        if (UserSettings != null)
		        UserSettings.DateEndChecked = DateEndFilter.Checked;

	        if (_oldDateEndChecked || !DateEndFilter.Checked)
		        return;

	        _oldDateEndChecked = true;
	        DateEndFilter.Value = _getEndDate.Invoke();
        }

        private void checkBoxShowTrns_CheckedChanged(object sender, EventArgs e)
        {
	        try
	        {
		        UserSettings.ShowTransactions = checkBoxShowTrns.Checked;

		        foreach (var page in tabControlViewer.TabPages.OfType<CustomTabPage>().ToList())
			        page.View.RefreshDescription(checkBoxShowTrns.Checked, out var _);

		        DgvData?.Refresh();
            }
	        catch (Exception)
	        {
		        // ignored
	        }
        }

        private void ComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        internal virtual void CobxTraceNameFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
	        UserSettings.TraceNameFilterContains = ((ComboBox)sender).Text.Like(Resources.Txt_LogsReaderForm_Contains);
        }

        internal virtual void TbxTraceNameFilterOnTextChanged(object sender, EventArgs e)
        {
            UserSettings.TraceNameFilter = ((TextBox)sender).Text;
        }

        internal virtual void CobxTraceMessageFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
	        UserSettings.TraceMessageFilterContains = ((ComboBox)sender).Text.Like(Resources.Txt_LogsReaderForm_Contains);
        }

        internal virtual void TbxTraceMessageFilterOnTextChanged(object sender, EventArgs e)
        {
            UserSettings.TraceMessageFilter = ((TextBox)sender).Text;
        }

        internal virtual void ChbxAlreadyUseFilter_CheckedChanged(object sender, EventArgs e)
        {

        }

        protected virtual void ValidationCheck(bool clearStatus)
        {
	        btnExport.Enabled = DgvData.RowCount > 0;
	        btnFilter.Enabled = ButtonHighlightEnabled = btnReset.Enabled = HasAnyResult;
        }

        protected virtual void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm(true);
        }

        protected virtual void ClearForm(bool saveData)
        {
            try
            {
                if (saveData)
                    SaveData();

                Clear();

                ClearStatus();

                _completedFilesStatus.Text = @"0";
                _totalFilesStatus.Text = @"0";
                _findedInfo.Text = @"0";

                STREAM.GarbageCollect();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        internal void SelectTransactions()
        {
	        if (checkBoxShowTrns.Checked)
		        MainViewer.SelectTransactions();
            DgvData.Refresh();
        }

        internal void DeselectTransactions()
        {
	        foreach (var page in tabControlViewer.TabPages.OfType<CustomTabPage>().ToList())
		        page.View.DeselectTransactions();
	        DgvData.Refresh();
        }

        protected void Clear()
        {
	        try
	        {
		        _oldSortedColumn = null;
		        _currentDGVResult = null;
		        _byAscending = true;

		        DgvData.DataSource = null;
		        DgvData.Rows.Clear();
		        DgvData.Refresh();

		        MainViewer.Clear();
		        foreach (var page in tabControlViewer.TabPages.OfType<CustomTabPage>().ToList())
		        {
			        if(page == MainViewer.Parent)
                        continue;

			        page.View.Clear();
			        tabControlViewer.TabPages.Remove(page);
		        }

                btnExport.Enabled = false;
		        btnFilter.Enabled = ButtonHighlightEnabled = btnReset.Enabled = HasAnyResult;
	        }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        }
        }

        private bool _isLastWasError;

        protected void ReportStatus(string message, ReportStatusType type)
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

        protected void ClearErrorStatus()
        {
            if (!_isLastWasError)
                return;
            _statusInfo.BackColor = SystemColors.Control;
            _statusInfo.ForeColor = Color.Black;
            _statusInfo.Text = string.Empty;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
	        UserSettings?.Dispose();

	        if (disposing)
		        components?.Dispose();
	        base.Dispose(disposing);
        }
	}
}