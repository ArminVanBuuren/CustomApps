using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using Utils;
using Utils.WinForm;
using Utils.WinForm.Expander;

namespace LogsReader.Reader
{
    public class LogsReaderFormGlobal : LogsReaderFormBase
    {
	    private readonly Panel panelFlowDoc;
		private readonly AdvancedFlowLayoutPanel flowPanelForExpanders;
		private readonly CheckBox checkBoxSelectAll;

		private readonly Func<LogsReaderFormScheme, Color> expanderBorderColor = (readerForm) => readerForm.BTNSearch.Enabled ? Color.ForestGreen : Color.Red;
		private readonly Func<LogsReaderFormScheme, Color> expanderPanelColor = (readerForm) => readerForm.BTNSearch.Enabled ? Color.FromArgb(217, 255, 217) : Color.FromArgb(255, 150, 170);

		private GlobalReaderItemsProcessing InProcessing { get; } = new GlobalReaderItemsProcessing();

		private Dictionary<LogsReaderFormScheme, ExpandCollapsePanel> AllExpanders { get; } = new Dictionary<LogsReaderFormScheme, ExpandCollapsePanel>();

		public LogsReaderMainForm MainForm { get; private set; }

		public override bool HasAnyResult => InProcessing.Any(x => x.Item1.HasAnyResult);

		public LogsReaderFormGlobal(Encoding defaultEncoding) : base(defaultEncoding, new UserSettings())
        {
			#region Initialize Controls

			flowPanelForExpanders = new AdvancedFlowLayoutPanel
	        {
		        Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right,
		        Location = new Point(0, -1),
		        Margin = new Padding(0),
		        Name = "FlowPanelForExpanders",
		        Size = new Size(147, 3623),
		        TabIndex = 27
	        };

	        panelFlowDoc = new Panel
	        {
		        AutoScroll = true,
		        AutoScrollMinSize = new Size(0, 1500),
		        Dock = DockStyle.Fill,
		        Location = new Point(0, 27),
		        Name = "PanelFlowDoc",
		        Size = new Size(181, 439),
		        TabIndex = 29
	        };
	        panelFlowDoc.Controls.Add(flowPanelForExpanders);

	        checkBoxSelectAll = new CheckBox
	        {
		        AutoSize = true,
		        CheckAlign = ContentAlignment.MiddleRight,
		        Dock = DockStyle.Right,
		        Location = new Point(80, 3),
		        Name = "checkBoxSelectAll",
		        Padding = new Padding(0, 0, 22, 0),
		        Size = new Size(96, 19),
		        TabIndex = 1,
		        Text = Resources.Txt_Global_SelectAll,
		        UseVisualStyleBackColor = true,
				Checked = UserSettings.GlobalSelectAllSchemas
			};
	        checkBoxSelectAll.CheckedChanged += CheckBoxSelectAllOnCheckedChanged;

	        var panelCollapseSelectAll = new Panel
	        {
		        BorderStyle = BorderStyle.FixedSingle,
		        Dock = DockStyle.Top,
		        Location = new Point(0, 0),
		        Name = "panelCollapseSelectAll",
		        Padding = new Padding(3),
		        Size = new Size(181, 27),
		        TabIndex = 28
	        };
			panelCollapseSelectAll.Controls.Add(checkBoxSelectAll);

			MainSplitContainer.Panel1.Controls.Add(panelFlowDoc);
			MainSplitContainer.Panel1.Controls.Add(panelCollapseSelectAll);
			MainSplitContainer.Panel1MinSize = 115;

			#endregion
		}

		public void Initialize(LogsReaderMainForm main)
	    {
		    MainForm = main;

			foreach (var readerForm in MainForm.SchemeForms.Values)
		    {
			    var expander = CreateExpander(readerForm);
			    AllExpanders.Add(readerForm, expander);
			    flowPanelForExpanders.Controls.Add(expander);
		    }
		    SchemeExpander_ExpandCollapse(this, null);

			// чекаем все валидные схемы
		    CheckBoxSelectAllOnCheckedChanged(checkBoxSelectAll, EventArgs.Empty);
	    }

		public override void ApplySettings()
		{
			checkBoxSelectAll.Text = Resources.Txt_Global_SelectAll;
			base.ApplySettings();
		}

