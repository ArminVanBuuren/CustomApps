using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Utils.Handles
{
    public class Address
    {
        public enum Type
        {
            Tcp,
            Msmq
        }

        const string _tagMsmq = "msmq";
        const string _tagTcp = "tcp";
        const string _tagPath = "path";
        const string _tagHost = "host";
        const string _tagPort = "port";

        const string _msgIncorrectFormat = "Incorrect address format";
        const string _msgNotInitialized = "Address is not initialized";

        public const string TemplateTcp = "<tcp host='' port='' />";
        public const string TemplateMsmq = "<msmq path='' />";

        string _string;
        string _xmlString;
        string _path;
        string _host;
        uint _port;
        bool _initialized;
        Type _addrType;

        public Address(string str)
        {
            Parse(str);
        }

        public Address(XmlNode node)
        {
            var sb = new StringBuilder();
            if (node.Name == _tagMsmq)
                sb.Append(node.Attributes?[_tagPath].Value);
            if (node.Name == _tagTcp)
            {
                sb.Append(node.Attributes?[_tagHost].Value);
                sb.Append(":");
                sb.Append(node.Attributes?[_tagPort].Value);
            }

            Parse(sb.ToString());
        }

        private void Parse(string str)
        {
            _string = str.Trim();

            var regexTcp = new Regex($"^(tcp://)?(?<{_tagHost}>[A-Za-z0-9\\-\\._]+):(?<{_tagPort}>\\d+)$");
            var regexMsmq = new Regex("^([A-Za-z0-9\\-\\._]+\\\\(PRIVATE\\$\\\\)?[A-Za-z0-9\\-\\._]+|FORMATNAME:DIRECT=[A-Za-z0-9\\-\\._]+:[A-Za-z0-9\\-\\._]+\\\\(PRIVATE\\$\\\\)?[A-Za-z0-9\\-\\._]+|FORMATNAME:MULTICAST=[A-Za-z0-9\\-\\._]+:\\d+|FORMATNAME:DL=\\S{36}(@[A-Za-z0-9\\-\\._]+)?)$", RegexOptions.IgnoreCase);

            var matchTcp = regexTcp.Match(_string);
            var matchMsmq = regexMsmq.Match(_string);

            if (matchTcp.Success)
            {
                _host = matchTcp.Groups[_tagHost].Value;
                if (uint.TryParse(matchTcp.Groups[_tagPort].Value, out _port) == false)
                    throw new Exception($"{_msgIncorrectFormat}: '{str}'");
                _addrType = Type.Tcp;
                _string = _host + ":" + _port.ToString();
                _xmlString = $"<{_tagTcp} {_tagHost}='{_host}' {_tagPort}='{_port}' />";
            }
            else if (matchMsmq.Success)
            {
                _path = _string;
                _addrType = Type.Msmq;
                _string = _path;

                _xmlString = $"<{_tagMsmq} {_tagPath}='{_path}' />";
            }
            else
            {
                throw new Exception($"{_msgIncorrectFormat}: '{str}'");
            }

            _initialized = true;
        }

        #region Public nethods

        public Address Clone()
        {
            return new Address(_string);
        }

        public new string ToString()
        {
            if (_initialized)
                return _string;
            else
                throw new Exception(_msgNotInitialized);
        }

        public string ToXmlString()
        {
            if (_initialized)
                return _xmlString;
            else
                throw new Exception(_msgNotInitialized);
        }

        public XmlElement ToXmlElement(XmlDocument context)
        {
            if (_initialized)
            {
                XmlElement el = null;
                switch (_addrType)
                {
                    case Type.Msmq:
                        el = context.CreateElement(_tagMsmq);
                        el.Attributes.Append(context.CreateAttribute(_tagPath));
                        el.Attributes[_tagPath].Value = _path;
                        break;
                    case Type.Tcp:
                        el = context.CreateElement(_tagTcp);
                        el.Attributes.Append(context.CreateAttribute(_tagHost));
                        el.Attributes.Append(context.CreateAttribute(_tagPort));
                        el.Attributes[_tagHost].Value = _host;
                        el.Attributes[_tagPort].Value = _port.ToString();
                        break;
                }

                return el;
            }
            else
                throw new Exception(_msgNotInitialized);
        }

        public XmlDocument ToXmlDocument()
        {
            if (_initialized)
            {
                var doc = new XmlDocument();
                doc.LoadXml(_xmlString);
                return doc;
            }
            else
                throw new Exception(_msgNotInitialized);
        }

        #endregion

        #region Public properties

        public string Path
        {
            get
            {
                if (_initialized)
                    return _path;
                else
                    throw new Exception("Address not initialized");
            }
        }

        public string Host
        {
            get
            {
                if (_initialized)
                    return _host;
                else
                    throw new Exception("Address not initialized");
            }
        }

        public uint Port
        {
            get
            {
                if (_initialized)
                    return _port;
                else
                    throw new Exception("Address not initialized");
            }
        }

        public Type AddrType
        {
            get
            {
                if (_initialized)
                    return _addrType;
                else
                    throw new Exception("Address not initialized");
            }
        }

        #endregion
    }
}