using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using LogsReader.Properties;
using LogsReader.Reader;
using SPAMassageSaloon.Common;
using Utils;

namespace LogsReader.Config
{
	public delegate void ReportStatusHandler(string message, ReportStatusType type);

	[XmlRoot("Scheme")]
	public class LRSettingsScheme
	{
		private LRGroups _servers = new LRGroups(new[]
		{
			new LRGroupItem("local", 0, "localhost")
		});

		private LRGroups _fileTypes = new LRGroups(new[]
		{
			new LRGroupItem("type", 0, "log")
		});

		private LRFolderGroup _logsFolder = new LRFolderGroup(new[]
		{
			new LRFolder(@"C:\", false)
		});

		private string _cultureListString = string.Empty;
		private string _schemeName = string.Empty;
		private string _orderBy = $"{nameof(DataTemplate.Tmp.Date)}, {DataTemplate.ReaderPriority}, {nameof(DataTemplate.Tmp.File)}, {nameof(DataTemplate.Tmp.FoundLineID)}";
		private int _maxLines = 50;
		private int _maxThreads = -1;
		private int _rowsLimit = 9999;

		private LRTraceParse _traceParce = new LRTraceParse();

		public event ReportStatusHandler ReportStatus;

		[XmlAttribute("name")]
		public string Name
		{
			get => _schemeName;
			set => _schemeName = value.IsNullOrWhiteSpace()
				                     ? throw new Exception(Resources.Txt_LRSettingsScheme_ErrSchemeName)
				                     : Regex.Replace(value, @"\s+", "").ToUpperInvariant();
		}

		[XmlAttribute("encoding")]
		public string EncodingName
		{
			get => Encoding.HeaderName;
			set
			{
				if (value.IsNullOrWhiteSpace())
					return;

				Encoding enc;
				try
				{
					enc = Encoding.GetEncoding(value);
				}
				catch (Exception ex)
				{
					ReportMessage.Show(ex.Message, MessageBoxIcon.Error, $"Scheme=\"{Name}\" - @encoding");
					return;
				}

				Encoding = enc;
			}
		}

		[XmlIgnore]
		public Encoding Encoding { get; private set; } = Encoding.GetEncoding("windows-1251");

		[XmlAttribute("cultureList")]
		public string CultureListString
		{
			get => _cultureListString;
			set
			{
				CultureList.Clear();

				if (value.IsNullOrWhiteSpace())
					return;

				AddCultureList(value);

				_cultureListString = string.Join(";", CultureList.Select(x => x.Name));
			}
		}

		[XmlIgnore]
		internal HashSet<CultureInfo> CultureList { get; } = new HashSet<CultureInfo>();

		[XmlAnyElement("ServersComment")]
		public XmlComment ServersComment
		{
			get => LRSettings.GetComment(Resources.Txt_LRSettingsScheme_Servers);
			set { }
		}

		[XmlElement("Servers")]
		public LRGroups Servers
		{
			get => _servers;
			set
			{
				if (value == null
				 || value.Groups.Count == 0
				 || value.Groups.Any(groupName => groupName.Key.IsNullOrWhiteSpace()
				                               || !groupName.Value.Item2.Any() ||
				                                  groupName.Value.Item2.Any(groupValue => groupValue.IsNullOrWhiteSpace())))
					throw new Exception(string.Format(Resources.Txt_LRSettingsScheme_ErrNode, Name, "Servers"));
				_servers = value;
			}
		}

		[XmlAnyElement("FileTypesComment")]
		public XmlComment FileTypesComment
		{
			get => LRSettings.GetComment(Resources.Txt_LRSettingsScheme_Types);
			set { }
		}

		[XmlElement("FileTypes")]
		public LRGroups FileTypes
		{
			get => _fileTypes;
			set
			{
				if (value == null
				 || value.Groups.Count == 0
				 || value.Groups.Any(groupName => groupName.Key.IsNullOrWhiteSpace()
				                               || !groupName.Value.Item2.Any() ||
				                                  groupName.Value.Item2.Any(groupValue => groupValue.IsNullOrWhiteSpace())))
					throw new Exception(string.Format(Resources.Txt_LRSettingsScheme_ErrNode, Name, "FileTypes"));
				_fileTypes = value;
			}
		}

		[XmlAnyElement("LogsFolderComment")]
		public XmlComment LogsFolderComment
		{
			get => LRSettings.GetComment(Resources.Txt_LRSettingsScheme_LogsDirectory);
			set { }
		}

		[XmlElement("LogsFolderGroup")]
		public LRFolderGroup LogsFolder
		{
			get => _logsFolder;
			set
			{
				if (value == null || value.Folders.Count == 0 || value.Folders.Keys.Any(x => x.IsNullOrWhiteSpace()))
					throw new Exception(string.Format(Resources.Txt_LRSettingsScheme_ErrNode, Name, "LogsFolderGroup"));
				_logsFolder = value;
			}
		}

		[XmlAnyElement("MaxLinesComment")]
		public XmlComment MaxLinesComment
		{
			get => LRSettings.GetComment(Resources.Txt_LRSettingsScheme_MaxTraceLines);
			set { }
		}

		[XmlElement("MaxLines")]
		public int MaxLines
		{
			get => _maxLines;
			set => _maxLines = value <= 0 ? 1 : value;
		}

		[XmlAnyElement("MaxThreadsComment")]
		public XmlComment MaxThreadsComment
		{
			get => LRSettings.GetComment(Resources.Txt_LRSettingsScheme_MaxThreads);
			set { }
		}

		[XmlElement("MaxThreads")]
		public int MaxThreads
		{
			get => _maxThreads;
			set => _maxThreads = value <= 0 ? -1 : value;
		}

		[XmlAnyElement("RowsLimitComment")]
		public XmlComment RowsLimitComment
		{
			get => LRSettings.GetComment(Resources.Txt_LRSettingsScheme_RowsLimit);
			set { }
		}

		[XmlElement("RowsLimit")]
		public int RowsLimit
		{
			get => _rowsLimit;
			set => _rowsLimit = value <= 0 ? 9999 : value;
		}

		[XmlAnyElement("OrderByComment")]
		public XmlComment OrderByComment
		{
			get => LRSettings.GetComment(Resources.Txt_LRSettingsScheme_OrderBy);
			set { }
		}

		[XmlElement("OrderBy")]
		public string OrderBy
		{
			get => _orderBy;
			set
			{
				if (value.IsNullOrWhiteSpace())
					return;

				Dictionary<string, bool> result;
				try
				{
					result = CheckOrderByItem(value);
				}
				catch (ArgumentException)
				{
					if (ReportStatus == null)
						ReportMessage.Show(Resources.Txt_LRSettingsScheme_ErrUnique, MessageBoxIcon.Error, $"Scheme=\"{Name}\" - OrderBy");
					else
						ReportStatus.Invoke(Resources.Txt_LRSettingsScheme_ErrUnique, ReportStatusType.Error);
					return;
				}
				catch (Exception ex)
				{
					if (ReportStatus == null)
						ReportMessage.Show(ex.Message, MessageBoxIcon.Error, $"Scheme=\"{Name}\" - OrderBy");
					else
						ReportStatus.Invoke(ex.Message, ReportStatusType.Error);
					return;
				}

				OrderByItems = result;
				_orderBy = string.Join(", ", OrderByItems.Select(x => $"{x.Key}" + (x.Value ? " desc" : "")));
			}
		}

		[XmlIgnore]
		public Dictionary<string, bool> OrderByItems { get; private set; }

		[XmlAnyElement("TraceParseComment")]
		public XmlComment TraceParseComment
		{
			get => LRSettings.GetComment(Resources.Txt_LRSettingsScheme_TraceParse);
			set { }
		}

		[XmlElement]
		public LRTraceParse TraceParse
		{
			get => _traceParce;
			set
			{
				if (value != null)
				{
					if (value.IsCorrect)
						_traceParce = value;
				}
			}
		}

		[XmlIgnore]
		public bool IsCorrect
		{
			get
			{
				if (TraceParse.IsCorrect)
					return true;

				ReportStatus?.Invoke(string.Format(Resources.Txt_LRSettingsScheme_ErrMandatory, Name), ReportStatusType.Error);
				return false;
			}
		}

		public LRSettingsScheme()
		{
		}

		internal LRSettingsScheme(DefaultSettings set)
		{
			_schemeName = set.ToString("G");
			_traceParce = new LRTraceParse(set);

			switch (set)
			{
				case DefaultSettings.MG:
					Servers = new LRGroups(new[]
					{
						new LRGroupItem("UZ-MG", 0, "mg1, mg2, mg3, mg4")
					});
					FileTypes = new LRGroups(new[]
					{
						new LRGroupItem("Connectors-Activity", 0,
						                "SOAPCON*Activity, SMSCON*SMS*Activity, SMSCON*USSD*Activity, IVRCON*Activity, EMAILCON*Activity"),
						new LRGroupItem("Connectors-Data", 0, "SOAPCON*Data, SMSCON*SMS*Data, SMSCON*USSD*Data, IVRCON*Data, EMAILCON*Data"),
						new LRGroupItem("Connectors-Error", 0, "SOAPCON*Error, SMSCON*SMS*Error, SMSCON*USSD*Error, IVRCON*Error, EMAILCON*Error"),
						new LRGroupItem("Handlers-Activity", 1, "CRMCON*Activity, WCF*Activity, DBCON*Activity"),
						new LRGroupItem("Handlers-Data", 1, "CRMCON*Data, DISPATCHER, WCF*Data, DBCON*Data"),
						new LRGroupItem("Handlers-Error", 1, "CRMCON*Error, WCF*Error, DBCON*Error"),
					});
					LogsFolder = new LRFolderGroup(new[]
					{
						new LRFolder(@"C:\FORISLOG\MG", true)
					});
					MaxLines = 100;
					MaxThreads = -1;
					OrderBy = _orderBy;
					break;
				case DefaultSettings.SPA:
					Servers = new LRGroups(new[]
					{
						new LRGroupItem("UZ-BPM", 0, "spa-bpm1, spa-bpm2, spa-bpm3, spa-bpm4, spa-bpm5, spa-bpm6"),
						new LRGroupItem("UZ-SA", 0, "spa-sa1, spa-sa2, spa-sa3, spa-sa4, spa-sa5, spa-sa6")
					});
					FileTypes = new LRGroups(new[]
					{
						new LRGroupItem("SPA.SA", 0, "AM, BMS, HLR*Huawei, HLR*ZTE, MCA, RBT, SCP, SMSC"),
						new LRGroupItem("SPA.BPM", 0, "SPA.BPM")
					});
					LogsFolder = new LRFolderGroup(new[]
					{
						new LRFolder(@"C:\FORISLOG\SPA", true)
					});
					MaxLines = 1;
					MaxThreads = -1;
					OrderBy = $"{nameof(DataTemplate.Tmp.Date)} desc, {nameof(DataTemplate.Tmp.ID)} desc";
					break;
				case DefaultSettings.MGA:
					Servers = new LRGroups(new[]
					{
						new LRGroupItem("UZ-MGA", 0, "crm-mg1, crm-mg2, crm-mg3, crm-mg4")
					});
					FileTypes = new LRGroups(new[]
					{
						new LRGroupItem("Default", 0, "Debug-All, Debug-Only")
					});
					LogsFolder = new LRFolderGroup(new[]
					{
						new LRFolder(@"C:\FORISLOG\MGAdapter", true)
					});
					MaxLines = 20000;
					MaxThreads = -1;
					OrderBy = _orderBy;
					break;
			}
		}

		void AddCultureList(string list)
		{
			foreach (var cultureStr in list.Split(';'))
			{
				try
				{
					CultureList.Add(CultureInfo.GetCultureInfo(cultureStr));
				}
				catch (Exception)
				{
					// ignored
				}
			}
		}

		internal static Dictionary<string, bool> CheckOrderByItem(string value)
		{
			var result = new Dictionary<string, bool>();
			foreach (var orderItem in value.Split(',').Where(x => !x.IsNullOrWhiteSpace()).Select(x => x.Trim()))
			{
				var orderStatement = orderItem.Split(' ');
				var isDescending = orderStatement.Length > 1 && orderStatement[1].Length > 0 && orderStatement[1].LikeAny("desc", "descending");

				if (!orderStatement[0].LikeAny(out var orderItem2, DataTemplateCollection.OrderByFields))
					throw new Exception(string.Format(Resources.Txt_LRSettingsScheme_ErrOrderBy, orderItem));

				result.Add(orderItem2, isDescending);
			}

			return result;
		}

		public override string ToString() => Name;
	}
}