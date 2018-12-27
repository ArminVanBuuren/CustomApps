using System;
using System.Collections;
using System.Web;
using System.Web.Services;
using System.Runtime.Remoting.Channels.Tcp;
using FORIS.MG.Interfaces;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Xml;
//using System.Diagnostics;

namespace ChangeSIM
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/CCDealerIntegrationWS/ChangeSIM")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class ChangeSIM : System.Web.Services.WebService
    {
        public ChangeSIM()
        {
            //Uncomment the following line if using designed components 
            //InitializeComponent(); 
            if (!IsConnected) Connect();
        }
        private static object SyncRoot = new object();
        private static TcpClientChannel clientChannel = null;
        private static IMGRemoteInvocation soapConnector = null;

        private static void Connect()
        {
            Hashtable Props = new Hashtable();
            Props["name"] = "ChangeSIM2SOAPConnector";
            Props["timeout"] = 120000;  //120 seconds. The value must be greater than delay in CRM
            Props["retryCount"] = "3";

            //     url = System.Configuration.ConfigurationSettings.AppSettings["URL"];  obsolete 
            string url = System.Configuration.ConfigurationManager.AppSettings["URL"];
            lock (SyncRoot)
            {
                clientChannel = new TcpClientChannel(Props, new BinaryClientFormatterSinkProvider());
                try
                {
                    if (Array.IndexOf(ChannelServices.RegisteredChannels, clientChannel) < 0)
                        ChannelServices.RegisterChannel(clientChannel, false);
                }
                catch (RemotingException) //The channel has already been registered
                {
                }
                soapConnector = Activator.GetObject(typeof(IMGRemoteInvocation), url) as IMGRemoteInvocation;
                if (soapConnector == null)
                    throw new ApplicationException("Could not locate SOAPConnector");
            }
        }
        private static void Disconnect()
        {
            lock (SyncRoot)
            {
                if (clientChannel != null && Array.IndexOf(ChannelServices.RegisteredChannels, clientChannel) >= 0)
                {
                    ChannelServices.UnregisterChannel(clientChannel);
                    clientChannel = null;
                }
                soapConnector = null;
            }
        }

        private static bool IsConnected
        {
            get
            {
                lock (SyncRoot)
                {
                    return clientChannel != null && soapConnector != null;
                }
            }
        }
        private static Hashtable InvokeMethod(string Methodname, Hashtable inMethodParams)
        {
            try
            {
                if (!IsConnected) Connect();
            }
            catch (Exception e)
            {
                Disconnect();
                throw new Exception("Connection error: " + e.Message);
            }
            try
            {
                return soapConnector.InvokeMethod(Methodname, inMethodParams);
            }
            catch (OperationCanceledException e)
            {
                Disconnect();
                throw new Exception("Connections limit error: " + e.Message);
            }
            catch (ArgumentNullException e)
            {
                Disconnect();
                throw new Exception("Argument error: " + e.Message);
            }
            catch (TimeoutException e)
            {
                Disconnect();
                throw new Exception("Timeout error: " + e.Message);
            }
            catch (Exception e)
            {
                Disconnect();
                throw new Exception("General exception: " + e.Message);
            }
        }
        /*
        private static void WriteLog(string logMessage) 
        {
            const string ApplicationSourceName = "FORIS.MessagingGateway.WebService.ChangeSIM";
            const string EventLogName = "Application";
            EventLog ServiceEventLog;

            if (!System.Diagnostics.EventLog.SourceExists(ApplicationSourceName))
                System.Diagnostics.EventLog.CreateEventSource(ApplicationSourceName, EventLogName);

            ServiceEventLog = new EventLog();
            ServiceEventLog.Source = ApplicationSourceName;
            ServiceEventLog.WriteEntry(logMessage, EventLogEntryType.Error, 0, 0);
        }
        */
        private const int ERRORCODE = -999;

        [WebMethod]
        public XmlNode ReplacementPossibility(string MSISDN, string ChangeSimType, string OperatorLogin)
        {
            Hashtable inMethodParams = new Hashtable();
            inMethodParams.Add("MSISDN",MSISDN);
            inMethodParams.Add("ChangeSimType", ChangeSimType);
            inMethodParams.Add("OperatorLogin", OperatorLogin);

            string errormsg = "";
            long ResultCode = ERRORCODE;
            string IMSI = "";
            try
            {
                Hashtable outMethodParams = InvokeMethod("ReplacementPossibility", inMethodParams);
                if (outMethodParams != null)
                {
                    ResultCode = Convert.ToInt64((string)outMethodParams["ResultCode"]);
                    IMSI = (string)outMethodParams["IMSI"];
                }
                else errormsg = "Remote invocation failed!";
            }
            catch(Exception e)
            {
                errormsg = e.Message;
            }
            string errortag = (errormsg == "") ? "" : string.Format("<ErrorMessage>{0}</ErrorMessage>", errormsg);

            string xmlformat =
            @"<?xml version=""1.0"" encoding=""utf-16""?>
            <ChangeSIM>
                <IMSI>{0}</IMSI> 
                <ResultCode>{1}</ResultCode>
                {2}
            </ChangeSIM>";

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(String.Format(xmlformat, IMSI, ResultCode, errortag));
            return xdoc.DocumentElement;
        }

        [WebMethod]
        public long ChangeSIMInitiation(string MSISDN, string NewIMSI, string ChangeSimType, string SourceMeans, string IsTarification, string OperatorLogin)
        {
            Hashtable inMethodParams = new Hashtable();
            inMethodParams.Add("MSISDN", MSISDN);
            inMethodParams.Add("NewIMSI", NewIMSI);
            inMethodParams.Add("OperatorLogin", OperatorLogin);
            inMethodParams.Add("ChangeSimType", ChangeSimType);
            inMethodParams.Add("SourceMeans", SourceMeans);
            inMethodParams.Add("IsTarification", IsTarification);

            long ResultCode = ERRORCODE;
            try
            {
                Hashtable outMethodParams = InvokeMethod("ChangeSIMInitiation", inMethodParams);
                if (outMethodParams != null)
                {
                    ResultCode = Convert.ToInt64((string)outMethodParams["ResultCode"]);
                }
            }
            catch (Exception) 
            {
            }
            return ResultCode;
        }

        [WebMethod]
        public long CheckChangeSIMStatus(long ReqID, string OperatorLogin)
        {
            Hashtable inMethodParams = new Hashtable();
            inMethodParams.Add("ReqID", ReqID.ToString());
            inMethodParams.Add("OperatorLogin", OperatorLogin);

            long ResultCode = ERRORCODE;
            try
            {
                Hashtable outMethodParams = InvokeMethod("CheckChangeSIMStatus", inMethodParams);
                if (outMethodParams != null)
                {
                    ResultCode = Convert.ToInt64((string)outMethodParams["ResultCode"]);
                }
            }
            catch (Exception) 
            {
            }
            return ResultCode;
        }
    }
}
