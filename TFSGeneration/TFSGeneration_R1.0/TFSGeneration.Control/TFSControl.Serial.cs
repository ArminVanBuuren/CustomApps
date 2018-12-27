using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml.Serialization;
using TFSAssist.Control.DataBase.Datas;
using TFSAssist.Control.DataBase.Settings;
using Utils;
using Utils.Crypto;
using static Utils.Customs;

namespace TFSAssist.Control
{
    public delegate void NotifyStatusHandler(string message);
    public delegate void WriteLogHandler(string stackTrace);
    public delegate void NotifyOfAnErrorHandler(WarnSeverity severity, string message, string stackMessage, bool lockProcess);

    public enum WarnSeverity
    {
        Info = 0,
        Attention = 1,
        Warning = 2,
        Error = 3
    }

    [Serializable]
    sealed partial class TFSControl : ISerializable, IDisposable
    {
        public static event NotifyOfAnErrorHandler NotifyUserAnError;
        public static string ApplicationName { get; }
        public static string ApplicationPath { get; }
        public static string ShortDateTimeFormat { get; }
        public static string LongDateTimeFormat { get; }
        internal static string AccountStorePath { get; }
        internal static string SettingsPath { get; }
        internal static string DataBasePath { get; }
        internal static string RegeditKey { get; }
        internal static string NotifyDeSerializetionFailor { get; } = "File:\"{0}\" is incorrect! It will be re-created after exit this apllication.";

        static TFSControl()
        {
            
            CertificateCallback.Initialize();
            AccountStorePath = $"{ApplicationFilePath}.dat";
            SettingsPath = $"{ApplicationFilePath}.xml";
            DataBasePath = $"{ApplicationFilePath}.Data.xml";
            //ApplicationName = Assembly.GetCallingAssembly().GetName().Name;
            ApplicationName = Assembly.GetEntryAssembly().GetName().Name;
            ApplicationPath = Assembly.GetEntryAssembly().Location; 
            RegeditKey = GetOrSetRegedit(ApplicationName, "This application implements reading mail items and on their basis creation TFS.");
            System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            //Thread.CurrentThread.CurrentCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();

            //обязательно устанавливаем необходимый формат даты под культуру "ru-RU"
            ShortDateTimeFormat = "dd.MM.yyyy";
            LongDateTimeFormat = "dd.MM.yyyy HH:mm:ss";
            Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = ShortDateTimeFormat;
            Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongDatePattern = LongDateTimeFormat;
        }

        /// <summary>
        /// Получить TFSControl
        /// </summary>
        /// <returns></returns>
        public static TFSAssist.Control.TFSControl GetControl()
        {
            TFSAssist.Control.TFSControl tfsControl = null;

            //десериализация необходимых свойств класса TFSControl
            if (File.Exists(AccountStorePath))
            {
                try
                {
                    using (Stream stream = new FileStream(AccountStorePath, FileMode.Open, FileAccess.Read))
                    {
                        tfsControl = new BinaryFormatter().Deserialize(stream) as TFSAssist.Control.TFSControl;
                    }
                }
                catch (Exception ex)
                {
                    //File.Delete(AccountStorePath);
                    NotifyUserIfHasError(WarnSeverity.Attention, string.Format(NotifyDeSerializetionFailor, AccountStorePath), ex, true);
                }
            }

            if (tfsControl == null)
                tfsControl = new TFSAssist.Control.TFSControl();

            return tfsControl;
        }

        //-------------------------------------- Reference Instance Controls (Non-Static) -----------------------------------

        public event NotifyStatusHandler NotifyUserAnStatus;
        public event WriteLogHandler WriteLog;

        //public event EventHandler ActivateWindow;
        private Thread _asyncThread;


        /// <summary>
        /// Возвращает статус TFSControl, если он занят обработкой
        /// </summary>
        public bool InProgress { get; private set; }

        /// <summary>
        /// Необходимые настройки TFSControl
        /// </summary>
        public SettingsCollection Settings { get; private set; }

        /// <summary>
        /// Храняться данные которые уже были обработанны
        /// </summary>
        public DataCollection Datas { get; private set; }

        TFSControl()
        {
            Initialize();
        }

