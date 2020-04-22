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
using SPAFilter.Properties;
using SPAFilter.SPA.Collection;
using SPAFilter.SPA.Components;
using SPAFilter.SPA.Components.ROBP;
using SPAFilter.SPA.SC;
using Utils;
using Utils.CollectionHelper;
using Utils.WinForm.CustomProgressBar;

namespace SPAFilter.SPA
{
    public class SPAProcessFilter
    {
        //readonly object BP_OP_SYNC = new object();
        readonly object ACTIVATORS_SYNC = new object();
        readonly Dictionary<string, ServiceActivator> _activators = new Dictionary<string, ServiceActivator>(StringComparer.InvariantCultureIgnoreCase);

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
                    throw new Exception(Resources.Filter_NotEnabled);

                Func<BusinessProcess, bool> bpFilter = null;

                #region filter BusinessProcesses

                if (!filterProcess.IsNullOrEmpty())
                {
                    if (filterProcess[0] == '%' && filterProcess[filterProcess.Length - 1] == '%')
                    {
                        var filterProcessContains = filterProcess.Substring(1, filterProcess.Length - 2);
                        bpFilter = (bp) => bp.Name.StringContains(filterProcessContains);
                    }
                    else if (filterProcess[0] == '%')
                    {
                        var filterProcessContains = filterProcess.Substring(1, filterProcess.Length - 1);
                        bpFilter = (bp) => bp.Name.EndsWith(filterProcessContains, StringComparison.CurrentCultureIgnoreCase);
                    }
                    else if (filterProcess[filterProcess.Length - 1] == '%')
                    {
                        var filterProcessContains = filterProcess.Substring(0, filterProcess.Length - 1);
                        bpFilter = (bp) => bp.Name.StartsWith(filterProcessContains, StringComparison.InvariantCultureIgnoreCase);
                    }
                    else
                    {
                        bpFilter = (bp) => bp.Name.Like(filterProcess);
                    }
                }

                #endregion

                Func<IHostType, bool> htFilter = null;
                Func<IOperation, bool> opFilter = null;

                #region filter HostType and filter Operations

                if (!filterHT.IsNullOrEmpty())
                {
                    if (filterHT[0] == '%' && filterHT[filterHT.Length - 1] == '%')
                    {
                        var filterNEContains = filterHT.Substring(1, filterHT.Length - 2);
                        htFilter = (ht) => ht.Name.StringContains(filterNEContains);
                    }
                    else if (filterHT[0] == '%')
                    {
                        var filterNEContains = filterHT.Substring(1, filterHT.Length - 1);
                        htFilter = (ht) => ht.Name.EndsWith(filterNEContains, StringComparison.InvariantCultureIgnoreCase);
                    }
                    else if (filterHT[filterHT.Length - 1] == '%')
                    {
                        var filterNEContains = filterHT.Substring(0, filterHT.Length - 1);
                        htFilter = (ht) => ht.Name.StartsWith(filterNEContains, StringComparison.InvariantCultureIgnoreCase);
                    }
                    else
                    {
                        htFilter = (ht) => ht.Name.Like(filterHT);
                    }
                }

                if (!filterOp.IsNullOrEmpty())
                {
                    if (filterOp[0] == '%' && filterOp[filterOp.Length - 1] == '%')
                    {
                        var filterOPContains = filterOp.Substring(1, filterOp.Length - 2);
                        opFilter = (op) => op.Name.StringContains(filterOPContains);
                    }
                    else if (filterOp[0] == '%')
                    {
                        var filterOPContains = filterOp.Substring(1, filterOp.Length - 1);
                        opFilter = (op) => op.Name.EndsWith(filterOPContains, StringComparison.InvariantCultureIgnoreCase);
                    }
                    else if (filterOp[filterOp.Length - 1] == '%')
                    {
                        var filterOPContains = filterOp.Substring(0, filterOp.Length - 1);
                        opFilter = (op) => op.Name.StartsWith(filterOPContains, StringComparison.InvariantCultureIgnoreCase);
                    }
                    else
                    {
                        opFilter = (op) => op.Name.Like(filterOp);
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
                var operationsDictionary = fileteredOperations.ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);
                if (htFilter == null && opFilter == null)
                {
                    // Если не установленно никаких фильтрров по операциям или хостам
                    // То помечаем бизнесспроцессы в которых файлы ROBP операций не существуют
                    if (ROBPHostTypesPath != null)
                    {
                        foreach (var process in Processes.Select(x => x).ToList())
                        {
                            var anyOperationInProcesss = false;
                            foreach (var operation in process.Operations)
                            {
                                if (operationsDictionary.ContainsKey(operation))
                                {
                                    anyOperationInProcesss = true;
                                    continue;
                                }

                                process.AllOperationsExist = false;
                            }

                            if (!anyOperationInProcesss)
                                Processes.Remove(process);
                        }
                    }
                }
                else
                {
                    // наоборот удаляем процессы, если был установлен фильтр по операциям или хостам
                    var isCatalog = HostTypes is ServiceCatalog;
                    foreach (var process in Processes.Select(x => x).ToList())
                    {
                        if (isCatalog && process.HasCatalogCall)
                            continue;

                        if (process.Operations.Any(x => operationsDictionary.ContainsKey(x)))
                            continue;

                        Processes.Remove(process);
                    }
                }

                Processes.ResetPublicID();
                HostTypes.Operations.ResetPublicID();

                #endregion

                progressCalc.Append(2);

                #region Filter Scenarios

                // обновляются свойства Scenarios и Commands
                lock (ACTIVATORS_SYNC)
                {
                    Refresh(_activators.Values);
                }
                // Получаем общие объекты по именам операций и сценариев. Т.е. фильтруем все сценарии по отфильтрованным операциям.
                var scenarios = Scenarios.Intersect(fileteredOperations, new SAComparer()).Cast<Scenario>().ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);

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
                