		ExpandCollapsePanel CreateExpander(LogsReaderFormScheme readerForm)
		{
			Button buttonBack = null;
			Button buttonFore = null;
			ExpandCollapsePanel schemeExpander = null;

			var colorDialog = new ColorDialog
			{
				AllowFullOpen = true,
				FullOpen = true, 
				CustomColors = new[]
				{
					ColorTranslator.ToOle(LogsReaderMainForm.SCHEME_COLOR_BACK),
					ColorTranslator.ToOle(LogsReaderMainForm.SCHEME_COLOR_FORE)
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

				if (button == buttonBack)
				{
					readerForm.UserSettings.BackColor = colorDialog.Color;
					schemeExpander.HeaderBackColor = colorDialog.Color;
				}
				else if (button == buttonFore)
				{
					readerForm.UserSettings.ForeColor = colorDialog.Color;
					schemeExpander.ForeColor = colorDialog.Color;
				}
			}

			var buttonSize = new Size(20, 17);

			var labelBack = new Label { AutoSize = true, ForeColor = Color.Black, Location = new Point(25, 3), Size = new Size(34, 15), Text = Resources.Txt_Global_Back };
			buttonBack = new Button
			{
				BackColor = readerForm.UserSettings.BackColor,
				FlatStyle = FlatStyle.Flat,
				Location = new Point(3, 3),
				Size = buttonSize,
				UseVisualStyleBackColor = false
			};
			buttonBack.Click += ChangeColor;

			var labelFore = new Label { AutoSize = true, ForeColor = Color.Black, Location = new Point(85, 3), Size = new Size(34, 15), Text = Resources.Txt_Global_Fore };
			buttonFore = new Button
			{
				BackColor = readerForm.UserSettings.ForeColor,
				FlatStyle = FlatStyle.Flat,
				Location = new Point(63, 3),
				Size = buttonSize,
				UseVisualStyleBackColor = false
			};
			buttonFore.Click += ChangeColor;

			schemeExpander = new ExpandCollapsePanel
	        {
		        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
		        BordersThickness = 0,
		        ButtonSize = ExpandButtonSize.Small,
		        ButtonStyle = ExpandButtonStyle.Circle,
		        CheckBoxShown = true,
		        ExpandedHeight = 300,
		        CheckBoxEnabled = readerForm.BTNSearch.Enabled,
				BackColor = expanderBorderColor.Invoke(readerForm),
				HeaderBorderBrush = Color.Azure,
		        HeaderBackColor = buttonBack.BackColor,
		        ForeColor = buttonFore.BackColor,
				HeaderLineColor = Color.White,
				Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
		        IsChecked = false,
		        IsExpanded = false,
		        UseAnimation = false,
		        Text = readerForm.CurrentSettings.Name,
		        Padding = new Padding(2),
                Margin = new Padding(3, 3, 3, 0)
	        };
			
			var expanderPanel = new Panel
	        {
		        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = expanderPanelColor.Invoke(readerForm),
                Location = new Point(2, 23),
                Size = new Size(schemeExpander.Size.Width - 4, 275)
            };
			schemeExpander.Controls.Add(expanderPanel);

			var treeView = readerForm.TreeViewContainer.CreateNewCopy();
	        treeView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
	        treeView.DrawMode = TreeViewDrawMode.OwnerDrawAll;
	        treeView.Location = new Point(-1, 23);
	        treeView.Size = new Size(schemeExpander.Size.Width - 2, 253);
	        void OnTreeViewMouseDown(object sender, MouseEventArgs e)
	        {
		        if (!readerForm.IsInitialized)
			        readerForm.Initialize();
		        treeView.MouseDown -= OnTreeViewMouseDown;
	        }
			treeView.MouseDown += OnTreeViewMouseDown;

			expanderPanel.Controls.Add(buttonBack);
	        expanderPanel.Controls.Add(buttonFore);
	        expanderPanel.Controls.Add(labelBack);
	        expanderPanel.Controls.Add(labelFore);
	        expanderPanel.Controls.Add(treeView);

			// если закрываются или открываются схемы для глобальной формы в глобальной форме
	        schemeExpander.ExpandCollapse += SchemeExpander_ExpandCollapse;
			// если выбирается схема в глобальной форме в checkbox
	        schemeExpander.CheckedChanged += (sender, args) =>
	        {
		        schemeExpander.BackColor = !schemeExpander.IsChecked && schemeExpander.CheckBoxEnabled ? Color.DimGray : expanderBorderColor.Invoke(readerForm);
		        
		        if (schemeExpander.IsChecked && schemeExpander.CheckBoxEnabled)
				{
					// обновляем инфу по всем выбранным схемам основываясь на глобальной
					readerForm.txtPattern.Text = txtPattern.Text;
					readerForm.useRegex.Checked = useRegex.Checked;
					readerForm.dateStartFilter.Value = dateStartFilter.Value;
					readerForm.dateStartFilter.Checked = dateStartFilter.Checked;
					readerForm.DateStartFilterOnValueChanged(this, EventArgs.Empty);
					readerForm.dateEndFilter.Value = dateEndFilter.Value;
					readerForm.dateEndFilter.Checked = dateEndFilter.Checked;
					readerForm.DateEndFilterOnValueChanged(this, EventArgs.Empty);
					readerForm.traceNameFilterComboBox.Text = traceNameFilterComboBox.Text;
					readerForm.traceNameFilter.Text = traceNameFilter.Text;
					readerForm.traceMessageFilterComboBox.Text = traceMessageFilterComboBox.Text;
					readerForm.traceMessageFilter.Text = traceMessageFilter.Text;
					readerForm.alreadyUseFilter.Checked = alreadyUseFilter.Checked;
				}

				ValidationCheck(true);
	        };
			// горячие клавишы для добавления сервера, типов и директорий в глобальной форме так и в основной
	        treeView.KeyDown += (sender, args) =>
	        {
		        readerForm.TreeViewContainer.MainFormKeyDown(treeView, args);
	        };
			// если изменились значения прогресса поиска
	        readerForm.OnProcessStatusChanged += (sender, args) =>
	        {
		        if (InProcessing.Count == 0 || !InProcessing.TryGetValue(readerForm.CurrentSettings.Name, out var _))
			        return;

		        var countMatches = 0;
				var progress = 0;
				var filesCompleted = 0;
				var totalFiles = 0;
				foreach (var schemeForm in InProcessing.Select(x => x.Item1))
				{
					countMatches += schemeForm.CountMatches;
					progress += schemeForm.Progress;
					filesCompleted += schemeForm.FilesCompleted;
					totalFiles += schemeForm.TotalFiles;
				}

				base.ReportProcessStatus(countMatches, progress / InProcessing.Count(), filesCompleted, totalFiles);
	        };
			// событие при смене языка формы
	        readerForm.OnAppliedSettings += (sender, args) =>
	        {
		        labelBack.Text = Resources.Txt_Global_Back;
		        labelFore.Text = Resources.Txt_Global_Fore;
	        };
			// в случае какой то неизвестной ошибки панели TreeView
			readerForm.TreeViewContainer.OnError += ex =>
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        };
			// если юзер выбрал допустимые кейсы для поиска в определенной схеме, то разблочиваем кнопку поиска в глобальной схеме
			readerForm.BTNSearch.EnabledChanged += (sender, args) =>
			{
				schemeExpander.BackColor = expanderBorderColor.Invoke(readerForm);
				expanderPanel.BackColor = expanderPanelColor.Invoke(readerForm);

				if (readerForm.BTNSearch.Enabled)
				{
					schemeExpander.CheckBoxEnabled = true;
					// выбираем только в случае если открыта Global форма
					if (MainForm.CurrentForm == this)
					{
						if (schemeExpander.IsChecked != checkBoxSelectAll.Checked)
							schemeExpander.IsChecked = checkBoxSelectAll.Checked;
						else
							ValidationCheck(true);
					}
				}
				else
				{
					schemeExpander.CheckBoxEnabled = false;
					if (schemeExpander.IsChecked)
						schemeExpander.IsChecked = false;
					else
						ValidationCheck(true);
				}
			};
			// если какая то форма схемы завершила поиск
			readerForm.OnSearchChanged += async (sender, args) =>
			{
				schemeExpander.Enabled = !readerForm.IsWorking;

				if (InProcessing.Count == 0 || !InProcessing.TryGetValue(readerForm.CurrentSettings.Name, out var _))
					return;

				if (InProcessing.IsAnyWorking)
				{
					// если выполняется повторный поиск в определенной схеме, то возобновляем процесс и дисейблим грид
					if (InProcessing.IsCompleted)
					{
						InProcessing.Continue();
						IsWorking = true;
						ReportStatus(Resources.Txt_LogsReaderForm_Working, ReportStatusType.Success);
					}

					return;
				}

				// заполняем DataGrid
				if (await AssignResult(alreadyUseFilter.Checked ? GetFilter() : null))
				{
					Progress = 100;
					ReportStatus(string.Format(Resources.Txt_LogsReaderForm_FinishedIn, InProcessing.Elapsed.ToReadableString()), ReportStatusType.Success);
				}

				IsWorking = false;
			};

			return schemeExpander;
        }

