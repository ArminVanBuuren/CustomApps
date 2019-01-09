using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using Utils.WinForm.CustomProgressBar;
using Utils.XmlHelper;
using Utils.XmlRtfStyle;

namespace SPAFilter.SPA.SC
{
    public class ServiceCatalog
    {
        CatalogComponents cfsList;
        private NetworkElementCollection NetworkElements { get; }
        private ProgressBarCompetition<string> _progressComp;
        public ServiceCatalog(NetworkElementCollection networkElements, ProgressBarCompetition<string> progressComp, DataTable serviceTable)
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

        public string Save(string exportFilePath)
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

            string destinationPath = Path.Combine(exportFilePath, $"SC_{DateTime.Now:yyyyMMdd-HHmmss}.xml");
            using (StreamWriter writer = new StreamWriter(destinationPath))
            {
                writer.Write(res);
                writer.Flush();
            }

            return destinationPath;
        }
    }
}