                Scenarios = new CollectionTemplate<Scenario>(scenarios2);
                Commands = new CollectionTemplate<Command>(filteredCommands.OrderBy(p => p.HostTypeName).ThenBy(p => p.FilePath));
                #endregion

                progressCalc.Append(1);
            }
            catch (Exception ex)
            {
                ReportMessage(ex.ToString(), "Filter");
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
                throw new Exception(string.Format(Resources.DirectoryNotFound, ProcessPath, "Process"));

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
                throw new Exception(string.Format(Resources.DirectoryNotFound, ROBPHostTypesPath, "Operations"));

            var hostTypesDir = GetDirectories(ROBPHostTypesPath);
            if (hostTypesDir.Count == 0)
                throw new Exception(Resources.Filter_ROBPOperationdDirInvalid);

            FilterProcesses(bpFilter);
            HostTypes = new CollectionHostType();
            var allBPOperations = Processes.AllOperationsNames;

            foreach (var hostTypeDir in hostTypesDir.ToList())
            {
                var robpHostType = new ROBPHostType(hostTypeDir);
                HostTypes.Add(robpHostType);
                FilterOperations(robpHostType, htFilter, opFilter, allBPOperations, false);
            }
            HostTypes.Commit();
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
                throw new Exception(string.Format(Resources.FileNotFound, SCPath));

            FilterProcesses(bpFilter);
            HostTypes = new ServiceCatalog(SCPath);
            if (HostTypes.Count == 0)
                return;
            var anyHasCatalogCall = Processes.AnyHasCatalogCall;

            foreach (var hostType in HostTypes.ToList())
            {
                FilterOperations(hostType, neFilter, opFilter, null, anyHasCatalogCall);
            }
            HostTypes.Commit();
        }

        static void FilterOperations(IHostType hostType, Func<IHostType, bool> neFilter, Func<IOperation, bool> opFilter, IDictionary<string, bool> allBPOperations, bool anyHasCatalogCall)
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