		protected override void OnResize(EventArgs e)
        {
	        base.OnResize(e);
	        SchemeExpander_ExpandCollapse(this, null);
        }

        private void SchemeExpander_ExpandCollapse(object sender, ExpandCollapseEventArgs e)
        {
			if(flowPanelForExpanders == null)
				return;

	        var height = 0;
	        foreach (var expander in flowPanelForExpanders.Controls.OfType<ExpandCollapsePanel>())
	        {
		        if (expander.IsExpanded)
			        height += expander.ExpandedHeight;
		        else
			        height += expander.CollapsedHeight;

		        height += expander.Margin.Top + expander.Margin.Bottom;
	        }

            panelFlowDoc.AutoScrollMinSize = new Size(0, height);
            
            if (panelFlowDoc.VerticalScroll.Visible)
            {
	            flowPanelForExpanders.Size = new Size(panelFlowDoc.Size.Width - 16, height);
	            checkBoxSelectAll.Padding = new Padding(0, 0, 23, 0);
            }
            else
            {
	            flowPanelForExpanders.Size = new Size(panelFlowDoc.Size.Width - 2, height);
	            checkBoxSelectAll.Padding = new Padding(0, 0, 9, 0);
            }
        }

        class GlobalReaderItemsProcessing : IEnumerable<(LogsReaderFormScheme, Task)>
		{
	        private Stopwatch _timeWatcher;
			private readonly Dictionary<string, (LogsReaderFormScheme, Task)> _items = new Dictionary<string, (LogsReaderFormScheme, Task)>();

