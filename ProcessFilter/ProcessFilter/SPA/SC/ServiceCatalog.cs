using System;
using System.Data;
using System.IO;
using System.Text;
using Utils;
using Utils.WinForm.CustomProgressBar;

namespace SPAFilter.SPA.SC
{
    public class ServiceCatalog
    {
        readonly CatalogComponents cfsList;

        private NetworkElementCollection NetworkElements { get; }

        public ServiceCatalog(NetworkElementCollection networkElements, DataTable servicesRD, ProgressCalculaterAsync progressCalc)
        {
            NetworkElements = networkElements;

            cfsList = new CatalogComponents(servicesRD);
            foreach (var netElement in networkElements)
            {
                foreach (var neOP in netElement.Operations)
                {
                    if (!File.Exists(neOP.FilePath))
                        continue;

                    var document = XML.LoadXml(neOP.FilePath);
                    cfsList.Add(neOP.Name, document, neOP.NetworkElement);

                    progressCalc++;
                }
            }

            cfsList.GenerateRFS();
        }

        public string Save(string exportFilePath)
        {
            var hostsList = new StringBuilder();

            foreach (var netElem in NetworkElements.AllNetworkElements)
            {
                hostsList.Append("<HostType name=\"");
                hostsList.Append(netElem);
                hostsList.Append("\" nriType=\"RI\" allowTerminalDeviceType=\"mobile\" description=\"-\" />");
            }

            var res = "<Configuration markers=\"*\" scenarioPrefix=\"SC.\" mainIdentities=\"MSISDN,PersonalAccountNumber,ContractNumber\" SICreation=\"\" formatVersion=\"1\">"
                         + "<HostTypeList>" + hostsList + "</HostTypeList>" + cfsList.ToXml() +
                       "</Configuration>";

            res = XML.PrintXml(res);

            var destinationPath = Path.Combine(exportFilePath, $"SC_{DateTime.Now:yyyyMMdd-HHmmss}.xml");
            using (var writer = new StreamWriter(destinationPath))
            {
                writer.Write(res);
                writer.Flush();
            }

            return destinationPath;
        }
    }
}