                foreach (var operation in hostType.Operations.Select(x => x).ToList())
                    if (!allBPOperations.ContainsKey(operation.Name) || !opFilter.Invoke(operation))
                        hostType.Operations.Remove(operation);
            }
            else if (!anyHasCatalogCall)
            {
                // если ни в одном бизнесс процессе нет вызова каталога, то все операции удалются
                hostType.Operations.Clear();
            }
            else if (opFilter != null)
            {
                // удаляем операции которые не попали под имя указынные в фильтре операций
                foreach (var operation in hostType.Operations.Select(x => x).ToList())
                    if (!opFilter.Invoke(operation))
                        hostType.Operations.Remove(operation);
            }
        }

        public async Task AssignActivatorAsync(IEnumerable<string> filePathList)
        {
            if(filePathList == null || !filePathList.Any())
                return;

            await Task.Factory.StartNew(() =>
            {
                List<ServiceActivator> lastActivatorList;
                lock (ACTIVATORS_SYNC)
                    lastActivatorList = new List<ServiceActivator>(_activators.Values);

                var result = MultiTasking.Run((inputFile) =>
                {
                    lock (ACTIVATORS_SYNC)
                    {
                        if (_activators.ContainsKey(inputFile))
                        {
                            throw new Exception(string.Format(Resources.Filter_ActivatorAlreadyExist, inputFile));
                        }
                        else
                        {
                            _activators.Add(inputFile, new ServiceActivator(inputFile));
                        }
                    }
                }, filePathList, new MultiTaskingTemplate(filePathList.Count(), ThreadPriority.Lowest));

                lock (ACTIVATORS_SYNC)
                    Refresh(lastActivatorList);

                var errors = string.Join(Environment.NewLine, result.CallBackList.Where(x => x.Error != null).Select(x => x.Error.Message));
                if (!errors.IsNullOrEmptyTrim())
                {
                    ReportMessage(errors, Resources.Filter_AddActivator);
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

                    Refresh(_activators.Values);
                }
            });
        }

        public async Task RemoveInstanceAsync(IEnumerable<string> uniqueNamesOfInstances)
        {
            await Task.Factory.StartNew(() =>
            {
                lock (ACTIVATORS_SYNC)
                {
                    foreach (var uniqueName in uniqueNamesOfInstances)
                    {
                        var instance = ServiceInstances[uniqueName];
                        instance.Parent.Instances.Remove(instance);
                    }

                    foreach (var activator in ServiceInstances.Select(x => x.Parent).Where(x => x.Instances.Count == 0))
                        _activators.Remove(activator.FilePath);

                    Refresh(_activators.Values);
                }
            });
        }

        public async Task RefreshActivatorsAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                lock (ACTIVATORS_SYNC)
                {
                    if (ServiceInstances != null)
                        foreach (var activator in ServiceInstances.Select(x => x.Parent).Where(x => x.Instances.Count == 0))
                            _activators.Remove(activator.FilePath);

                    if (_activators != null)
                    {
                        foreach (var fileActivator in _activators.Where(x => !File.Exists(x.Key)).Select(x => x.Key).ToList())
                            _activators.Remove(fileActivator);

                        Refresh(_activators.Values);
                    }
                }
            });
        }

        public async Task ReloadActivatorsAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                lock (ACTIVATORS_SYNC)
                {
                    if (_activators != null)
                    {
                        foreach (var fileActivator in _activators.Where(x => !File.Exists(x.Key)).Select(x => x.Key).ToList())
                            _activators.Remove(fileActivator);

                        Reload(_activators.Values);
                    }
                }
            });
        }

        /// <summary>
        /// Перезагрузить все экземпляры активаторов, сценарии и комманды.
        /// </summary>
        void Refresh(IEnumerable<ServiceActivator> activators)
        {
            if (activators != null && activators.Any())
            {
                var result = MultiTasking.Run((sa) => sa.Refresh(), activators, new MultiTaskingTemplate(activators.Count(), ThreadPriority.Lowest));

                var errors = string.Join(Environment.NewLine, result.CallBackList.Where(x => x.Error != null).Select(x => x.Error.Message));
                if (!errors.IsNullOrEmptyTrim())
                {
                    ReportMessage(errors, "Refresh");
                }
            }

            LoadActivators();
        }

        /// <summary>
        /// Перезагрузить все экземпляры активаторов, сценарии и комманды.
        /// </summary>
        void Reload(IEnumerable<ServiceActivator> activators)
        {
            if (activators != null && activators.Any())
            {
                var result = MultiTasking.Run((sa) => sa.Reload(), activators, new MultiTaskingTemplate(activators.Count(), ThreadPriority.Lowest));

                var errors = string.Join(Environment.NewLine, result.CallBackList.Where(x => x.Error != null).Select(x => x.Error.Message));
                if (!errors.IsNullOrEmptyTrim())
                {
                    ReportMessage(errors, "Reload");
                }
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
            var sameHostScenarios = allInstances.SelectMany(x => x.Scenarios).GroupBy(x => $"{x.HostTypeName.ToLowerInvariant()}=[{x.Name.ToLowerInvariant()}]").Where(p => p.Count() > 1).ToList();
            var hrdrIDs = sameHostScenarios.SelectMany(x => x).GroupBy(x => x.Parent.HardwareID).Select(x => x.Key).ToList();
            var instances = allInstances.Where(x => hrdrIDs.Any(h => h == x.HardwareID)).ToList();

            foreach (var instance in instances)
                instance.IsCorrect = false;

            if (sameHostScenarios.Count > 0 && instances.Count > 0)
            {
                ReportMessage(string.Format(Resources.Filter_InitializeActivatorsWarning, string.Join(";", hrdrIDs), sameHostScenarios.Count), "Get Service Instances", MessageBoxIcon.Warning);
            }

            foreach (var instance in allInstances.OrderBy(p => p.HostTypeName).ThenBy(p => p.Name))
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
                if(!instance.IsCorrect)
                    continue;

                foreach (var scenario in instance.Scenarios)
                {
                    allScenarios.Add(scenario);
                    allCommands.AddRange(scenario.Commands);
                }
            }

            resultScenarios = new CollectionTemplate<Scenario>(allScenarios);
            resultCommands = new CollectionTemplate<Command>(allCommands);
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
                    throw new Exception(Resources.Filter_GenerateSCWithoutOperations);

                var sc = new ServiceCatalogBuilder(HostTypes, rdServices, progressCalc);
                return sc.Save(exportFilePath);
            }
            catch (Exception ex)
            {
                ReportMessage(ex.ToString(), "Get Service Catalog");
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
                    Thread.Sleep(500);
                    attempts++;
                    if (attempts > 3)
                        return;
                }

                IO.WriteFile(filePath, formatting, false, new UTF8Encoding(false));
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
            return files.Where(x => x.EndsWith(mask.Trim().Trim('*'), StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        void ReportMessage(string message, string caption = null, MessageBoxIcon icon = MessageBoxIcon.Error)
        {
            SPAMassageSaloon.Common.ReportMessage.Show(message, icon, caption, false);
        }
    }
}