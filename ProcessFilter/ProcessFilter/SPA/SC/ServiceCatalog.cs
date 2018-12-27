using System.Data;
using System.Text;

namespace ProcessFilter.SPA.SC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using Utils.XmlHelper;

    public class ServiceCatalog
    {
        CollectionCFS cfsList;

        public ServiceCatalog(List<NetworkElement> networkElements, DataTable serviceTable = null)
        {
            cfsList = new CollectionCFS(serviceTable);
            foreach (NetworkElement netElement in networkElements)
            {
                foreach (NetworkElementOpartion neOP in netElement.Operations)
                {
                    if (!File.Exists(neOP.FilePath))
                        continue;

                    try
                    {
                        XmlDocument document = XmlHelper.LoadXml(neOP.FilePath);
                        cfsList.Add(document, neOP.NetworkElement);
                    }
                    catch (Exception ex)
                    {
                        // ignored
                    }
                }
            }

            cfsList.GenerateRFS();
        }

        public bool IsValid => cfsList.Count > 0;

        public void Save(string exportFilePath)
        {
            string res =
                @"<Configuration markers=""*"" scenarioPrefix=""SC."" mainIdentities=""MSISDN,PersonalAccountNumber,ContractNumber"" SICreation="""" formatVersion=""1"">
  <HostTypeList />
  <IdentityList />
  <AttributeList />
  <ResourceList />
  <RFSParameterList />
  <FlagList />" + cfsList.ToXml() +
                @"  <CFSGroupList />
  <RFSGroupList />
  <RFSDependencyList />
  <HandlerList />
  <ScenarioList />
  <WfmScenarioList />
</Configuration>";
            using (StreamWriter writer = new StreamWriter($"SC_{DateTime.Now:yyyyMMdd-HHmmss}.xml"))
            {
                writer.Write(res);
                writer.Flush();
            }

            //File.Create(Path.Combine(exportFilePath, ));

                //XmlDocument document = new XmlDocument();
                //document.LoadXml(Properties.Resources.Template.ToString());
                //XPathNavigator navigator = document.CreateNavigator();
                //navigator.MoveToChild("CFSList", "http://www.contoso.com/books");
                //navigator.AppendChild()
            }
    }
}