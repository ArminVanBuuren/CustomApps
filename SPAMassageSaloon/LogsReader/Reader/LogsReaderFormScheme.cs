using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using LogsReader.Reader.Forms;
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
		private readonly Button configureButton;
		private readonly CustomTreeView TreeMain;

		protected override TransactionsMarkingType DefaultTransactionsMarkingType => TransactionsMarkingType.Both;

		/// <summary>
		///     Сохранить изменения в конфиг
		/// </summary>
		public event EventHandler OnSchemeChanged;

		/// <summary>
		///     Поиск логов начался или завершился
		/// </summary>
		public event EventHandler OnSearchChanged;

		/// <summary>
		///     Происходит при полной очистке
		/// </summary>
		public event EventHandler OnClear;

		/// <summary>
		///     Текущая схема настроек
		/// </summary>
		public LRSettingsScheme CurrentSettings { get; private set; }

		public LogsReaderPerformerScheme MainReader { get; private set; }

		public TreeViewContainer TreeViewContainer { get; }

		public DataTemplateCollection OverallResultList { get; private set; }

		public override bool HasAnyResult => OverallResultList != null && OverallResultList.Count > 0;

		public bool IsTreeViewSynchronized { get; private set; }

		public override RefreshDataType DgvDataAfterAssign => RefreshDataType.VisibleRows;

		public LogsReaderFormScheme(LRSettingsScheme scheme)
			: base(scheme.Encoding, new UserSettings(scheme.Name))
		{
			try
			{
				CurrentSettings = scheme;
				CurrentSettings.ReportStatus += ReportStatus;

				#region Initialize Controls

				MaxLinesLabel = new Label
				{
					AutoSize = true,
					Location = new Point(3, 10),
					Name = "MaxLinesLabel",
					Size = new Size(59, 15),
					TabIndex = 13,
					Text = Resources.Txt_LogsReaderForm_MaxLines
				};
				maxLinesStackText = new TextBox
				{
					Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
					Location = new Point(84, 7),
					Name = "maxLinesStackText",
					Size = new Size(50, 23),
					TabIndex = 15
				};
				maxLinesStackText.TextChanged += MaxLinesStackText_TextChanged;
				maxLinesStackText.Leave += MaxLinesStackText_Leave;

				MaxThreadsLabel = new Label
				{
					AutoSize = true,
					Location = new Point(3, 36),
					Name = "MaxThreadsLabel",
					Size = new Size(74, 15),
					TabIndex = 9,
					Text = Resources.Txt_LogsReaderForm_MaxThreads
				};
				maxThreadsText = new TextBox
				{
					Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
					Location = new Point(84, 33),
					Name = "maxThreadsText",
					Size = new Size(50, 23),
					TabIndex = 16
				};
				maxThreadsText.TextChanged += MaxThreadsText_TextChanged;
				maxThreadsText.Leave += MaxThreadsText_Leave;

				RowsLimitLabel = new Label
				{
					AutoSize = true,
					Location = new Point(3, 62),
					Name = "RowsLimitLabel",
					Size = new Size(65, 15),
					TabIndex = 15,
					Text = Resources.Txt_LogsReaderForm_RowsLimit
				};
				rowsLimitText = new TextBox
				{
					Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
					Location = new Point(84, 59),
					Name = "rowsLimitText",
					Size = new Size(50, 23),
					TabIndex = 17
				};
				rowsLimitText.TextChanged += RowsLimitText_TextChanged;
				rowsLimitText.Leave += RowsLimitText_Leave;

				OrderByLabel = new Label
				{
					AutoSize = true,
					Location = new Point(3, 88),
					Name = "OrderByLabel",
					Size = new Size(53, 15),
					TabIndex = 17,
					Text = Resources.Txt_LogsReaderForm_OrderBy
				};
				orderByText = new TextBox
				{
					Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
					Location = new Point(84, 85),
					Name = "orderByText",
					Size = new Size(50, 23),
					TabIndex = 18
				};
				orderByText.Leave += OrderByText_Leave;

				configureButton = new Button
				{
					Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
					Location = new Point(3, 111),
					Name = "configureButton",
					Size = new Size(133, 25),
					TabIndex = 19,
					Text = @"Configure",
					BackColor = Color.FromArgb(71, 203, 172),
					ForeColor = Color.White,
				};
				configureButton.Click += ConfigureButton_Click;

				TreeMain = new CustomTreeView
				{
					Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
					Location = new Point(1, 140),
					Name = "TreeMain",
					Size = new Size(137, 275),
					TabIndex = 20,
				};

				CustomPanel.Controls.AddRange(new []
				{
					(Control)OrderByLabel,
					MaxLinesLabel,
					orderByText,
					MaxThreadsLabel,
					maxLinesStackText,
					RowsLimitLabel,
					maxThreadsText,
					rowsLimitText,
					configureButton,
					TreeMain
				});

				#endregion

				#region Options

				orderByText.GotFocus += OrderByText_GotFocus;

				#endregion

				#region TreeView Container

				TreeViewContainer = new TreeViewContainer(CurrentSettings, TreeMain);
				TreeViewContainer.OnChanged += (clearStatus, onSchemeChanged) =>
				{
					ValidationCheck(clearStatus);
					if (onSchemeChanged)
						OnSchemeChanged?.Invoke(this, EventArgs.Empty);
				};
				TreeViewContainer.OnError += ReportStatus;
				
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
				InitSettings();
				Clear();
				ValidationCheck(false);
			}
		}

		private void ConfigureButton_Click(object sender, EventArgs e)
		{
			var configureForm = new ConfigureForm(CurrentSettings)
			{
				StartPosition = FormStartPosition.CenterParent
			};
			var result = configureForm.ShowDialog();

			if (result == DialogResult.OK && configureForm.SettingsOfScheme != null)
			{
				CurrentSettings = configureForm.SettingsOfScheme;
				InitSettings();
				TreeViewContainer.Reload(CurrentSettings);
				OnSchemeChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		void InitSettings()
		{
			CurrentTransactionsMarkingType = CurrentSettings.TraceParse.SelectionTransactionsType;
			maxLinesStackText.AssignValue(CurrentSettings.MaxLines, MaxLinesStackText_TextChanged);
			maxThreadsText.AssignValue(CurrentSettings.MaxThreads, MaxThreadsText_TextChanged);
			rowsLimitText.AssignValue(CurrentSettings.RowsLimit, RowsLimitText_TextChanged);
			orderByText.Text = CurrentSettings.OrderBy;
		}

		public void SynchronizeTreeView()
		{
			if (IsTreeViewSynchronized)
				return;

			TreeViewContainer.EnableSync();
			IsTreeViewSynchronized = true;
		}

		public override void ApplySettings()
		{
			base.ApplySettings();
			MaxLinesLabel.Text = Resources.Txt_LogsReaderForm_MaxLines;
			MaxThreadsLabel.Text = Resources.Txt_LogsReaderForm_MaxThreads;
			RowsLimitLabel.Text = Resources.Txt_LogsReaderForm_RowsLimit;
			OrderByLabel.Text = Resources.Txt_LogsReaderForm_OrderBy;
			configureButton.Text = Resources.Txt_Form_ConfigureButton;
			TreeViewContainer.ApplySettings();
		}

		protected override void ApplyTooltip()
		{
			if (LRSettings.DisableHintTooltip)
				return;

			base.ApplyTooltip();
			Tooltip.SetToolTip(maxThreadsText, Resources.Txt_LRSettingsScheme_MaxThreads);
			Tooltip.SetToolTip(maxLinesStackText, Resources.Txt_LRSettingsScheme_MaxTraceLines);
			Tooltip.SetToolTip(rowsLimitText, Resources.Txt_LRSettingsScheme_RowsLimit);
			Tooltip.SetToolTip(orderByText, Resources.Txt_LRSettingsScheme_OrderBy);
			Tooltip.SetToolTip(TreeMain, Resources.Txt_Form_trvMainComment);
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

		internal override void RefreshButtonPauseState(TraceReader reader)
		{
			if (reader.Settings != CurrentSettings)
				return;

			base.RefreshButtonPauseState(reader);
		}

		internal override async void BtnSearch_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				TimeWatcher = new Stopwatch();

				try
				{
					base.BtnSearch_Click(sender, e);

					// получение экземплярва фильтра, если необходимо выполнять фильтр во время поиска
					var filter = ChbxAlreadyUseFilter.Checked ? GetFilter() : null;

					Dictionary<string, int> GetGroupItems(string parentNodeName)
					{
						var result = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

						foreach (TreeNode groupNode in TreeMain.Nodes[parentNodeName].Nodes)
						{
							foreach (var item in groupNode.Nodes.OfType<TreeNode>()
							                              .Where(x => x.Checked)
							                              .Select(x => x.Text)
							                              .GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase)
							                              .Select(x => x.Key)
							                              .ToList())
							{
								if (result.TryGetValue(item, out var existPiority))
								{
									var currentPriority = TreeViewContainer.GetGroupPriority(groupNode);
									if (currentPriority < existPiority)
										result[item] = currentPriority;
								}
								else
								{
									result.Add(item, TreeViewContainer.GetGroupPriority(groupNode));
								}
							}
						}

						return result;
					}

					// получение серверов по чекбоксам
					var servers = GetGroupItems(TreeViewContainer.TRVServers);
					// получение названия файлов типов  по чекбоксам
					var fileTypes = GetGroupItems(TreeViewContainer.TRVTypes);
					// получение папок по чекбоксам
					var folders = TreeViewContainer.GetFolders(TreeMain, true);
					IsWorking = true;
					MainReader = new LogsReaderPerformerScheme(CurrentSettings, TbxPattern.Text, ChbxUseRegex.Checked, servers, fileTypes, folders, filter);
					TimeWatcher.Start();
					ReportStatus(Resources.Txt_LogsReaderForm_LogFilesSearching, ReportStatusType.Success);
					await MainReader.GetTargetFilesAsync(); // получение файлов логов
					await UploadReadersAsync(); // загружаем ридеры в таблицу прогресса
					ReportStatus(string.Format(Resources.Txt_LogsReaderForm_Working, string.Empty), ReportStatusType.Success);
					await MainReader.StartAsync(); // вополнение поиска

					// результат выполнения
					OverallResultList = new DataTemplateCollection(CurrentSettings, MainReader.ResultsOfSuccess);
					if (MainReader.ResultsOfError != null)
						OverallResultList.AddRange(MainReader.ResultsOfError.OrderBy(x => x.Date));

					// заполняем DataGrid
					if (await AssignResultAsync(null, null, true))
						ReportStatus(string.Format(Resources.Txt_LogsReaderForm_FinishedIn, TimeWatcher.Elapsed.ToReadableString()), ReportStatusType.Success);
					TimeWatcher.Stop();
				}
				catch (Exception ex)
				{
					if (IsWorking)
						IsWorking = false;
					ReportStatus(ex);
				}
				finally
				{
					if (IsWorking)
						IsWorking = false;
					if (TimeWatcher.IsRunning)
						TimeWatcher.Stop();
					if (MainReader?.TraceReaders != null)
						ReportProcessStatus(MainReader.TraceReaders.Values);
					Progress = 100;
				}
			}
			else
			{
				MainReader?.Abort();
				ReportStatus(Resources.Txt_LogsReaderForm_Stopping, ReportStatusType.Success);
			}
		}

		protected override IEnumerable<DataTemplate> GetResultTemplates()
			=> OverallResultList == null ? new List<DataTemplate>() : new List<DataTemplate>(OverallResultList);

		internal override IEnumerable<TraceReader> GetResultReaders()
			=> MainReader?.TraceReaders?.Values == null ? new List<TraceReader>() : new List<TraceReader>(MainReader.TraceReaders.Values);

		protected override void ChangeFormStatus()
		{
			base.ChangeFormStatus();

			maxLinesStackText.Enabled = !IsWorking;
			maxThreadsText.Enabled = !IsWorking;
			rowsLimitText.Enabled = !IsWorking;
			orderByText.Enabled = !IsWorking;
			configureButton.Enabled = !IsWorking;
			TreeMain.Enabled = !IsWorking;

			OnSearchChanged?.Invoke(this, EventArgs.Empty);
		}

		internal override bool TryGetTemplate(DataGridViewRow row, out DataTemplate template)
		{
			template = null;
			if (OverallResultList == null)
				return false;

			var privateID = (int) (row?.Cells[DgvDataPrivateIDColumn.Name]?.Value ?? -1);
			if (privateID <= -1)
				return false;

			template = OverallResultList[privateID];
			return template != null;
		}

		internal override bool TryGetReader(DataGridViewRow row, out TraceReader reader)
		{
			reader = null;
			if (MainReader?.TraceReaders == null)
				return false;

			var privateID = (int) (row?.Cells[DgvReaderPrivateIDColumn.Name]?.Value ?? -1);
			return privateID > -1 && MainReader.TraceReaders.TryGetValue(privateID, out reader);
		}

		internal override void PauseAll()
		{
			base.PauseAll();
			MainReader?.Pause();
		}

		internal override void ResumeAll()
		{
			base.ResumeAll();
			MainReader?.Resume();
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

		private void MaxLinesStackTextSave(int value)
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

		private void MaxThreadsTextSave(int value)
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

		private void RowsLimitTextSave(int value)
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

		protected override void CheckBoxTransactionsMarkingTypeChanged(TransactionsMarkingType newType)
		{
			CurrentSettings.TraceParse.SelectionTransactionsType = newType;
			OnSchemeChanged?.Invoke(this, EventArgs.Empty);
		}

		protected override void ValidationCheck(bool clearStatus)
		{
			try
			{
				var settIsCorrect = CurrentSettings?.IsCorrect ?? false;
				if (settIsCorrect && clearStatus)
					ClearErrorStatus();

				BtnSearch.Enabled = settIsCorrect
				                 && TreeMain.Nodes[TreeViewContainer.TRVServers]
				                            .Nodes.OfType<TreeNode>()
				                            .Any(x => x.Nodes.OfType<TreeNode>().Any(x2 => x2.Checked))
				                 && TreeMain.Nodes[TreeViewContainer.TRVTypes]
				                            .Nodes.OfType<TreeNode>()
				                            .Any(x => x.Nodes.OfType<TreeNode>().Any(x2 => x2.Checked))
				                 && TreeMain.Nodes[TreeViewContainer.TRVFolders].Nodes.OfType<TreeNode>().Any(x => x.Checked);
				
				base.ValidationCheck(clearStatus);
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		protected override void BtnClear_Click(object sender, EventArgs e)
		{
			try
			{
				base.BtnClear_Click(this, e);
				OnSearchChanged?.Invoke(this, EventArgs.Empty);
				OnClear?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		protected override void ClearData()
		{
			OverallResultList?.Clear();
			OverallResultList = null;
			MainReader?.Dispose();
			base.ClearData();
		}

		public override string ToString() => CurrentSettings?.ToString() ?? base.ToString();
	}
}