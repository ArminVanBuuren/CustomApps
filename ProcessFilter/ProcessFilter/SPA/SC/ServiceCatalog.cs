using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Text;
using Utils.XmlHelper;
using Utils.XmlRtfStyle;

namespace ProcessFilter.SPA.SC
{
    public class ServiceCatalog
    {
        CollectionCFS cfsList;
        private NetworkElementCollection NetworkElements { get; }

        public ServiceCatalog(NetworkElementCollection networkElements, DataTable serviceTable = null)
        {
            NetworkElements = networkElements;
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
                        cfsList.Add(Path.GetFileName(neOP.FilePath), document, neOP.NetworkElement);
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
            StringBuilder hostsList = new StringBuilder();
            foreach (string netElem in NetworkElements.AllNetworkElements)
            {
                hostsList.Append($"<HostType name=\"{netElem}\" nriType=\"RI\" allowTerminalDeviceType=\"mobile\" description=\"-\" />");
            }

            string res =
                "<Configuration markers=\"*\" scenarioPrefix=\"SC.\" mainIdentities=\"MSISDN,PersonalAccountNumber,ContractNumber\" SICreation=\"\" formatVersion=\"1\">"
                + "<HostTypeList>" + hostsList.ToString() + "</HostTypeList>" + cfsList.ToXml() +
                "</Configuration>";


            res = RtfFromXml.GetXmlString(res);

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