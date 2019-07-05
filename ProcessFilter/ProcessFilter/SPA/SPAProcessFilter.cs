using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using OfficeOpenXml;
using SPAFilter.SPA.Collection;
using SPAFilter.SPA.Components;
using SPAFilter.SPA.SC;
using Utils;
using Utils.WinForm.CustomProgressBar;

namespace SPAFilter.SPA
{
    public enum SPAProcessFilterType
    {
        Processes = 0,
        NetElements = 1,
        ServiceCatalogOperations = 2,
        Activators_Add = 4,
        Activators_Remove = 8
    }

    

    public class SPAProcessFilter
    {
        public string ProcessPath { get; private set; }
        public string NetElementsPath { get; private set; }
        public XmlDocument ServiceCatalog { get; private set; }
        public List<string> Activators { get; private set; } = new List<string>();

        public bool IsEnabledFilter => !ProcessPath.IsNullOrEmpty() && Directory.Exists(ProcessPath) && !NetElementsPath.IsNullOrEmpty() && Directory.Exists(NetElementsPath);
        public bool CanGenerateSC => NetElements != null && NetElements?.Count > 0;
        public int WholeItemsCount => Processes.Count + NetElements.Count + Scenarios.Count + Commands.Count;

        public CollectionBusinessProcess Processes { get; } = new CollectionBusinessProcess();
        public CollectionNetworkElement NetElements { get; } = new CollectionNetworkElement();
        public CollectionScenarios Scenarios { get; } = new CollectionScenarios();
        public CollectionCommands Commands { get; } = new CollectionCommands();


        public bool Assign(string path, SPAProcessFilterType type)
        {
            var trimPath = path.Trim(' ');
            switch (type)
            {
                case SPAProcessFilterType.Processes:
                    ProcessPath = Directory.Exists(trimPath) ? trimPath : null;
                    break;
                case SPAProcessFilterType.NetElements:
                    NetElementsPath = Directory.Exists(trimPath) ? trimPath : null;
                    break;
                case SPAProcessFilterType.ServiceCatalogOperations when File.Exists(trimPath):
                    if (XML.IsFileXml(trimPath, out var documnet))
                        ServiceCatalog = documnet;
                    else
                    {
                        ServiceCatalog = null;
                        
                    }
                    break;
                case SPAProcessFilterType.Activators_Add when File.Exists(trimPath):
                    Activators.Add(trimPath);
                    break;
                case SPAProcessFilterType.Activators_Remove:
                    Activators.Remove(trimPath);
                    break;
                default: return false;
            }
            return true;
        }

        public static List<string> GetConfigFiles(string path)
        {
            var files = Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly).ToList();
            files.Sort(StringComparer.CurrentCulture);
            return files;
        }

        public void PrintXML(CustomStringBuilder stringErrors, ProgressCalculationAsync progrAsync)
        {
            GetFiles(Processes, stringErrors, progrAsync);
            GetFiles(NetElements.AllOperations, stringErrors, progrAsync);
            GetFiles(Scenarios, stringErrors, progrAsync);
            GetFiles(Commands, stringErrors, progrAsync);
        }

        static void GetFiles(IEnumerable<ObjectTemplate> fileObj, CustomStringBuilder stringErrors, ProgressCalculationAsync progressCalc)
        {
            if (fileObj == null || !fileObj.Any())
                return;

            foreach (var file in fileObj)
            {
                FormattingXML(file.FilePath, stringErrors);
                progressCalc++;
            }
        }

        static void FormattingXML(string filePath, CustomStringBuilder stringErrors)
        {
            try
            {
                var fileString = IO.SafeReadFile(filePath);
                if (!fileString.IsXml(out var document))
                    return;

                var formatting = document.PrintXml();

                var attempts = 0;
                while (!IO.IsFileReady(filePath))
                {
                    System.Threading.Thread.Sleep(500);
                    attempts++;
                    if (attempts > 3)
                        return;
                }

                IO.WriteFile(filePath, formatting, Encoding.UTF8);
            }
            catch (Exception e)
            {
                stringErrors.AppendLine($"Message=\"{e.Message}\" File=\"{filePath}]\"");
            }
        }

