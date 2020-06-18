using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using LogsReader.Config;
using LogsReader.Properties;
using SPAMassageSaloon.Common;
using Utils;
using Utils.WinForm;
using Utils.WinForm.DataGridViewHelper;
using Utils.WinForm.Notepad;

namespace LogsReader.Reader
{
    public abstract partial class LogsReaderFormBase : UserControl, IUserForm
    {
	    private readonly Func<DateTime> _getStartDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        private readonly Func<DateTime> _getEndDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

        private bool _oldDateStartChecked;
        private bool _oldDateEndChecked;
        private bool _settingsLoaded;

        private readonly ToolStripStatusLabel _statusInfo;
        private readonly ToolStripStatusLabel _findedInfo;
        private readonly ToolStripStatusLabel _completedFilesStatus;
        private readonly ToolStripStatusLabel _totalFilesStatus;
        private readonly ToolStripStatusLabel _filtersCompleted1;
        private readonly ToolStripStatusLabel _filtersCompleted2;
        private readonly ToolStripStatusLabel _overallFound1;
        private readonly ToolStripStatusLabel _overallFound2;

        protected ToolTip Tooltip { get; }

        protected Editor Message { get; }

        protected Editor TraceMessage { get; }

        /// <summary>
        /// Статус выполнения поиска
        /// </summary>
        public bool IsWorking { get; protected set; }

        /// <summary>
        /// Юзерские настройки 
        /// </summary>
        public UserSettings UserSettings { get; }

        public int Progress
        {
            get => IsWorking ? progressBar.Value : 100;
            protected set => progressBar.Value = value;
        }

        public abstract bool HasAnyResult { get; }

