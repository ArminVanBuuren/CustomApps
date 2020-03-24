using System;
using System.IO;
using System.Linq;
using Utils;
using Utils.Handles;

namespace LogsReader.Config
{
    [Serializable]
    public class UserSettings : IDisposable
    {
        private RegeditControl parentRegistry;

        public UserSettings(string schemeName)
        {
            try
            {
                Scheme = schemeName;
                var userName = Environment.UserName.Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty);
                UserName = Path.GetInvalidFileNameChars().Aggregate(userName, (current, ch) => current.Replace(ch.ToString(), string.Empty));
                parentRegistry = new RegeditControl(ASSEMBLY.ApplicationName);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public string Scheme { get; }
        public string UserName { get; }

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

        public string TraceLike
        {
            get => GetValue(nameof(TraceLike));
            set => SetValue(nameof(TraceLike), value);
        }

        public string TraceNotLike
        {
            get => GetValue(nameof(TraceNotLike));
            set => SetValue(nameof(TraceNotLike), value);
        }

        public string Message
        {
            get => GetValue(nameof(Message));
            set => SetValue(nameof(Message), value);
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
            if (parentRegistry == null)
                return string.Empty;

            try
            {
                using (var schemeControl = new RegeditControl(Scheme, parentRegistry))
                {
                    return (string)schemeControl[$"{UserName}_{name}"] ?? string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public void SetValue(string name, object value)
        {
            if (parentRegistry == null)
                return;

            try
            {
                using (var schemeControl = new RegeditControl(Scheme, parentRegistry))
                {
                    schemeControl[$"{UserName}_{name}"] = value;
                }
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
