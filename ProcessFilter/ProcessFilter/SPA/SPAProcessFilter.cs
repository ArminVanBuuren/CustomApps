using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;
using SPAFilter.SPA.Collection;
using SPAFilter.SPA.Components;
using SPAFilter.SPA.Components.ROBP;
using SPAFilter.SPA.SC;
using Utils;
using Utils.CollectionHelper;
using Utils.Handles;
using Utils.WinForm.CustomProgressBar;

namespace SPAFilter.SPA
{
    public class SPAProcessFilter
    {
        //readonly object BP_OP_SYNC = new object();
        readonly object ACTIVATORS_SYNC = new object();
        readonly Dictionary<string, ServiceActivator> _activators = new Dictionary<string, ServiceActivator>(StringComparer.CurrentCultureIgnoreCase);

        public string ProcessPath { get; private set; }
        public string ROBPHostTypesPath { get; private set; }
        public string SCPath { get; private set; }

        public bool IsEnabledFilter => !ProcessPath.IsNullOrEmpty() && Directory.Exists(ProcessPath) && ((!ROBPHostTypesPath.IsNullOrEmpty() && Directory.Exists(ROBPHostTypesPath)) || (!SCPath.IsNullOrEmpty() || File.Exists(SCPath)));
        public bool CanGenerateSC => (HostTypes?.DriveOperationsCount ?? 0) > 0 && !(HostTypes is ServiceCatalog);
        public int WholeDriveItemsCount => (Processes?.Count ?? 0) + (HostTypes?.DriveOperationsCount ?? 0) + (ServiceInstances?.Count ?? 0) + (Scenarios?.Count ?? 0) + (Commands?.Count ?? 0);

        public CollectionTemplate<ServiceInstance> ServiceInstances { get; private set; }

        public CollectionBusinessProcess Processes { get; private set; }
        public CollectionHostType HostTypes { get; private set; }
        public CollectionTemplate<Scenario> Scenarios { get; private set; }
        public CollectionTemplate<Command> Commands { get; private set; }

        public async Task DataFilterAsync(string filterProcess, string filterHT, string filterOp, ProgressCalculationAsync progressCalc)
        {
            await Task.Factory.StartNew(() => DataFilter(filterProcess, filterHT, filterOp, progressCalc));
        }

        void DataFilter(string filterProcess, string filterHT, string filterOp, ProgressCalculationAsync progressCalc)
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

                // Фильтруем БП и операции.
                // В фильтре операций используется вызов повторного обновления БП, поэтому не вызываем отдельно
                if (ROBPHostTypesPath != null)
                    FilterROBPOperations(bpFilter, htFilter, opFilter);
                else
                    FilterSCOperations(bpFilter, htFilter, opFilter);

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

