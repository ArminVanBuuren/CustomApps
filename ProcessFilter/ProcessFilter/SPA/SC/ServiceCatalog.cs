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
        CatalogComponents cfsList;
        private NetworkElementCollection NetworkElements { get; }
        private ProgressBarCompetition _progressComp;
        public ServiceCatalog(NetworkElementCollection networkElements, ProgressBarCompetition progressComp, DataTable serviceTable)
        {
            _progressComp = progressComp;
            NetworkElements = networkElements;
            cfsList = new CatalogComponents(serviceTable);
            foreach (NetworkElement netElement in networkElements)
            {
                foreach (NetworkElementOpartion neOP in netElement.Operations)
                {
                    if (!File.Exists(neOP.FilePath))
                        continue;

                    XmlDocument document = XmlHelper.LoadXml(neOP.FilePath);
                    cfsList.Add(neOP.Name, document, neOP.NetworkElement);
                }
            }

            _progressComp.ProgressValue = 3;
            cfsList.GenerateRFS();
        }

        public bool IsValid => cfsList.Count > 0;

        public void Save(string exportFilePath)
        {
            _progressComp.ProgressValue = 4;
            StringBuilder hostsList = new StringBuilder();
            foreach (string netElem in NetworkElements.AllNetworkElements)
            {
                hostsList.Append($"<HostType name=\"{netElem}\" nriType=\"RI\" allowTerminalDeviceType=\"mobile\" description=\"-\" />");
            }

            string res =
                "<Configuration markers=\"*\" scenarioPrefix=\"SC.\" mainIdentities=\"MSISDN,PersonalAccountNumber,ContractNumber\" SICreation=\"\" formatVersion=\"1\">"
                + "<HostTypeList>" + hostsList.ToString() + "</HostTypeList>" + cfsList.ToXml() +
                "</Configuration>";

            _progressComp.ProgressValue = 5;
            res = RtfFromXml.GetXmlString(res);
            _progressComp.ProgressValue = 6;

            using (StreamWriter writer = new StreamWriter($"SC_{DateTime.Now:yyyyMMdd-HHmmss}.xml"))
            {
                writer.Write(res);
                writer.Flush();
            }
        }
    }
}