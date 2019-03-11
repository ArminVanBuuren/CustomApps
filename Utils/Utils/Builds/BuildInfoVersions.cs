using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Utils.Builds
{
    [Serializable, XmlRoot("Versions")]
    public class BuildInfoVersions
    {
        public const string FILE_NAME = "version.xml";

        [XmlElement("Build")]
        public List<FileBuildInfo> Builds { get; set; } = new List<FileBuildInfo>();
    }



    [Serializable, XmlRoot("Versions")]
    public class BuildInfoVersionsTEmp : IXmlSerializable
    {
        public const string FILE_NAME = "version.xml";

        [XmlComment(Value = "The application version, NOT the file version!")]
        [XmlElement("Build")]
        public List<FileBuildInfo> Builds { get; set; } = new List<FileBuildInfo>();


        public void WriteXml(XmlWriter writer)
        {
            var properties = GetType().GetProperties();

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.IsDefined(typeof(XmlCommentAttribute), false))
                {
                    writer.WriteComment(propertyInfo.GetCustomAttributes(typeof(XmlCommentAttribute), false).Cast<XmlCommentAttribute>().Single().Value);
                }

                writer.WriteElementString(propertyInfo.Name, propertyInfo.GetValue(this, null).ToString());
            }
        }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class XmlCommentAttribute : Attribute
    {
        public string Value { get; set; }
    }

}