                        Processes.RemoveAt(index);
                        index--;
                    }
                }

                Processes.InitSequence();

                #endregion

                progressCalc.Append(2);

                #region Filter Scenarios

                // обновляются свойства Scenarios и Commands
                lock (ACTIVATORS_SYNC)
                {
                    ReloadActivators(_activators.Values);
                }
                // Получаем общие объекты по именам операций и сценариев. Т.е. фильтруем все сценарии по отфильтрованным операциям.
                var scenarios = Scenarios.Intersect(fileteredOperations, new SAComparer()).Cast<Scenario>().ToDictionary(x => x.Name, x => x, StringComparer.CurrentCultureIgnoreCase);

                // Проверка на существование сценария для операции. Ищем несуществующие сценарии.
                foreach (var operation in fileteredOperations.Except(scenarios.Values, new SAComparer()).Cast<IOperation>())
                {
                    operation.IsScenarioExist = false;
                }

                #endregion

                progressCalc.Append(3);

                #region Filter Commands

                var filteredSubScenarios = new DistinctList<Scenario>();
                var filteredCommands  = new DistinctList<Command>();
                GetCommandsAndSubscenarios(scenarios, scenarios.Values, filteredSubScenarios, filteredCommands);

                var scenarios2 = scenarios.Values.ToList();
                scenarios2.AddRange(filteredSubScenarios);
                scenarios2 = scenarios2.OrderBy(p => p.HostTypeName).ThenBy(p => p.Name).ToList();
                
                Scenarios = CollectionTemplate<Scenario>.ToCollection(scenarios2, false);
                Scenarios.InitSequence();

                Commands = CollectionTemplate<Command>.ToCollection(filteredCommands.OrderBy(p => p.HostTypeName).ThenBy(p => p.FilePath));

                #endregion

                progressCalc.Append(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void GetCommandsAndSubscenarios(IDictionary<string, Scenario> checkExist, IEnumerable<Scenario> scenarios, DistinctList<Scenario> subScenarios, DistinctList<Command> commandsResult)
        {
            foreach (var scenario in scenarios)
            {
                if (scenario.SubScenarios.Count > 0)
                {
                    foreach (var subScenario in scenario.SubScenarios)
                    {
                        // проверяем если обычный сценарий является также вложенным сценарием, то тогда оставляем обычный
                        if(!checkExist.ContainsKey(subScenario.Name))
                            subScenarios.Add(subScenario);
                    }
                    GetCommandsAndSubscenarios(checkExist, scenario.SubScenarios, subScenarios, commandsResult);
                }
                commandsResult.AddRange(scenario.Commands);
            }
        }

        public async Task AssignProcessesAsync(string processPath)
        {
            ProcessPath = processPath;
            Processes = null;

            if(ProcessPath.IsNullOrEmptyTrim())
                return;

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
            Scenarios = null;
            Commands = null;

            if(ROBPHostTypesPath.IsNullOrEmptyTrim())
                return;

            await Task.Factory.StartNew(() => FilterROBPOperations(null, null, null));
        }

        void FilterROBPOperations(Func<BusinessProcess, bool> bpFilter, Func<IHostType, bool> htFilter, Func<IOperation, bool> opFilter)
        {
            if (!Directory.Exists(ROBPHostTypesPath))
                throw new Exception($"Directory \"{ROBPHostTypesPath}\" not found!");

            var hostTypesDir = GetDirectories(ROBPHostTypesPath);
            if (hostTypesDir.Count == 0)
                throw new Exception("You must select a folder with exported ROBP's host type directories.");

            FilterProcesses(bpFilter);
            HostTypes = new CollectionHostType(); 
            var allBPOperations = Processes.AllOperationsNames;

            foreach (var hostTypeDir in hostTypesDir)
            {
                var robpHostType = new ROBPHostType(hostTypeDir);
                FilterOperations(robpHostType, htFilter, opFilter, allBPOperations, false);
                HostTypes.Add(robpHostType);
            }
        }

        public async Task AssignSCOperationsAsync(string serviceCatalogFilePath)
        {
            ROBPHostTypesPath = null;
            SCPath = serviceCatalogFilePath;
            HostTypes = null;
            Scenarios = null;
            Commands = null;

            if(SCPath.IsNullOrEmptyTrim())
                return;

            await Task.Factory.StartNew(() => FilterSCOperations(null, null, null));
        }

        void FilterSCOperations(Func<BusinessProcess, bool> bpFilter, Func<IHostType, bool> neFilter, Func<IOperation, bool> opFilter)
        {
            if (!File.Exists(SCPath))
                throw new Exception($"File \"{SCPath}\" not found");

            FilterProcesses(bpFilter);
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
            // не менять порядок проверок!
            if (neFilter != null && !neFilter.Invoke(hostType))
            {
                // если фильтруются операции по другому хосту
                hostType.Operations.Clear();
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
                        hostType.Operations.RemoveAt(index);
                        index--;
                    }
                }
            }
            else if (!anyHasCatalogCall)
            {
                // если ни в одном бизнесс процессе нет вызова каталога, то все операции удалются
                hostType.Operations.Clear();
            }
            else if (opFilter != null)
            {
                // удаляем операции которые не попали под имя указынные в фильтре операций
                for (var index = 0; index < hostType.Operations.Count; index++)
                {
                    var operation = hostType.Operations[index];
                    if (!opFilter.Invoke(operation))
                    {
                        hostType.Operations.RemoveAt(index);
                        index--;
                    }
                }
            }
        }

        public async Task AssignActivatorAsync(IEnumerable<string> filePathList)
        {
            await Task.Factory.StartNew(() =>
            {
                lock (ACTIVATORS_SYNC)
                {
                    var lastActivatorList = new List<ServiceActivator>(_activators.Values);

                    var result = MultiTasking.Run((inputFile) =>
                    {
                        try
                        {
                            if (_activators.ContainsKey(inputFile))
                            {
                                return $"Activator \"{inputFile}\" already exist.";
                            }
                            else
                            {
                                _activators.Add(inputFile, new ServiceActivator(inputFile));
                                return string.Empty;
                            }
                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }
                    }, filePathList, null, 10);

                    ReloadActivators(lastActivatorList);

                    var errors = string.Join(Environment.NewLine, result.Where(x => !x.Value.IsNullOrEmpty()).Select(x => x.Value));
                    if (!errors.IsNullOrEmptyTrim())
                    {
                        MessageBox.Show(errors, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            });
        }

        public async Task RemoveActivatorAsync(IEnumerable<string> filePathList)
        {
            await Task.Factory.StartNew(() =>
            {
                lock (ACTIVATORS_SYNC)
                {
                    foreach (var filePath in filePathList)
                    {
                        if (_activators.ContainsKey(filePath))
                        {
                            _activators.Remove(filePath);
                        }
                    }

                    ReloadActivators(_activators.Values);
                }
            });
        }


        public async Task ReloadActivatorsAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                lock (ACTIVATORS_SYNC)
                {
                    ReloadActivators(_activators.Values);
                }
            });
        }

        /// <summary>
        /// Перезагрузить все экземпляры активаторов, сценарии и комманды.
        /// </summary>
        void ReloadActivators(IEnumerable<ServiceActivator> activators)
        {
            if (activators != null && activators.Any())
            {
                MultiTasking.Run((sa) =>
                {
                    try
                    {
                        sa.Refresh();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }, activators, null, 10);
            }

            LoadActivators();
        }

        void LoadActivators()
        {
            ServiceInstances = GetServiceInstances();
            GetScenariosAndCommands(ServiceInstances, out var allScenarios, out var allCommands);
            Scenarios = allScenarios;
            Commands = allCommands;
        }

        CollectionTemplate<ServiceInstance> GetServiceInstances(bool getValid = false)
        {
            var intsances = new CollectionTemplate<ServiceInstance>();

            var allInstances = _activators.Values.SelectMany(p => p.Instances).ToList();

            // Проверям инстансы на коллизии сценариев
            // Если имеются два инстанса например: "FP:AM и FP:SCP" которые имеют разные каталоги сценариев и при этом общие названия файлов
            // то возникает коллизия и нужно выбрать какой то один инстанс.
            // В отличии от "FP:SCP1 и FP:SCP2" которые имеют общий каталог сценариев, тогда коллизий не будет.
            var intersectInstancesStr = string.Empty;
            var instancesWithSameHost = allInstances.GroupBy(x => x.HostTypeName.ToLower()).Where(p => p.Count() > 1).ToList();
            foreach (var instance in instancesWithSameHost)
            {
                var sameScenarios = instance.SelectMany(x => x.Scenarios).GroupBy(x => x.FilePath.ToLower()).SelectMany(x => x).GroupBy(p => p.Name.ToLower()).Where(t => t.Count() > 1).ToList();
                if (sameScenarios.Any())
                {
                    var intersectInstances = sameScenarios.SelectMany(x => x).GroupBy(x => x.Parent).Select(p => p.Key).ToList();
                    foreach (var instance2 in intersectInstances)
                    {
                        instance2.IsCorrect = false;
                    }

                    intersectInstancesStr += $"\"{string.Join(",", intersectInstances.Select(x => x.HardwareID))}\"; ";
                }
            }

            if (!string.IsNullOrEmpty(intersectInstancesStr))
                MessageBox.Show($"Scenario collisions found when instances: {intersectInstancesStr.Trim().Trim(';')} were initialized.\r\nChoose only one instance or several with a common configuration.", @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);


            foreach (var instance in allInstances.OrderBy(p => p.HostTypeName).ThenBy(p => p.Name))
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

        public async Task<DataTable> GetRDServicesFromXslxAsync(FileInfo file, CustomProgressCalculation progressCalc)
        {
            return await Task<DataTable>.Factory.StartNew(() => GetRDServicesFromXslx(file, progressCalc));
        }

        DataTable GetRDServicesFromXslx(FileInfo file, CustomProgressCalculation progressCalc)
        {
            var serviceTable = new DataTable();

            progressCalc.BeginOpenXslxFile();

            using (var xslPackage = new ExcelPackage(file))
            {
                progressCalc.BeginReadXslxFile();

                if(xslPackage.Workbook.Worksheets.Count == 0)
                    throw new Exception("No worksheet found");

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

                for (var rowNum = 2; rowNum <= totalRows; rowNum++)
                {
                    var row = myWorksheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString()).Take(totalColumns);
                    serviceTable.Rows.Add(values: row.ToArray());
                    progressCalc.ReadXslxFileLine();
                }
            }

            progressCalc.EndOpenXslxFile();

            return serviceTable;
        }

        public async Task<string> GetServiceCatalogAsync(DataTable rdServices, string exportFilePath, CustomProgressCalculation progressCalc)
        {
            return await Task.Factory.StartNew(() => GetServiceCatalog(rdServices, exportFilePath, progressCalc));
        }

        string GetServiceCatalog(DataTable rdServices, string exportFilePath, CustomProgressCalculation progressCalc)
        {
            try
            {
                if (HostTypes == null || HostTypes is ServiceCatalog)
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

        private CancellationTokenSource _cancellationPrintXML;
        public async Task PrintXMLAsync(ProgressCalculationAsync progrAsync, CustomStringBuilder stringErrors)
        {
            _cancellationPrintXML = new CancellationTokenSource();
            try
            {
                await Task.Factory.StartNew((token) => PrintXML((CancellationToken)token, progrAsync, stringErrors), _cancellationPrintXML.Token);
            }
            catch (OperationCanceledException)
            {
                
            }
            finally
            {
                _cancellationPrintXML = null;
            }
        }

        public void PrintXMLAbort()
        {
            _cancellationPrintXML?.Cancel();
        }

        void PrintXML(CancellationToken token, ProgressCalculationAsync progrAsync, CustomStringBuilder stringErrors)
        {
            FormatXmlFiles(token, Processes, progrAsync, stringErrors);
            FormatXmlFiles(token, HostTypes.Operations.OfType<DriveTemplate>(), progrAsync, stringErrors);
            FormatXmlFiles(token, ServiceInstances, progrAsync, stringErrors);
            FormatXmlFiles(token, Scenarios, progrAsync, stringErrors);
            FormatXmlFiles(token, Commands, progrAsync, stringErrors);
        }

        static void FormatXmlFiles(CancellationToken token, IEnumerable<DriveTemplate> fileObj, ProgressCalculationAsync progressCalc, CustomStringBuilder stringErrors)
        {
            if (fileObj == null || !fileObj.Any())
                return;

            foreach (var file in fileObj)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                FormatXmlFile(file.FilePath, stringErrors);
                progressCalc.Append(1);
            }
        }

        static void FormatXmlFile(string filePath, CustomStringBuilder stringErrors)
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

                IO.WriteFile(filePath, formatting, new UTF8Encoding(false));
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
            return files.Where(x => x.EndsWith(mask.Trim().Trim('*'), StringComparison.CurrentCultureIgnoreCase)).ToList();
        }
    }
}