			public int Count => _items.Count;

			public bool IsAnyWorking
			{
				get
				{
					var isAnyWorking = _items != null && _items.Any(x => x.Value.Item1.IsWorking);
					if (!isAnyWorking)
					{
						IsCompleted = true;
						_timeWatcher.Stop();
					}

					return isAnyWorking;
				}
			}

			public TimeSpan Elapsed => _timeWatcher.Elapsed;

			public bool IsCompleted { get; private set; } = false;

			public bool TryGetValue(string schemeName, out LogsReaderFormScheme result)
			{
				result = null;
				if (_items.TryGetValue(schemeName, out var result2))
				{
					result = result2.Item1;
					return true;
				}

				return false;
			}

			public void Start(IEnumerable<LogsReaderFormScheme> readerFormCollection)
			{
				Clear();
				_timeWatcher = new Stopwatch();
				_timeWatcher.Start();

				foreach (var readerForm in readerFormCollection)
				{
					if (readerForm.IsWorking)
						continue;

					_items.Add(
						readerForm.CurrentSettings.Name, 
						(readerForm, Task.Factory.StartNew(() => readerForm.SafeInvoke(() => readerForm.BtnSearch_Click(this, EventArgs.Empty)))));
				}
			}

			public void TryToStop()
			{
				foreach (var reader in _items
					.Where(x => x.Value.Item1.IsWorking)
					.Select(x => x.Value.Item1))
					reader.BtnSearch_Click(this, EventArgs.Empty);
			}

			public void Continue()
			{
				_timeWatcher?.Start();
			}

			public void Clear()
			{
				IsCompleted = false;
				_items.Clear();
			}

			public IEnumerator<(LogsReaderFormScheme, Task)> GetEnumerator()
			{
				return _items.Values.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _items.Values.GetEnumerator();
			}
		}