        /// <summary>
        /// Когда происходит десериализация класса TFSControl
        /// </summary>
        /// <param name="propertyBag"></param>
        /// <param name="context"></param>
        TFSControl (SerializationInfo propertyBag, StreamingContext context)
        {
            Initialize();

            if (Settings == null)
                throw new ArgumentException(string.Format("'{0}' Is Not Initialized!", SettingsPath));

            try
            {
                object[] types =
                {
                    Settings.MailOption,
                    Settings.TFSOption
                };

                foreach (object tpObj in types)
                {
                    Type tp = tpObj.GetType();
                    //получить все свойства класса SettingsCollection, Для заполнения расшифрованными данными (логины, пароли)
                    PropertyInfo[] props = tp.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

                    foreach (SerializationEntry result in propertyBag)
                    {
                        foreach (PropertyInfo prop in props)
                        {
                            if (result.Name.Equals(prop.Name))
                            {
                                //SettingValue<string> settValue =  result.Value as SettingValue<string>;
                                string settValue = result.Value as string;
                                if (settValue == null)
                                    continue;
                                prop.SetValue(tpObj, new SettingValue<string>() {
                                                                                    Value = AES.DecryptStringAES(settValue, RegeditKey)
                                                                                });
                            }
                        }
                    }
                }
            }
            catch (CryptoException ex)
            {
                //расшифровка файла неудачна, возможно если ключ в реестре отличается от ключа зашифрованных значений
                NotifyUserIfHasError(WarnSeverity.Attention, string.Format(NotifyDeSerializetionFailor, AccountStorePath), ex, true);
            }
            catch (Exception ex)
            {
                NotifyUserIfHasError(WarnSeverity.Attention, string.Format(NotifyDeSerializetionFailor, AccountStorePath), ex, true);
            }
        }

