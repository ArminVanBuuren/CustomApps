using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using LogsReader.Reader.Forms;
using SPAMassageSaloon.Common;
using SPAMassageSaloon.Common.StyleControls;
using Utils;
using Utils.WinForm;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader.Reader
{
	public enum RefreshDataType
	{
		VisibleRows = 0,
		AllRows = 1
	}

	public abstract partial class LogsReaderFormBase : UserControl, IUserForm
	{
		private readonly Func<DateTime> _getStartDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
		private readonly Func<DateTime> _getEndDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

		private readonly object syncRootStatus = new object();
		private readonly object onPauseSync = new object();

		private bool _settingIsApplied;
		private bool _oldDateStartChecked;
		private bool _oldDateEndChecked;
		private bool _settingsLoaded;
		private bool _isWorking;
		private bool _onPause;

		private int _countMatches;
		private int _countErrorMatches;
		private int _filesCompleted;
		private int _totalFiles;

		private int _prevReadersDistance = -1;
		private bool _shownProcessReadesPanel = true;

		private IEnumerable<DataTemplate> _currentDGVResult;
		private DataGridViewColumn _oldSortedColumn;
		private bool _byAscending = true;

		private Color _formBackColor;
		private Color _formForeColor;

		private readonly ToolStripButton _openProcessingReadersBtn;
		private readonly ToolStripStatusLabel _statusInfo;
		private readonly ToolStripStatusLabel _findedInfo;
		private readonly ToolStripStatusLabel _completedFilesStatus;
		private readonly ToolStripStatusLabel _totalFilesStatus;
		private readonly ToolStripStatusLabel _filtersCompleted1;
		private readonly ToolStripStatusLabel _filtersCompleted2;
		private readonly ToolStripStatusLabel _overallFound;
		private readonly ToolStripStatusLabel _errorFound;
		private readonly ToolStripStatusLabel _errorFoundValue;

		private readonly ToolStripButton buttonFilteredPrev;
		private readonly ToolStripButton buttonFilteredNext;
		private readonly ToolStripButton buttonErrPrev;
		private readonly ToolStripButton buttonErrNext;
		private readonly ToolStripButton buttonTrnPrev;
		private readonly ToolStripButton buttonTrnNext;

		private readonly Bitmap imgOnWaiting = Resources.waiting;
		private readonly Bitmap imgOnProcessing = Resources.processing;
		private readonly Bitmap imgPause = Resources.onPause;
		private readonly Padding paddingImgPause = new Padding(0, 0, 0, 0);
		private readonly Bitmap imgOnAborted = Resources.aborted;
		private readonly Bitmap imgOnFailed = Resources.failed;
		private readonly Bitmap imgOnFinished = Resources.finished;
		private readonly Bitmap imgPlay = Resources.bt_play;
		private readonly Padding paddingImgPlay = new Padding(0, 0, 1, 0);

		private string Txt_LogsReaderForm_Working = Resources.Txt_LogsReaderForm_Working;
		private string Txt_LogsReaderForm_DateValueIsIncorrect = Resources.Txt_LogsReaderForm_DateValueIsIncorrect;
		private string Txt_LogsReaderForm_DoesntMatchByPattern = Resources.Txt_LogsReaderForm_DoesntMatchByPattern;
		private string TxtReader_BtnPause = Resources.TxtReader_BtnPause;
		private string TxtReader_BtnResume = Resources.TxtReader_BtnResume;
		private string TxtReader_StatusWaiting = Resources.TxtReader_StatusWaiting;
		private string TxtReader_StatusProcessing = Resources.TxtReader_StatusProcessing;
		private string TxtReader_StatusOnPause = Resources.TxtReader_StatusOnPause;
		private string TxtReader_StatusAborted = Resources.TxtReader_StatusAborted;
		private string TxtReader_StatusFailed = Resources.TxtReader_StatusFailed;
		private string TxtReader_StatusFinished = Resources.TxtReader_StatusFinished;
		private string TxtReader_BtnAbort = Resources.TxtReader_BtnAbort;

		private TransactionsMarkingType _currentTransactionsMarkingType = TransactionsMarkingType.None;
		private TransactionsMarkingType _prevTransactionsMarkingType = TransactionsMarkingType.None;

		protected static Image Img_Failed { get; }

		protected static Image Img_Filtered { get; }

		protected static Image Img_Selected { get; }

		protected static Image Img_Failed_Filtered { get; }

		/// <summary>
		///     Юзерские настройки
		/// </summary>
		protected UserSettings UserSettings { get; }

		protected Stopwatch TimeWatcher { get; set; } = new Stopwatch();

		protected ToolTip Tooltip { get; }

		protected bool ButtonHighlightEnabled
		{
			get => buttonHighlightOn.Enabled;
			set => buttonHighlightOn.Enabled = buttonHighlightOff.Enabled = value;
		}

		protected abstract TransactionsMarkingType DefaultTransactionsMarkingType { get; }

		protected TransactionsMarkingType CurrentTransactionsMarkingType
		{
			get => _currentTransactionsMarkingType;
			set
			{
				_currentTransactionsMarkingType = value;
				_prevTransactionsMarkingType = value == TransactionsMarkingType.None ? DefaultTransactionsMarkingType : value;

				try
				{
					checkBoxShowTrns.CheckedChanged -= checkBoxShowTrns_CheckedChanged;
					checkBoxShowTrns.Checked = value != TransactionsMarkingType.None;
					checkBoxShowTrns_CheckedChanged(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					ReportStatus(ex);
				}
				finally
				{
					checkBoxShowTrns.CheckedChanged += checkBoxShowTrns_CheckedChanged;
				}
			}
		}

		/// <summary>
		///     При загрузке ридеров
		/// </summary>
		public event EventHandler OnUploadReaders;

		/// <summary>
		///     Поиск логов начался или завершился
		/// </summary>
		public event EventHandler OnProcessStatusChanged;

		/// <summary>
		///     При смене языка
		/// </summary>
		public event EventHandler OnAppliedSettings;

		/// <summary>
		///     Статус выполнения поиска
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
			private set
			{
				_countMatches = value;
				_findedInfo.Text = _countMatches.ToString();
			}
		}

		public int CountErrorMatches
		{
			get => _countErrorMatches;
			private set
			{
				_countErrorMatches = value;
				_errorFoundValue.Text = _countErrorMatches.ToString();
			}
		}

		public int Progress
		{
			get => IsWorking ? progressBar.Value : 100;
			protected set
			{
				if (value >= 0 && value <= 100)
					progressBar.Value = value;
			}
		}

		public int FilesCompleted
		{
			get => _filesCompleted;
			private set
			{
				_filesCompleted = value;
				_completedFilesStatus.Text = _filesCompleted.ToString();
			}
		}

		public int TotalFiles
		{
			get => _totalFiles;
			private set
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

		public abstract RefreshDataType DgvDataAfterAssign { get; }

		public TraceItemView MainViewer { get; }

		public bool OnPause
		{
			get
			{
				lock (onPauseSync)
					return _onPause;
			}
			protected set
			{
				lock (onPauseSync)
					_onPause = value;
			}
		}

		static LogsReaderFormBase()
		{
			Image GetImage(IEnumerable<Image> images)
			{
				var bmp = new Bitmap(images.Sum(x => x.Width), images.Max(x => x.Height));

				using (var g = Graphics.FromImage(bmp))
				{
					var x = 0;

					foreach (var img in images)
					{
						g.DrawImage(img, x, 0, img.Width, img.Height);
						x += img.Width;
					}
				}

				return bmp;
			}

			Img_Failed = Resources.Error1;
			Img_Filtered = Resources.filtered;
			Img_Selected = Resources.trn;
			Img_Failed_Filtered = GetImage(new[] { Resources.Error1, Resources.filtered });
		}

		protected LogsReaderFormBase(Encoding defaultEncoding, UserSettings userSettings)
		{
			InitializeComponent();

			DefaultEncoding = defaultEncoding;
			UserSettings = userSettings;

			_formBackColor = UserSettings.BackColor;
			_formForeColor = UserSettings.ForeColor;

			base.Font = LogsReaderMainForm.DefFont;
			ChbxAlreadyUseFilter.Font = LogsReaderMainForm.DefFont;
			tabControlViewer.Font = LogsReaderMainForm.DgvDataFont;

			#region Initialize StripStatus

			Tooltip = new ToolTip
			{
				AutoPopDelay = 15000,
				InitialDelay = 100,
				ReshowDelay = 500
			};

			var toolToolStripCollection = new List<ToolStripItem>();
			var toolToolStripCollection2 = new List<ToolStripItem>();
			var statusStripItemsPaddingStart = new Padding(0, 2, 0, 2);
			var statusStripItemsPaddingMiddle = new Padding(-3, 2, 0, 2);
			var statusStripItemsPaddingEnd = new Padding(-3, 2, 1, 2);
			
			_openProcessingReadersBtn = new ToolStripButton
			{
				Text = @"ᐯᐱ",
				Font = new Font("Verdana", 8.5f, FontStyle.Bold, GraphicsUnit.Point, 0),
				BackColor = Color.FromArgb(54, 187, 156),
				ForeColor = Color.White,
				Margin = new Padding(0, 2, 0, 2),
				Padding = new Padding(1, 0, 0, 0)
			};
			_openProcessingReadersBtn.Click += buttonNextBlock_Click;
			buttonNextBlock_Click(null, EventArgs.Empty);
			toolToolStripCollection.Add(_openProcessingReadersBtn);
			toolToolStripCollection.Add(new ToolStripSeparator());
			
			_filtersCompleted1 = new ToolStripStatusLabel
			{
				Font = base.Font,
				Margin = statusStripItemsPaddingStart
			};
			_completedFilesStatus = new ToolStripStatusLabel("0")
			{
				Font = base.Font,
				Margin = statusStripItemsPaddingMiddle
			};
			_filtersCompleted2 = new ToolStripStatusLabel
			{
				Font = base.Font,
				Margin = statusStripItemsPaddingMiddle
			};
			_totalFilesStatus = new ToolStripStatusLabel("0")
			{
				Font = base.Font,
				Margin = statusStripItemsPaddingEnd
			};
			toolToolStripCollection.Add(_filtersCompleted1);
			toolToolStripCollection.Add(_completedFilesStatus);
			toolToolStripCollection.Add(_filtersCompleted2);
			toolToolStripCollection.Add(_totalFilesStatus);
			_overallFound = new ToolStripStatusLabel
			{
				Font = base.Font,
				Margin = statusStripItemsPaddingStart
			};
			_findedInfo = new ToolStripStatusLabel("0")
			{
				Font = base.Font,
				Margin = statusStripItemsPaddingMiddle
			};
			_errorFound = new ToolStripStatusLabel
			{
				Font = base.Font,
				Margin = statusStripItemsPaddingEnd
			};
			_errorFoundValue = new ToolStripStatusLabel("0")
			{
				Font = base.Font,
				Margin = statusStripItemsPaddingMiddle
			};
			toolToolStripCollection.Add(new ToolStripSeparator());
			toolToolStripCollection.Add(_overallFound);
			toolToolStripCollection.Add(_findedInfo);
			toolToolStripCollection.Add(new ToolStripSeparator());
			toolToolStripCollection.Add(_errorFound);
			toolToolStripCollection.Add(_errorFoundValue);
			toolToolStripCollection.Add(new ToolStripSeparator());
			
			_statusInfo = new ToolStripStatusLabel("")
			{
				Font = new Font(LogsReaderMainForm.MainFontFamily, 8.5F, FontStyle.Bold),
				Margin = statusStripItemsPaddingStart
			};
			toolToolStripCollection.Add(_statusInfo);
			
			statusStrip.ShowItemToolTips = true;
			statusStrip.ImageScalingSize = new Size(13, 15);
			var worker1 = new BackgroundWorker();
			worker1.DoWork += (sender, e) => statusStrip.SafeInvoke(() => { statusStrip.Items.AddRange(toolToolStripCollection.ToArray()); });
			worker1.RunWorkerAsync();
			
			buttonErrPrev = new ToolStripButton
			{
				Image = Resources.backError,
				Margin = new Padding(0, 2, 2, 2),
				Padding = new Padding(0, 0, 0, 0)
			};
			buttonErrPrev.Click += buttonErrorPrev_Click;
			toolToolStripCollection2.Add(buttonErrPrev);
			toolToolStripCollection2.Add(new ToolStripStatusLabel { Image = Resources.Error1 });
			
			buttonErrNext = new ToolStripButton
			{
				Image = Resources.arrowError,
				Margin = new Padding(2, 2, 0, 2),
				Padding = new Padding(0, 0, 0, 0)
			};
			buttonErrNext.Click += buttonErrorNext_Click;
			toolToolStripCollection2.Add(buttonErrNext);
			toolToolStripCollection2.Add(new ToolStripSeparator());
			
			buttonFilteredPrev = new ToolStripButton
			{
				Image = Resources.backFiltered,
				Margin = new Padding(0, 2, 2, 2),
				Padding = new Padding(0, 0, 0, 0)
			};
			buttonFilteredPrev.Click += buttonFilteredPrev_Click;
			toolToolStripCollection2.Add(buttonFilteredPrev);
			toolToolStripCollection2.Add(new ToolStripStatusLabel { Image = Resources.filtered });
			
			buttonFilteredNext = new ToolStripButton
			{
				Image = Resources.arrowFiltered,
				Margin = new Padding(2, 2, 0, 2),
				Padding = new Padding(0, 0, 0, 0)
			};
			buttonFilteredNext.Click += buttonFilteredNext_Click;
			toolToolStripCollection2.Add(buttonFilteredNext);
			toolToolStripCollection2.Add(new ToolStripSeparator());
			
			buttonTrnPrev = new ToolStripButton
			{
				Image = Resources.backTrn,
				Margin = new Padding(0, 2, 2, 2),
				Padding = new Padding(0, 0, 0, 0)
			};
			buttonTrnPrev.Click += buttonTrnPrev_Click;
			toolToolStripCollection2.Add(buttonTrnPrev);
			toolToolStripCollection2.Add(new ToolStripStatusLabel { Image = Resources.trn });
			
			buttonTrnNext = new ToolStripButton
			{
				Image = Resources.arrowTrn,
				Margin = new Padding(2, 2, 0, 2),
				Padding = new Padding(0, 0, 0, 0)
			};
			buttonTrnNext.Click += buttonTrnNext_Click;
			toolToolStripCollection2.Add(buttonTrnNext);
			
			statusStripBtns.ShowItemToolTips = true;
			statusStripBtns.ImageScalingSize = new Size(13, 15);
			var worker2 = new BackgroundWorker();
			worker2.DoWork += (sender, e) => statusStripBtns.SafeInvoke(() => { statusStripBtns.Items.AddRange(toolToolStripCollection2.ToArray()); });
			worker2.RunWorkerAsync();

			#endregion

			try
			{
				#region Initialize DgvData

				DgvData.AutoGenerateColumns = false;
				DgvData.TabStop = false;
				DgvData.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
				DgvData.DefaultCellStyle.Font = LogsReaderMainForm.DgvDataFont;
				DgvData.Font = LogsReaderMainForm.DgvDataFont;
				DgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
				DgvData.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
				foreach (DataGridViewColumn c in DgvData.Columns)
					c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
				DgvData.CellDoubleClick += DgvData_CellDoubleClick;
				DgvData.SelectionChanged += DgvData_SelectionChanged;
				DgvData.MouseDown += DgvData_MouseDown;
				DgvData.CellFormatting += DataGridViewOnCellFormatting;
				DgvData.ColumnHeaderMouseClick += DgvDataOnColumnHeaderMouseClick;
				DgvData.DataBindingComplete += (sender, args) => DgvData.ClearSelection();
				DgvData.ColumnWidthChanged += (sender, args) =>
				{
					if (DgvDataPromptColumn.Width > DgvDataPromptColumn.MinimumWidth)
						DgvDataPromptColumn.Width = DgvDataPromptColumn.MinimumWidth;
				};
				
				DgvDataPromptColumn.DataPropertyName = DgvDataPromptColumn.Name = nameof(DataTemplate.Tmp.Prompt);
				DgvDataPromptColumn.HeaderText = DataTemplate.HeaderPrompt;
				
				DgvDataIDColumn.DataPropertyName = DgvDataIDColumn.Name = DgvDataIDColumn.HeaderText = nameof(DataTemplate.Tmp.ID);
				
				DgvDataServerColumn.DataPropertyName = DgvDataServerColumn.Name = DgvDataServerColumn.HeaderText = nameof(DataTemplate.Tmp.Server);
				
				DgvDataTraceNameColumn.DataPropertyName = DgvDataTraceNameColumn.Name = nameof(DataTemplate.Tmp.TraceName);
				DgvDataTraceNameColumn.HeaderText = nameof(DataTemplate.Tmp.TraceName);
				
				DgvDataDateOfTraceColumn.DataPropertyName = DgvDataDateOfTraceColumn.Name = nameof(DataTemplate.Tmp.DateString);
				DgvDataDateOfTraceColumn.HeaderText = nameof(DataTemplate.Tmp.Date);
				
				DgvDataElapsedSecColumn.DataPropertyName = DgvDataElapsedSecColumn.Name = nameof(DataTemplate.Tmp.ElapsedSecString);
				DgvDataElapsedSecColumn.HeaderText = nameof(DataTemplate.Tmp.ElapsedSec);
				
				DgvDataSchemeNameColumn.DataPropertyName = DgvDataSchemeNameColumn.Name = nameof(DataTemplate.Tmp.SchemeName); // not visible
				
				DgvDataPrivateIDColumn.DataPropertyName = DgvDataPrivateIDColumn.Name = nameof(DataTemplate.Tmp.PrivateID); // not visible
				
				DgvDataIsSuccessColumn.DataPropertyName = DgvDataIsSuccessColumn.Name = nameof(DataTemplate.Tmp.IsSuccess); // not visible
				
				DgvDataIsFilteredColumn.DataPropertyName = DgvDataIsFilteredColumn.Name = nameof(DataTemplate.Tmp.IsFiltered); // not visible
				
				DgvDataFileColumn.DataPropertyName = DgvDataFileColumn.Name = nameof(DataTemplate.Tmp.FileNamePartial);
				DgvDataFileColumn.HeaderText = nameof(DataTemplate.Tmp.File);
				
				label7.Text = nameof(DataTemplate.Tmp.TraceName);
				label11.Text = DataTemplate.HeaderTraceMessage;

				#endregion

				#region Initialize DgvProcessing

				DgvReaderSelectColumn.CellTemplate = new DgvCheckBoxCell
				{
					Checked = true,
					Enabled = false
				};
				DgvReaderProcessColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
				DgvReaderAbortColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
				
				DgvReader.AutoGenerateColumns = false;
				DgvReader.TabStop = false;
				DgvReader.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				DgvReader.DefaultCellStyle.Font = LogsReaderMainForm.DgvReaderFont;
				DgvReader.Font = LogsReaderMainForm.DgvReaderFont;
				DgvReader.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
				DgvReader.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
				foreach (DataGridViewColumn c in DgvReader.Columns)
					c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
				DgvReader.Scroll += (sender, args) => RefreshAllRows(DgvReader, DgvReaderRefreshRow);
				DgvReader.ColumnHeaderMouseClick += (sender, args) => RefreshAllRows(DgvReader, DgvReaderRefreshRow);
				DgvReader.ColumnHeaderMouseDoubleClick += (sender, args) => RefreshAllRows(DgvReader, DgvReaderRefreshRow);
				DgvReader.Sorted += (sender, args) => DgvReader.CheckStatusHeader(DgvReaderSelectColumn);
				DgvReader.CellContentClick += (sender, args) =>
				{
					if (args.RowIndex < 0)
						return;

					var row = DgvReader.Rows[args.RowIndex];

					if (args.ColumnIndex == DgvReaderAbortColumn.Index
					 && row.Cells[DgvReaderAbortColumn.Name] is DgvDisableButtonCell cellAbort
					 && cellAbort.Enabled
					 && TryGetReader(DgvReader.Rows[args.RowIndex], out var reader1))
					{
						reader1.Abort();
						RefreshAllRows(DgvReader, DgvReaderRefreshRow);
						return;
					}

					if (args.ColumnIndex == DgvReaderProcessColumn.Index
					 && row.Cells[DgvReaderProcessColumn.Name] is DgvDisableButtonCell cellPauseResume
					 && cellPauseResume.Enabled
					 && TryGetReader(DgvReader.Rows[args.RowIndex], out var reader2))
					{
						RefreshButtonPauseState(reader2);
						RefreshAllRows(DgvReader, DgvReaderRefreshRow);
					}
				};

				#endregion

				#region Apply All Settings

				DateStartFilter.ValueChanged += DateStartFilterOnValueChanged;
				DateEndFilter.ValueChanged += DateEndFilterOnValueChanged;
				
				TbxPattern.AssignValue(UserSettings.PreviousSearch.First(), TxtPatternOnTextChanged);
				TbxPattern.Items.AddRange(UserSettings.PreviousSearch.ToArray());

				ChbxUseRegex.Checked = UserSettings.UseRegex;
				DateStartFilter.Checked = UserSettings.DateStartChecked;
				if (DateStartFilter.Checked)
					DateStartFilter.Value = _getStartDate.Invoke();
				DateEndFilter.Checked = UserSettings.DateEndChecked;
				if (DateEndFilter.Checked)
					DateEndFilter.Value = _getEndDate.Invoke();

				TbxTraceNameFilter.AssignValue(UserSettings.TraceNameFilter.First(), TbxTraceNameFilterOnTextChanged);
				TbxTraceNameFilter.Items.AddRange(UserSettings.TraceNameFilter.ToArray());

				TbxTraceMessageFilter.AssignValue(UserSettings.TraceMessageFilter.First(), TbxTraceMessageFilterOnTextChanged);
				TbxTraceMessageFilter.Items.AddRange(UserSettings.TraceMessageFilter.ToArray());

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

		protected abstract IEnumerable<DataTemplate> GetResultTemplates();

		internal abstract IEnumerable<TraceReader> GetResultReaders();

		protected abstract void CheckBoxTransactionsMarkingTypeChanged(TransactionsMarkingType newType);

		internal abstract bool TryGetTemplate(DataGridViewRow row, out DataTemplate template);

		internal abstract bool TryGetReader(DataGridViewRow row, out TraceReader reader);

		#region Pause - Resume Button

		internal virtual void RefreshButtonPauseState(TraceReader reader)
		{
			if (reader.Status == TraceReaderStatus.OnPause)
			{
				reader.Resume();
				// если какой то запустили, но до этого все поставили на паузу, то обновляем статус и кнопку
				ResumeState();
			}
			else
			{
				reader.Pause();
				// если все остановили вручную, то обновляем статус и кнопку
				if (!GetResultReaders().Any(x => x.Status == TraceReaderStatus.Waiting || x.Status == TraceReaderStatus.Processing))
					PauseState();
			}
		}

		/// <summary>
		///     Запускаем все приостановленные процессы
		/// </summary>
		internal virtual void ResumeAll()
		{
			if (!IsWorking)
				return;

			ResumeState();
		}

		private void ResumeState()
		{
			OnPause = false;
			if (buttonPause.Image != imgPause)
				buttonPause.Image = imgPause;
			if (buttonPause.Padding != paddingImgPause)
				buttonPause.Padding = paddingImgPause;
		}

		/// <summary>
		///     Приостанавливаем все процессы
		/// </summary>
		internal virtual void PauseAll()
		{
			if (!IsWorking)
				return;

			PauseState();
		}

		private void PauseState()
		{
			OnPause = true;
			if (buttonPause.Image != imgPlay)
				buttonPause.Image = imgPlay;
			if (buttonPause.Padding != paddingImgPlay)
				buttonPause.Padding = paddingImgPlay;
		}

		private void ButtonPause_Click(object sender, EventArgs e)
		{
			OnPause = !OnPause;
			if (OnPause)
				PauseAll();
			else
				ResumeAll();
		}

		#endregion

		public void ApplyFormSettings()
		{
			try
			{
				if (UserSettings == null)
					return;

				for (var i = 0; i < DgvData.Columns.Count; i++)
				{
					var valueStr = UserSettings.GetValue("COL" + i);
					if (!valueStr.IsNullOrWhiteSpace() && int.TryParse(valueStr, out var value) && value > 1 && value <= 1000)
						DgvData.Columns[i].Width = value;
				}

				ParentSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(ParentSplitContainer), 25, 2000, ParentSplitContainer.SplitterDistance);
				MainSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(MainSplitContainer), 25, 2000, MainSplitContainer.SplitterDistance);
				MainViewer.SplitterDistance = UserSettings.GetValue(nameof(MainViewer), 25, 2000, MainViewer.SplitterDistance);
				
				ParentSplitContainer.SplitterMoved += (sender, args) => SaveData();
				MainSplitContainer.SplitterMoved += (sender, args) => SaveData();
				MainViewer.SplitterMoved += (sender, args) => SaveData();
				
				_settingIsApplied = true;
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		#region Change Language

		public virtual void ApplySettings()
		{
			try
			{
				_filtersCompleted1.Text = Resources.Txt_LogsReaderForm_FilesCompleted_1;
				_filtersCompleted2.Text = Resources.Txt_LogsReaderForm_FilesCompleted_2;
				_overallFound.Text = $"{Resources.TxtReader_DgvMatches}:";
				_errorFound.Text = $"{Resources.TxtReader_DgvErrors}:";
				CobxTraceNameFilter.Items.Clear();
				CobxTraceNameFilter.Items.Add(Resources.Txt_LogsReaderForm_Contains);
				CobxTraceNameFilter.Items.Add(Resources.Txt_LogsReaderForm_NotContains);
				if (UserSettings != null)
					CobxTraceNameFilter.AssignValue(UserSettings.TraceNameFilterContains
						                                ? Resources.Txt_LogsReaderForm_Contains
						                                : Resources.Txt_LogsReaderForm_NotContains,
					                                CobxTraceNameFilter_SelectedIndexChanged);
				
				CobxTraceMessageFilter.Items.Clear();
				CobxTraceMessageFilter.Items.Add(Resources.Txt_LogsReaderForm_Contains);
				CobxTraceMessageFilter.Items.Add(Resources.Txt_LogsReaderForm_NotContains);
				if (UserSettings != null)
					CobxTraceMessageFilter.AssignValue(UserSettings.TraceMessageFilterContains
						                                   ? Resources.Txt_LogsReaderForm_Contains
						                                   : Resources.Txt_LogsReaderForm_NotContains,
					                                   CobxTraceMessageFilter_SelectedIndexChanged);
				
				ApplyTooltip();
				
				ChbxUseRegex.Text = Resources.Txt_LogsReaderForm_UseRegex;
				BtnSearch.Text = IsWorking ? Resources.Txt_LogsReaderForm_Stop : Resources.Txt_LogsReaderForm_Search;
				btnClear.Text = Resources.Txt_LogsReaderForm_Clear;
				btnClear.Size = new Size(Convert.ToInt32(Resources.LogsReaderForm_btnClear_Width), btnClear.Height);
				btnFilter.Text = Resources.Txt_LogsReaderForm_Filter;
				btnReset.Text = Resources.Txt_LogsReaderForm_Reset;
				ChbxAlreadyUseFilter.Text = Resources.Txt_LogsReaderForm_UseFilterWhenSearching;
				
				DgvReaderSchemeNameColumn.HeaderText = Resources.TxtReader_DgvScheme;
				DgvReaderStatusColumn.HeaderText = Resources.TxtReader_DgvStatus;
				DgvReaderProcessColumn.HeaderText = Resources.TxtReader_DgvProcess;
				DgvReaderAbortColumn.HeaderText = Resources.TxtReader_DgvFlow;
				DgvReaderThreadIdColumn.HeaderText = Resources.TxtReader_DgvThreadID;
				DgvReaderCountMatchesColumn.HeaderText = Resources.TxtReader_DgvMatches;
				DgvReaderCountErrorMatchesColumn.HeaderText = Resources.TxtReader_DgvErrors;
				DgvReaderFileColumn.HeaderText = Resources.TxtReader_DgvFile;
				DgvReaderFileCreationTimeColumn.HeaderText = Resources.TxtReader_DgvCreationTime;
				DgvReaderFileLastWriteTimeColumn.HeaderText = Resources.TxtReader_DgvLastWrite;
				DgvReaderFilePriorityColumn.HeaderText = Resources.TxtReader_DgvPriority;
				DgvReaderFileSizeColumn.HeaderText = Resources.TxtReader_DgvSize;
				
				DgvReaderStatusColumn.MinimumWidth = int.Parse(Resources.TxtReader_DgvStatusMinWidth);
				DgvReaderProcessColumn.MinimumWidth = int.Parse(Resources.TxtReader_DgvProcessMinWidth);
				DgvReaderAbortColumn.MinimumWidth = int.Parse(Resources.TxtReader_DgvAbortMinWidth);
				DgvReaderCountMatchesColumn.MinimumWidth = int.Parse(Resources.TxtReader_DgvCountMatchesMinWidth);
				DgvReaderCountErrorMatchesColumn.MinimumWidth = int.Parse(Resources.TxtReader_DgvCountErrorMatchesMinWidth);
				DgvReaderFileLastWriteTimeColumn.MinimumWidth = int.Parse(Resources.TxtReader_DgvCountLastWriteTimeMinWidth);
				
				Txt_LogsReaderForm_Working = Resources.Txt_LogsReaderForm_Working;
				Txt_LogsReaderForm_DateValueIsIncorrect = Resources.Txt_LogsReaderForm_DateValueIsIncorrect;
				Txt_LogsReaderForm_DoesntMatchByPattern = Resources.Txt_LogsReaderForm_DoesntMatchByPattern;
				TxtReader_BtnPause = Resources.TxtReader_BtnPause;
				TxtReader_BtnResume = Resources.TxtReader_BtnResume;
				TxtReader_StatusWaiting = Resources.TxtReader_StatusWaiting;
				TxtReader_StatusProcessing = Resources.TxtReader_StatusProcessing;
				TxtReader_StatusOnPause = Resources.TxtReader_StatusOnPause;
				TxtReader_StatusAborted = Resources.TxtReader_StatusAborted;
				TxtReader_StatusFailed = Resources.TxtReader_StatusFailed;
				TxtReader_StatusFinished = Resources.TxtReader_StatusFinished;
				TxtReader_BtnAbort = Resources.TxtReader_BtnAbort;
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
			finally
			{
				_settingsLoaded = true;
				OnAppliedSettings?.Invoke(this, EventArgs.Empty);
			}
		}

		protected virtual void ApplyTooltip()
		{
			Tooltip.RemoveAll();
			
			if (LRSettings.DisableHintTooltip)
				return;

			Tooltip.SetToolTip(TbxPattern, Resources.Txt_Form_SearchComment);
			Tooltip.SetToolTip(ChbxUseRegex, Resources.Txt_LRSettings_UseRegexComment);
			Tooltip.SetToolTip(DateStartFilter, Resources.Txt_Form_DateFilterComment);
			Tooltip.SetToolTip(DateEndFilter, Resources.Txt_Form_DateFilterComment);
			Tooltip.SetToolTip(TbxTraceNameFilter, Resources.Txt_Form_TraceNameFilterComment);
			Tooltip.SetToolTip(TbxTraceMessageFilter, Resources.Txt_Form_TraceFilterComment);
			Tooltip.SetToolTip(ChbxAlreadyUseFilter, Resources.Txt_Form_AlreadyUseFilterComment);
			Tooltip.SetToolTip(btnExport, Resources.Txt_LogsReaderForm_ExportComment);
			Tooltip.SetToolTip(checkBoxShowTrns, Resources.Txt_Forms_ShowTransactions);
			Tooltip.SetToolTip(buttonHighlightOn, Resources.Txt_LogsReaderForm_HighlightTxt);
			Tooltip.SetToolTip(buttonHighlightOff, Resources.Txt_LogsReaderForm_HighlightTxtOff);
			
			buttonFilteredPrev.ToolTipText = Resources.Txt_LogsReaderForm_PrevFilteredButt;
			buttonFilteredNext.ToolTipText = Resources.Txt_LogsReaderForm_NextFilteredButt;
			buttonErrPrev.ToolTipText = Resources.Txt_LogsReaderForm_PrevErrButt;
			buttonErrNext.ToolTipText = Resources.Txt_LogsReaderForm_NextErrButt;
			buttonTrnPrev.ToolTipText = Resources.Txt_LogsReaderForm_PrevTrnButt;
			buttonTrnNext.ToolTipText = Resources.Txt_LogsReaderForm_NextTrnButt;
			DgvReaderSelectColumn.ToolTipText = Resources.TxtReader_CheckBoxes;
		}

		#endregion

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
				ReportStatus(ex);
			}
		}

		public virtual void LogsReaderKeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				switch (e.KeyCode)
				{
					case Keys.F1 when ButtonHighlightEnabled && !e.Shift:
						buttonHighlight_Click(this, EventArgs.Empty);
						break;

					case Keys.F1 when ButtonHighlightEnabled && e.Shift:
						buttonHighlightOff_Click(this, EventArgs.Empty);
						break;

					case Keys.F2 when e.Shift:
						buttonFilteredPrev_Click(this, EventArgs.Empty);
						break;

					case Keys.F2 when !e.Shift:
						buttonFilteredNext_Click(this, EventArgs.Empty);
						break;

					case Keys.F3 when e.Shift:
						buttonErrorPrev_Click(this, EventArgs.Empty);
						break;

					case Keys.F3 when !e.Shift:
						buttonErrorNext_Click(this, EventArgs.Empty);
						break;

					case Keys.F4 when e.Shift:
						buttonTrnPrev_Click(this, EventArgs.Empty);
						break;

					case Keys.F4 when !e.Shift:
						buttonTrnNext_Click(this, EventArgs.Empty);
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

					case Keys.S when e.Control && btnExport.Enabled:
						BtnExport_Click(this, EventArgs.Empty);
						break;

					case Keys.C when e.Control && DgvData.SelectedRows.Count > 0 && DgvData.Focused:
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
				ReportStatus(ex);
			}
		}

		internal virtual void BtnSearch_Click(object sender, EventArgs e)
		{
			if (TbxPattern.Text.IsNullOrEmpty())
				throw new Exception(Resources.Txt_LogsReaderForm_SearchPatternIsNull);

			AssignComboboxWithHistory(TbxPattern);
			AssignComboboxWithHistory(TbxTraceNameFilter);
			AssignComboboxWithHistory(TbxTraceMessageFilter);
		}

		private static void AssignComboboxWithHistory(ComboBox cb, int maxItems = 15)
		{
			if (maxItems <= 0)
				maxItems = 1;

			var items = cb.Items.OfType<string>().Where(x => !x.IsNullOrEmpty() && x != cb.Text).ToList().Take(maxItems - 1);
			cb.Items.Clear();
			cb.Items.AddRange(new List<string> { cb.Text }.Concat(items).ToArray());
		}

		protected virtual void ReportProcessStatus(IEnumerable<TraceReader> readers)
			=> this.SafeInvoke(() =>
			{
				try
				{
					var countMatches = 0;
					var countErrorMatches = 0;
					var totalFiles = 0;
					var filesCompleted = 0;
					var progress = 0;

					lock (syncRootStatus)
					{
						if (IsWorking)
							ReportStatus(string.Format(Txt_LogsReaderForm_Working, $" ({TimeWatcher.Elapsed.ToReadableString()})"), ReportStatusType.Success);
						
						countMatches = readers.Sum(x => x.CountMatches);
						countErrorMatches = readers.Sum(x => x.CountErrors);
						totalFiles = readers.Count();
						filesCompleted = readers.Count(x => x.Status != TraceReaderStatus.Waiting
						                                 && x.Status != TraceReaderStatus.Processing
						                                 && x.Status != TraceReaderStatus.OnPause
						                                 && !x.ThreadId.IsNullOrWhiteSpace());
						progress = 0;
						if (filesCompleted > 0 && TotalFiles > 0)
							progress = filesCompleted * 100 / TotalFiles;
						
						if (CountMatches == countMatches
						 && CountErrorMatches == countErrorMatches
						 && TotalFiles == totalFiles
						 && FilesCompleted == filesCompleted
						 && Progress == progress)
							return;
					}

					CountMatches = countMatches;
					CountErrorMatches = countErrorMatches;
					TotalFiles = totalFiles;
					FilesCompleted = filesCompleted;
					if (IsWorking)
						Progress = progress;
				}
				finally
				{
					RefreshAllRows(DgvReader, DgvReaderRefreshRow);
				}
			});

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

				BtnSearch.Enabled = btnClear.Enabled = buttonSelectTraceNames.Enabled =
					btnExport.Enabled = btnFilter.Enabled = ButtonHighlightEnabled = btnReset.Enabled = false;
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

							await writer.WriteLineAsync(GetCSVRow(new[]
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
						await writer.WriteLineAsync(GetCSVRow(new[] { nameof(DataTemplate.Tmp.File), DataTemplate.HeaderTraceMessage }));

						foreach (DataGridViewRow row in DgvData.Rows)
						{
							if (!TryGetTemplate(row, out var template))
								continue;

							await writer.WriteLineAsync(GetCSVRow(new[] { template.ParentReader.FilePath, $"\"{template.TraceMessage.Trim()}\"" }));
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

		private static string GetCSVRow(IReadOnlyCollection<string> @params)
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
				if (DgvReader.RowCount > 0)
				{
					var checkedFilePaths = GetCheckedFilePaths();
					await AssignResultAsync(GetFilter(), x => checkedFilePaths.Contains(x.ParentReader.FilePath), false);
				}
				else
				{
					await AssignResultAsync(GetFilter(), null, false);
				}
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		private HashSet<string> GetCheckedFilePaths()
			=> DgvReader.Rows.OfType<DataGridViewRow>()
			            .Where(x => x.Cells[DgvReaderSelectColumn.Name] is DgvCheckBoxCell selectCell && selectCell.Checked)
			            .Select(x => x.Cells[DgvReaderFileColumn.Name]?.Value?.ToString())
			            .Where(x => !x.IsNullOrWhiteSpace())
			            .ToHashSet(x => x, StringComparer.InvariantCultureIgnoreCase);

		protected DataFilter GetFilter()
			=> new DataFilter(DateStartFilter.Checked ? DateStartFilter.Value : DateTime.MinValue,
			                  DateEndFilter.Checked ? DateEndFilter.Value : DateTime.MaxValue,
			                  TbxTraceNameFilter.Text,
			                  CobxTraceNameFilter.Text.Like(Resources.Txt_LogsReaderForm_Contains),
			                  TbxTraceMessageFilter.Text,
			                  CobxTraceMessageFilter.Text.Like(Resources.Txt_LogsReaderForm_Contains));

		private async void BtnReset_Click(object sender, EventArgs e)
		{
			try
			{
				ChangeStateDgvReaderBoxes(true, true);
				await AssignResultAsync(null, null, false);
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		private void buttonHighlight_Click(object sender, EventArgs e)
		{
			if (DgvData.RowCount == 0)
				return;

			DataGridViewRow selected = null;
			if (DgvData.SelectedRows.Count > 0)
				selected = DgvData.SelectedRows[0];
			var selectedCellIndex = DgvData.CurrentCell?.ColumnIndex ?? -1;

			try
			{
				var filter = GetFilter();

				if (filter == null)
				{
					foreach (var row in DgvData.Rows.OfType<DataGridViewRow>())
						row.Cells[DgvDataIsFilteredColumn.Name].Value = false;
					return;
				}

				var count = 0;

				foreach (var row in DgvData.Rows.OfType<DataGridViewRow>())
				{
					var isFiltered = TryGetTemplate(row, out var template) && filter.IsAllowed(template);
					row.Cells[DgvDataIsFilteredColumn.Name].Value = isFiltered;
					if (isFiltered)
						count++;
				}

				if (count == 0)
					ReportStatus(Resources.Txt_LogsReaderForm_NoFilterResultsFound, ReportStatusType.Warning);
				else
					ClearErrorStatus();
				DgvData.Refresh();
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
			finally
			{
				DgvData.SelectRow(selected, selectedCellIndex);
			}
		}

		private void buttonHighlightOff_Click(object sender, EventArgs e)
		{
			if (DgvData.RowCount == 0)
				return;

			DataGridViewRow selected = null;
			if (DgvData.SelectedRows.Count > 0)
				selected = DgvData.SelectedRows[0];
			var selectedCellIndex = DgvData.CurrentCell?.ColumnIndex ?? -1;

			try
			{
				ClearErrorStatus();
				
				foreach (var row in DgvData.Rows.OfType<DataGridViewRow>())
					row.Cells[DgvDataIsFilteredColumn.Name].Value = false;
				
				DgvData.Refresh();
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
			finally
			{
				DgvData.SelectRow(selected, selectedCellIndex);
			}
		}

		protected async Task<bool> AssignResultAsync(DataFilter filter, Func<DataTemplate, bool> condition, bool isNewData)
		{
			try
			{
				if (DgvData.DataSource != null || DgvData.RowCount > 0)
				{
					DgvData.DataSource = null;
					DgvData.Rows.Clear();
					DgvData.Refresh();
				}

				MainViewer?.Clear();

				_currentDGVResult = condition != null ? GetResultTemplates().Where(condition) : GetResultTemplates();

				if (_currentDGVResult == null || !_currentDGVResult.Any())
				{
					ReportStatus(Resources.Txt_LogsReaderForm_NoLogsFound, ReportStatusType.Warning);
					return false;
				}

				if (filter != null)
				{
					_currentDGVResult = filter.FilterCollection(_currentDGVResult);

					if (!_currentDGVResult.Any())
					{
						ReportStatus(Resources.Txt_LogsReaderForm_NoFilterResultsFound, ReportStatusType.Warning);
						return false;
					}
				}

				await AssignCurrentDgvResultAsync(isNewData);
				return true;
			}
			catch (Exception ex)
			{
				DgvData.Focus();
				ReportStatus(ex);
				return false;
			}
		}

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
				
				buttonPause.Enabled = IsWorking;
				buttonPause.Image = imgPause;
				buttonPause.Padding = paddingImgPause;
				
				ChangeStateDgvReaderBoxes(!IsWorking, true);

				if (IsWorking)
				{
					new Action(CheckProgress).BeginInvoke(null, null);
					
					ParentSplitContainer.Cursor = Cursors.WaitCursor;
					statusStrip.Cursor = Cursors.Default;
					panel1.Cursor = Cursors.Default;
					Clear(true, true);
				}
				else
				{
					ParentSplitContainer.Cursor = Cursors.Default;
				}

				buttonSelectTraceNames.Enabled = btnExport.Enabled = DgvData.RowCount > 0;
				btnFilter.Enabled = ButtonHighlightEnabled = btnReset.Enabled = HasAnyResult;
			}
			finally
			{
				if (IsWorking)
					Focus();
			}
		}

		private void ChangeStateDgvReaderBoxes(bool enabled, bool @checked)
		{
			DgvReaderSelectColumn.CellTemplate = new DgvCheckBoxCell
			{
				Checked = @checked,
				Enabled = enabled
			};
			
			if (!(DgvReaderSelectColumn.HeaderCell is DgvColumnCheckBoxHeaderCell dgvChkbxColumnHeader))
				return;

			dgvChkbxColumnHeader.Enabled = enabled;
			dgvChkbxColumnHeader.Checked = @checked;

			foreach (var row in DgvReader.Rows.OfType<DataGridViewRow>())
			{
				if (row.Cells[DgvReaderSelectColumn.Name] is DgvCheckBoxCell cell)
				{
					cell.Enabled = enabled;
					cell.Checked = @checked;
				}
			}
		}

		/// <summary>
		///     Загружаются все TraceReader во время поиска
		/// </summary>
		/// <returns></returns>
		protected async Task UploadReadersAsync()
		{
			try
			{
				//var prevSortedColumn = DgvReader.SortedColumn;
				//var prevSortOrder = DgvReader.SortOrder;
				await DgvReader.AssignCollectionAsync(GetResultReaders().OrderBy(x => x.SchemeName).ThenBy(x => x.ID), null, true);
				RefreshAllRows(DgvReader, DgvReaderRefreshRow);
				DgvReader.ColumnHeadersVisible = true;

				//if(prevSortOrder != SortOrder.None)
				//	DgvReader.Sort(prevSortedColumn, prevSortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
				OnUploadReaders?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		/// <summary>
		///     Обновляет все статусы во время поиска
		/// </summary>
		private void CheckProgress()
		{
			try
			{
				while (IsWorking)
				{
					var readers = GetResultReaders();
					var count = readers.Count();
					var delay = count <= 20 ? 50 : count > 100 ? count > 300 ? 400 : 200 : 100;
					ReportProcessStatus(readers);
					Thread.Sleep(delay);
				}

				var readersAfterWorking = GetResultReaders();
				if (readersAfterWorking.Any())
					ReportProcessStatus(readersAfterWorking);
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		private void DataGridViewOnCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0)
				return;

			try
			{
				var dgv = (DataGridView) sender;
				var row = dgv.Rows[e.RowIndex];
				if (dgv == DgvData)
					DgvDataRefreshRow(row, true);
				else if (dgv == DgvReader)
					DgvReaderRefreshRow(row, true);
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		/// <summary>
		///     Чтобы обновлялись не все строки, а только те что показаны. Т.к. для обновления свойтсва Image тратиться много
		///     времени
		/// </summary>
		protected void RefreshVisibleRows(CustomDataGridView dgv, Action<DataGridViewRow, bool> refreshRow)
		{
			try
			{
				if (dgv == null || dgv.RowCount == 0)
					return;

				var countVisible = dgv.DisplayedRowCount(false);
				var firstVisibleRowIndex = dgv.FirstDisplayedScrollingRowIndex;
				var above = CurrentTransactionsMarkingType == TransactionsMarkingType.Prompt ? 2 : 100;
				var firstIndex = Math.Max(0, firstVisibleRowIndex - above);
				var lastIndex = Math.Min(dgv.RowCount - 1, firstVisibleRowIndex + countVisible + above);
				for (var index = firstIndex; index <= lastIndex; index++)
					refreshRow(dgv.Rows[index], firstVisibleRowIndex - 5 <= index && index <= firstVisibleRowIndex + countVisible + 5);
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		/// <summary>
		///     Вызывается после присвоения значений в DatagridView, чтобы отрисовать все строки
		/// </summary>
		protected void RefreshAllRows(CustomDataGridView dgv, Action<DataGridViewRow, bool> refreshRow)
		{
			if (dgv == null || dgv.RowCount == 0)
				return;

			try
			{
				var countVisible = dgv.DisplayedRowCount(false);
				var firstVisibleRowIndex = dgv.FirstDisplayedScrollingRowIndex;
				foreach (var row in dgv.Rows.OfType<DataGridViewRow>())
					refreshRow(row, firstVisibleRowIndex - 5 <= row.Index && row.Index <= firstVisibleRowIndex + countVisible + 5);
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		protected void DgvDataRefreshRow(DataGridViewRow row, bool refreshHeavy)
		{
			if (!TryGetTemplate(row, out var template))
				return;

			try
			{
				// обновение картинки и Font самый тяжелый процесс, поэтому обновляем только то что видно
				if (!refreshHeavy)
					return;

				var cellTraceName = row.Cells[DgvDataTraceNameColumn.Name];

				if (template.IsSuccess)
				{
					if (template.Date == null && row.Index > 0)
						foreach (DataGridViewCell cell2 in row.Cells)
							if (cell2.ToolTipText != Txt_LogsReaderForm_DateValueIsIncorrect)
								cell2.ToolTipText = Txt_LogsReaderForm_DateValueIsIncorrect;
				}
				else
				{
					// Костыль. Если изменить стиль ячейки первой строки, то это применится ко всем ячейкам. Ебучий майкрософт. А если менять фонт для остальных неимоверно все тупит
					if (row.Index == 0)
					{
						if (!Equals(row.DefaultCellStyle.Font, LogsReaderMainForm.ErrFont))
							row.DefaultCellStyle.Font = LogsReaderMainForm.ErrFont;
					}
					else
					{
						if (!template.IsMatched)
							foreach (DataGridViewCell cell2 in row.Cells)
								if (cell2.ToolTipText != Txt_LogsReaderForm_DoesntMatchByPattern)
									cell2.ToolTipText = Txt_LogsReaderForm_DoesntMatchByPattern;
						
						if (!Equals(cellTraceName.Style.Font, LogsReaderMainForm.ErrFont))
							cellTraceName.Style.Font = LogsReaderMainForm.ErrFont;
					}
				}

				if (row.Cells[DgvDataFileColumn.Name] is DataGridViewCell cellFile && cellFile.ToolTipText != template.File)
					cellFile.ToolTipText = template.File;
				
				if (!(row.Cells[DgvDataPromptColumn.Name] is DgvTextAndImageCell imgCellPrompt))
					return;
				
				if (!(row.Cells[DgvDataTraceNameColumn.Name] is DgvTextAndImageCell imgCellTraceName))
					return;

				var isfiltered = false;
				if (row.Cells[DgvDataIsFilteredColumn.Name] is DataGridViewCell isfilteredCell && isfilteredCell.Value != null)
					isfiltered = bool.Parse(isfilteredCell.Value.ToString());
				
				if (!template.IsSuccess)
					imgCellTraceName.Image = isfiltered ? Img_Failed_Filtered : Img_Failed;
				else
					imgCellTraceName.Image = isfiltered ? Img_Filtered : null;
				
				imgCellPrompt.Image = template.IsSelected && DgvDataPromptColumn.Visible ? Img_Selected : null;
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
			finally
			{
				if (template.IsSelected
				 && (CurrentTransactionsMarkingType == TransactionsMarkingType.Color || CurrentTransactionsMarkingType == TransactionsMarkingType.Both))
				{
					if (row.DefaultCellStyle.BackColor != LogsReaderMainForm.TRN_COLOR_BACK)
						row.DefaultCellStyle.BackColor = LogsReaderMainForm.TRN_COLOR_BACK;
					if (row.DefaultCellStyle.ForeColor != LogsReaderMainForm.TRN_COLOR_FORE)
						row.DefaultCellStyle.ForeColor = LogsReaderMainForm.TRN_COLOR_FORE;
				}
				else
				{
					ColorizationDGV(row, template);
				}
			}
		}

		protected virtual void ColorizationDGV(DataGridViewRow row, DataTemplate template)
		{
			var color = row.Index.IsParity() ? LogsReaderMainForm.SCHEME_DGV_ROW_BACK_COLOR_1 : LogsReaderMainForm.SCHEME_DGV_ROW_BACK_COLOR_2;
			if (row.DefaultCellStyle.BackColor != color)
				row.DefaultCellStyle.BackColor = color;
			if (row.DefaultCellStyle.ForeColor != LogsReaderMainForm.SCHEME_DGV_ROW_FORE_COLOR)
				row.DefaultCellStyle.ForeColor = LogsReaderMainForm.SCHEME_DGV_ROW_FORE_COLOR;
		}

		protected void DgvReaderRefreshRow(DataGridViewRow row, bool refreshHeavy)
		{
			if (!TryGetReader(row, out var reader))
				return;

			DataGridViewRow selected = null;
			if (DgvReader.SelectedRows.Count > 0)
				selected = DgvReader.SelectedRows[0];
			if (selected == null)
				selected = DgvReader.CurrentRow;

			try
			{
				var cellPauseResume = row.Cells[DgvReaderProcessColumn.Name] as DgvDisableButtonCell;

				void CanPause(bool isAlowed)
				{
					if (cellPauseResume == null)
						return;

					if (isAlowed)
					{
						if (!cellPauseResume.Enabled)
							cellPauseResume.Enabled = true;
					}
					else
					{
						if (cellPauseResume.Enabled)
							cellPauseResume.Enabled = false;
					}

					if (cellPauseResume.Value == null || cellPauseResume.Value?.ToString() != TxtReader_BtnPause)
						cellPauseResume.Value = TxtReader_BtnPause;
				}

				void AllowedResume()
				{
					if (cellPauseResume == null)
						return;

					if (!cellPauseResume.Enabled)
						cellPauseResume.Enabled = true;
					if (cellPauseResume.Value == null || cellPauseResume.Value?.ToString() != TxtReader_BtnResume)
						cellPauseResume.Value = TxtReader_BtnResume;
				}

				var backColor = Color.LightGray;
				var statusText = string.Empty;
				Bitmap img = null;

				if (row.Cells[DgvReaderStatusColumn.Name] is DgvTextAndImageCell cellImage)
				{
					switch (reader.Status)
					{
						case TraceReaderStatus.Waiting:
							img = imgOnWaiting;
							backColor = Color.LightGray;
							statusText = TxtReader_StatusWaiting;
							CanPause(true);
							break;

						case TraceReaderStatus.Processing:
							img = imgOnProcessing;
							backColor = Color.White;
							statusText = TxtReader_StatusProcessing;
							CanPause(true);
							break;

						case TraceReaderStatus.OnPause:
							img = imgPause;
							backColor = LogsReaderMainForm.READER_COLOR_BACK_ONPAUSE;
							statusText = TxtReader_StatusOnPause;
							AllowedResume();
							break;

						case TraceReaderStatus.Aborted:
							img = imgOnAborted;
							backColor = LogsReaderMainForm.READER_COLOR_BACK_ERROR;
							statusText = TxtReader_StatusAborted;
							CanPause(false);
							break;

						case TraceReaderStatus.Failed:
							img = imgOnFailed;
							backColor = LogsReaderMainForm.READER_COLOR_BACK_ERROR;
							statusText = TxtReader_StatusFailed;
							CanPause(false);
							break;

						case TraceReaderStatus.Finished:
							img = imgOnFinished;
							backColor = LogsReaderMainForm.READER_COLOR_BACK_SUCCESS;
							statusText = TxtReader_StatusFinished;
							CanPause(false);
							break;
					}

					if (cellImage.Value == null || cellImage.Value?.ToString() != statusText)
						cellImage.Value = statusText;

					// обновение картинки самый тяжелый процесс, поэтому обновляем только то что видно
					if (refreshHeavy)
						cellImage.Image = img;
				}

				if (row.DefaultCellStyle.BackColor != backColor)
					row.DefaultCellStyle.BackColor = backColor;

				if (row.Cells[DgvReaderAbortColumn.Name] is DgvDisableButtonCell cellAbort)
				{
					if (cellAbort.Value == null || cellAbort.Value?.ToString() != TxtReader_BtnAbort)
						cellAbort.Value = TxtReader_BtnAbort;
					if (cellAbort.Enabled
					 && (reader.Status == TraceReaderStatus.Aborted
					  || reader.Status == TraceReaderStatus.Failed
					  || reader.Status == TraceReaderStatus.Finished))
						cellAbort.Enabled = false;
					else if (!cellAbort.Enabled
					      && (reader.Status == TraceReaderStatus.Waiting
					       || reader.Status == TraceReaderStatus.Processing
					       || reader.Status == TraceReaderStatus.OnPause))
						cellAbort.Enabled = true;
				}

				var cellThreadId = row.Cells[DgvReaderThreadIdColumn.Name];
				if (cellThreadId.Value == null || cellThreadId.Value?.ToString() != reader.ThreadId)
					cellThreadId.Value = reader.ThreadId;
				var cellMatches = row.Cells[DgvReaderCountMatchesColumn.Name];
				if (cellMatches.Value == null || cellMatches.Value?.ToString() != reader.CountMatches.ToString())
					cellMatches.Value = reader.CountMatches;
				var cellErrors = row.Cells[DgvReaderCountErrorMatchesColumn.Name];
				if (cellErrors.Value == null || cellErrors.Value?.ToString() != reader.CountErrors.ToString())
					cellErrors.Value = reader.CountErrors;
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
			finally
			{
				if (selected != null)
					selected.Selected = true;
			}
		}

		/// <summary>
		///     Открыти формы с ридерами
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonNextBlock_Click(object sender, EventArgs e)
		{
			try
			{
				if (sender != null)
					this.SuspendHandle();
				_shownProcessReadesPanel = !_shownProcessReadesPanel;

				if (_shownProcessReadesPanel)
				{
					_openProcessingReadersBtn.Text = @"ᐯ";
					splitContainerMainFilter.SplitterDistance = _prevReadersDistance > 0
						? _prevReadersDistance
						: splitContainerMainFilter.Height - splitContainerMainFilter.Height / 3;
					splitContainerMainFilter.Panel2Collapsed = !_shownProcessReadesPanel;
					RefreshAllRows(DgvReader, DgvReaderRefreshRow); // надо обновить при первой загрузке, иначе не прорисовываются
					DgvReader.Refresh();
				}
				else
				{
					if (sender != null)
						_prevReadersDistance = splitContainerMainFilter.SplitterDistance;
					_openProcessingReadersBtn.Text = @"ᐱ";
					splitContainerMainFilter.Panel2Collapsed = !_shownProcessReadesPanel;
				}
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
			finally
			{
				if (sender != null)
					this.ResumeHandle();
			}
		}

		private async void buttonSelectTraceNames_Click(object sender, EventArgs e)
		{
			var checkedFilePaths = GetCheckedFilePaths() ?? new HashSet<string>();
			var original = GetResultTemplates()?.Where(x => checkedFilePaths.Contains(x.ParentReader.FilePath));
			if (original == null || !original.Any())
				return;

			var alreadyAddedTraceNames = TbxTraceNameFilter.Text.Split(',')
			                                               .Select(x => x.Trim())
			                                               .GroupBy(x => x)
			                                               .ToDictionary(x => x.Key, StringComparer.InvariantCultureIgnoreCase);
			var filtered = original.Where(x => x?.TraceName != null)
			                       .GroupBy(x => x.TraceName, StringComparer.InvariantCultureIgnoreCase)
			                       .Select(x => new TraceNameFilter(alreadyAddedTraceNames.ContainsKey(x.Key),
			                                                        x.Key,
			                                                        x.Count(m => m.Error == null),
			                                                        x.Count(m => !m.IsSuccess)))
			                       .OrderBy(x => x.TraceName)
			                       .ToDictionary(x => x.TraceName);
			var form = await TraceNameFilterForm.GetAsync(filtered);
			var result = form.ShowDialog();
			if (result == DialogResult.OK)
				TbxTraceNameFilter.Text = string.Join(", ", filtered.Values.Where(x => x.Checked).Select(x => x.TraceName)).Trim();
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
		//  ReportStatus(ex);
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

		//private DataGridViewColumn _dgvReaderPrevSortColumn;
		//private bool _dgvReaderByAscending = true;

		//private async void DgvReader_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		//{
		//	if(e.ColumnIndex < 0 || IsWorking)
		//		return;

		//	var column = DgvReader.Columns[e.ColumnIndex];
		//	if (column == DgvReaderSelectColumn || column.DataPropertyName.IsNullOrWhiteSpace())
		//		return;

		//	var readers = GetResultReaders();
		//	if(!readers.Any())
		//		return;

		//	try
		//	{
		//		var byAscending = _dgvReaderPrevSortColumn?.Index != e.ColumnIndex || (_dgvReaderPrevSortColumn?.Index == e.ColumnIndex && !_dgvReaderByAscending);
		//		var sortOrder = byAscending ? SortOrder.Ascending : SortOrder.Descending;
		//		var result = readers.AsQueryable();

		//		result = byAscending ? result.OrderBy(column.DataPropertyName) : result.OrderByDescending(column.DataPropertyName);

		//		await DgvReader.AssignCollectionAsync(result, null, true);
		//		RefreshAllRows(DgvReader, DgvReaderRefreshRow);

		//		if (_dgvReaderPrevSortColumn != null)
		//			_dgvReaderPrevSortColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
		//		column.HeaderCell.SortGlyphDirection = sortOrder;

		//		_dgvReaderPrevSortColumn = column;
		//		_dgvReaderByAscending = byAscending;
		//	}
		//	catch (Exception ex)
		//	{
		//		ReportStatus(ex);
		//	}
		//}

		private void buttonFilteredPrev_Click(object sender, EventArgs e)
			=> SearchPrev(x => bool.Parse(x.Cells[DgvDataIsFilteredColumn.Name].Value?.ToString()));

		private void buttonErrorPrev_Click(object sender, EventArgs e) => SearchPrev(x => !bool.Parse(x.Cells[DgvDataIsSuccessColumn.Name].Value?.ToString()));

		private void buttonTrnPrev_Click(object sender, EventArgs e) => SearchPrev(x => TryGetTemplate(x, out var template) && template.IsSelected);

		private void SearchPrev(Func<DataGridViewRow, bool> condition)
		{
			try
			{
				if (DgvData.SelectedRows.Count == 0 && DgvData.CurrentRow == null)
					return;

				var countVisible = DgvData.DisplayedRowCount(false);
				var firstVisible = DgvData.FirstDisplayedScrollingRowIndex;
				var selected = DgvData.SelectedRows.Count > 0 ? DgvData.SelectedRows[0] : DgvData.CurrentRow;
				if (selected == null)
					return;

				foreach (var row in DgvData.Rows.OfType<DataGridViewRow>().Where(x => x.Index < selected.Index).Where(condition).Reverse())
				{
					DgvData.FirstDisplayedScrollingRowIndex = row.Index >= firstVisible && row.Index < firstVisible + countVisible ? firstVisible : row.Index;
					DgvData.SelectRow(row, DgvData.CurrentCell?.ColumnIndex ?? -1);
					return;
				}
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		private void buttonFilteredNext_Click(object sender, EventArgs e)
			=> SearchNext(x => bool.Parse(x.Cells[DgvDataIsFilteredColumn.Name].Value?.ToString()));

		private void buttonErrorNext_Click(object sender, EventArgs e) => SearchNext(x => !bool.Parse(x.Cells[DgvDataIsSuccessColumn.Name].Value?.ToString()));

		private void buttonTrnNext_Click(object sender, EventArgs e) => SearchNext(x => TryGetTemplate(x, out var template) && template.IsSelected);

		private void SearchNext(Func<DataGridViewRow, bool> condition)
		{
			try
			{
				if (DgvData.SelectedRows.Count == 0 && DgvData.CurrentRow == null)
					return;

				var countVisible = DgvData.DisplayedRowCount(false);
				var firstVisible = DgvData.FirstDisplayedScrollingRowIndex;
				var selected = DgvData.SelectedRows.Count > 0 ? DgvData.SelectedRows[0] : DgvData.CurrentRow;
				if (selected == null)
					return;

				foreach (var row in DgvData.Rows.OfType<DataGridViewRow>().Where(x => x.Index > selected.Index).Where(condition))
				{
					DgvData.FirstDisplayedScrollingRowIndex = row.Index >= firstVisible && row.Index < firstVisible + countVisible ? firstVisible :
						row.Index - countVisible >= 0
							?
							row.Index - countVisible
							: row.Index;
					DgvData.SelectRow(row, DgvData.CurrentCell?.ColumnIndex ?? -1);
					return;
				}
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		private async void DgvDataOnColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			try
			{
				if (!HasAnyResult || _currentDGVResult == null || e.ColumnIndex < 0)
					return;

				var oldSortedColumn = _oldSortedColumn; // поле очищается в методе ClearDGV
				var newColumn = DgvData.Columns[e.ColumnIndex];
				
				var columnName = DgvData.Columns[e.ColumnIndex].HeaderText;
				if (columnName == DgvDataPromptColumn.HeaderText)
					return;

				var byAscending = _oldSortedColumn?.Index != e.ColumnIndex || _oldSortedColumn?.Index == e.ColumnIndex && !_byAscending;
				var source = _currentDGVResult;
				
				var orderByOption = byAscending
					? new Dictionary<string, bool> { { columnName, false } }
					: new Dictionary<string, bool> { { columnName, true } };
				
				if (!orderByOption.ContainsKey(nameof(DataTemplate.Tmp.File)))
					orderByOption.Add(nameof(DataTemplate.Tmp.File), !byAscending);
				if (!orderByOption.ContainsKey(nameof(DataTemplate.Tmp.FoundLineID)))
					orderByOption.Add(nameof(DataTemplate.Tmp.FoundLineID), !byAscending);
				
				_currentDGVResult = DataTemplateCollection.DoOrdering(source, orderByOption);
				
				await AssignCurrentDgvResultAsync(false);
				
				if (oldSortedColumn != null)
					oldSortedColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
				if (newColumn != null)
					newColumn.HeaderCell.SortGlyphDirection = byAscending ? SortOrder.Ascending : SortOrder.Descending;
				
				_oldSortedColumn = newColumn;
				_byAscending = byAscending;
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		/// <summary>
		///     Основной метод загрузки данных
		/// </summary>
		/// <param name="isNewData"></param>
		/// <returns></returns>
		private async Task AssignCurrentDgvResultAsync(bool isNewData)
		{
			try
			{
				DgvData.CellFormatting -= DataGridViewOnCellFormatting;
				DgvData.SuspendLayout();
				
				var selectedSchemeName = string.Empty;
				var selectedPrivateID = -1;
				var selectedRangeToFirstVisible = -1;

				if (!isNewData
				 && DgvData.SelectedRows.Count > 0
				 && int.TryParse(DgvData.SelectedRows[0].Cells[DgvDataPrivateIDColumn.Name].Value?.ToString(), out var privateID2))
				{
					selectedSchemeName = DgvData.SelectedRows[0].Cells[DgvDataSchemeNameColumn.Name].Value?.ToString();
					selectedPrivateID = privateID2;
					selectedRangeToFirstVisible = DgvData.SelectedRows[0].Index - DgvData.FirstDisplayedScrollingRowIndex;
				}

				ClearErrorStatus();
				
				await DgvData.AssignCollectionAsync(_currentDGVResult, null);
				DgvDataPromptColumn.Visible = CurrentTransactionsMarkingType == TransactionsMarkingType.Both
				                           || CurrentTransactionsMarkingType == TransactionsMarkingType.Prompt;
				buttonSelectTraceNames.Enabled = btnExport.Enabled = DgvData.RowCount > 0;

				if (selectedPrivateID > -1 && !selectedSchemeName.IsNullOrWhiteSpace())
				{
					// возвращяем к предыдущей выбранной строке (происходит при фильтре и сортировке)
					// если новые данные (нового поиска) то не возвращает
					foreach (var row in DgvData.Rows.OfType<DataGridViewRow>())
					{
						if (!Equals(row.Cells[DgvDataPrivateIDColumn.Name].Value, selectedPrivateID)
						 || !Equals(row.Cells[DgvDataSchemeNameColumn.Name].Value, selectedSchemeName))
							continue;

						var firstVisible = row.Index - selectedRangeToFirstVisible;
						if (firstVisible > -1 && DgvData.RowCount > firstVisible)
							DgvData.FirstDisplayedScrollingRowIndex = firstVisible;
						DgvData.SelectRow(row);
						return;
					}
				}
			}
			finally
			{
				RefreshAllRows(DgvData, DgvDataRefreshRow);
				DgvData.Refresh();
				DgvData.Focus();
				DgvData.CellFormatting += DataGridViewOnCellFormatting;
				DgvData.ResumeLayout();
			}
		}

		private void DgvData_SelectionChanged(object sender, EventArgs e)
		{
			try
			{
				if (DgvData.IsSuspendLayout || DgvData.SelectedRows.Count == 0 || !TryGetTemplate(DgvData.SelectedRows[0], out var template))
					return;

				MainViewer.ChangeTemplate(template, checkBoxShowTrns.Checked, out var noChanged);
				
				if (checkBoxShowTrns.Checked && !noChanged)
					RefreshVisibleRows(DgvData, DgvDataRefreshRow);
				
				if (MainViewer.Parent is TabPage page)
					tabControlViewer.SelectTab(page);
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
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
				ReportStatus(ex);
			}
			finally
			{
				DgvData.Focus();
			}
		}

		private void AddViewer(TraceItemView traceViewer, DataTemplate template)
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
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, e) => tabControlViewer.SafeInvoke(() => { tabControlViewer.TabPages.Add(tabPage); });
			worker.RunWorkerAsync();
		}

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
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		internal virtual void TxtPatternOnTextChanged(object sender, EventArgs e)
		{
			UserSettings.PreviousSearch = ((ComboBox)sender).Items.OfType<string>().ToList();
			ValidationCheck(true);
		}

		internal virtual void ChbxUseRegex_CheckedChanged(object sender, EventArgs e) => UserSettings.UseRegex = ((CheckBox) sender).Checked;

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
				if (tabControlViewer.TabCount > 0)
					foreach (var page in tabControlViewer.TabPages.OfType<CustomTabPage>().ToList())
						page.View.RefreshDescription(checkBoxShowTrns.Checked, out var _);

				if (checkBoxShowTrns.Checked)
				{
					_currentTransactionsMarkingType = _prevTransactionsMarkingType == TransactionsMarkingType.None
						? DefaultTransactionsMarkingType
						: _prevTransactionsMarkingType;
				}
				else
				{
					_prevTransactionsMarkingType = _currentTransactionsMarkingType;
					_currentTransactionsMarkingType = TransactionsMarkingType.None;
				}

				DgvDataPromptColumn.Visible = _currentTransactionsMarkingType == TransactionsMarkingType.Both
				                           || _currentTransactionsMarkingType == TransactionsMarkingType.Prompt;
				CheckBoxTransactionsMarkingTypeChanged(_currentTransactionsMarkingType);
				RefreshVisibleRows(DgvData, DgvDataRefreshRow);
				DgvData.Focus();
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
		}

		private void ComboBox_KeyPress(object sender, KeyPressEventArgs e) => e.Handled = true;

		internal virtual void CobxTraceNameFilter_SelectedIndexChanged(object sender, EventArgs e)
			=> UserSettings.TraceNameFilterContains = ((ComboBox) sender).Text.Like(Resources.Txt_LogsReaderForm_Contains);

		internal virtual void TbxTraceNameFilterOnTextChanged(object sender, EventArgs e)
			=> UserSettings.TraceNameFilter = ((ComboBox)sender).Items.OfType<string>().ToList();

		internal virtual void CobxTraceMessageFilter_SelectedIndexChanged(object sender, EventArgs e)
			=> UserSettings.TraceMessageFilterContains = ((ComboBox) sender).Text.Like(Resources.Txt_LogsReaderForm_Contains);

		internal virtual void TbxTraceMessageFilterOnTextChanged(object sender, EventArgs e) 
			=> UserSettings.TraceMessageFilter = ((ComboBox)sender).Items.OfType<string>().ToList();

		internal virtual void ChbxAlreadyUseFilter_CheckedChanged(object sender, EventArgs e)
		{
		}

		protected virtual void ValidationCheck(bool clearStatus)
		{
			buttonSelectTraceNames.Enabled = btnExport.Enabled = DgvData.RowCount > 0;
			btnFilter.Enabled = ButtonHighlightEnabled = btnReset.Enabled = HasAnyResult;
		}

		protected virtual void BtnClear_Click(object sender, EventArgs e) => Clear(true, true);

		public void Clear() => Clear(false, false);

		protected async void Clear(bool saveData, bool collect)
		{
			try
			{
				if (saveData)
					SaveData();
				_oldSortedColumn = null;
				_currentDGVResult = null;
				_byAscending = true;

				if (DgvData.DataSource != null || DgvData.RowCount > 0)
				{
					DgvData.DataSource = null;
					DgvData.Rows.Clear();
					DgvData.Refresh();
				}

				MainViewer.Clear();

				foreach (var page in tabControlViewer.TabPages.OfType<CustomTabPage>().ToList())
				{
					if (page == MainViewer.Parent)
						continue;

					page.View.Clear();
					tabControlViewer.TabPages.Remove(page);
				}

				CountMatches = 0;
				CountErrorMatches = 0;
				Progress = 0;
				FilesCompleted = 0;
				TotalFiles = 0;
				ReportStatus(string.Empty, ReportStatusType.Success);
				ClearData();
				buttonSelectTraceNames.Enabled = btnExport.Enabled = false;
				btnFilter.Enabled = ButtonHighlightEnabled = btnReset.Enabled = HasAnyResult;
			}
			catch (Exception ex)
			{
				ReportStatus(ex);
			}
			finally
			{
				if (collect)
					await STREAM.GarbageCollectAsync().ConfigureAwait(false);
				OnProcessStatusChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		protected virtual void ClearData()
		{
			DgvReader.DataSource = null;
			DgvReader.Rows.Clear();
			DgvReader.Refresh();
			DgvReader.ColumnHeadersVisible = false;
		}

		protected virtual void CustomPanel_Resize(object sender, EventArgs e)
		{
		}

		internal void SelectTransactions()
		{
			if (checkBoxShowTrns.Checked)
				MainViewer.SelectTransactions();
			RefreshVisibleRows(DgvData, DgvDataRefreshRow); // не менять на RefreshAllRows. Производительность катастрофически падает
			DgvData.Focus();
		}

		internal void DeselectTransactions()
		{
			foreach (var page in tabControlViewer.TabPages.OfType<CustomTabPage>().ToList())
				page.View.DeselectTransactions();
			RefreshVisibleRows(DgvData, DgvDataRefreshRow); // не менять на RefreshAllRows. Производительность катастрофически падает
			DgvData.Focus();
		}

		private bool _isLastWasError;

		protected void ReportStatus(Exception ex)
			=> ReportStatus(ex.GetType().Name != nameof(Exception) && ex.GetType().Name != nameof(ArgumentException)
				                ? $"{ex.GetType().Name}: {ex.Message}"
				                : ex.Message,
			                ReportStatusType.Error);

		protected void ReportStatus(string message, ReportStatusType type)
			=> this.SafeInvoke(() =>
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
			});

		protected void ClearErrorStatus()
		{
			if (!_isLastWasError)
				return;

			_statusInfo.BackColor = SystemColors.Control;
			_statusInfo.ForeColor = Color.Black;
			_statusInfo.Text = string.Empty;
		}

		/// <summary>
		///     Clean up any resources being used.
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