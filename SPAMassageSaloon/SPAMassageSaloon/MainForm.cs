using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using LogsReader;
using Microsoft.WindowsAPICodePack.Taskbar;
using RegExTester;
using SPAFilter;
using SPAMassageSaloon.Common;
using SPAMassageSaloon.Properties;
using Utils;
using Utils.AppUpdater;
using Utils.Handles;
using Utils.UIControls.Main;
using Utils.WinForm;
using XPathTester;
using FontStyle = System.Drawing.FontStyle;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace SPAMassageSaloon
{
	public enum NationalLanguage
	{
		English = 0,
		Russian = 1
	}

	public partial class MainForm : Form
	{
		internal static readonly string AppName = "SPA Massage Saloon";
		internal static readonly string FormName;
		internal static readonly string BuildTime;

		private AboutWindow _about;
		private NationalLanguage _language;
		private int _countOfLastProcess;
		private int _countOfXPathTester;
		private int _countOfRegexTester;

		public ToolStripStatusLabel _cpuUsage;
		public ToolStripStatusLabel _threadsUsage;
		public ToolStripStatusLabel _ramUsage;

		private ApplicationUpdater AppUpdater { get; set; }

		internal int CountOfDefaultStatusItems { get; private set; }

		public bool FormOnClosing { get; private set; }

		public bool IsActivated { get; private set; }

		public bool IsMinimized { get; private set; }

		public NationalLanguage Language
		{
			get => _language;
			private set
			{
				try
				{
					_language = value;
					SetRegeditValue(nameof(Language), value);
					var culture = value == NationalLanguage.Russian ? new CultureInfo("ru-RU") : new CultureInfo("en-US");
					
					CultureInfo.DefaultThreadCurrentCulture = culture;
					CultureInfo.DefaultThreadCurrentUICulture = culture;
					
					Thread.CurrentThread.CurrentCulture = culture;
					Thread.CurrentThread.CurrentUICulture = culture;
					Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy HH:mm:ss.fff";
					Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongDatePattern = "dd.MM.yyyy HH:mm:ss.fff";
					
					CultureInfo.DefaultThreadCurrentCulture.DateTimeFormat = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat;
					CultureInfo.DefaultThreadCurrentUICulture.DateTimeFormat = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat;

					foreach (var form in MdiChildren.OfType<ISaloonForm>())
					{
						try
						{
							form.ApplySettings();
						}
						catch (Exception)
						{
							// ignored
						}
					}

					_about?.ApplySettings();
					languageToolStripMenuItem.Text = Resources.Txt_Language;
					aboutToolStripMenuItem.Text = Resources.Txt_About;
				}
				catch (Exception)
				{
					// ignored
				}
			}
		}

		protected bool IsSuspended { get; private set; }

		static MainForm()
		{
			var currentAssembly = Assembly.GetExecutingAssembly();
			BuildTime = $"Build time: {currentAssembly.GetLinkerTime():dd.MM.yyyy HH:mm:ss}";
			FormName = $"{AppName} {currentAssembly.GetAssemblyInfo().Version}";
		}

		public MainForm()
		{
			OpenReservedThreads();
			InitializeComponent();
		}

		private void OpenReservedThreads()
		{
			if (ThreadPool.SetMinThreads(40, 0))
			{
				//ThreadPool.GetMinThreads(out int MinWorkerThreads, out int MinCompletionPortThreads);
				for (var i = 0; i < 20; i++)
					Task.Factory.StartNew(() => { Thread.Sleep(1); });
			}

			//ThreadPool.SetMaxThreads(100, 0);
		}

		private ProgressForm ChildLoadingProgress { get; set; }

		private void MainForm_Load(object sender, EventArgs e)
		{
			try
			{
				Visible = false;
				IsMdiContainer = true;
				base.Text = FormName;

				try
				{
					ChildLoadingProgress = new ProgressForm();
					ToolStripManager.LoadSettings(this);
					InitializeToolbarsMenu();

					if (!IsMdiChild)
					{
						WindowState = Settings.Default.FormState;

						if (WindowState != FormWindowState.Maximized
						 && (Settings.Default.FormSize.Height < 100
						  || Settings.Default.FormSize.Width < 100
						  || Settings.Default.FormLocation.X < 0
						  || Settings.Default.FormLocation.Y < 0))
						{
							WindowState = FormWindowState.Maximized;
						}
						else if (Settings.Default.FormSize.Height > 300 && Settings.Default.FormSize.Width > 300)
							Size = Settings.Default.FormSize;
					}
				}
				catch (Exception)
				{
					// ignored
				}

				if (!Enum.TryParse(GetRegeditValue(nameof(Language)), out NationalLanguage lang))
					lang = NationalLanguage.English;

				switch (lang)
				{
					case NationalLanguage.Russian:
						russianToolStripMenuItem_Click(this, null);
						break;

					default:
						englishToolStripMenuItem_Click(this, null);
						break;
				}

				try
				{
					AppUpdater = new ApplicationUpdater(Assembly.GetExecutingAssembly(), 2);
					AppUpdater.OnUpdate += AppUpdater_OnUpdate;
					AppUpdater.OnSuccessfulUpdated += AppUpdater_OnSuccessfulUpdated;
					AppUpdater.Start();
				}
				catch (Exception)
				{
					// ignored
				}

				var statusStripItemsPaddingStart = new Padding(0, 2, 0, 2);
				var statusStripItemsPaddingEnd = new Padding(-3, 2, 1, 2);
				
				var autor = new ToolStripButton("?")
				{
					Font = new Font("Verdana", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0),
					Margin = new Padding(0, 0, 0, 2),
					ForeColor = Color.Blue
				};
				autor.Click += (s, args) =>
				{
					Presenter.ShowOwner();
					STREAM.GarbageCollect();
				};
				
				var toolToolStripCollection = new List<ToolStripItem>();
				toolToolStripCollection.Add(autor);
				toolToolStripCollection.Add(new ToolStripSeparator());
				
				toolToolStripCollection.Add(new ToolStripStatusLabel("CPU:")
				{
					Font = Font,
					Margin = statusStripItemsPaddingStart
				});
				_cpuUsage = new ToolStripStatusLabel("    ")
				{
					Font = Font,
					Margin = new Padding(-7, 2, 1, 2)
				};
				toolToolStripCollection.Add(_cpuUsage);
				toolToolStripCollection.Add(new ToolStripSeparator());
				
				toolToolStripCollection.Add(new ToolStripStatusLabel("Threads:")
				{
					Font = Font,
					Margin = statusStripItemsPaddingStart
				});
				_threadsUsage = new ToolStripStatusLabel("  ")
				{
					Font = Font,
					Margin = statusStripItemsPaddingEnd
				};
				toolToolStripCollection.Add(_threadsUsage);
				toolToolStripCollection.Add(new ToolStripSeparator());
				toolToolStripCollection.Add(new ToolStripStatusLabel("RAM:")
				{
					Font = Font,
					Margin = statusStripItemsPaddingStart
				});
				_ramUsage = new ToolStripStatusLabel("       ")
				{
					Font = Font,
					Margin = statusStripItemsPaddingEnd
				};
				toolToolStripCollection.Add(_ramUsage);
				toolToolStripCollection.Add(new ToolStripSeparator());
				CountOfDefaultStatusItems = toolToolStripCollection.Count;
				statusStrip.ItemRemoved += (o, args) =>
				{
					if (statusStrip?.Items.Count == CountOfDefaultStatusItems)
						base.Text = FormName;
				};
				statusStrip.Items.AddRange(toolToolStripCollection.ToArray());
				
				Closing += (o, args) =>
				{
					try
					{
						Settings.Default.FormLocation = Location;
						Settings.Default.FormSize = Size;
						Settings.Default.FormState = WindowState;
						Settings.Default.Save();
					}
					catch (Exception)
					{
						// ignored
					}
					finally
					{
						FormOnClosing = true;
					}
				};
				Activated += (o, args) => IsActivated = true;
				Deactivate += (o, args) => IsActivated = false;
				Resize += (o, args) =>
				{
					IsMinimized = WindowState == FormWindowState.Minimized;
					if (ChildLoadingProgress != null && ChildLoadingProgress.Visible)
						ChildLoadingProgress.ResetLocation();
				};
				LocationChanged += (o, args) =>
				{
					if (ChildLoadingProgress != null && ChildLoadingProgress.Visible)
						ChildLoadingProgress.ResetLocation();
				};
			}
			catch (Exception ex)
			{
				ReportMessage.Show(ex);
			}
			finally
			{
				CenterToScreen();
				Visible = true;
				var monitoring = new Thread(CalculateLocalResources)
				{
					IsBackground = true,
					Priority = ThreadPriority.Lowest
				};
				monitoring.Start();
			}
		}

		private void AppUpdater_OnSuccessfulUpdated(object sender, ApplicationUpdaterArgs args)
		{
			try
			{
				if (args.Control == null)
					return;

				var separator = $"\r\n{new string('-', 61)}\r\n";
				var description = separator.TrimStart()
				                + string.Join(separator,
				                              args.Control.Select(x
					                                                  => $"{x.RemoteFile.Location} Version = {x.RemoteFile.VersionString}\r\nDescription = {x.RemoteFile.Description}"))
				                        .Trim()
				                + separator.TrimEnd();
				ReportMessage.Show(string.Format(Resources.Txt_Updated, AppName, this.GetAssemblyInfo().Version, BuildTime, description).Trim(),
				                   MessageBoxIcon.Information,
				                   AppName);
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void AppUpdater_OnUpdate(object sender, ApplicationUpdaterArgs args)
		{
			try
			{
				SaveData();
				AppUpdater.DoUpdate(args.Control);
			}
			catch (Exception)
			{
				AppUpdater.Refresh();
			}
		}

		public void SaveData()
		{
			try
			{
				foreach (var form in MdiChildren.OfType<ISaloonForm>())
				{
					try
					{
						form.SaveData();
					}
					catch (Exception)
					{
						// ignored
					}
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}

		/// <summary>
		///     Мониторинг системных ресурсов
		/// </summary>
		private void CalculateLocalResources()
		{
			try
			{
				var curProcName = SERVER.ObtainCurrentProcessName();
				PerformanceCounter appCPU = null;

				if (curProcName != null)
				{
					appCPU = new PerformanceCounter("Process", "% Processor Time", curProcName, true);
					appCPU.NextValue();
				}

				var taskBarNotify = TaskbarManager.IsPlatformSupported ? (Action<int, int>) ChangeTaskbarState : null;
				var cpuUsage = "0";
				var threadsUsage = "0";
				var ramUsage = "0";

				while (!FormOnClosing)
				{
					try
					{
						if (!IsMinimized)
						{
							double percent = 0;
							if (appCPU != null)
								double.TryParse(appCPU.NextValue().ToString(), out percent);
							cpuUsage = $"{(int) (percent / Environment.ProcessorCount),3}%";
							var currentProcess = Process.GetCurrentProcess();
							threadsUsage = $"{currentProcess.Threads.Count,-2}";
							ramUsage = $"{currentProcess.PrivateMemorySize64.ToFileSize(),-5}";
						}

						this.SafeInvoke(() => Monitoring(cpuUsage, threadsUsage, ramUsage, taskBarNotify));
					}
					catch (OutOfMemoryException)
					{
						Thread.Sleep(10000); // wait 10 sec
					}
					catch (InvalidOperationException)
					{
						// ignored
					}
					finally
					{
						Thread.Sleep(1000);
					}
				}
			}
			catch (Exception ex)
			{
				ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, Resources.Txt_SystemResourceMonitoring);
			}
		}

		private void ChangeTaskbarState(int processesCount, int totalProgress)
		{
			if (IsSuspended)
				return;

			try
			{
				if (processesCount == 0)
				{
					TaskbarManager.Instance.SetOverlayIcon(null, "");
					TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress); // TaskbarProgressBarState.Normal
				}
				else
				{
					SetTaskBarOverlay(processesCount);
					TaskbarManager.Instance.SetProgressValue(totalProgress, processesCount * 100);
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void Monitoring(string cpuUsage, string threadsUsage, string ramUsage, Action<int, int> taskBarNotify)
		{
			_cpuUsage.Text = cpuUsage;
			_threadsUsage.Text = threadsUsage;
			_ramUsage.Text = ramUsage;
			var processCount = 0;
			var totalProgress = 0;

			foreach (var form in MdiChildren.OfType<ISaloonForm>())
			{
				if (form is Form child && child.IsDisposed)
					continue;

				processCount += form.ActiveProcessesCount;
				totalProgress += form.ActiveTotalProgress;
			}

			if (_countOfLastProcess > 0 && processCount == 0)
				FlashWindow.Flash(this);

			taskBarNotify?.Invoke(processCount, totalProgress);
			_countOfLastProcess = processCount;
		}

		private static void SetTaskBarOverlay(int countItem)
		{
			var bmp = new Bitmap(32, 32);

			using (var g = Graphics.FromImage(bmp))
			{
				g.FillEllipse(Brushes.Red, new Rectangle(new Point(4, 4), new Size(27, 27)));
				g.DrawString(countItem.ToString(),
				             new Font("Sans serif", 16, GraphicsUnit.Point),
				             Brushes.White,
				             new Rectangle(new Point(countItem <= 9 ? 8 : 1, 5), bmp.Size));
			}

			TaskbarManager.Instance.SetOverlayIcon(Icon.FromHandle(bmp.GetHicon()), "");
		}

		public void ChangeChildLoadingProgress(ISaloonForm child, int percent, bool isCompleted)
		{
			try
			{
				if (!isCompleted)
				{
					if (!ChildLoadingProgress.Visible)
						ChildLoadingProgress.Show(this);

					ChildLoadingProgress.SetProgress(percent);
				}
				else
				{
					if (ChildLoadingProgress == null || !ChildLoadingProgress.Visible)
						return;

					ChildLoadingProgress.Close();
					ChildLoadingProgress = new ProgressForm();
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void InitializeToolbarsMenu()
		{
			foreach (Control ctrl in toolStrip.Controls)
				if (ctrl is ToolStrip strip && !(ctrl is MenuStrip))
					strip.AllowItemReorder = true;
		}

		private void russianToolStripMenuItem_Click(object sender, EventArgs e)
		{
			englishToolStripMenuItem.Checked = false;
			russianToolStripMenuItem.Checked = true;
			Language = NationalLanguage.Russian;
		}

		private void englishToolStripMenuItem_Click(object sender, EventArgs e)
		{
			englishToolStripMenuItem.Checked = true;
			russianToolStripMenuItem.Checked = false;
			Language = NationalLanguage.English;
		}

		private void toolStripLogsReaderButton_Click(object sender, EventArgs e)
			=> ShowMdiSaloonForm(() =>
			{
				var form = new LogsReaderMainForm(ChangeChildLoadingProgress);
				form.Closed += (o, args) => form.Dispose();
				return form;
			});

		private void toolStripSpaFilterButton_Click(object sender, EventArgs e) 
			=> ShowMdiSaloonForm(() => SPAFilterForm.GetControl());

		private void toolStripXPathButton_Click(object sender, EventArgs e)
			=> ShowMdiSaloonForm(() =>
			                     {
				                     var tester = new XPathTesterForm();
				                     tester.Load += (o, args) => _countOfXPathTester++;
				                     tester.Closed += (o, args) => _countOfXPathTester--;
				                     return tester;
			                     },
			                     _countOfXPathTester < 4);

		private void toolStripRegexTester_Click(object sender, EventArgs e)
			=> ShowMdiNotSaloonForm(() =>
			                        {
				                        var tester = new frmMain();
				                        tester.Load += (o, args) => _countOfRegexTester++;
				                        tester.Closed += (o, args) => _countOfRegexTester--;
				                        return tester;
			                        },
			                        _countOfRegexTester < 3);

		public T ShowMdiNotSaloonForm<T>(Func<T> formMaker, bool newInstance = false) where T : Form
		{
			try
			{
				SuspendHandle();
				return ShowMdiForm(formMaker, newInstance);
			}
			finally
			{
				ResumeHandle();
				Focus();
				Activate();
			}
		}

		public T ShowMdiSaloonForm<T>(Func<T> formMaker, bool newInstance = false) where T : Form, ISaloonForm
		{
			try
			{
				return ShowMdiForm(formMaker, newInstance);
			}
			finally
			{
				Focus();
				Activate();
			}
		}

		private T ShowMdiForm<T>(Func<T> formMaker, bool newInstance = false) where T : Form
		{
			if (!newInstance && ActivateMdiForm(out T form))
				return form;

			try
			{
				form = formMaker.Invoke();
				if (form == null)
					return null;

				form.MdiParent = this;
				//  form.WindowState = FormWindowState.Maximized;
				form.TopLevel = false;
				form.ControlBox = false;
				form.Dock = DockStyle.Fill;
				form.FormBorderStyle = FormBorderStyle.None;
				form.Load += MDIManagerButton_Load;
				form.Show();
				return form;
			}
			catch (Exception ex)
			{
				ReportMessage.Show(ex);
				return null;
			}
		}

		private bool ActivateMdiForm<T>(out T form) where T : Form
		{
			foreach (var f in MdiChildren)
			{
				form = f as T;

				if (form != null)
				{
					f.Activate();
					return true;
				}
			}

			form = null;
			return false;
		}

		private static void MDIManagerButton_Load(object sender, EventArgs e)
		{
			try
			{
				var mdiForm = sender as Form;
				var button = new MDIManagerButton(mdiForm) { BackColor = Color.White };
				var mainForm = mdiForm?.MdiParent as MainForm;
				var status = mainForm?.statusStrip;
				if (status == null)
					return;

				mainForm.Text = $"{FormName}  [{mdiForm.Text.Trim()}]";
				button.Activated += (o, args) =>
				{
					try
					{
						var mdiButton = (MDIManagerButton) o;
						mainForm.Text = $"{FormName}  [{mdiButton.mdiForm.Text.Trim()}]";
					}
					catch (Exception)
					{
						// ignored
					}
				};
				button.Click += (o, args) =>
				{
					try
					{
						mainForm.SuspendHandle();
						var mdiButton = (MDIManagerButton) o;
						mdiButton.mdiForm.Activate();
					}
					catch (Exception)
					{
						// ignored
					}
					finally
					{
						mainForm.ResumeHandle();
					}
				};
				button.MClose += (o, args) =>
				{
					try
					{
						mainForm.SuspendHandle();
						var mdiButton = (MDIManagerButton) o;
						mdiButton.mdiForm.Close();
					}
					catch (Exception)
					{
						// ignored
					}
					finally
					{
						mainForm.ResumeHandle();
						mainForm.Activate();
					}
				};

				switch (sender)
				{
					case LogsReaderMainForm _:
						status.Items.Insert(mainForm.CountOfDefaultStatusItems, button);
						break;

					case SPAFilterForm _:
						status.Items.Insert(status.Items.Count > mainForm.CountOfDefaultStatusItems
						                 && status.Items[mainForm.CountOfDefaultStatusItems] is MDIManagerButton mdiButton
						                 && mdiButton.mdiForm is LogsReaderMainForm
							                    ? mainForm.CountOfDefaultStatusItems + 1
							                    : mainForm.CountOfDefaultStatusItems,
						                    button);
						break;

					default:
						status.Items.Add(button);
						break;
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void SuspendHandle()
		{
			IsSuspended = true;
			Win32.SuspendHandle(this);
		}

		private void ResumeHandle()
		{
			IsSuspended = false;
			Win32.ResumeHandle(this);
		}

		private void toolButtonAbout_ButtonClick(object sender, EventArgs e) => toolButtonAbout.ShowDropDown();

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (_about != null)
				{
					_about.Focus();
					_about.Activate();
				}
				else
				{
					_about = new AboutWindow
					{
						//Topmost = true,
						WindowStartupLocation = WindowStartupLocation.CenterScreen
					};
					_about.Focus();
					_about.Show();
					//about.ShowDialog();
					_about.Closed += About_Closed;
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void About_Closed(object sender, EventArgs e)
		{
			try
			{
				_about.Closed -= About_Closed;
				_about = null;
			}
			catch (Exception)
			{
				// ignored
			}
			finally
			{
				STREAM.GarbageCollect();
			}
		}

		private string GetRegeditValue(string name)
		{
			try
			{
				using (var reg = new RegeditControl(this.GetAssemblyInfo().ApplicationName))
					return (string) reg[name];
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		private void SetRegeditValue(string name, object value)
		{
			try
			{
				using (var reg = new RegeditControl(this.GetAssemblyInfo().ApplicationName))
					reg[name] = value;
			}
			catch (Exception)
			{
				// ignored
			}
		}
	}
}