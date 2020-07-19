using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using FastColoredTextBoxNS;
using LogsReader.Reader;
using Microsoft.Win32;
using Utils;
using Utils.Handles;

namespace LogsReader.Config
{
    [Serializable]
    public class UserSettings : IDisposable
    {
        private RegeditControl parentRegistry;

        public static string UserName { get; }

        public string Scheme { get; }

        public string PreviousSearch
        {
            get => GetValue(nameof(PreviousSearch));
            set => SetValue(nameof(PreviousSearch), value);
        }

        public bool UseRegex
        {
            get
            {
                var resStr = GetValue(nameof(UseRegex));
                if (resStr.IsNullOrEmptyTrim() || !bool.TryParse(resStr, out var res))
                    return true;
                return res;
            }
            set => SetValue(nameof(UseRegex), value);
        }

        public bool DateStartChecked
        {
            get
            {
                if (bool.TryParse(GetValue(nameof(DateStartChecked)), out var result))
                    return result;
                return false;
            }
            set => SetValue(nameof(DateStartChecked), value);
        }

        public bool DateEndChecked
        {
            get
            {
                if (bool.TryParse(GetValue(nameof(DateEndChecked)), out var result))
                    return result;
                return false;
            }
            set => SetValue(nameof(DateEndChecked), value);
        }

        public string TraceNameFilter
        {
            get => GetValue(nameof(TraceNameFilter));
            set => SetValue(nameof(TraceNameFilter), value);
        }

        public bool TraceNameFilterContains
        {
            get
            {
                if (bool.TryParse(GetValue(nameof(TraceNameFilterContains)), out var result))
                    return result;
                return true;
            }
            set => SetValue(nameof(TraceNameFilterContains), value);
        }

        public string TraceMessageFilter
        {
            get => GetValue(nameof(TraceMessageFilter));
            set => SetValue(nameof(TraceMessageFilter), value);
        }

        public bool TraceMessageFilterContains
        {
            get
            {
                if (bool.TryParse(GetValue(nameof(TraceMessageFilterContains)), out var result))
                    return result;
                return true;
            }
            set => SetValue(nameof(TraceMessageFilterContains), value);
        }

        public Language MessageLanguage
        {
            get
            {
                if (Enum.TryParse(GetValue(nameof(MessageLanguage)), out Language langResult))
                    return langResult;
                return Language.XML;
            }
            set => SetValue(nameof(MessageLanguage), value);
        }

        public bool MessageWordWrap
        {
            get
            {
                var resStr = GetValue(nameof(MessageWordWrap));
                if (resStr.IsNullOrEmptyTrim() || !bool.TryParse(resStr, out var res))
                    return true;
                return res;
            }
            set => SetValue(nameof(MessageWordWrap), value);
        }

        public bool MessageHighlights
        {
            get
            {
                if (bool.TryParse(GetValue(nameof(MessageHighlights)), out var res))
                    return res;
                return false;
            }
            set => SetValue(nameof(MessageHighlights), value);
        }

        public Language TraceLanguage
        {
            get
            {
                if (Enum.TryParse(GetValue(nameof(TraceLanguage)), out Language langResult))
                    return langResult;
                return Language.Custom;
            }
            set => SetValue(nameof(TraceLanguage), value);
        }

        public bool TraceWordWrap
        {
            get
            {
                var resStr = GetValue(nameof(TraceWordWrap));
                if (resStr.IsNullOrEmptyTrim() || !bool.TryParse(resStr, out var res))
                    return true;
                return res;
            }
            set => SetValue(nameof(TraceWordWrap), value);
        }

        public bool TraceHighlights
        {
            get
            {
                if (bool.TryParse(GetValue(nameof(TraceHighlights)), out var res))
                    return res;
                return false;
            }
            set => SetValue(nameof(TraceHighlights), value);
        }

        public Color BackColor
        {
	        get
	        {
		        if (int.TryParse(GetValue(nameof(BackColor)), out var res))
			        return Color.FromArgb(res);
		        return LogsReaderMainForm.SCHEME_COLOR_BACK;
            }
	        set => SetValue(nameof(BackColor), value.ToArgb());
        }

        public Color ForeColor
        {
	        get
	        {
		        if (int.TryParse(GetValue(nameof(ForeColor)), out var res))
			        return Color.FromArgb(res);
		        return LogsReaderMainForm.SCHEME_COLOR_FORE;
	        }
	        set => SetValue(nameof(ForeColor), value.ToArgb());
        }

        public Dictionary<string, TreeNodeItem> Template
        {
	        get
	        {
		        try
		        {
			        using (var regControl = _getRegeditControl.Invoke())
			        {
				        var obj = regControl[nameof(Template)];
				        if (obj is byte[] array)
				        {
					        using (var stream = new MemoryStream(array))
						        return new BinaryFormatter().Deserialize(stream) as Dictionary<string, TreeNodeItem>;
				        }
			        }
		        }
		        catch (Exception)
		        {
			        // ignored
		        }
		        return null;
            }
	        set
	        {
		        try
		        {
			        using (var stream = value.SerializeToStream())
			        using (var regControl = _getRegeditControl.Invoke())
				        regControl[nameof(Template), RegistryValueKind.Binary] = stream.ToArray();
                }
		        catch (Exception)
		        {
			        // ignored
		        }
            }
        }

        public bool GlobalSelectAllSchemas
        {
	        get
	        {
		        var resStr = GetValue(nameof(GlobalSelectAllSchemas));
		        if (resStr.IsNullOrEmptyTrim() || !bool.TryParse(resStr, out var res))
			        return true;
		        return res;
	        }
	        set => SetValue(nameof(GlobalSelectAllSchemas), value);
        }

        public bool ShowTransactions
        {
	        get
	        {
		        var resStr = GetValue(nameof(ShowTransactions));
		        if (resStr.IsNullOrEmptyTrim() || !bool.TryParse(resStr, out var res))
			        return true;
		        return res;
	        }
	        set => SetValue(nameof(ShowTransactions), value);
        }

        static UserSettings()
        {
	        try
	        {
		        var userName = Environment.UserName.Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty);
		        UserName = Path.GetInvalidFileNameChars().Aggregate(userName, (current, ch) => current.Replace(ch.ToString(), string.Empty));
	        }
	        catch (Exception)
	        {
		        // ignored
	        }
        }

        private Func<RegeditControl> _getRegeditControl;

        public UserSettings()
        {
	        try
	        {
		        _getRegeditControl = () => new RegeditControl(this.GetAssemblyInfo().ApplicationName);
            }
	        catch (Exception)
	        {
		        // ignored
	        }
        }

        public UserSettings(string schemeName)
        {
	        try
	        {
		        Scheme = schemeName;
		        parentRegistry = new RegeditControl(this.GetAssemblyInfo().ApplicationName);
                _getRegeditControl = () => new RegeditControl(Scheme, parentRegistry);
	        }
	        catch (Exception)
	        {
		        // ignored
	        }
        }

        public int GetValue(string name, int rangeFrom, int rangeTo, int @default)
        {
            var vlueStr = GetValue(name);
            if (int.TryParse(vlueStr, out var result) && result >= rangeFrom && result <= rangeTo)
                return result;
            return @default;
        }

        public string GetValue(string name)
        {
	        try
            {
                using (var regControl = _getRegeditControl.Invoke())
	                return (string)regControl[name] ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public void SetValue(string name, object value)
        {
	        try
            {
                using (var regControl = _getRegeditControl.Invoke())
	                regControl[name] = value;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void Dispose()
        {
            parentRegistry?.Dispose();
        }
    }
}