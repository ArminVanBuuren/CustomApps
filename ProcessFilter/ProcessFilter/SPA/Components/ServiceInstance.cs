using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    public sealed class ServiceInstance : DriveTemplate, ISAComponent
    {
        [DGVColumn(ColumnPosition.After, "HardwareID")]
        public string HardwareID { get; private set; }

        public string HostTypeName { get; private set; }

        public override string Name { get; set; }

        public List<Scenario> Scenarios { get; } = new List<Scenario>();

        public Dictionary<string, Command> Commands { get; } = new Dictionary<string, Command>(StringComparer.InvariantCultureIgnoreCase);

        [DGVColumn(ColumnPosition.Last, "IsCorrect", false)]
        public bool IsCorrect { get; internal set; } = false;

        public ServiceActivator Parent { get; }
        public XmlNode InstanceNode { get; }

        public ServiceInstance(ServiceActivator parent, XmlNode node) :base(parent.FilePath)
        {
            Parent = parent;
            InstanceNode = node.Clone();
            Refresh();
        }

        public void Refresh()
        {
            var activatorDirPath = Path.GetDirectoryName(FilePath);
            Scenarios.Clear();
            Commands.Clear();

            HardwareID = InstanceNode.Attributes?["hardwareID"]?.Value;
            if (HardwareID == null)
            {
                ShowError("Attribute:'@hardwareID' not found in serviceInstance");
                return;
            }

            HostTypeName = HardwareID.Split(':')[0];

            var scenarioConfigNode = InstanceNode.SelectSingleNode("//fileScenarioSource") ?? InstanceNode.SelectSingleNode("//config");
            var scenarios = scenarioConfigNode?.Attributes?["dir"]?.Value ?? InstanceNode.SelectSingleNode("//scenarios")?.InnerText;
            if (scenarios == null)
            {
                ShowError("Attribute:'@dir' not found in \"fileScenarioSource\". Or 'scenarios' node not found.");
                return;
            }
            var scenariosPath = GetDir(activatorDirPath, scenarios);
            if (!Directory.Exists(scenariosPath))
            {
                ShowError($"Directory:\"{scenarios}\" not found. Final path:\"{scenariosPath}\"");
                return;
            }

            var dict = InstanceNode.SelectSingleNode("//context/object[@name='dictionary']/config") ?? InstanceNode.SelectSingleNode("//context/object[@name='dictionary']/fileDictionarySource");
            var dictionaryPath = dict?.Attributes?["path"]?.Value ?? Path.GetDirectoryName(InstanceNode.SelectSingleNode("//dictionary")?.InnerText);
            var dictionaryXML = dict?.Attributes?["xml"]?.Value ?? Path.GetFileName(InstanceNode.SelectSingleNode("//dictionary")?.InnerText);
            if (dictionaryPath == null || dictionaryXML == null)
            {
                ShowError("Attributes:'@path'\\'@xml' not found. Or \"dictionary\" node not found.");
                return;
            }
            var dictionaryFilePath = GetDir(activatorDirPath, dictionaryPath);
            if (dictionaryFilePath == null)
            {
                ShowError($"Directory:\"{dictionaryPath}\" not found");
                return;
            }
            dictionaryFilePath = Path.Combine(dictionaryFilePath, dictionaryXML);
            if (!File.Exists(dictionaryFilePath))
            {
                ShowError($"File:\"{dictionaryFilePath}\" not found");
                return;
            }

            if (!XML.IsFileXml(dictionaryFilePath, out var dictionaryConfig))
            {
                ShowError($"Xml file:\"{dictionaryFilePath}\" is invalid");
                return;
            }

            var commandsRoot = dictionaryConfig.SelectSingleNode(@"/Dictionary/CommandsList/@Root")?.Value;
            var commandsMask = dictionaryConfig.SelectSingleNode(@"/Dictionary/CommandsList/@Mask")?.Value ?? "*.xml";
            if (commandsRoot == null)
            {
                ShowError($"Attribute:'@Root' not found in file dictionary:\"{dictionaryFilePath}\"");
                return;
            }
            var commandsDir = GetDir(activatorDirPath, Path.Combine(dictionaryPath, commandsRoot));
            if (commandsDir == null)
            {
                ShowError($"Directory:\"{commandsRoot}\" not found when initializing file dictionary:\"{dictionaryFilePath}\"");
                return;
            }
            if (commandsDir == activatorDirPath)
                commandsDir = Path.Combine(commandsDir, commandsRoot);
            if (!Directory.Exists(commandsDir))
            {
                ShowError($"Directory:\"{commandsDir}\" not found when initializing file dictionary:\"{dictionaryFilePath}\"");
                return;
            }

            foreach (var fileCommand in SPAProcessFilter.GetFiles(commandsDir, commandsMask))
            {
                Commands.Add(IO.GetLastNameInPath(fileCommand, true), new Command(this, fileCommand));
            }

            foreach (var fileScenario in SPAProcessFilter.GetFiles(scenariosPath))
            {
                Scenarios.Add(new Scenario(this, fileScenario, Commands));
            }

            IsCorrect = true;
        }

        public static string GetDir(string basePath, string rootPath)
        {
            if (Path.IsPathRooted(rootPath))
                return rootPath;
            else
                return IO.EvaluateFirstMatchPath(rootPath, basePath);
        }

        void ShowError(string message)
        {
            Program.ReportMessage($"{(!HardwareID.IsNullOrEmpty() ? $"[{HardwareID}]={FilePath}" : FilePath)}\r\n\r\n{message}");
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj) && obj is ServiceInstance instance && HardwareID.Equals(instance.HardwareID);
        }

        public override string ToString()
        {
            return HardwareID;
        }
    }
}