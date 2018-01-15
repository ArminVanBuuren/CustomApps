using System;
using System.Text;
using Protas.Components.Functions;
using Protas.Control.Resource.Base;
using Protas.Components.XPackage;

namespace Protas.Control.Resource
{
    internal class ResourceSignature : IDisposable
    {
        public const char CONTEXT_RESOURCE_SEPARATOR = ' ';
        public const char OPEN_COLLECTION_CONSTRUCTOR = '(';
        public const char CLOSE_COLLECTION_CONSTRUCTOR = ')';
        public const char OPEN_COSTRUCTOR_PARAM = '\'';
        public const char CLOSE_COSTRUCTOR_PARAM = '\'';
        public const char COLLECTION_COSTRUCTOR_SEPARATOR = ',';
        public const char PROPS_SEPARATOR = '.';

        string _contextName = string.Empty;
        string _resourceName = string.Empty;
        int _startProp = 0;
        int _indexOfLastCh = 0;

        ResourceKernel Main { get; }
        public ResourceShell Shell { get; private set; }
        /// <summary>
        /// конструктор непосредственного ресурса
        /// </summary>
        public ResourceConstructor Constructor { get; }
        /// <summary>
        /// входящая стринговая строка, источник
        /// </summary>
        public string Source { get; private set; }
        /// <summary>
        /// свосйтсва которые необходиы на выходе
        /// </summary>
        public string OutputPath { get; private set; }
        /// <summary>
        /// если действительно существует ресурс с текущими свойствами линка
        /// </summary>
        public bool IsCorrectSyntax { get; private set; } = false;
        /// <summary>
        /// если ресурс находится в блеклисте то возвращаем true (данная проверка необходима если возникают архитектурные конфликты когда ресурсы используется как рекурсивные т.е. вызывают сами себя в бесконечности)
        /// </summary>
        public bool InBlackList { get; private set; } = false;

        public ResourceSignature(ResourceKernel main, string input)
        {
            Constructor = new ResourceConstructor(main);
            Main = main;
            if (!string.IsNullOrEmpty(input))
                Initialize(input);
        }

        void Initialize(string input)
        {
            Source = input;
            string constructorAndProps = string.Empty;
            string inputStr1 = input.Trim();

            //удовлетворяет ли входная строка ситнаксису {xxxxx}
            if (!(inputStr1.IndexOf('{') == 0 && (inputStr1.IndexOf('}', inputStr1.Length - 1) == inputStr1.Length - 1)))
                return;

            string inputStr2 = inputStr1.Substring(1, inputStr1.Length - 2);
            StringBuilder source = new StringBuilder();
            foreach (char ch in inputStr2)
            {
                switch (ch)
                {
                    case CONTEXT_RESOURCE_SEPARATOR:
                        {
                            if (string.IsNullOrEmpty(source.ToString().Trim()))
                            {
                                source.Remove(0, source.Length);
                                continue;
                            }
                            if (string.IsNullOrEmpty(_contextName))
                            {
                                _contextName = source.ToString();
                                source.Remove(0, source.Length);
                                continue;
                            }
                            break;
                        }
                    case PROPS_SEPARATOR:
                    case OPEN_COLLECTION_CONSTRUCTOR:
                        {
                            if (!string.IsNullOrEmpty(_contextName) && !string.IsNullOrEmpty(_resourceName))
                                break;

                            bool result = false;
                            if (!string.IsNullOrEmpty(_contextName) && string.IsNullOrEmpty(source.ToString().Trim()))
                            {
                                result = GetShellAndCheckFromBlackList(string.Empty, _contextName);
                            }
                            else if (string.IsNullOrEmpty(_contextName) && string.IsNullOrEmpty(_resourceName) && !string.IsNullOrEmpty(source.ToString().Trim()))
                            {
                                result = GetShellAndCheckFromBlackList(string.Empty, source.ToString().Trim());
                            }
                            else if (!string.IsNullOrEmpty(_contextName) && string.IsNullOrEmpty(_resourceName) && !string.IsNullOrEmpty(source.ToString().Trim()))
                            {
                                result = GetShellAndCheckFromBlackList(_contextName, source.ToString().Trim());
                            }

                            if (result)
                                source.Remove(0, source.Length);
                            break;
                        }
                }
                source.Append(ch);
            }


            string sourceTrim = source.ToString().Trim();
            source.Remove(0, source.Length);
            if (!string.IsNullOrEmpty(sourceTrim))
            {
                if (string.IsNullOrEmpty(_contextName) && string.IsNullOrEmpty(_resourceName))
                {
                    GetShellAndCheckFromBlackList(string.Empty, sourceTrim);
                }
                else if (!string.IsNullOrEmpty(_contextName) && string.IsNullOrEmpty(_resourceName))
                {
                    GetShellAndCheckFromBlackList(_contextName, sourceTrim);
                }
                else
                {
                    constructorAndProps = sourceTrim;
                }
            }

            if (!string.IsNullOrEmpty(constructorAndProps))
            {
                GetConstructorAndOutProps(constructorAndProps);
            }

            if (Shell == null)
                return;

            //Если количество параметров конструктора в обработанном строчном синтаксическом выражении, меньше чем должно быть - то данное выражение некорректное.
            if (Constructor.Count >= Shell.Entity.MinConstructorParams)
                IsCorrectSyntax = true;
        }

