using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Script
{
    [Serializable, XmlRoot("Settings")]
    public class ScriptSettings
    {
        [XmlArray("ProcessList")]
        [XmlArrayItem("Process")]
        public List<ConfigurationProcess> ProcessList { get; set; } = new List<ConfigurationProcess>();
    }

    [Serializable, XmlRoot("Process")]
    public class ConfigurationProcess
    {
        [XmlAttribute]
        public string ConfiguraionName { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlAttribute]
        public string Path { get; set; }
    }
}
