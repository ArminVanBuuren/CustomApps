using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Script.Control.Handlers.Arguments;
using Script.Control.Handlers.GetValue;
using Script.Control.Handlers.GetValue.Based;
using Script.Control.Handlers.SysObj.Based;
using XPackage;

namespace Script.Control.Handlers
{

    [IdentifierClass(typeof(GetValueHandler), "Выполняет поиск по файлу согласно контенту поиска")]
    public class GetValueHandler : GetValueBase
    {
        int _maxLines = 1000;
        protected Exec _exec;


        [Identifier("Pattern", "Искомое выражение", "Обязательный")]
        protected string Pattern { get; set; }

        [Identifier("Replacement", "Текстовый контент для замены")]
        protected string Replacement { get; set; }

        [Identifier("Options", "Установить опцию поиска слова", StringComparison.CurrentCulture, typeof(StringComparison))]
        StringComparison StringOption { get; }

        [Identifier("Need_Replace", "Нужно ли выполнять замену искомого на тексовый контент замены", false)]
        protected bool NeedReplace { get; set; } = false;

        [Identifier("Reserve_Lines", "Резервирование строк, если например через регулярное выраженик не подходит паттерн. если установлено = 50, то считывает 1000 строк, если ничего не нашел то удаляет 950 строк и оставляет последние 50. Далее к ним добавляет следующие 950 строк). Применяется только при поиске.", 0)]
        protected int ReserveLines { get; set; } = 0;

        [Identifier("Reverse_Read", "Читать файл с конца", false)]
        protected bool ReverseRead { get; set; } = false;

        [Identifier("Max_Count_Match", "Максимальное количество совпадений за итерацию. Например если установлено значение 3, то поиск по тексту будет продолжаться пока не найдет 3 совпадения, затем завершится", int.MaxValue)]
        protected int MaxCountMatch { get; set; } = int.MaxValue;

        [Identifier("Capture", "Eсли нашли контент то вырезем до найденного значения X символов и после Y символов.", "0;0")]
        int CaptureLenghtLeft { get; } = 0;

        int CaptureLenghtRight { get; } = 0;

        /// <summary>
        /// Конструктор только для наследований
        /// </summary>
        /// <param name="parentPack"></param>
        /// <param name="node"></param>
        /// <param name="logFill"></param>
        /// <param name="forInheritClass"></param>
        protected GetValueHandler(XPack parentPack, XmlNode node, LogFill logFill, bool forInheritClass) : base(parentPack, node, logFill)
        {
            DefaultInitialize();
        }
        /// <summary>
        /// Конструктор только для инициализации
        /// </summary>
        /// <param name="parentPack"></param>
        /// <param name="node"></param>
        /// <param name="logFill"></param>
        public GetValueHandler(XPack parentPack, XmlNode node, LogFill logFill) : base(parentPack, node, logFill)
        {
            StringOption = Functions.GetStringOptions(Attributes[GetXMLAttributeName(nameof(StringOption))]);
            var capture_length = Attributes[GetXMLAttributeName(nameof(CaptureLenghtLeft))];
            int temp_capture_length;
            if (int.TryParse(capture_length, out temp_capture_length))
            {
                CaptureLenghtLeft = temp_capture_length;
                CaptureLenghtRight = temp_capture_length;
            }
            else
            {
                var cup_left_right = capture_length?.Split(';');
                if (cup_left_right?.Length > 1)
                {
                    if (int.TryParse(cup_left_right[0], out temp_capture_length))
                        CaptureLenghtLeft = temp_capture_length;
                    if (int.TryParse(cup_left_right[1], out temp_capture_length))
                        CaptureLenghtRight = temp_capture_length;
                }
            }

            DefaultInitialize();
        }

