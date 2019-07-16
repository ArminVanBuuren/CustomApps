using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        readonly List<ServiceActivator> _activators = new List<ServiceActivator>();

        public string ProcessPath { get; private set; }
        public string ROBPHostTypesPath { get; private set; }
        public string SCPath { get; private set; }

        public bool IsEnabledFilter => (Processes?.Count ?? 0) > 0 && (HostTypes?.OperationsCount ?? 0) > 0;
        public bool CanGenerateSC => (HostTypes?.OperationsCount ?? 0) > 0 && !(HostTypes is ServiceCatalog);
        public int WholeItemsCount => (Processes?.Count ?? 0) + (HostTypes?.OperationsCount ?? 0) + (ServiceInstances?.Count ?? 0) + (Scenarios?.Count ?? 0) + (Commands?.Count ?? 0);

        public CollectionTemplate<ServiceInstance> ServiceInstances { get; private set; }

        public CollectionBusinessProcess Processes { get; private set; }
        public CollectionHostType HostTypes { get; private set; }
        public CollectionTemplate<Scenario> Scenarios { get; private set; }
        public CollectionTemplate<Command> Commands { get; private set; }

        public void DataFilter(string filterProcess, string filterHT, string filterOp, bool filterInROBP, ProgressCalculationAsync progressCalc)
        {
            try
            {
                if(!IsEnabledFilter)
                    throw new Exception("Filter is not enabled!");

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

                progressCalc.Append(2);

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

                if (filterInROBP)
                    FilterROBPOperations(htFilter, opFilter);
                else
                    FilterSCOperations(htFilter, opFilter);

                #region remove BusinessProcesses without exists Operations and without SCCall

                var isCatalog = HostTypes is ServiceCatalog;
                var existsOperations = HostTypes.OperationNames;
                for (var index = 0; index < Processes.Count; index++)
                {
                    var businessProcess = Processes[index];
                    var hasMatch = businessProcess.Operations.Any(x => existsOperations.Any(y => x.Equals(y, StringComparison.CurrentCultureIgnoreCase)));
                    if (!hasMatch)
                    {
                        if (isCatalog && HostTypes.Count > 0 && businessProcess.HasCatalogCall)
                            continue;

                        Processes.Remove(businessProcess);
                        index--;
                    }
                }

                #endregion

                progressCalc.Append(2);

                SIRefresh();

                Scenarios = CollectionTemplate<Scenario>.ToCollection(Scenarios.Intersect(HostTypes.Operations, new OperationsComparer()).Cast<Scenario>().OrderBy(p => p.HostTypeName).ThenBy(p => p.Name));

                progressCalc.Append(2);

                var filteredSubScenarios = new DistinctList<Scenario>();
                var filteredCommands  = new DistinctList<Command>();
                GetCommandsAndSubs(Scenarios, filteredSubScenarios, filteredCommands);
                Scenarios.AddRange(filteredSubScenarios.OrderBy(p => p.FilePath));

                Commands = CollectionTemplate<Command>.ToCollection(filteredCommands.OrderBy(p => p.HostTypeName).ThenBy(p => p.Name));

                progressCalc.Append(2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void GetCommandsAndSubs(IEnumerable<Scenario> scenarios, DistinctList<Scenario> subScenarios, DistinctList<Command> commandsResult)
        {
            foreach (var scenario in scenarios)
            {
                if (scenario.SubScenarios.Count > 0)
                {
                    subScenarios.AddRange(scenario.SubScenarios);
                    GetCommandsAndSubs(scenario.SubScenarios, subScenarios, commandsResult);
                }
                commandsResult.AddRange(scenario.Commands);
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

            Processes = new CollectionBusinessProcess();
            foreach (var file in GetConfigFiles(ProcessPath))
            {
                if (BusinessProcess.IsBusinessProcess(file, out var result) && (bpFilter == null || bpFilter.Invoke(result)))
                    Processes.Add(result);
            }
            Processes.InitId();
        }

        public static List<string> GetConfigFiles(string path, string mask = "*.xml")
        {
            var files = Directory.GetFiles(path, mask, SearchOption.TopDirectoryOnly).ToList();
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

            HostTypes = new CollectionHostType(); 
            var files = Directory.GetDirectories(ROBPHostTypesPath).ToList();
            files.Sort(StringComparer.CurrentCulture);
            var allBPOperations = Processes.AllOperationsNames;

            foreach (var neDirPath in files)
            {
                var robpHostType = new ROBPHostType(neDirPath);
                FilterOperations(robpHostType, htFilter, opFilter, allBPOperations, false);
                HostTypes.Add(robpHostType);
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

            HostTypes = new ServiceCatalog(SCPath);
            if (HostTypes.Count == 0)
                return;
            var anyHasCatalogCall = Processes.AnyHasCatalogCall;

            foreach (var hostType in HostTypes)
            {
                FilterOperations(hostType, neFilter, opFilter, null, anyHasCatalogCall);
            }
        }

        static void FilterOperations(HostType hostType, Func<HostType, bool> neFilter, Func<Operation, bool> opFilter, IReadOnlyDictionary<string, string> allBPOperations, bool AnyHasCatalogCall)
        {
            if (neFilter != null && !neFilter.Invoke(hostType))
            {
                hostType.Operations.Clear();
                return;
            }
            else if (allBPOperations != null)
            {
                if (opFilter == null)
                    opFilter = (op) => true;

                for (var index = 0; index < hostType.Operations.Count; index++)
                {
                    var operation = hostType.Operations[index];
                    if (!allBPOperations.ContainsKey(operation.Name) || !opFilter.Invoke(operation))
                    {
                        hostType.Operations.Remove(operation);
                        index--;
                    }
                }
            }
            else if (!AnyHasCatalogCall)
            {
                hostType.Operations.Clear();
                return;
            }
            else if (opFilter != null)
            {
                for (var index = 0; index < hostType.Operations.Count; index++)
                {
                    var operation = hostType.Operations[index];
                    if (!opFilter.Invoke(operation))
                    {
                        hostType.Operations.Remove(operation);
                        index--;
                    }
                }
            }
        }

        public async Task AssignActivatorAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new Exception($"File \"{filePath}\" not found!");

            await Task.Factory.StartNew(() =>
            {
                if (_activators.Any(x => x.FilePath.Equals(filePath, StringComparison.CurrentCultureIgnoreCase)))
                {
                    MessageBox.Show($"Activator \"{filePath}\" already exist.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _activators.Add(new ServiceActivator(filePath));

                SIReinitialization();
            });
        }

        public async Task RemoveActivatorAsync(List<string> filePathList)
        {
            await Task.Factory.StartNew(() =>
            {
                foreach (var filePath in filePathList)
                {
                    for (var index = 0; index < _activators.Count; index++)
                    {
                        var activator = _activators[index];
                        if (!activator.FilePath.Equals(filePath, StringComparison.CurrentCultureIgnoreCase))
                            continue;

                        _activators.Remove(activator);
                        break;
                    }
                }

                SIReinitialization();
            });
        }

        void SIRefresh()
        {
            foreach (var activator in _activators)
            {
                activator.Refresh();
            }
            SIReinitialization();
        }

        void SIReinitialization()
        {
            ServiceInstances = GetServiceInstances();
            ServiceInstances.InitId();
            GetScenariosAndCommands(GetServiceInstances(true), out var allScenarios, out var allCommands);
            Scenarios = allScenarios;
            Commands = allCommands;
        }

        CollectionTemplate<ServiceInstance> GetServiceInstances(bool getValid = false)
        {
            var intsances = new CollectionTemplate<ServiceInstance>();
            foreach (var instance in _activators.SelectMany(p => p.Instances).OrderBy(p => p.HostTypeName).ThenBy(p => p.FilePath))
            {
                if (getValid && !instance.IsCorrect)
                    continue;
                intsances.Add(instance);
            }

            return intsances;
        }

        static void GetScenariosAndCommands(IEnumerable<ServiceInstance> serviceInstances, out CollectionTemplate<Scenario> resultScenarios, out CollectionTemplate<Command> resultCommands)
        {
            var allScenarios = new DistinctList<Scenario>();
            var allCommands = new DistinctList<Command>();
            foreach (var instance in serviceInstances)
            {
                foreach (var scenario in instance.Scenarios)
                {
                    allScenarios.Add(scenario);
                    allCommands.AddRange(scenario.Commands);
                }
            }

            resultScenarios = CollectionTemplate<Scenario>.ToCollection(allScenarios);
            resultCommands = CollectionTemplate<Command>.ToCollection(allCommands);
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
                if (HostTypes is ServiceCatalog)
                    throw new Exception("You can create Service Catalog only with ROBP operations.");

                var sc = new ServiceCatalogBuilder(HostTypes, rdServices, progressCalc);
                return sc.Save(exportFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                if (progressCalc.CurrentProgressIterator < progressCalc.TotalProgressIterator)
                    progressCalc.Append(progressCalc.TotalProgressIterator - progressCalc.CurrentProgressIterator);
            }
        }

        public void PrintXML(CustomStringBuilder stringErrors, ProgressCalculationAsync progrAsync)
        {
            GetFiles(Processes, stringErrors, progrAsync);
            GetFiles(HostTypes.Operations, stringErrors, progrAsync);
            GetFiles(ServiceInstances, stringErrors, progrAsync);
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
    }
}