        internal override void BtnSearch_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				try
				{
					base.BtnSearch_Click(sender, e);
					InProcessing.Clear(); // реализовывать ClearForm в форме Global нельзя!

					IsWorking = true;
					ReportStatus(Resources.Txt_LogsReaderForm_Working, ReportStatusType.Success);

					InProcessing.Start(AllExpanders
						.Where(x => x.Value.IsChecked)
						.Select(x => x.Key));
				}
				catch (Exception ex)
				{
					ReportStatus(ex.Message, ReportStatusType.Error);
				}
			}
			else
			{
				InProcessing?.TryToStop();
				ReportStatus(Resources.Txt_LogsReaderForm_Stopping, ReportStatusType.Success);
			}
		}

        protected override void ChangeFormStatus()
        {
	        base.ChangeFormStatus();
	        checkBoxSelectAll.Enabled = !IsWorking;
        }

        protected override IEnumerable<DataTemplate> GetResultTemplates()
        {
	        var result = new List<DataTemplate>();

			foreach (var schemeForm in AllExpanders
				.Where(x => x.Value.IsChecked)
				.Select(x => x.Key)
				.Intersect(InProcessing.Select(x => x.Item1)))
	        {
		        if (!schemeForm.HasAnyResult || schemeForm.IsWorking)
			        continue;
		        result.AddRange(schemeForm.OverallResultList);
	        }

	        return result.OrderBy(x => x.Date).ThenBy(x => x.File).ThenBy(x => x.FoundLineID).ToList();
        }

        internal override bool TryGetTemplate(DataGridViewRow row, out DataTemplate template)
		{
			template = null;
			var schemeName = row?.Cells["SchemeName"]?.Value?.ToString();
			if (schemeName == null
			    || InProcessing == null
			    || !InProcessing.TryGetValue(schemeName, out var readerForm)
			    || !readerForm.TryGetTemplate(row, out var templateResult))
				return false;

			template = templateResult;
			return true;
		}

		protected override void BtnClear_Click(object sender, EventArgs e)
		{
			InProcessing.Clear();
			base.BtnClear_Click(sender, e);
		}

		protected override void СolorizationDGV(DataGridViewRow row, DataTemplate template)
		{
			if(!InProcessing.TryGetValue(template.SchemeName, out var result))
				return;

			row.DefaultCellStyle.BackColor = result.UserSettings.BackColor;
			row.DefaultCellStyle.ForeColor = result.UserSettings.ForeColor;
		}

        private void CheckBoxSelectAllOnCheckedChanged(object sender, EventArgs e)
        {
			foreach (var expander in AllExpanders.Values.Where(expander => expander.CheckBoxEnabled))
				expander.IsChecked = checkBoxSelectAll.Checked;
			UserSettings.GlobalSelectAllSchemas = checkBoxSelectAll.Checked;
		}

        internal override void TxtPattern_TextChanged(object sender, EventArgs e)
        {
	        base.TxtPattern_TextChanged(sender, e);
	        foreach (var schemeForm in GetSelectedSchemas())
		        schemeForm.txtPattern.Text = ((TextBox) sender).Text;
        }

        internal override void UseRegex_CheckedChanged(object sender, EventArgs e)
		{
			base.UseRegex_CheckedChanged(sender, e);
			foreach (var schemeForm in GetSelectedSchemas())
				schemeForm.useRegex.Checked = ((CheckBox)sender).Checked;
		}

		internal override void DateStartFilterOnValueChanged(object sender, EventArgs e)
		{
			base.DateStartFilterOnValueChanged(sender, e);
			foreach (var schemeForm in GetSelectedSchemas())
			{
				schemeForm.dateStartFilter.Value = dateStartFilter.Value;
				schemeForm.dateStartFilter.Checked = dateStartFilter.Checked;
				schemeForm.DateStartFilterOnValueChanged(this, EventArgs.Empty);
			}
		}

		internal override void DateEndFilterOnValueChanged(object sender, EventArgs e)
		{
			base.DateEndFilterOnValueChanged(sender, e);
			foreach (var schemeForm in GetSelectedSchemas())
			{
				schemeForm.dateEndFilter.Value = dateEndFilter.Value;
				schemeForm.dateEndFilter.Checked = dateEndFilter.Checked;
				schemeForm.DateEndFilterOnValueChanged(this, EventArgs.Empty);
			}
		}

		internal override void traceNameFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			base.traceNameFilterComboBox_SelectedIndexChanged(sender, e);
			foreach (var schemeForm in GetSelectedSchemas())
				schemeForm.traceNameFilterComboBox.Text = ((ComboBox)sender).Text;
		}

		internal override void TraceNameFilter_TextChanged(object sender, EventArgs e)
		{
			base.TraceNameFilter_TextChanged(sender, e);
			foreach (var schemeForm in GetSelectedSchemas())
				schemeForm.traceNameFilter.Text = ((TextBox)sender).Text;
		}

		internal override void traceMessageFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			base.traceMessageFilterComboBox_SelectedIndexChanged(sender, e);
			foreach (var schemeForm in GetSelectedSchemas())
				schemeForm.traceMessageFilterComboBox.Text = ((ComboBox)sender).Text;
		}

		internal override void TraceMessageFilter_TextChanged(object sender, EventArgs e)
		{
			base.TraceMessageFilter_TextChanged(sender, e);
			foreach (var schemeForm in GetSelectedSchemas())
				schemeForm.traceMessageFilter.Text = ((TextBox)sender).Text;
		}

		internal override void alreadyUseFilter_CheckedChanged(object sender, EventArgs e)
		{
			foreach (var schemeForm in GetSelectedSchemas())
				schemeForm.alreadyUseFilter.Checked = ((CheckBox)sender).Checked;
		}

		IEnumerable<LogsReaderFormScheme> GetSelectedSchemas()
		{
			return AllExpanders.Where(x => x.Value.IsChecked).Select(x => x.Key).ToList();
		}

		protected override void ValidationCheck(bool clearStatus)
		{
			BTNSearch.Enabled = AllExpanders.Any(x => x.Key.BTNSearch.Enabled && x.Value.IsChecked);
			base.ValidationCheck(clearStatus);
		}
    }
}