        public void DataFilter(string filterProcess, string filterNE, string filterOp, ProgressCalculationAsync progressCalc)
        {
            try
            {
                if(!IsEnabledFilter)
                    throw new Exception("Filter is not enabled!");

                Processes.Clear();
                NetElements.Clear();
                Scenarios.Clear();
                Commands.Clear();
                var filteredSubScenarios = new List<Scenario>();


                Func<BusinessProcess, bool> bpFilter = null;

                #region filter BusinessProcesses

                if (!filterProcess.IsNullOrEmpty())
                {
                    if (filterProcess[0] == '%' || filterProcess[filterProcess.Length - 1] == '%')
                    {
                        var filterProcessLike = filterProcess.Replace("%", "");
                        bpFilter = (bp) => bp.Name.IndexOf(filterProcessLike, StringComparison.CurrentCultureIgnoreCase) != -1;
                    }
                    else
                    {
                        bpFilter = (bp) => bp.Name.Equals(filterProcess, StringComparison.CurrentCultureIgnoreCase);
                    }
                }

                #endregion

                AssignBP(ProcessPath, Processes, bpFilter);

                Func<NetworkElement, bool> neFilter = null;
                Func<Operation, bool> opFilter = null;

                #region filter NetworkElements and filter Operations

                if (!filterNE.IsNullOrEmpty())
                {
                    if (filterNE[0] == '%' || filterNE[filterNE.Length - 1] == '%')
                    {
                        var filterNELike = filterNE.Replace("%", "");
                        neFilter = (ne) => ne.Name.IndexOf(filterNELike, StringComparison.CurrentCultureIgnoreCase) != -1;
                    }
                    else
                    {
                        neFilter = (ne) => ne.Name.Equals(filterNE, StringComparison.CurrentCultureIgnoreCase);
                    }
                }

                if (!filterOp.IsNullOrEmpty())
                {
                    if (filterOp[0] == '%' || filterOp[filterOp.Length - 1] == '%')
                    {
                        var filterOPLike = filterOp.Replace("%", "");
                        opFilter = (op) => op.Name.IndexOf(filterOPLike, StringComparison.CurrentCultureIgnoreCase) != -1;
                    }
                    else
                    {
                        opFilter = (op) => op.Name.Equals(filterOp, StringComparison.CurrentCultureIgnoreCase);
                    }
                }

                #endregion

                AssignNE(NetElementsPath, NetElements, neFilter, opFilter);

                if (ServiceCatalog != null)
                {

                }

                #region remove BusinessProcesses without exists Operations

                var endOfBpCollection = Processes.Count;
                var existsOperations = NetElements.AllOperationsName;
                for (int i = 0; i < endOfBpCollection; i++)
                {
                    var businessProcess = Processes[i];

                    bool hasMatch = businessProcess.Operations.Any(x => existsOperations.Any(y => x.Equals(y, StringComparison.CurrentCultureIgnoreCase)));
                    if (!hasMatch)
                    {
                        Processes.Remove(businessProcess);
                        i--;
                        endOfBpCollection--;
                    }
                }

                #endregion

                progressCalc.Append(2);

                //Action<Operation> getScenario = null;
                //if (Scenarios != null)
                //{
                //    getScenario = operation =>
                //    {
                //        var scenarios = Scenarios.Where(p => p.Name.Equals(operation.Name, StringComparison.CurrentCultureIgnoreCase)).ToList();

                //        foreach (Scenario scenario in scenarios)
                //        {
                //            if (scenario.AddBodyCommands())
                //            {
                //                _filteredScenarioCollection.Add(scenario);
                //                filteredSubScenarios.AddRange(scenario.SubScenarios);
                //            }
                //        }
                //    };
                //}

                //int maxForCurrentIterator = progressCalc.TotalProgressIterator / 2;
                //if (_filteredNetElemCollection != null && _filteredNetElemCollection.Count > 0)
                //{
                //    int progressIterator;

                //    Action calcProgress;
                //    if (_filteredNetElemCollection.Count > maxForCurrentIterator)
                //    {
                //        int numberOfIter = 0;
                //        progressIterator = _filteredNetElemCollection.Count / maxForCurrentIterator;
                //        calcProgress = () =>
                //        {
                //            numberOfIter++;
                //            if (numberOfIter < progressIterator)
                //                return;
                //            progressCalc++;
                //            numberOfIter = 0;
                //        };
                //    }
                //    else
                //    {
                //        progressIterator = maxForCurrentIterator / _filteredNetElemCollection.Count;
                //        calcProgress = () => progressCalc.Append(progressIterator);
                //    }

                //    foreach (var netElem in _filteredNetElemCollection)
                //    {
                //        int endOfOpCollection = netElem.Operations.Count;
                //        for (int i = 0; i < endOfOpCollection; i++)
                //        {
                //            bool hasMatch = _filteredProcessCollection.Any(x => x.Operations.Any(y => netElem.Operations[i].Name.Equals(y, StringComparison.CurrentCultureIgnoreCase)));

                //            if (hasMatch)
                //            {
                //                getScenario?.Invoke(netElem.Operations[i]);
                //                continue;
                //            }

                //            netElem.Operations.Remove(netElem.Operations[i]);
                //            i--;
                //            endOfOpCollection--;
                //        }

                //        calcProgress.Invoke();
                //    }
                //}
                //else
                //{
                //    progressCalc.Append(maxForCurrentIterator);
                //}



                //if (Commands != null && _filteredScenarioCollection.Count > 0)
                //{
                //    foreach (Command command in Commands)
                //    {
                //        bool hasValue = _filteredScenarioCollection.Any(x => x.Commands.Any(y => y.Equals(command.Name, StringComparison.CurrentCultureIgnoreCase)));
                //        if (hasValue)
                //        {
                //            _filteredCMMCollection.Add(command);
                //        }
                //    }
                //}


                //progressCalc.Append(2);

                //filteredSubScenarios = filteredSubScenarios.Distinct(new ItemEqualityComparer<Scenario>()).ToList();
                //_filteredScenarioCollection.AddRange(filteredSubScenarios);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        static void AssignBP(string processPath, CollectionBusinessProcess processes, Func<BusinessProcess, bool> bpFilter)
        {
            var bpID = 0;
            foreach (var file in GetConfigFiles(processPath))
            {
                if (BusinessProcess.IsBusinessProcess(file, ++bpID, out var result) && (bpFilter == null || bpFilter.Invoke(result)))
                {
                    processes.Add(result);
                }
                else
                {
                    bpID--;
                }
            }
        }

        static void AssignNE(string netElementsPath, CollectionNetworkElement networkElements, Func<NetworkElement, bool> neFilter, Func<Operation, bool> opFilter)
        {
            var files = Directory.GetDirectories(netElementsPath).ToList();
            files.Sort(StringComparer.CurrentCulture);
            var neID = 0;
            foreach (var neDirPath in files)
            {
                var ne = new NetworkElement(neDirPath, ++neID);
                if (neFilter != null && !neFilter.Invoke(ne))
                {
                    neID--;
                    continue;
                }

                if (opFilter != null)
                {
                    var ops = new List<Operation>();
                    foreach (var op in ne.Operations)
                    {
                        if (opFilter.Invoke(op))
                            ops.Add(op);
                    }

                    if (ops.Count <= 0)
                    {
                        neID--;
                        continue;
                    }

                    ne = new NetworkElement(neDirPath, neID, ops);
                }

                networkElements.Add(ne);
            }
        }


        private readonly string[] mandatoryXslxColumns = new string[] { "#", "SPA_SERVICE_CODE", "GLOBAL_SERVICE_CODE", "SERVICE_NAME", "SERVICE_FULL_NAME", "SERVICE_FULL_NAME2", "DESCRIPTION", "SERVICE_CODE", "SERVICE_NAME2", "EXTERNAL_CODE", "EXTERNAL_CODE2" };

        public DataTable GetRDServicesFromXslx(FileInfo file, CustomProgressCalculation progressCalc)
        {
            var serviceTable = new DataTable();

            progressCalc.BeginOpenXslxFile();

            using (var xslPackage = new ExcelPackage(file))
            {
                progressCalc.BeginReadXslxFile();

                var myWorksheet = xslPackage.Workbook.Worksheets.First(); //sheet
                var totalRows = myWorksheet.Dimension.End.Row;
                var totalColumns = mandatoryXslxColumns.Length;

                progressCalc.EndReadXslxFile(totalRows);

                var columnsNames = myWorksheet.Cells[1, 1, 1, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());

                if (!columnsNames.Any())
                    return null;

                var i = 0;
                foreach (var columnName in columnsNames)
                {
                    var columnNameUp = columnName.ToUpper();
                    if (mandatoryXslxColumns[i++] != columnNameUp)
                    {
                        throw new Exception($"Wrong column name before \'{columnNameUp}\' from file '{file.Name}'.\r\n\r\nColumns names and orders must be like:\r\n'{string.Join("','", mandatoryXslxColumns)}'");
                    }

                    serviceTable.Columns.Add(columnNameUp, typeof(string));
                    if (i == mandatoryXslxColumns.Length)
                        break;
                }

                if (i != mandatoryXslxColumns.Length)
                    throw new Exception($"Wrong file '{file.Name}'. Missing some required columns. \r\nColumns names should be like:\r\n'{string.Join("','", mandatoryXslxColumns)}'");

                for (int rowNum = 2; rowNum <= totalRows; rowNum++)
                {
                    var row = myWorksheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).Take(totalColumns);
                    serviceTable.Rows.Add(values: row.ToArray());
                    progressCalc.ReadXslxFileLine();
                }
            }

            progressCalc.EndOpenXslxFile();

            return serviceTable;
        }

        public string GetServiceCatalog(DataTable rdServices, string exportFilePath, CustomProgressCalculation progressCalc)
        {
            try
            {
                var sc = new ServiceCatalogBuilder(NetElements, rdServices, progressCalc);
                return sc.Save(exportFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}