        protected LogsReaderFormBase(Encoding defaultEncoding, UserSettings userSettings)
        {
	        InitializeComponent();

	        UserSettings = userSettings;

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

            _overallFound1 = new ToolStripStatusLabel { Font = base.Font, Margin = statusStripItemsPaddingStart };
            _findedInfo = new ToolStripStatusLabel("0") { Font = base.Font, Margin = statusStripItemsPaddingMiddle };
            _overallFound2 = new ToolStripStatusLabel { Font = base.Font, Margin = statusStripItemsPaddingEnd };
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
	            dgvFiles.AutoGenerateColumns = false;
	            dgvFiles.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
	            dgvFiles.CellFormatting += DgvFiles_CellFormatting;

                #region Initialize Controls

                Message = notepad.AddDocument(new BlankDocument {HeaderName = "Message", Language = Language.XML});
                Message.BackBrush = null;
                Message.BorderStyle = BorderStyle.FixedSingle;
                Message.Cursor = Cursors.IBeam;
                Message.DelayedEventsInterval = 1000;
                Message.DisabledColor = Color.FromArgb(100, 171, 171, 171);
                Message.IsReplaceMode = false;
                Message.SelectionColor = Color.FromArgb(50, 0, 0, 255);
                Message.LanguageChanged += Message_LanguageChanged;

                TraceMessage = notepad.AddDocument(new BlankDocument {HeaderName = "Trace"});
                TraceMessage.BackBrush = null;
                TraceMessage.BorderStyle = BorderStyle.FixedSingle;
                TraceMessage.Cursor = Cursors.IBeam;
                TraceMessage.DelayedEventsInterval = 1000;
                TraceMessage.DisabledColor = Color.FromArgb(100, 171, 171, 171);
                TraceMessage.IsReplaceMode = false;
                TraceMessage.SelectionColor = Color.FromArgb(50, 0, 0, 255);
                TraceMessage.LanguageChanged += TraceMessage_LanguageChanged;

                notepad.SelectEditor(0);
                notepad.DefaultEncoding = defaultEncoding;
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
                if (Message.Language != langMessage)
                    Message.ChangeLanguage(langMessage);
                if (TraceMessage.Language != langTrace)
                    TraceMessage.ChangeLanguage(langTrace);

                Message.WordWrap = UserSettings.MessageWordWrap;
                Message.Highlights = UserSettings.MessageHighlights;
                TraceMessage.WordWrap = UserSettings.TraceWordWrap;
                TraceMessage.Highlights = UserSettings.TraceHighlights;

                #endregion
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, Resources.Txt_Initialization);
            }
        }

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

                ParentSplitContainer.SplitterMoved += (sender, args) => { SaveData(); };
                MainSplitContainer.SplitterMoved += (sender, args) => { SaveData(); };
                EnumSplitContainer.SplitterMoved += (sender, args) => { SaveData(); };
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

                Tooltip.RemoveAll();
                Tooltip.SetToolTip(txtPattern, Resources.Txt_Form_SearchComment);
                Tooltip.SetToolTip(useRegex, Resources.Txt_LRSettings_UseRegexComment);
                Tooltip.SetToolTip(dateStartFilter, Resources.Txt_Form_DateFilterComment);
                Tooltip.SetToolTip(dateEndFilter, Resources.Txt_Form_DateFilterComment);
                Tooltip.SetToolTip(traceNameFilter, Resources.Txt_Form_TraceNameFilterComment);
                Tooltip.SetToolTip(traceMessageFilter, Resources.Txt_Form_TraceFilterComment);
                Tooltip.SetToolTip(alreadyUseFilter, Resources.Txt_Form_AlreadyUseFilterComment);
                Tooltip.SetToolTip(buttonExport, Resources.Txt_LogsReaderForm_ExportComment);

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

                ClearForm(false);
                ReportStatus(string.Empty, ReportStatusType.Success);
                ValidationCheck(false);
            }
            catch (Exception ex)
            {
	            ClearForm(false);
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

        public virtual void LogsReaderKeyDown(object sender, KeyEventArgs e)
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
                    case Keys.C when e.Control && dgvFiles.SelectedRows.Count > 0:
                        var templateList = new List<DataTemplate>();
                        foreach (DataGridViewRow row in dgvFiles.SelectedRows)
                        {
                            if (TryGetTemplate(row, out var template))
                                templateList.Insert(0, template);
                        }

                        var clipboardText = new StringBuilder(templateList.Sum(x => x.TraceMessage.Length) + (templateList.Count * 10) + 250);
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

        protected abstract void BtnSearch_Click(object sender, EventArgs e);

        protected void ReportProcessStatus(int countMatches, int percentOfProgeress, int filesCompleted, int totalFiles)
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

                var i = 0;
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

        protected DataFilter GetFilter()
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

        protected abstract Task<bool> AssignResult(DataFilter filter);

        protected async Task AssignToDGV(IEnumerable<DataTemplate> result)
        {
	        await dgvFiles.AssignCollectionAsync(result, null);

	        buttonExport.Enabled = dgvFiles.RowCount > 0;
        }

        protected virtual void ChangeFormStatus()
        {
            btnSearch.Text = IsWorking ? Resources.Txt_LogsReaderForm_Stop : Resources.Txt_LogsReaderForm_Search;
            btnClear.Enabled = !IsWorking;
            txtPattern.Enabled = !IsWorking;

            foreach (var dgvChild in dgvFiles.Controls.OfType<Control>()) // решает баг с задисейбленным скролл баром DataGridView
                dgvChild.Enabled = !IsWorking;
            dgvFiles.Enabled = !IsWorking;

            notepad.Enabled = !IsWorking;
            descriptionText.Enabled = !IsWorking;
            useRegex.Enabled = !IsWorking;
            dateStartFilter.Enabled = !IsWorking;
            dateEndFilter.Enabled = !IsWorking;
            traceNameFilterComboBox.Enabled = !IsWorking;
            traceNameFilter.Enabled = !IsWorking;
            traceMessageFilterComboBox.Enabled = !IsWorking;
            traceMessageFilter.Enabled = !IsWorking;
            alreadyUseFilter.Enabled = !IsWorking;
            buttonExport.Enabled = dgvFiles.RowCount > 0;
            buttonFilter.Enabled = buttonReset.Enabled = HasAnyResult;

            if (IsWorking)
            {
                ParentSplitContainer.Cursor = Cursors.WaitCursor;
                ClearForm();
                Focus();
            }
            else
            {
	            ParentSplitContainer.Cursor = Cursors.Default;
                dgvFiles.Focus();
            }
        }

        protected virtual void DgvFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
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

                row.Cells["File"].ToolTipText = template.ToString();
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        protected void DgvFiles_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                Message.Text = string.Empty;
                TraceMessage.Text = string.Empty;

                if (dgvFiles.CurrentRow == null || dgvFiles.SelectedRows.Count == 0)
                    return;

                if(!TryGetTemplate(dgvFiles.SelectedRows[0], out var template))
                    return;

                var foundByTransactionValue = string.Empty;
                if (!template.TransactionValue.IsNullOrEmptyTrim())
	                foundByTransactionValue = $" | Found by Trn = \"{template.TransactionValue}\"";

                descriptionText.Text = $"FoundLineID = {template.FoundLineID}{foundByTransactionValue}\r\n{template.Description}";

                if (Message.Language == Language.XML || Message.Language == Language.HTML)
                {
                    var messageXML = XML.RemoveUnallowable(template.Message, " ");
                    Message.Text = messageXML.IsXml(out var xmlDoc) ? xmlDoc.PrintXml() : messageXML.TrimWhiteSpaces();
                }
                else
                {
                    Message.Text = template.Message.TrimWhiteSpaces();
                }
                Message.DelayedEventsInterval = 10;

                TraceMessage.Text = template.TraceMessage;
                TraceMessage.DelayedEventsInterval = 10;
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

        protected abstract bool TryGetTemplate(DataGridViewRow row, out DataTemplate template);

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

        private void TxtPattern_TextChanged(object sender, EventArgs e)
        {
            UserSettings.PreviousSearch = txtPattern.Text;
            ValidationCheck(true);
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
            UserSettings.MessageLanguage = Message.Language;
            if((prev == Language.HTML || prev == Language.XML) && (Message.Language == Language.XML || Message.Language == Language.HTML))
                return;
            DgvFiles_SelectionChanged(dgvFiles, EventArgs.Empty);
        }

        private void TraceMessage_LanguageChanged(object sender, EventArgs e)
        {
            UserSettings.TraceLanguage = TraceMessage.Language;
        }

        private void Notepad_WordWrapStateChanged(object sender, EventArgs e)
        {
            if (!(sender is Editor editor))
                return;

            if (editor == Message)
                UserSettings.MessageWordWrap = editor.WordWrap;
            else if (editor == TraceMessage)
                UserSettings.TraceWordWrap = editor.WordWrap;
        }

        private void Notepad_WordHighlightsStateChanged(object sender, EventArgs e)
        {
            if (!(sender is Editor editor))
                return;

            if (editor == Message)
                UserSettings.MessageHighlights = editor.Highlights;
            else if (editor == TraceMessage)
                UserSettings.TraceHighlights = editor.Highlights;
        }

        protected virtual void ValidationCheck(bool clearStatus)
        {
	        buttonExport.Enabled = dgvFiles.RowCount > 0;
	        buttonFilter.Enabled = buttonReset.Enabled = HasAnyResult;
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected virtual void ClearForm(bool saveData = true)
        {
            try
            {
                if (saveData)
                    SaveData();

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

        protected void ClearDGV()
        {
            try
            {
                dgvFiles.DataSource = null;
                dgvFiles.Rows.Clear();
                dgvFiles.Refresh();
                descriptionText.Text = string.Empty;
                if (Message != null)
                    Message.Text = string.Empty;
                if (TraceMessage != null)
                    TraceMessage.Text = string.Empty;
                buttonExport.Enabled = false;
                buttonFilter.Enabled = buttonReset.Enabled = HasAnyResult;
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