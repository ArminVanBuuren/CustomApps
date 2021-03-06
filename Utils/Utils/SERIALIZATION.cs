using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Utils
{
    public static class SERIALIZATION
    {
        /// <summary>
        /// Get root node element name for type. Determines and returns the name of the XmlElement that should represent instances of the given type
        /// in an XML stream.
        /// </summary>
        /// <param name="serializedObjectType"></param>
        /// <returns></returns>        
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static string GetRNENFT(Type serializedObjectType)
        {
            // Determine if the Type contains an XmlRoot Attribute.  If so, the XmlRoot attribute should contain
            // the name of the element-name for this type.
            // Otherwise, the name of the type should 've been used for serializing objects of this type.

            if (Attribute.GetCustomAttribute(serializedObjectType, typeof(XmlRootAttribute)) is XmlRootAttribute theAttrib)
            {
                if (string.IsNullOrEmpty(theAttrib.ElementName) == false)
                {
                    return theAttrib.ElementName;
                }
                else
                {
                    return serializedObjectType.Name;
                }
            }
            else
            {
                return serializedObjectType.Name;
            }
        }
    }
}
