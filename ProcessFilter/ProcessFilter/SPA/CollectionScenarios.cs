using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA
{
    public class CollectionScenarios : List<Scenario>
    {
        internal CollectionScenarios()
        {
            
        }

        public CollectionScenarios(string path)
        {
            //List<string> files = Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories).OrderBy(c => c).ToList();
            List<string> files = Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly).ToList();
            files.Sort(StringComparer.CurrentCulture);

            int i = 0;
            foreach (string bpPath in files)
            {
                Add(new Scenario(bpPath, ++i));
            }
        }

        public CollectionScenarios Clone()
        {
            CollectionScenarios clone = new CollectionScenarios();
            clone.AddRange(this);
            return clone;
        }
    }

    class ItemEqualityComparer : IEqualityComparer<Scenario>
    {
        public bool Equals(Scenario x, Scenario y)
        {
            return x.Name.Trim().Equals(y.Name.Trim(), StringComparison.CurrentCultureIgnoreCase);
        }

        public int GetHashCode(Scenario obj)
        {
            return obj.Name.ToLower().GetHashCode();
        }
    }

    public class Scenario : ObjectTempalte
    {
        internal List<string> Commands { get; private set; } = new List<string>();
        internal CollectionScenarios SubScenarios { get; private set; } = new CollectionScenarios();

        [DGVEnhancer.DGVColumnAttribute(DGVEnhancer.ColumnPosition.Before, "Scenario")]
        public override string Name { get; protected set; }

        [DGVEnhancer.DGVColumnAttribute(DGVEnhancer.ColumnPosition.After, "SubScenario Count")]
        public int ExistSubScenarios => SubScenarios.Count;

        

        public Scenario(string path, int id) : base(path, id)
        {

        }

        public bool AddBodyCommands()
        {
            Commands.Clear();
            SubScenarios.Clear();

            ParceScenario allfinded = ParceXmlScenario(FilePath);
            if (allfinded == null)
                return false;

            Commands = allfinded.Commands.Distinct().ToList();
            foreach (string subScenarioPath in allfinded.SubScenarios.Distinct().ToList())
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

            XmlDocument document = XML.LoadXml(scenarioPath, true);
            if (document == null)
                return null;

            ParceScenario prsCs = new ParceScenario();
            prsCs.Commands = EvaluateXPath(document, @"//parameterslist/param[@name='command']/@value");
            prsCs.SubScenarios = EvaluateXPath(document, @"//parameterslist/param[@name='scenario']/@value");

            int i = -1;
            string getParentDirectory = FilePath.GetParentDirectoryInPath();
            for (var index = 0; index < prsCs.SubScenarios.Count; index++)
            {
                string subScenario = prsCs.SubScenarios[index];
                i++;
                string subScenarioPath = subScenario + ".xml";
                if (File.Exists(subScenario))
                {
                    prsCs.SubScenarios[i] = subScenarioPath;
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

                ParceScenario getSub = ParceXmlScenario(subScenarioPath);
                if (getSub == null)
                    continue;

                prsCs.Commands.AddRange(getSub.Commands);
                prsCs.SubScenarios.AddRange(getSub.SubScenarios);
            }
            return prsCs;
        }

        public static List<string> EvaluateXPath(XmlDocument document, string xpath)
        {
            List<string> collection = new List<string>();
            XmlNodeList listByXpath = document.SelectNodes(xpath);
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
