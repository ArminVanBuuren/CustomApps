using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using BigMath.Utils;
using SPAFilter.SPA.Collection;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    public sealed class Scenario : ObjectTemplate
    {
        readonly ServiceInstance _parent;

        internal List<Command> Commands { get; } = new List<Command>();
        internal List<Scenario> SubScenarios { get; private set; } = new List<Scenario>();

        [DGVColumn(ColumnPosition.Before, "HostType")]
        public string HostTypeName => _parent.HostTypeName;

        [DGVColumn(ColumnPosition.After, "Scenario")]
        public override string Name { get; protected set; }

        [DGVColumn(ColumnPosition.After, "SubScenarios")]
        public int ExistSubScenarios => SubScenarios.Count;

        class ParceScenario
        {
            public string ParentPath { get; set; }
            public DistinctList<string> Commands { get; set; }
            public DistinctList<string> SubScenarios { get; set; }
        }

        public Scenario(ServiceInstance parent, string filePath, IReadOnlyDictionary<string, Command> commandList) : base(filePath)
        {
            _parent = parent;

            var prsCs = new ParceScenario
            {
                ParentPath = FilePath,
                Commands = new DistinctList<string>(),
                SubScenarios = new DistinctList<string>()
            };

            CheckScenario(prsCs, commandList);
        }

        public Scenario(ServiceInstance parent, string filePath, IReadOnlyDictionary<string, Command> commandList, string parentFileScenario) : base(filePath)
        {
            ID = -1;

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
            }

            foreach (var subScenarioPath in prsCs.SubScenarios)
            {
                //Path.GetDirectoryName(FilePath)
                SubScenarios.Add(new Scenario(_parent, subScenarioPath, commandList, FilePath));
            }
        }

        void ParceXmlScenario(ParceScenario prsCs)
        {
            var document = XML.LoadXml(FilePath, true);
            if (document == null)
                return;

            var commands = EvaluateXPath(document, @"//parameterslist/param[@name='command']/@value");
            var subScenarios = EvaluateXPath(document, @"//parameterslist/param[@name='scenario']/@value");

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
                //if (!collection.Any(p => p.Equals(xm.InnerText)))
                collection.Add(xm.InnerText);
            }
            return collection;
        }
    }
}
