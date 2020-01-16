using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Script.Control.Handlers.Arguments;
using Script.Handlers;
using XPackage;

namespace Script.Control.Handlers
{
    [IdentifierClass(typeof(GetValueByRegexHandler), "Выполняет поиск по файлу, по регулярному выражению")]
    public class GetValueByRegexHandler : GetValueHandler
    {
        [Identifier("Options", "Установить опцию для поиска по регулярному выражению.", RegexOptions.None, typeof(RegexOptions))]
        public RegexOptions RegexOption { get; }
        private Regex _getMatches;

        public GetValueByRegexHandler(XPack parentPack, XmlNode node, LogFill logFill) : base(parentPack, node, logFill, true)
        {
            RegexOption = Functions.GetRegexOptions(Attributes[GetXMLAttributeName(nameof(RegexOption))]);
            if ((RegexOption & RegexOptions.Compiled) != RegexOptions.Compiled)
                RegexOption |= RegexOptions.Compiled;

            _getMatches = new Regex(Pattern, RegexOption);


            var needReplace = Attributes[GetXMLAttributeName(nameof(NeedReplace))];
            if (Replacement != null)
            {
                bool _needReplace;
                if (bool.TryParse(needReplace, out _needReplace))
                    NeedReplace = _needReplace;
            }
        }

        public override bool GetListMatches(string input, int maxMatch, out List<string> matches)
        {
            matches = new List<string>();
            foreach (Match regxMatch in _getMatches.Matches(input))
            {
                matches.Add(regxMatch.Value);
                if (matches.Count + 1 >= maxMatch)
                {
                    return true;
                }
            }
            if (matches.Count > 0)
                return true;
            return false;
        }

        public override string ReplaceContentMatches(string input, int maxMatch, out List<string> matches)
        {
            matches = new List<string>();
            string result = null;
            foreach (Match regxMatch in _getMatches.Matches(input))
            {
                result = result + regxMatch.Value;
                matches.Add(regxMatch.Value);
                if (matches.Count + 1 >= maxMatch)
                {
                    return result;
                }
            }
            return result;
        }

    }
}
