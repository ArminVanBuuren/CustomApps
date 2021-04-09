using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using SPAMassageSaloon.Common;
using Utils;
using Utils.WinForm;
using Utils.WinForm.Expander;

namespace LogsReader.Reader
{
	public sealed class LogsReaderFormGlobal : LogsReaderFormBase
	{
		private readonly Panel panelFlowDoc;
		private readonly Panel panelCollapseSelectAll;
		private readonly AdvancedFlowLayoutPanel flowPanelForExpanders;
		private readonly CheckBox checkBoxSelectAll;

		private readonly Func<LogsReaderFormScheme, Color> expanderPanelColor = readerForm
			=> readerForm.BtnSearch.Enabled ? Color.FromArgb(155, 255, 176) : Color.FromArgb(255, 150, 170);

		private Color GetExpanderBorderColor(LogsReaderFormScheme readerForm, ExpandCollapsePanel schemeExpander)
		{
			if (schemeExpander.IsChecked)
				return Color.FromArgb(0, 193, 0);
			if (schemeExpander.CheckBoxEnabled)
				return InProcessing.Count > 0 && InProcessing.TryGetValue(readerForm.CurrentSettings.Name, out var _)
					? Color.FromArgb(60, 60, 60)
					: Color.FromArgb(189, 189, 189);

			return Color.Red;
		}

		private bool _onAllChekingExpanders;

		private GlobalReaderItemsProcessing InProcessing { get; } = new GlobalReaderItemsProcessing();

		private Dictionary<LogsReaderFormScheme, ExpandCollapsePanel> AllExpanders { get; } = new Dictionary<LogsReaderFormScheme, ExpandCollapsePanel>();

		protected override TransactionsMarkingType DefaultTransactionsMarkingType => TransactionsMarkingType.Prompt;

		public LogsReaderMainForm MainForm { get; private set; }

		public override bool HasAnyResult => InProcessing.Any(x => x.Item1.HasAnyResult) && !InProcessing.IsAnyWorking;

		public override RefreshDataType DgvDataAfterAssign => RefreshDataType.AllRows;

		public LogsReaderFormGlobal(Encoding defaultEncoding)
			: base(defaultEncoding, new UserSettings())
		{
			try
			{
				CurrentTransactionsMarkingType = UserSettings.ShowTransactions ? DefaultTransactionsMarkingType : TransactionsMarkingType.None;
				
				DgvReaderSchemeNameColumn.Visible = true;

				#region Initialize Controls

				flowPanelForExpanders = new AdvancedFlowLayoutPanel
				{
					Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
					Location = new Point(0, -1),
					Margin = new Padding(0),
					Name = "FlowPanelForExpanders",
					Size = new Size(147, 5000),
					TabIndex = 27
				};
				
				panelFlowDoc = new Panel
				{
					AutoScroll = true,
					AutoScrollMinSize = new Size(0, 1500),
					Dock = DockStyle.Fill,
					Location = new Point(0, 27),
					Name = "PanelFlowDoc",
					Size = new Size(181, 5000),
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
				
				panelCollapseSelectAll = new Panel
				{
					BorderStyle = BorderStyle.FixedSingle,
					Dock = DockStyle.Top,
					Location = new Point(0, 0),
					Name = "panelCollapseSelectAll",
					Padding = new Padding(3),
					Size = new Size(181, 27),
					BackColor = Color.FromArgb(251, 251, 251),
					TabIndex = 28
				};
				panelCollapseSelectAll.Controls.Add(checkBoxSelectAll);
				
				CustomPanel.Controls.Add(panelFlowDoc);
				CustomPanel.Controls.Add(panelCollapseSelectAll);

				#endregion
			}
			catch (Exception ex)
			{
				ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, Resources.Txt_Initialization);
			}
			finally
			{
				ValidationCheck(false);
			}
		}

		public async void Initialize(LogsReaderMainForm main)
		{
			MainForm = main;

			try
			{
				this.SuspendHandle();
				//var current = SynchronizationContext.Current;

				foreach (var readerForm in MainForm.SchemeForms.Values)
				{
					var expander = CreateExpander(readerForm);
					AllExpanders.Add(readerForm, expander);
					flowPanelForExpanders.Controls.Add(expander);
				}

				//// чекаем все валидные схемы
				await CheckBoxSelectAllOnCheckedChangedAsync(checkBoxSelectAll, EventArgs.Empty);
			}
			finally
			{
				this.ResumeHandle();
			}
		}

