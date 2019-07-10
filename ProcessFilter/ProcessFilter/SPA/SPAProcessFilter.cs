using System;
using System.CodeDom;
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
using SPAFilter.SPA.Components.ROBP;
using SPAFilter.SPA.SC;
using Utils;
using Utils.WinForm.CustomProgressBar;

namespace SPAFilter.SPA
{
    public class SPAProcessFilter
    {
        public string ProcessPath { get; private set; }
        public string ROBPHostTypesPath { get; private set; }
        public string SCPath { get; private set; }

        public bool IsEnabledFilter => !ProcessPath.IsNullOrEmpty() && Directory.Exists(ProcessPath) && !ROBPHostTypesPath.IsNullOrEmpty() && Directory.Exists(ROBPHostTypesPath);
        public bool CanGenerateSC => HostTypes != null && HostTypes?.Count > 0;
        public int WholeItemsCount => Processes.Count + HostTypes.Count; //+ Scenarios.Count + Commands.Count;

        public CollectionBusinessProcess Processes { get; } = new CollectionBusinessProcess();
        public CollectionHostType HostTypes { get; } = new CollectionHostType();
        public ServiceCatalog Catalog { get; private set; }
        public CollectionServiceActivator ServiceActivators { get; } = new CollectionServiceActivator();


        public void DataFilter(string filterProcess, string filterHT, string filterOp, ProgressCalculationAsync progressCalc)
        {
            try
            {
                if(!IsEnabledFilter)
                    throw new Exception("Filter is not enabled!");

                Processes.Clear();
                HostTypes.Clear();
                //Scenarios.Clear();
                //Commands.Clear();
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

                FilterProcesses(bpFilter);

                Func<HostType, bool> htFilter = null;
                Func<Operation, bool> opFilter = null;

                #region filter HostType and filter Operations

                if (!filterHT.IsNullOrEmpty())
                {
                    if (filterHT[0] == '%' || filterHT[filterHT.Length - 1] == '%')
                    {
                        var filterNELike = filterHT.Replace("%", "");
                        htFilter = (ht) => ht.Name.IndexOf(filterNELike, StringComparison.CurrentCultureIgnoreCase) != -1;
                    }
                    else
                    {
                        htFilter = (ht) => ht.Name.Equals(filterHT, StringComparison.CurrentCultureIgnoreCase);
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

                FilterROBPOperations(htFilter, opFilter);
                FilterSCOperations(htFilter, opFilter);

                #region remove BusinessProcesses without exists Operations and without SCCall

                var existsOperations = HostTypes.AllOperationsName;
                foreach (var businessProcess in Processes)
                {
                    bool hasMatch = businessProcess.Operations.Any(x => existsOperations.Any(y => x.Equals(y, StringComparison.CurrentCultureIgnoreCase)));
                    if (!hasMatch)
                    {
                        if (Catalog != null && Catalog.Count > 0 && businessProcess.HasCatalogCall)
                            continue;

                        businessProcess.IsFiltered = false;
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


        public async Task AssignProcessesAsync(string processPath)
        {
            ProcessPath = processPath;
            await Task.Factory.StartNew(() => FilterProcesses(null));
        }

        void FilterProcesses(Func<BusinessProcess, bool> bpFilter)
        {
            if (!Directory.Exists(ProcessPath))
                throw new Exception($"Directory \"{ProcessPath}\" not found!");

            Processes.Clear();
            var bpID = 0;
            foreach (var file in GetConfigFiles(ProcessPath))
            {
                if (BusinessProcess.IsBusinessProcess(file, ++bpID, out var result) && (bpFilter == null || bpFilter.Invoke(result)))
                    Processes.Add(result);
                else
                    bpID--;
            }
        }

        public static List<string> GetConfigFiles(string path)
        {
            var files = Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly).ToList();
            files.Sort(StringComparer.CurrentCulture);
            return files;
        }

        public async Task AssignROBPOperationsAsync(string robpHostTypesPath)
        {
            ROBPHostTypesPath = robpHostTypesPath;
            await Task.Factory.StartNew(() => FilterROBPOperations(null, null));
        }

        void FilterROBPOperations(Func<HostType, bool> htFilter, Func<Operation, bool> opFilter)
        {
            if (!Directory.Exists(ROBPHostTypesPath))
                throw new Exception($"Directory \"{ROBPHostTypesPath}\" not found!");

            HostTypes.Clear();
            var files = Directory.GetDirectories(ROBPHostTypesPath).ToList();
            files.Sort(StringComparer.CurrentCulture);
            var neID = 0;
            foreach (var neDirPath in files)
            {
                var robpHt = new ROBPHostType(neDirPath, ++neID);
                FilterOperations(robpHt, htFilter, opFilter);
                HostTypes.Add(robpHt);
            }
        }

        public async Task AssignSCOperationsAsync(string filePath)
        {
            SCPath = filePath;
            await Task.Factory.StartNew(() => FilterSCOperations(null, null));
        }

        void FilterSCOperations(Func<HostType, bool> neFilter, Func<Operation, bool> opFilter)
        {
            if (!File.Exists(SCPath))
                throw new Exception($"File \"{SCPath}\" not found!");

            Catalog = new ServiceCatalog(SCPath);
            if (Catalog.Count == 0)
                return;

            foreach (var ht in Catalog)
            {
                FilterOperations(ht, neFilter, opFilter);
            }
        }

        static void FilterOperations(HostType hostType, Func<HostType, bool> neFilter, Func<Operation, bool> opFilter)
        {
            if (neFilter != null && !neFilter.Invoke(hostType))
            {
                foreach (var op in hostType.Operations)
                {
                    op.IsFiltered = false;
                }
            }
            else if (opFilter != null)
            {
                foreach (var op in hostType.Operations)
                {
                    if (!opFilter.Invoke(op))
                    {
                        op.IsFiltered = false;
                    }
                }
            }
        }

        public async Task AssignActivatorAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new Exception($"File \"{filePath}\" not found!");

            await Task.Factory.StartNew(() => ServiceActivators.Add(filePath));
        }

        public async Task RemoveActivatorAsync(List<string> filePath)
        {
            await Task.Factory.StartNew(() => ServiceActivators.Remove(filePath));
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
                var sc = new ServiceCatalogBuilder(HostTypes, rdServices, progressCalc);
                return sc.Save(exportFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public void PrintXML(CustomStringBuilder stringErrors, ProgressCalculationAsync progrAsync)
        {
            GetFiles(Processes, stringErrors, progrAsync);
            GetFiles(HostTypes.AllOperations, stringErrors, progrAsync);
            //GetFiles(Scenarios, stringErrors, progrAsync);
            //GetFiles(Commands, stringErrors, progrAsync);
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
    }
}