        bool GetShellAndCheckFromBlackList(string contextName, string resourceName)
        {
            Shell = Main.Contexts.GetResource(contextName, resourceName);
            if (Shell == null)
                return false;

            _contextName = Shell.ContextName;
            _resourceName = Shell.ResourceName;

            foreach (ResourceShell item in Main.BlackList)
            {
                if (Shell.Equals(item))
                {
                    InBlackList = true;
                    break;
                }
            }

            return true;
        }

        void GetConstructorAndOutProps(string constructorAndProps)
        {
            int closeConstructor = ParceConstructor(constructorAndProps);
            string tempOutProps;
            if (closeConstructor != -1)
                tempOutProps = constructorAndProps.Substring(closeConstructor + 1,
                    constructorAndProps.Length - closeConstructor - 1);
            else
                tempOutProps = constructorAndProps;
            ParceOutputProps(tempOutProps);
        }

        int ParceConstructor(string input)
        {
            int indexStart = input.IndexOf(OPEN_COLLECTION_CONSTRUCTOR, 0);
            int indexClose = input.LastIndexOf(CLOSE_COLLECTION_CONSTRUCTOR);
            if (indexStart == -1 || indexClose == -1)
                return -1;
            string strProps = input.Substring(indexStart + 1, indexClose - indexStart - 1);
            if (!string.IsNullOrEmpty(strProps))
                GetConstructorParams(strProps);
            return indexClose;
        }

        void GetConstructorParams(string str)
        {
            StringBuilder builder = new StringBuilder();
            int state = 0;
            foreach (char ch in str)
            {
                switch (state)
                {
                    case 0:
                        state = WaitStartEndSimbol(ch, builder, Constructor);
                        break;
                    case 1:
                        state = WaitNextProp(ch, builder, state, Constructor);
                        break;
                }
            }
            if (builder.Length > 0)
            {
                AddConstructorParam(Constructor, builder);
            }
        }


        int WaitStartEndSimbol(char ch, StringBuilder builder, ResourceConstructor props)
        {
            if (_startProp == 0 && ch == OPEN_COSTRUCTOR_PARAM)
            {
                _startProp++;
                builder.Remove(0, builder.Length);
                return 0;
            }

            if (ch == OPEN_COSTRUCTOR_PARAM)
            {
                _indexOfLastCh = builder.Length;
                builder.Append(ch);
                return 1;
            }

            if (_startProp == 0 && ch == COLLECTION_COSTRUCTOR_SEPARATOR && builder.Length > 0)
            {
                AddConstructorParam(props, builder);
                _startProp = 0;
                return 0;
            }

            builder.Append(ch);
            return 0;
        }

        int WaitNextProp(char ch, StringBuilder builder, int state, ResourceConstructor props)
        {
            if (ch == OPEN_COSTRUCTOR_PARAM)
            {
                return WaitStartEndSimbol(ch, builder, props);
            }
            if (ch == COLLECTION_COSTRUCTOR_SEPARATOR)
            {
                AddConstructorParam(props, builder);
                _startProp = 0;
                return 0;
            }
            builder.Append(ch);
            return state;
        }

        void AddConstructorParam(ResourceConstructor props, StringBuilder builder)
        {
            string str = builder.ToString(0, _indexOfLastCh);
            props.Add(str);
            builder.Remove(0, builder.Length);
        }

        void ParceOutputProps(string input)
        {
            if (input.IndexOf(PROPS_SEPARATOR) == -1)
                OutputPath = XPack.DefaultName;
            else
                OutputPath = GetOutputProperties(input);
        }

        public static string GetOutputProperties(string input)
        {
            string resultPath = string.Empty;
            int i = 0;
            foreach (string str in input.Split(PROPS_SEPARATOR))
            {
                i++;
                if (i == 1)
                    continue;
                resultPath = string.Format("{0}/{1}", resultPath, ProtasFunk.TrimString(str));
            }
            if (string.IsNullOrEmpty(resultPath))
                resultPath = XPack.DefaultName;
            return resultPath;
        }

        public override string ToString()
        {
            return Source;
        }

        public void Dispose()
        {

        }
    }
}