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
        public bool CanGenerateSC => (HostTypes?.DriveOperationsCount ?? 0) > 0 && !(HostTypes is ServiceCatalog);
        public int WholeDriveItemsCount => (Processes?.Count ?? 0) + (HostTypes?.DriveOperationsCount ?? 0) + (ServiceInstances?.Count ?? 0) + (Scenarios?.Count ?? 0) + (Commands?.Count ?? 0);

        public CollectionTemplate<ServiceInstance> ServiceInstances { get; private set; }

        public CollectionBusinessProcess Processes { get; private set; }
        public CollectionHostType HostTypes { get; private set; }
        public CollectionTemplate<Scenario> Scenarios { get; private set; }
        public CollectionTemplate<Command> Commands { get; private set; }

        public void DataFilter(string filterProcess, string filterHT, string filterOp, ProgressCalculationAsync progressCalc)
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

                progressCalc.Append(1);

                Func<IHostType, bool> htFilter = null;
                Func<IOperation, bool> opFilter = null;

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

                if (ROBPHostTypesPath != null)
                    FilterROBPOperations(htFilter, opFilter);
                else
                    FilterSCOperations(htFilter, opFilter);

                progressCalc.Append(2);

                #region Mark or Exclude Processes

                var fileteredOperations = HostTypes.Operations;
                var operationsDictionary = fileteredOperations.ToDictionary(x => x.Name, x => x, StringComparer.CurrentCultureIgnoreCase);
                if (htFilter == null && opFilter == null)
                {
                    // Если не установленно никаких фильтрров по операциям или хостам
                    // То помечаем бизнесспроцессы в которых файлы ROBP операций не существуют
                    if (ROBPHostTypesPath != null)
                    {
                        foreach (var process in Processes)
                        {
                            foreach (var operation in process.Operations)
                            {
                                if (operationsDictionary.ContainsKey(operation))
                                    continue;

                                process.AllOperationsExist = false;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // наоборот удаляем процессы, если был установлен фильтр по операциям или хостам
                    var isCatalog = HostTypes is ServiceCatalog;
                    for (var index = 0; index < Processes.Count; index++)
                    {
                        var process = Processes[index];
                        if(isCatalog && process.HasCatalogCall)
                            continue;

                        if (process.Operations.Any(x => operationsDictionary.ContainsKey(x)))
                            continue;

                        Processes.Remove(process);
                        index--;
                    }
                }

                Processes.InitSequence();

                #endregion

                progressCalc.Append(1);

                #region Filter Scenarios

                ReinitializationActivators();
                Scenarios = CollectionTemplate<Scenario>.ToCollection(Scenarios.Intersect(fileteredOperations, new OperationsComparer()).Cast<Scenario>().OrderBy(p => p.HostTypeName).ThenBy(p => p.Name), false);

                // проверка на существование сценария для операции
                foreach (var operation in fileteredOperations.Except(Scenarios, new OperationsComparer()).Cast<IOperation>())
                {
                    operation.IsScenarioExist = false;
                }

                #endregion

                progressCalc.Append(3);

                #region Filter Commands

                var filteredSubScenarios = new DistinctList<Scenario>();
                var filteredCommands  = new DistinctList<Command>();
                GetCommandsAndSubscenarios(Scenarios, filteredSubScenarios, filteredCommands);
                Scenarios.AddRange(filteredSubScenarios.OrderBy(p => p.FilePath));
                Scenarios.InitSequence();

                Commands = CollectionTemplate<Command>.ToCollection(filteredCommands.OrderBy(p => p.HostTypeName).ThenBy(p => p.Name));

                #endregion

                progressCalc.Append(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void GetCommandsAndSubscenarios(IEnumerable<Scenario> scenarios, DistinctList<Scenario> subScenarios, DistinctList<Command> commandsResult)
        {
            foreach (var scenario in scenarios)
            {
                if (scenario.SubScenarios.Count > 0)
                {
                    subScenarios.AddRange(scenario.SubScenarios);
                    GetCommandsAndSubscenarios(scenario.SubScenarios, subScenarios, commandsResult);
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
            foreach (var file in GetFiles(ProcessPath))
            {
                if (BusinessProcess.IsBusinessProcess(file, out var result) && (bpFilter == null || bpFilter.Invoke(result)))
                    Processes.Add(result);
            }
        }

        public async Task AssignROBPOperationsAsync(string robpHostTypesPath)
        {
            ROBPHostTypesPath = robpHostTypesPath;
            SCPath = null;
            HostTypes = null;
            await Task.Factory.StartNew(() => FilterROBPOperations(null, null));
        }

        void FilterROBPOperations(Func<IHostType, bool> htFilter, Func<IOperation, bool> opFilter)
        {
            if (!Directory.Exists(ROBPHostTypesPath))
                throw new Exception($"Directory \"{ROBPHostTypesPath}\" not found!");

            HostTypes = new CollectionHostType(); 
            var allBPOperations = Processes.AllOperationsNames;

            foreach (var neDirPath in GetDirectories(ROBPHostTypesPath))
            {
                var robpHostType = new ROBPHostType(neDirPath);
                FilterOperations(robpHostType, htFilter, opFilter, allBPOperations, false);
                HostTypes.Add(robpHostType);
            }
        }

        public async Task AssignSCOperationsAsync(string filePath)
        {
            SCPath = filePath;
            ROBPHostTypesPath = null;
            HostTypes = null;
            await Task.Factory.StartNew(() => FilterSCOperations(null, null));
        }

        void FilterSCOperations(Func<IHostType, bool> neFilter, Func<IOperation, bool> opFilter)
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

        static void FilterOperations(IHostType hostType, Func<IHostType, bool> neFilter, Func<IOperation, bool> opFilter, IReadOnlyDictionary<string, string> allBPOperations, bool anyHasCatalogCall)
        {
            if (neFilter != null && !neFilter.Invoke(hostType))
            {
                // если фильтруются операции по другому хосту
                hostType.Operations.Clear();
                return;
            }
            else if (allBPOperations != null)
            {
                // удаляем операции которых не используются ни в одном бизнес процессе
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
            else if (!anyHasCatalogCall)
            {
                // если ни в одном бизнесс процессе нет вызова каталога, то все операции удалются
                hostType.Operations.Clear();
                return;
            }
            else if (opFilter != null)
            {
                // удаляем операции которые не попали под имя указынные в фильтре операций
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
                ServiceInstances = GetServiceInstances();
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

                ServiceInstances = GetServiceInstances();
            });
        }

        void ReinitializationActivators()
        {
            foreach (var activator in _activators)
            {
                activator.Refresh();
            }

            ServiceInstances = GetServiceInstances();
            GetScenariosAndCommands(ServiceInstances, out var allScenarios, out var allCommands);
            Scenarios = allScenarios;
            Commands = allCommands;
        }

        CollectionTemplate<ServiceInstance> GetServiceInstances(bool getValid = false)
        {
            var intsances = new CollectionTemplate<ServiceInstance>();
            foreach (var instance in _activators.SelectMany(p => p.Instances).OrderBy(p => p.HostTypeName).ThenBy(p => p.Name))
            {
                if (getValid && !instance.IsCorrect)
                    continue;
                intsances.Add(instance);
            }
            intsances.InitSequence();
            return intsances;
        }

        static void GetScenariosAndCommands(IEnumerable<ServiceInstance> serviceInstances, out CollectionTemplate<Scenario> resultScenarios, out CollectionTemplate<Command> resultCommands)
        {
            var allScenarios = new DistinctList<Scenario>();
            var allCommands = new DistinctList<Command>();
            foreach (var instance in serviceInstances)
            {
                if(!instance.IsCorrect)
                    continue;

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

                var myWorksheet = xslPackage.Workbook.Worksheets.First();
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
            FormattingXmlFiles(Processes, stringErrors, progrAsync);
            FormattingXmlFiles(HostTypes.Operations.OfType<DriveTemplate>(), stringErrors, progrAsync);
            FormattingXmlFiles(ServiceInstances, stringErrors, progrAsync);
            FormattingXmlFiles(Scenarios, stringErrors, progrAsync);
            FormattingXmlFiles(Commands, stringErrors, progrAsync);
        }

        static void FormattingXmlFiles(IEnumerable<DriveTemplate> fileObj, CustomStringBuilder stringErrors, ProgressCalculationAsync progressCalc)
        {
            if (fileObj == null || !fileObj.Any())
                return;

            foreach (var file in fileObj)
            {
                FormattingXmlFile(file.FilePath, stringErrors);
                progressCalc++;
            }
        }

        static void FormattingXmlFile(string filePath, CustomStringBuilder stringErrors)
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

        public static List<string> GetDirectories(string dirPath)
        {
            var dirs = Directory.GetDirectories(dirPath, "*", SearchOption.TopDirectoryOnly).ToList();
            dirs.Sort(StringComparer.CurrentCulture);
            return dirs;
        }

        public static List<string> GetFiles(string dirPath, string mask = "*.xml")
        {
            var files = Directory.GetFiles(dirPath, mask, SearchOption.TopDirectoryOnly).ToList();
            files.Sort(StringComparer.CurrentCulture);
            return files;
        }
    }
}