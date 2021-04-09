using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using LogsReader.Properties;
using SPAMassageSaloon.Common;
using Utils;

namespace LogsReader.Config
{
	[Serializable]
	public abstract class TraceParse
	{
		internal abstract bool IsCorrect { get; set; }

		protected Regex GetCDataNode(XmlNode[] input, out XmlNode[] cdataResult, RegexOptions optional = RegexOptions.None)
			=> GetCDataNode(input, false, out cdataResult, optional);

		protected Regex GetCDataNode(XmlNode[] input, bool isMandatory, out XmlNode[] cdataResult, RegexOptions optional = RegexOptions.None)
		{
			cdataResult = null;
			if (input != null && input.Length > 0)
				cdataResult = input[0].NodeType == XmlNodeType.CDATA ? input : new XmlNode[] { new XmlDocument().CreateCDataSection(input[0].Value) };
			if (cdataResult == null || cdataResult.Length == 0)
				return null;

			var text = cdataResult[0].Value?.ReplaceUTFCodeToSymbol();
			if (text.IsNullOrWhiteSpace() && !isMandatory)
				return null;

			if (!REGEX.Verify(text))
			{
				ReportMessage.Show(string.Format(Resources.Txt_LRTraceParse_ErrPattern, text), MessageBoxIcon.Error, "TraceParse Reader");
				return null;
			}

			return optional == RegexOptions.None
				? new Regex(text, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline, LRSettings.PARSING_MATCH_TIMEOUT)
				: new Regex(text, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline | optional, LRSettings.PARSING_MATCH_TIMEOUT);
		}
	}
}