        void DefaultInitialize()
        {
            Pattern = Attributes[GetXMLAttributeName(nameof(Pattern))];
            if (string.IsNullOrEmpty(Pattern))
                throw new HandlerInitializationException(GetIdentifier(nameof(Pattern)), false);

            Replacement = Attributes[GetXMLAttributeName(nameof(Replacement))];
            
            var reverse_read = Attributes[GetXMLAttributeName(nameof(ReverseRead))];
            var max_count_match = Attributes[GetXMLAttributeName(nameof(MaxCountMatch))];
            

            if (Replacement != null)
                NeedReplace = true;

            bool temp_reverse_read;
            if (bool.TryParse(reverse_read, out temp_reverse_read))
                ReverseRead = temp_reverse_read;

            //string save_matches = pack.Attributes["save_matches"];
            //bool temp_save_matches;
            //if (bool.TryParse(save_matches, out temp_save_matches))
            //    SaveMatches = temp_reverse_read;

            int temp_max_count_match;
            if (int.TryParse(max_count_match, out temp_max_count_match))
                MaxCountMatch = temp_max_count_match;
            if (MaxCountMatch <= 0)
                throw new HandlerInitializationException("[{0}]='{1}' Must Be Greater Than Zero", GetXMLAttributeName(nameof(MaxCountMatch)), MaxCountMatch);



            var reserve_lines = Attributes[GetXMLAttributeName(nameof(ReserveLines))];
            int temp_reserve_lines;
            if (int.TryParse(reserve_lines, out temp_reserve_lines))
                ReserveLines = temp_reserve_lines;
            if (ReserveLines > _maxLines)
                throw new HandlerInitializationException("[{0}]='{1}' Must Be Less Than '{2}'", GetXMLAttributeName(nameof(ReserveLines)), ReserveLines, _maxLines);

            var parentSysObj = Parent as FindBase;
            if (parentSysObj == null)
                throw new HandlerInitializationException(this);

            _exec = ExecSysObjects;
        }
        public override void Execute()
        {
            _exec.Invoke();
        }




        delegate void DLSearchOrReplaceContent(InnerTextMatch innerMatch, string dirBakPath);
        DLSearchOrReplaceContent _GetOrReplaceContent;
        delegate void DLClearLinesList(List<string> tempStringContent);
        DLClearLinesList _clearLinesList;
        public void ExecSysObjects()
        {
            var _parent = (FindBase)Parent;
            if(_parent.Matches.Count <= 0)
                return;

            string dirBakPath = null;
            if (NeedReplace)
            {
                dirBakPath = Path.Combine(Functions.LocalPath, string.Format("TEMP_{0:yyyyddMMhhmmss}", DateTime.Now));
                Directory.CreateDirectory(dirBakPath);
                _GetOrReplaceContent = ReplaceTextInFile;
                _clearLinesList = tempStringContent => tempStringContent.Clear();
            }
            else
            {
                _GetOrReplaceContent = GetTextInFile;
                if (ReserveLines > 0)
                {
                    _clearLinesList = tempStringContent => tempStringContent.RemoveRange(0, tempStringContent.Count - ReserveLines);
                }
                else
                {
                    _clearLinesList = tempStringContent => tempStringContent.Clear();
                }
            }



            
            foreach (var match in _parent.Matches.Where(x => x.SysObjType == FindType.Files))
            {
                var innerMatch = new InnerTextMatch(match);
                _GetOrReplaceContent(innerMatch, dirBakPath);

                if (innerMatch.Count > 0)
                    Matches.Add(innerMatch);
                else
                    AddLog(LogType.Info, this, "Not Found Any Matches In Object=[{0}]", match);
            }

            if (dirBakPath != null)
                Directory.Delete(dirBakPath);
        }


        void GetTextInFile(InnerTextMatch innerMatch, string dirBakPath)
        {
            var tempStringContent = new List<string>();
            using (var inputStream = File.OpenRead(innerMatch.FullPath))
            {
                using (var inputReader = new StreamReader(inputStream))
                {
                    while (true)
                    {
                        string tempString;
                        if ((tempString = inputReader.ReadLine()) != null)
                        {
                            if (tempStringContent.Count >= _maxLines)
                            {
                                SearchContent(tempStringContent, innerMatch);

                                //если искать больше не надо, нашли максимально необходимое, то завершаем процесс и заканчиваем дальнейшую обработку
                                if (innerMatch.Count >= MaxCountMatch)
                                {
                                    tempStringContent.Clear();
                                    return;
                                }

                                _clearLinesList.Invoke(tempStringContent);
                            }

                            tempStringContent.Add(tempString);
                        }
                        else
                        {
                            SearchContent(tempStringContent, innerMatch);
                            tempStringContent.Clear();
                            return;
                        }
                    }
                }
            }
        }
        void SearchContent(List<string> tempStringContent, InnerTextMatch innerMatch)
        {
            var contentStr = string.Join(Environment.NewLine, tempStringContent);
            var maxMatch = MaxCountMatch - innerMatch.Count;
            List<string> _matches;
            if (GetListMatches(contentStr, maxMatch, out _matches))
            {
                innerMatch.AddRange(_matches);
            }
        }
        public virtual bool GetListMatches(string input, int maxMatch, out List<string> matches)
        {
            matches = new List<string>();
            var startIndex = 0;
            var index = input.IndexOf(Pattern, StringOption);
            while (index != -1)
            {
                var captureStartIndex = index - CaptureLenghtLeft <= 0 ? 0 : index - CaptureLenghtLeft;
                var captureLengthEndIndex = index + Pattern.Length + CaptureLenghtRight > input.Length ? input.Length - captureStartIndex : CaptureLenghtLeft + Pattern.Length + CaptureLenghtRight;
                var finded = input.Substring(captureStartIndex, captureLengthEndIndex);
                matches.Add(finded);

                startIndex = index + Pattern.Length;

                if (matches.Count + 1 > maxMatch)
                    index = -1;
                else
                    index = input.IndexOf(Pattern, startIndex, StringOption);

                if (index == -1)
                {
                    return true;
                }
            }
            return false;
        }