        /// <summary>
        /// Когда происходит сериализация класса TFSControl (с аттрибутом XmlIgnore)
        /// которые нужно сериализовать и шифровать по ключю из реестра
        /// </summary>
        /// <param name="propertyBag"></param>
        /// <param name="context"></param>
        void ISerializable.GetObjectData(SerializationInfo propertyBag, StreamingContext context)
        {
            object[] types =
            {
                Settings.MailOption,
                Settings.TFSOption
            };

            foreach (object tpObj in types)
            {
                Type tp = tpObj.GetType();
                PropertyInfo[] props = tp.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                foreach (PropertyInfo prop in props)
                {
                    if (prop.PropertyType != typeof(SettingValue<string>))
                        continue;

                    object[] attrs = prop.GetCustomAttributes(true);
                    foreach (object attr in attrs)
                    {
                        XmlIgnoreAttribute identAttr = attr as XmlIgnoreAttribute;
                        if (identAttr == null)
                            continue;


                        SettingValue<string> fieldTextBox = (SettingValue<string>)prop.GetValue(tpObj);
                        propertyBag.AddValue(prop.Name, AES.EncryptStringAES(fieldTextBox.Value, RegeditKey));
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Основная иницализация всегда выполняется первой, даже в случае десериализации TFSControl
        /// </summary>
        void Initialize()
        {
            uint tryLoad = 0;
            Settings = LoadSettings(SettingsPath, ref tryLoad);
            Datas = DeserializeDatas(DataBasePath) ?? new DataCollection();
        }

        static SettingsCollection LoadSettings(string settPath, ref uint tryLoad)
        {
            SettingsCollection sett = DeserializeSettings(settPath);

            if (sett != null)
                return sett;

            if (tryLoad == 0)
            {
                // создаем настройки из файла примера в ресурсах Example_Config и бэкапим неудавшуюся настройку
                FileInfo fileInfo = new FileInfo(settPath);
                if (fileInfo.Exists)
                {
                    int index = 0;
                    string bakFileName = $"{settPath}_Incorrect.bak";
                    while (File.Exists(bakFileName))
                    {
                        bakFileName = $"{settPath}_Incorrect_{++index}.bak";
                    }
                    File.Copy(settPath, bakFileName);
                }
                
                using (StreamWriter tw = new StreamWriter(settPath, false))
                {
                    // var thisExe = System.Reflection.Assembly.GetExecutingAssembly();
                    tw.Write(Properties.Resources.Example_Config);
                    tw.Close();
                }

                tryLoad++;
                return LoadSettings(settPath, ref tryLoad);
            }
            else
            {
                // если даже из ресурсов пример не подходит, то просто создаем новый настройки
                return new SettingsCollection();
            }
        }

        #region Deserialize - Serialize Settings And Datas

        /// <summary>
        /// Сериализуем (обновляем) обработанные данные
        /// </summary>
        public void SerializeDatas()
        {
            try
            {
                using (FileStream stream = new FileStream(DataBasePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    new XmlSerializer(typeof(DataCollection)).Serialize(stream, Datas);
                }
            }
            catch (Exception exSer)
            {
                NotifyUserIfHasError(WarnSeverity.Error, string.Format("Cant't save Datas to=[{1}]{0}{2}", Environment.NewLine, DataBasePath, exSer.Message),
                                     exSer);
            }
        }

        static DataCollection DeserializeDatas(string datasPath)
        {
            if (!File.Exists(datasPath))
                return null;

            try
            {
                using (FileStream stream = new FileStream(datasPath, FileMode.Open, FileAccess.Read))
                {
                    return new XmlSerializer(typeof(DataCollection)).Deserialize(stream) as DataCollection;
                }
            }
            catch (Exception ex)
            {
                NotifyUserIfHasError(WarnSeverity.Attention, string.Format(NotifyDeSerializetionFailor, datasPath), ex, true);
            }
            return null;
        }

        /// <summary>
        /// Сериализуем (обновляем) конфигурацию с настройками
        /// </summary>
        public void SerializeSettings()
        {
            try
            {
                using (FileStream stream = new FileStream(SettingsPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    new XmlSerializer(typeof(SettingsCollection)).Serialize(stream, Settings);
                }
            }
            catch (Exception exSer)
            {
                NotifyUserIfHasError(WarnSeverity.Error, string.Format("Cant't save Settings to=[{1}]{0}{2}", Environment.NewLine, SettingsPath, exSer.Message),
                    exSer);
            }
        }

        static SettingsCollection DeserializeSettings(string settPath)
        {
            if (!File.Exists(settPath))
                return null;

            try
            {
                using (FileStream stream = new FileStream(settPath, FileMode.Open, FileAccess.Read))
                {
                    return new XmlSerializer(typeof(SettingsCollection)).Deserialize(stream) as SettingsCollection;
                }
            }
            catch (Exception ex)
            {
                NotifyUserIfHasError(WarnSeverity.Attention, string.Format(NotifyDeSerializetionFailor, settPath), ex, true);
            }
            return null;
        }

        /// <summary>
        /// сериализация приватных данных, паролей и логинов
        /// </summary>
        /// <param name="mainControl"></param>
        static void SerializePrivateDatas(TFSControl mainControl)
        {
            using (FileStream stream = new FileStream(AccountStorePath, FileMode.Create, FileAccess.ReadWrite))
            {
                new BinaryFormatter().Serialize(stream, mainControl);
            }
        }

        #endregion

        /// <summary>
        /// Если все настройки верны, запускается процесс обработки
        /// </summary>
        public void Start()
        {
            if (InProgress)
                return;

            SerializeSettings();
            _asyncThread = new Thread(new ThreadStart(StartPerforming));
            _asyncThread.Start();
        }

        /// <summary>
        /// Остановить процесс обработки
        /// </summary>
        public void Stop()
        {
            if (!InProgress)
                return;

            _asyncThread?.Abort();
        }

        #region Notifications To Main Window

        /// <summary>
        /// Уведомление пользователя по статусу работы приложения (нижняя строка приложения)
        /// </summary>
        /// <param name="message"></param>
        void NotifyUserCurrentStatus(string message)
        {
            NotifyUserAnStatus?.Invoke(message);
        }

        /// <summary>
        /// Нотифицируем в случае ошибки при выполнении - выскакивает окно с ошибкой
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="lockProcess"></param>
        static void NotifyUserIfHasError(WarnSeverity severity, string message, Exception ex, bool lockProcess = false)
        {
            if (NotifyUserAnError == null)
                return;

            string stackMessage = string.Empty;
            GetSourceException(ex, ref stackMessage);
            NotifyUserIfHasError(severity, message, string.Format("{0}{1}{2}", stackMessage, Environment.NewLine, ex.StackTrace), lockProcess);
        }

        static void GetSourceException(Exception ex, ref string result)
        {
            while (true)
            {
                result = string.Format("{0}[{1}]:{2}", result.IsNullOrEmpty() ? string.Empty : result + Environment.NewLine, ex.GetType().Name, ex.Message);

                if (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    continue;
                }
                break;
            }
        }

        static void NotifyUserIfHasError(WarnSeverity severity, string message, string stackMessage, bool lockProcess = false)
        {
            NotifyUserAnError?.Invoke(severity, message, stackMessage, lockProcess);
        }

        void OnWriteLog (string stacktrace)
        {
            WriteLog?.Invoke(stacktrace);
        }

        #endregion




        public void Dispose()
        {
            //обязательно удалять асинхронный поток
            _asyncThread?.Abort();


            // Возникает эксепшн если закрыть всплювающие окна на автоизацию к TFS серверу
            //_tfsService?.Disconnect();
            //_tfsService?.Dispose();


            SerializeSettings();
            SerializeDatas();
            SerializePrivateDatas(this);
            //сериализация приватных данных, паролей и логинов
            //using (FileStream stream = new FileStream(AccountStorePath, FileMode.Create, FileAccess.ReadWrite))
            //{
            //    new BinaryFormatter().Serialize(stream, this);
            //}
        }


    }
}
