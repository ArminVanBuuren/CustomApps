using System;
using System.Data;
using System.IO;
using System.Text;
using SPAFilter.SPA.Collection;
using Utils;
using Utils.WinForm.CustomProgressBar;

namespace SPAFilter.SPA.SC
{
    public struct ServiceCatalogBuilder
    {
        public string Configuration { get; }

        public ServiceCatalogBuilder(CollectionNetworkElement networkElements, DataTable servicesRD, CustomProgressCalculation progressCalc)
        {
            progressCalc.AddBootPercent(3);

            var cfsList = new CatalogComponents(servicesRD);

            foreach (var netElement in networkElements)
            {
                foreach (var neOP in netElement.Operations)
                {
                    if (!File.Exists(neOP.FilePath))
                        continue;

                    var document = XML.LoadXml(neOP.FilePath);
                    cfsList.Add(neOP.Name, document, neOP.NetworkElement);

                    progressCalc.Append();
                }
            }

            cfsList.GenerateRFS();

            var scConfig = new StringBuilder();
            scConfig.Append("<Configuration markers=\"*\" scenarioPrefix=\"SC.\" mainIdentities=\"MSISDN,PersonalAccountNumber,ContractNumber\" SICreation=\"\" formatVersion=\"1\">");
            scConfig.Append("<HostTypeList>");
            foreach (var netElem in networkElements.AllNetworkElements)
            {
                scConfig.Append("<HostType name=\"");
                scConfig.Append(netElem);
                scConfig.Append("\" nriType=\"RI\" allowTerminalDeviceType=\"mobile\" description=\"-\" />");
            }
            scConfig.Append("</HostTypeList>");
            scConfig.Append(cfsList.ToXml());
            scConfig.Append("</Configuration>");

            Configuration = XML.PrintXml(scConfig.ToString());

            if (progressCalc.CurrentProgressIterator < progressCalc.TotalProgressIterator)
                progressCalc.Append(progressCalc.TotalProgressIterator - progressCalc.CurrentProgressIterator);
        }

        public string Save(string exportDirectory)
        {
            if (!Directory.Exists(exportDirectory))
                Directory.CreateDirectory(exportDirectory);

            var destinationPath = Path.Combine(exportDirectory, $"SC_{DateTime.Now:yyyyMMdd-HHmmss}.xml");
            using (var writer = new StreamWriter(destinationPath))
            {
                writer.Write(Configuration);
                writer.Flush();
            }

            return destinationPath;
        }
    }
}