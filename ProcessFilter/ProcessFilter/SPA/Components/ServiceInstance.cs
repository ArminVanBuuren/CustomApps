using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    public sealed class ServiceInstance : DriveTemplate
    {
        const string NOT_FOUND_ATTRIBUTE = "Attributes \"{0}\" not found in {1}";
        const string NOT_FOUND_DIR = "Directory \"{0}\" not found when initializing {1}";
        const string NOT_FOUND_FILE = "File \"{0}\" not found when initializing {1}";
        const string INVALID_XML = "Xml file \"{0}\" is invalid";

        [DGVColumn(ColumnPosition.After, "HardwareID")]
        public string HardwareID { get; private set; }

        public string HostTypeName { get; }

        public override string Name { get; set; }

        public List<Scenario> Scenarios { get; } = new List<Scenario>();
        public Dictionary<string, Command> Commands { get; } = new Dictionary<string, Command>(StringComparer.CurrentCultureIgnoreCase);

        [DGVColumn(ColumnPosition.Last, "IsCorrect", false)]
        public bool IsCorrect { get; internal set; } = false;

        public ServiceInstance(string filePath, XmlNode node) :base(filePath)
        {
            var activatorDirPath = Path.GetDirectoryName(filePath);
            var serviceInstanceNode = node.Clone();

            HardwareID = serviceInstanceNode.Attributes?["hardwareID"]?.Value;
            if (HardwareID == null)
            {
                ShowError(string.Format(NOT_FOUND_ATTRIBUTE, "hardwareID", "serviceInstance node"));
                return;
            }

            HostTypeName = HardwareID.Split(':')[0];

            var scenarioConfigNode = serviceInstanceNode.SelectSingleNode("//fileScenarioSource") ?? serviceInstanceNode.SelectSingleNode("//config");
            var scenarios = scenarioConfigNode?.Attributes?["dir"]?.Value;
            if (scenarios == null)
            {
                ShowError(string.Format(NOT_FOUND_ATTRIBUTE, "dir", "\"fileScenarioSource\" node"));
                return;
            }
            var scenariosPath = GetDir(activatorDirPath, scenarios);
            if (!Directory.Exists(scenariosPath))
            {
                ShowError(string.Format(NOT_FOUND_DIR, scenarios, "\"fileScenarioSource\" node"));
                return;
            }

            var dict = serviceInstanceNode.SelectSingleNode("//context/object[@name='dictionary']/config") ?? serviceInstanceNode.SelectSingleNode("//context/object[@name='dictionary']/fileDictionarySource");
            var dictionary = dict?.Attributes?["path"]?.Value;
            var dictionaryXML = dict?.Attributes?["xml"]?.Value;
            if (dictionary == null || dictionaryXML == null)
            {
                ShowError(string.Format(NOT_FOUND_ATTRIBUTE, "path or xml", "object-\"dictionary\" node"));
                return;
            }
            var dictionaryPath = GetDir(activatorDirPath, dictionary);
            if (dictionaryPath == null)
            {
                ShowError(string.Format(NOT_FOUND_DIR, dictionary, "object-\"dictionary\" node"));
                return;
            }
            dictionaryPath = Path.Combine(dictionaryPath, dictionaryXML);
            if (!File.Exists(dictionaryPath))
            {
                ShowError(string.Format(NOT_FOUND_FILE, dictionaryPath, "object-\"dictionary\" node"));
                return;
            }

            if (!XML.IsFileXml(dictionaryPath, out var dictionaryConfig))
            {
                ShowError(string.Format(INVALID_XML, dictionaryPath));
                return;
            }

            var commandsRoot = dictionaryConfig.SelectSingleNode(@"/Dictionary/CommandsList/@Root")?.Value;
            var commandsMask = dictionaryConfig.SelectSingleNode(@"/Dictionary/CommandsList/@Mask")?.Value ?? "*.xml";
            if (commandsRoot == null)
            {
                ShowError(string.Format(NOT_FOUND_ATTRIBUTE, "Root", $"dictionary=\"{dictionaryPath}\""));
                return;
            }
            var commandsDir = GetDir(activatorDirPath, commandsRoot);
            if (commandsDir == null)
            {
                ShowError(string.Format(NOT_FOUND_DIR, commandsRoot, $"dictionary=\"{dictionaryPath}\""));
                return;
            }
            if (commandsDir == activatorDirPath)
                commandsDir = Path.Combine(commandsDir, commandsRoot);
            if (!Directory.Exists(commandsDir))
            {
                ShowError(string.Format(NOT_FOUND_DIR, commandsDir, $"dictionary=\"{dictionaryPath}\""));
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

        static string GetDir(string basePath, string rootPath)
        {
            return Path.IsPathRooted(rootPath) ? rootPath : IO.EvaluateFirstMatchPath(rootPath, basePath);
        }

        void ShowError(string message)
        {
            MessageBox.Show($"{(!HardwareID.IsNullOrEmpty() ? $"[{HardwareID}]={FilePath}" : FilePath)}\r\n\r\n{message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}