		public override void ApplySettings()
		{
			base.ApplySettings();
			checkBoxSelectAll.Text = Resources.Txt_Global_SelectAll;
		}

		private ExpandCollapsePanel CreateExpander(LogsReaderFormScheme readerForm)
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
						ColorTranslator.ToOle(LogsReaderMainForm.SCHEME_COLOR_BACK), ColorTranslator.ToOle(LogsReaderMainForm.SCHEME_COLOR_FORE)
					}
			};
			
			var buttonSize = new Size(20, 17);
			
			var labelBack = new Label
			{
				AutoSize = true,
				ForeColor = Color.Black,
				Location = new Point(25, 3),
				Size = new Size(34, 15),
				Text = Resources.Txt_Global_Back
			};
			buttonBack = new Button
			{
				BackColor = readerForm.FormBackColor,
				FlatStyle = FlatStyle.Flat,
				Location = new Point(3, 3),
				Size = buttonSize,
				UseVisualStyleBackColor = false,
				FlatAppearance =
				{
					BorderColor = Color.Black,
					BorderSize = 1
				}
			};
			buttonBack.Click += ChangeColor;
			
			var labelFore = new Label
			{
				AutoSize = true,
				ForeColor = Color.Black,
				Location = new Point(85, 3),
				Size = new Size(34, 15),
				Text = Resources.Txt_Global_Fore
			};
			buttonFore = new Button
			{
				BackColor = readerForm.FormForeColor,
				FlatStyle = FlatStyle.Flat,
				Location = new Point(63, 3),
				Size = buttonSize,
				UseVisualStyleBackColor = false,
				FlatAppearance =
				{
					BorderColor = Color.Black,
					BorderSize = 1
				}
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
				CheckBoxEnabled = readerForm.BtnSearch.Enabled,
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
			schemeExpander.BackColor = GetExpanderBorderColor(readerForm, schemeExpander);
			
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
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, e) => expanderPanel.SafeInvoke(() =>
			{
				expanderPanel.Controls.AddRange(new[]
				{
					(Control) buttonBack,
					buttonFore,
					labelBack,
					labelFore,
					treeView
				});
			});
			worker.RunWorkerAsync();

			// если закрываются или открываются схемы для глобальной формы в глобальной форме
			schemeExpander.ExpandCollapse += SchemeExpander_ExpandCollapse;
			// если выбирается схема в глобальной форме в checkbox
			schemeExpander.CheckedChanged += async (sender, args) =>
			{
				schemeExpander.BackColor = GetExpanderBorderColor(readerForm, schemeExpander);

				if (schemeExpander.IsChecked && schemeExpander.CheckBoxEnabled)
				{
					// обновляем инфу по всем выбранным схемам основываясь на глобальной
					readerForm.TbxPattern.Text = TbxPattern.Text;
					readerForm.ChbxUseRegex.Checked = ChbxUseRegex.Checked;
					readerForm.DateStartFilter.Value = DateStartFilter.Value;
					readerForm.DateStartFilter.Checked = DateStartFilter.Checked;
					readerForm.DateStartFilterOnValueChanged(this, EventArgs.Empty);
					readerForm.DateEndFilter.Value = DateEndFilter.Value;
					readerForm.DateEndFilter.Checked = DateEndFilter.Checked;
					readerForm.DateEndFilterOnValueChanged(this, EventArgs.Empty);
					readerForm.CobxTraceNameFilter.Text = CobxTraceNameFilter.Text;
					readerForm.TbxTraceNameFilter.Text = TbxTraceNameFilter.Text;
					readerForm.CobxTraceMessageFilter.Text = CobxTraceMessageFilter.Text;
					readerForm.TbxTraceMessageFilter.Text = TbxTraceMessageFilter.Text;
					readerForm.ChbxAlreadyUseFilter.Checked = ChbxAlreadyUseFilter.Checked;
				}

				if (InProcessing.Count > 0
				 && InProcessing.TryGetValue(readerForm.CurrentSettings.Name, out var _)
				 && !_onAllChekingExpanders
				 && !InProcessing.IsAnyWorking)
					await AssignResultAsync(null, null, false);
				ValidationCheck(true);
			};
			// горячие клавишы для добавления сервера, типов и директорий в глобальной форме так и в основной
			treeView.KeyDown += (sender, args) => { readerForm.TreeViewContainer.MainFormKeyDown(treeView, args); };
			// если изменились значения прогресса поиска
			readerForm.OnProcessStatusChanged += (sender, args) =>
			{
				if (InProcessing.ContainsKey(readerForm))
					ReportProcessStatus(GetResultReaders());
			};
			// При загрузке ридеров
			readerForm.OnUploadReaders += async (sender, args) =>
			{
				if (InProcessing.ContainsKey(readerForm))
					await UploadReadersAsync();
			};
			// событие при смене языка формы
			readerForm.OnAppliedSettings += (sender, args) =>
			{
				labelBack.Text = Resources.Txt_Global_Back;
				labelFore.Text = Resources.Txt_Global_Fore;
			};
			// в случае какой то неизвестной ошибки панели TreeView
			readerForm.TreeViewContainer.OnError += ReportStatus;
			// если юзер выбрал допустимые кейсы для поиска в определенной схеме, то разблочиваем кнопку поиска в глобальной схеме
			readerForm.BtnSearch.EnabledChanged += (sender, args) =>
			{
				if (readerForm.BtnSearch.Enabled)
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

				schemeExpander.BackColor = GetExpanderBorderColor(readerForm, schemeExpander);
				expanderPanel.BackColor = expanderPanelColor.Invoke(readerForm);
			};
			// если какая то форма схемы завершила поиск
			readerForm.OnSearchChanged += async (sender, args) =>
			{
				schemeExpander.Enabled = !readerForm.IsWorking;
				if (!InProcessing.ContainsKey(readerForm))
					return;

				if (InProcessing.IsAnyWorking)
				{
					// если выполняется повторный поиск в определенной схеме, то возобновляем процесс и дисейблим грид
					if (InProcessing.IsCompleted)
					{
						TimeWatcher.Start();
						IsWorking = true;
						ReportStatus(Resources.Txt_LogsReaderForm_Working, ReportStatusType.Success);
					}

					return;
				}

				TimeWatcher.Stop();
				MainViewer.Clear();

				// заполняем DataGrid
				if (await AssignResultAsync(ChbxAlreadyUseFilter.Checked ? GetFilter() : null, null, true))
					ReportStatus(string.Format(Resources.Txt_LogsReaderForm_FinishedIn, TimeWatcher.Elapsed.ToReadableString()), ReportStatusType.Success);
				IsWorking = false;
				ReportProcessStatus(GetResultReaders());
				Progress = 100;
			};
			readerForm.OnClear += async (sender, args) =>
			{
				if (InProcessing.ContainsKey(readerForm))
				{
					MainViewer.Clear();
					await UploadReadersAsync();
					await STREAM.GarbageCollectAsync().ConfigureAwait(false);
				}
			};
			return schemeExpander;

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
					readerForm.FormBackColor = colorDialog.Color;
					schemeExpander.HeaderBackColor = colorDialog.Color;
				}
				else if (button == buttonFore)
				{
					readerForm.FormForeColor = colorDialog.Color;
					schemeExpander.ForeColor = colorDialog.Color;
				}

				RefreshAllRows(DgvData, DgvDataRefreshRow);
			}
		}

		internal override void RefreshButtonPauseState(TraceReader reader)
		{
			// отправляем непосредственно в форму где создана, для общей синзронизации кнопки
			if (InProcessing.TryGetValue(reader.SchemeName, out var schemeForm))
				schemeForm.RefreshButtonPauseState(reader);
		}

		protected override void ReportProcessStatus(IEnumerable<TraceReader> readers)
		{
			if (InProcessing.Count == 0)
				return;

			base.ReportProcessStatus(readers);
			if (InProcessing.Where(x => x.Item1.IsWorking).All(x => x.Item1.OnPause))
				base.PauseAll(); // меняем иконку на пуск
			else
				base.ResumeAll(); // меняем иконку на паузу
		}

		internal override void PauseAll()
		{
			base.PauseAll();
			foreach (var reader in InProcessing)
				reader.Item1.PauseAll();
		}

		internal override void ResumeAll()
		{
			base.ResumeAll();
			foreach (var reader in InProcessing)
				reader.Item1.ResumeAll();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			SchemeExpander_ExpandCollapse(this, null);
		}

		protected override void CustomPanel_Resize(object sender, EventArgs args) => SchemeExpander_ExpandCollapse(this, null);

		private void SchemeExpander_ExpandCollapse(object sender, ExpandCollapseEventArgs e)
		{
			if (flowPanelForExpanders == null || panelFlowDoc == null)
				return;

			try
			{
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
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
			finally
			{
				panelFlowDoc.Invalidate();
				Refresh();
			}
		}

		private class GlobalReaderItemsProcessing : IEnumerable<(LogsReaderFormScheme, Task)>
		{
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
					}

					return isAnyWorking;
				}
			}

			public bool IsCompleted { get; private set; }

			public bool ContainsKey(LogsReaderFormScheme readerForm) => TryGetValue(readerForm.CurrentSettings.Name, out var _);

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

				foreach (var readerForm in readerFormCollection)
				{
					if (readerForm.IsWorking)
						continue;

					_items.Add(readerForm.CurrentSettings.Name,
					           (readerForm, Task.Factory.StartNew(() => readerForm.SafeInvoke(() => readerForm.BtnSearch_Click(this, EventArgs.Empty)))));
				}
			}

			public void TryToStop()
			{
				foreach (var reader in _items.Where(x => x.Value.Item1.IsWorking).Select(x => x.Value.Item1))
					reader.BtnSearch_Click(this, EventArgs.Empty);
			}

			public void Clear()
			{
				IsCompleted = false;
				_items.Clear();
			}

			public IEnumerator<(LogsReaderFormScheme, Task)> GetEnumerator() => _items.Values.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => _items.Values.GetEnumerator();
		}

		internal override void BtnSearch_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				TimeWatcher = new Stopwatch();

				try
				{
					base.BtnSearch_Click(sender, e);
					InProcessing.Clear(); // реализовывать ClearForm в форме Global нельзя!
					
					IsWorking = true;
					ReportStatus(Resources.Txt_LogsReaderForm_Working, ReportStatusType.Success);
					
					TimeWatcher.Start();
					InProcessing.Start(AllExpanders.Where(x => x.Value.IsChecked).Select(x => x.Key));
				}
				catch (Exception ex)
				{
					ReportStatus(ex);
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
			checkBoxSelectAll.Cursor = Cursors.Default;
			panelFlowDoc.Cursor = Cursors.Default;
		}

		protected override IEnumerable<DataTemplate> GetResultTemplates()
		{
			var result = new List<DataTemplate>();

			foreach (var schemeForm in AllExpanders.Where(x => x.Value.IsChecked).Select(x => x.Key).Intersect(InProcessing.Select(x => x.Item1)))
			{
				if (!schemeForm.HasAnyResult || schemeForm.IsWorking)
					continue;

				result.AddRange(schemeForm.OverallResultList);
			}

			return result.OrderBy(x => x.Date).ThenBy(x => x.ParentReader.Priority).ThenBy(x => x.File).ThenBy(x => x.FoundLineID).ToList();
		}

		internal override IEnumerable<TraceReader> GetResultReaders() => InProcessing.SelectMany(x => x.Item1.GetResultReaders()).ToList();

		internal override bool TryGetTemplate(DataGridViewRow row, out DataTemplate template)
		{
			template = null;
			var schemeName = row?.Cells[DgvDataSchemeNameColumn.Name]?.Value?.ToString();
			if (schemeName == null
			 || InProcessing == null
			 || !InProcessing.TryGetValue(schemeName, out var readerForm)
			 || !readerForm.TryGetTemplate(row, out var templateResult))
				return false;

			template = templateResult;
			return true;
		}

		internal override bool TryGetReader(DataGridViewRow row, out TraceReader reader)
		{
			reader = null;
			var schemeName = row?.Cells[DgvReaderSchemeNameColumn.Name]?.Value?.ToString();
			if (schemeName == null
			 || InProcessing == null
			 || !InProcessing.TryGetValue(schemeName, out var readerForm)
			 || !readerForm.TryGetReader(row, out var readerResult))
				return false;

			reader = readerResult;
			return true;
		}

		protected override void BtnClear_Click(object sender, EventArgs e)
		{
			InProcessing.Clear();
			base.BtnClear_Click(this, e);
		}

		protected override void ColorizationDGV(DataGridViewRow row, DataTemplate template)
		{
			if (!InProcessing.TryGetValue(template.SchemeName, out var result))
				return;

			if (row.DefaultCellStyle.BackColor != result.FormBackColor)
				row.DefaultCellStyle.BackColor = result.FormBackColor;
			if (row.DefaultCellStyle.ForeColor != result.FormForeColor)
				row.DefaultCellStyle.ForeColor = result.FormForeColor;
		}

		protected override void CheckBoxTransactionsMarkingTypeChanged(TransactionsMarkingType newType)
			=> UserSettings.ShowTransactions = newType != TransactionsMarkingType.None;

		private async void CheckBoxSelectAllOnCheckedChanged(object sender, EventArgs e) => await CheckBoxSelectAllOnCheckedChangedAsync(sender, e);

		private async Task CheckBoxSelectAllOnCheckedChangedAsync(object sender, EventArgs e)
		{
			try
			{
				_onAllChekingExpanders = true;
				foreach (var expander in AllExpanders.Values.Where(expander => expander.CheckBoxEnabled))
					expander.IsChecked = checkBoxSelectAll.Checked;
				if (InProcessing.Count > 0)
					await AssignResultAsync(null, null, false);
				UserSettings.GlobalSelectAllSchemas = checkBoxSelectAll.Checked;
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
			finally
			{
				_onAllChekingExpanders = false;
			}
		}

		internal override void TxtPatternOnTextChanged(object sender, EventArgs e)
		{
			base.TxtPatternOnTextChanged(sender, e);
			foreach (var schemeForm in GetSelectedSchemas())
				schemeForm.TbxPattern.Text = ((TextBox) sender).Text;
		}

		internal override void ChbxUseRegex_CheckedChanged(object sender, EventArgs e)
		{
			base.ChbxUseRegex_CheckedChanged(sender, e);
			foreach (var schemeForm in GetSelectedSchemas())
				schemeForm.ChbxUseRegex.Checked = ((CheckBox) sender).Checked;
		}

		internal override void DateStartFilterOnValueChanged(object sender, EventArgs e)
		{
			base.DateStartFilterOnValueChanged(sender, e);

			foreach (var schemeForm in GetSelectedSchemas())
			{
				schemeForm.DateStartFilter.Value = DateStartFilter.Value;
				schemeForm.DateStartFilter.Checked = DateStartFilter.Checked;
				schemeForm.DateStartFilterOnValueChanged(this, EventArgs.Empty);
			}
		}

		internal override void DateEndFilterOnValueChanged(object sender, EventArgs e)
		{
			base.DateEndFilterOnValueChanged(sender, e);

			foreach (var schemeForm in GetSelectedSchemas())
			{
				schemeForm.DateEndFilter.Value = DateEndFilter.Value;
				schemeForm.DateEndFilter.Checked = DateEndFilter.Checked;
				schemeForm.DateEndFilterOnValueChanged(this, EventArgs.Empty);
			}
		}

		internal override void CobxTraceNameFilter_SelectedIndexChanged(object sender, EventArgs e)
		{
			base.CobxTraceNameFilter_SelectedIndexChanged(sender, e);
			foreach (var schemeForm in GetSelectedSchemas())
				schemeForm.CobxTraceNameFilter.Text = ((ComboBox) sender).Text;
		}

		internal override void TbxTraceNameFilterOnTextChanged(object sender, EventArgs e)
		{
			base.TbxTraceNameFilterOnTextChanged(sender, e);
			foreach (var schemeForm in GetSelectedSchemas())
				schemeForm.TbxTraceNameFilter.Text = ((TextBox) sender).Text;
		}

		internal override void CobxTraceMessageFilter_SelectedIndexChanged(object sender, EventArgs e)
		{
			base.CobxTraceMessageFilter_SelectedIndexChanged(sender, e);
			foreach (var schemeForm in GetSelectedSchemas())
				schemeForm.CobxTraceMessageFilter.Text = ((ComboBox) sender).Text;
		}

		internal override void TbxTraceMessageFilterOnTextChanged(object sender, EventArgs e)
		{
			base.TbxTraceMessageFilterOnTextChanged(sender, e);
			foreach (var schemeForm in GetSelectedSchemas())
				schemeForm.TbxTraceMessageFilter.Text = ((TextBox) sender).Text;
		}

		internal override void ChbxAlreadyUseFilter_CheckedChanged(object sender, EventArgs e)
		{
			foreach (var schemeForm in GetSelectedSchemas())
				schemeForm.ChbxAlreadyUseFilter.Checked = ((CheckBox) sender).Checked;
		}

		private IEnumerable<LogsReaderFormScheme> GetSelectedSchemas() => AllExpanders.Where(x => x.Value.IsChecked).Select(x => x.Key).ToList();

		protected override void ValidationCheck(bool clearStatus)
		{
			BtnSearch.Enabled = AllExpanders.Any(x => x.Key.BtnSearch.Enabled && x.Value.IsChecked);
			base.ValidationCheck(clearStatus);
		}
	}
}