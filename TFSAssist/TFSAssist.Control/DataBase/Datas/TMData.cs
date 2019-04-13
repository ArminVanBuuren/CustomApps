using System;
using System.Xml.Serialization;

namespace TFSAssist.Control.DataBase.Datas
{
    public enum ProcessingStatus
    {
        Unknown = 0,
        Created = 1,
        Skipped = 2,
        Failure = 3,
        FatalException = 4
    }

    public class TMData
    {
        [XmlIgnore]
        public ProcessingStatus Status { get; set; }

        [XmlAttribute("Status")]
        public string StatusAsString
        {
            get { return Status.ToString("G"); }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Status = default(ProcessingStatus);
                    return;
                }

                foreach (ProcessingStatus stat in Enum.GetValues(typeof(ProcessingStatus)))
                {
                    string ss1 = value.Trim();
                    string ss2 = stat.ToString("G");
                    string ss3 = stat.ToString("D");
                    if (ss1.Equals(ss2, StringComparison.CurrentCultureIgnoreCase) || ss1.Equals(ss3, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Status = stat;
                        break;
                    }
                }
            }
        }

        [XmlAttribute]
        public string ReceivedDate { get; set; }

        [XmlAttribute]
        public string From { get; set; }

        [XmlAttribute]
        public string ID { get; set; }


        [XmlElement("ItemExecuted")]
        public ItemExecuted Executed { get; set; }

        public override string ToString ()
        {
            return string.Format("From=[{0}]; ReceivedData=[{1}];", From, ReceivedDate);
        }
    }
}
