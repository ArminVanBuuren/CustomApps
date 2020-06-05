using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using SPAMassageSaloon.Common;
using Utils;

namespace LogsReader.Config
{
	[Serializable]
	public abstract class TraceParse
	{
		public static readonly TimeSpan MATCH_TIMEOUT = new TimeSpan(0, 0, 10);

		internal abstract bool IsCorrectRegex { get; set; }

		protected Regex GetCDataNode(XmlNode[] input, bool isMandatory, out XmlNode[] cdataResult)
		{
			cdataResult = null;
			if (input != null)
				if (input.Length > 0)
					cdataResult = input[0].NodeType == XmlNodeType.CDATA ? input : new XmlNode[] { new XmlDocument().CreateCDataSection(input[0].Value) };

			if (cdataResult == null || cdataResult.Length == 0)
				return null;

			var text = cdataResult[0].Value.ReplaceUTFCodeToSymbol();
			if (text.IsNullOrEmptyTrim() && !isMandatory)
				return null;

			if (!REGEX.Verify(text))
			{
				ReportMessage.Show(string.Format(Properties.Resources.Txt_LRTraceParse_ErrPattern, text), MessageBoxIcon.Error, "TraceParse Reader");
				return null;
			}
			else
			{
				return new Regex(text, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline, MATCH_TIMEOUT);
			}
		}
	}
}
