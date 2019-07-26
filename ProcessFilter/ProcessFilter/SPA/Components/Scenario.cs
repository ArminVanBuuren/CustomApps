using System.Collections.Generic;
using System.IO;
using System.Xml;
using SPAFilter.SPA.Collection;
using Utils;
using Utils.CollectionHelper;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    public sealed class Scenario : DriveTemplate
    {
        readonly ServiceInstance _parent;

        internal List<Command> Commands { get; } = new List<Command>();
        internal List<Scenario> SubScenarios { get; private set; } = new List<Scenario>();

        [DGVColumn(ColumnPosition.After, "HostType")]
        public string HostTypeName => _parent.HostTypeName;

        [DGVColumn(ColumnPosition.After, "Scenario")]
        public override string Name { get; set; }

        /// <summary>
        /// Количество вложенных сценариев
        /// </summary>
        [DGVColumn(ColumnPosition.Last, "SubScenarios")]
        public int ExistSubScenarios => SubScenarios.Count;

        /// <summary>
        /// Указывает является ли текущий сценарий вложенным
        /// </summary>
        [DGVColumn(ColumnPosition.Last, "IsSubScenario", false)]
        public bool IsSubScenario { get; }

        /// <summary>
        /// Проверка на существование комманд в сценарии
        /// </summary>
        [DGVColumn(ColumnPosition.Last, "AllCommandsExist", false)]
        public bool AllCommandsExist { get; private set; } = true;

        class ParceScenario
        {
            public string ParentPath { get; set; }
            public DistinctList<string> Commands { get; set; }
            public DistinctList<string> SubScenarios { get; set; }
        }

        public Scenario(ServiceInstance parent, string filePath, IReadOnlyDictionary<string, Command> commandList) : base(filePath)
        {
            IsSubScenario = false;
            _parent = parent;

            var prsCs = new ParceScenario
            {
                ParentPath = FilePath,
                Commands = new DistinctList<string>(),
                SubScenarios = new DistinctList<string>()
            };

            CheckScenario(prsCs, commandList);
        }

        Scenario(ServiceInstance parent, string filePath, IReadOnlyDictionary<string, Command> commandList, string parentFileScenario) : base(filePath)
        {
            IsSubScenario = true;
            _parent = parent;

            var prsCs = new ParceScenario
            {
                ParentPath = parentFileScenario,
                Commands = new DistinctList<string>(),
                SubScenarios = new DistinctList<string>()
            };

            CheckScenario(prsCs, commandList);
        }

        void CheckScenario(ParceScenario prsCs, IReadOnlyDictionary<string, Command> commandList)
        {
            ParceXmlScenario(prsCs);

            foreach (var command in prsCs.Commands)
            {
                if (commandList.TryGetValue(command, out var res))
                    Commands.Add(res);
                else
                    AllCommandsExist = false;
            }

            foreach (var subScenarioPath in prsCs.SubScenarios)
            {
                SubScenarios.Add(new Scenario(_parent, subScenarioPath, commandList, FilePath));
            }
        }

        void ParceXmlScenario(ParceScenario prsCs)
        {
            var document = XML.LoadXml(FilePath);
            if (document == null)
                return;

            var commands = EvaluateXPath(document, @"//parameterslist/param[@name='Command']/@value");
            var subScenarios = EvaluateXPath(document, @"//parameterslist/param[@name='Scenario']/@value");

            var i = -1;
            var getParentDirectory = Path.GetDirectoryName(prsCs.ParentPath);
            for (var index = 0; index < subScenarios.Count; index++)
            {
                i++;
                var subScenario = subScenarios[index] + ".xml";

                if (File.Exists(subScenario))
                {
                    subScenarios[i] = subScenario;
                }
                else
                {
                    var subScenarioPath = Path.GetFullPath($"{getParentDirectory}\\{subScenario}");
                    if (File.Exists(subScenarioPath))
                    {
                        subScenarios[i] = subScenarioPath;
                    }
                }
            }

            prsCs.Commands.AddRange(commands);
            prsCs.SubScenarios.AddRange(subScenarios);
        }

        public static List<string> EvaluateXPath(XmlDocument document, string xpath)
        {
            var collection = new List<string>();

            var listByXpath = document.SelectNodes(xpath);
            if (listByXpath == null)
                return collection;

            foreach (XmlNode xm in listByXpath)
            {
                collection.Add(xm.InnerText);
            }
            return collection;
        }
    }
}
