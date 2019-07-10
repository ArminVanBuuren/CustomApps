using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SPAFilter.SPA.Collection;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    public class Scenario : ObjectTemplate
    {
        internal List<string> Commands { get; private set; } = new List<string>();
        internal CollectionScenarios SubScenarios { get; private set; } = new CollectionScenarios();

        [DGVColumn(ColumnPosition.Before, "Scenario")]
        public override string Name { get; protected set; }

        [DGVColumn(ColumnPosition.After, "SubScenarios")]
        public int ExistSubScenarios => SubScenarios.Count;


        public Scenario(string path, int id) : base(path, id)
        {

        }

        public bool AddBodyCommands()
        {
            Commands.Clear();
            SubScenarios.Clear();

            var allfinded = ParceXmlScenario(FilePath);
            if (allfinded == null)
                return false;

            Commands = allfinded.Commands.Distinct().ToList();
            foreach (string subScenarioPath in allfinded.SubScenarios.Distinct())
            {
                SubScenarios.Add(new Scenario(subScenarioPath, -1));
            }

            return true;
        }



        class ParceScenario
        {
            public List<string> Commands { get; set; }
            public List<string> SubScenarios { get; set; }
        }


        ParceScenario ParceXmlScenario(string scenarioPath)
        {
            if (scenarioPath.IsNullOrEmpty())
                return null;

            var document = XML.LoadXml(scenarioPath, true);
            if (document == null)
                return null;

            var prsCs = new ParceScenario
            {
                Commands = EvaluateXPath(document, @"//parameterslist/param[@name='command']/@value"),
                SubScenarios = EvaluateXPath(document, @"//parameterslist/param[@name='scenario']/@value")
            };

            var i = -1;
            var getParentDirectory = IO.GetLastNameInPath(FilePath);
            for (var index = 0; index < prsCs.SubScenarios.Count; index++)
            {
                string subScenario = prsCs.SubScenarios[index];
                i++;
                string subScenarioPath = subScenario + ".xml";
                if (File.Exists(subScenario))
                {
                    prsCs.SubScenarios[i] = subScenario;
                }
                else
                {
                    subScenarioPath = Path.GetFullPath($"{getParentDirectory}\\{subScenario}.xml");
                    if (File.Exists(subScenarioPath))
                    {
                        prsCs.SubScenarios[i] = subScenarioPath;
                    }
                    else
                    {
                        continue;
                    }
                }

                var getSub = ParceXmlScenario(subScenarioPath);
                if (getSub == null)
                    continue;

                prsCs.Commands.AddRange(getSub.Commands);
                prsCs.SubScenarios.AddRange(getSub.SubScenarios);
            }
            return prsCs;
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