        delegate void DLWriteContent(string content);
        void ReplaceTextInFile(InnerTextMatch innerMatch, string dirBakPath)
        {
            string filePathBak;
            do
            {
                filePathBak = Path.Combine(dirBakPath, string.Format("{0}.bak", new Random().Next(10000, 99999)));
            } while (File.Exists(filePathBak));

            DLWriteContent _wrtContentBakFile = delegate(string content)
                                                {
                                                    using (var outputWriter = File.AppendText(filePathBak))
                                                    {
                                                        outputWriter.WriteLine(content);
                                                    }
                                                };


            var tempStringContent = new List<string>();
            using (var inputStream = File.OpenRead(innerMatch.FullPath))
            {
                using (var inputReader = new StreamReader(inputStream))
                {
                    while (true)
                    {
                        string tempString;
                        if ((tempString = inputReader.ReadLine()) != null)
                        {
                            if (tempStringContent.Count >= _maxLines)
                            {
                                ReplaceContent(tempStringContent, innerMatch, _wrtContentBakFile);

                                //если искать больше не надо, нашли максимально необходимое, то завершаем процесс и заканчиваем дальнейшую обработку
                                if (innerMatch.Count >= MaxCountMatch)
                                {
                                    _wrtContentBakFile.Invoke(Environment.NewLine + inputReader.ReadToEnd());
                                    tempStringContent.Clear();
                                    break;
                                }

                                _clearLinesList.Invoke(tempStringContent);
                            }

                            tempStringContent.Add(tempString);
                        }
                        else
                        {
                            ReplaceContent(tempStringContent, innerMatch, _wrtContentBakFile);
                            tempStringContent.Clear();
                            break;
                        }
                    }
                }



	            if (innerMatch.Count > 0)
                    PerformerBase.CopyFile(filePathBak, innerMatch.FullPath, true);
	            File.Delete(filePathBak);
            }
        }

        void ReplaceContent(List<string> tempStringContent, InnerTextMatch innerMatch, DLWriteContent _wrtContentBakFile)
        {
            var contentStr = string.Join(Environment.NewLine, tempStringContent);
            var maxMatch = MaxCountMatch - innerMatch.Count;
            List<string> _matches;
            var result = ReplaceContentMatches(contentStr, maxMatch, out _matches);
            if (result != null && !result.Equals(contentStr))
            {
                innerMatch.AddRange(_matches);
                _wrtContentBakFile.Invoke(result);
            }
            else
            {
                _wrtContentBakFile.Invoke(contentStr);
            }
        }

        public virtual string ReplaceContentMatches(string input, int maxMatch, out List<string> matches)
        {
            matches = new List<string>();
            string result = null;
            var startIndex = 0;
            var index = input.IndexOf(Pattern, StringOption);
            while (index != -1)
            {
                var captureStartIndex = index - CaptureLenghtLeft <= 0 ? 0 : index - CaptureLenghtLeft;
                var captureLengthEndIndex = index + Pattern.Length + CaptureLenghtRight > input.Length ? input.Length - captureStartIndex : CaptureLenghtLeft + Pattern.Length + CaptureLenghtRight;
                var finded = input.Substring(captureStartIndex, captureLengthEndIndex);
                matches.Add(finded);

                result = result + input.Substring(startIndex, index - startIndex) + Replacement;
                startIndex = index + Pattern.Length;

                if (matches.Count + 1 > maxMatch)
                    index = -1;
                else
                    index = input.IndexOf(Pattern, startIndex, StringOption);

                if (index == -1)
                {
                    result = result + input.Substring(startIndex, input.Length - startIndex);
                }
            }

            return result;
        }

    }

}
