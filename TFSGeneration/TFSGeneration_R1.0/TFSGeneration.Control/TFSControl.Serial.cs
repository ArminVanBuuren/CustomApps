﻿using System;
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
using Utils.Handles;
using static Utils.ASSEMBLY;

namespace TFSAssist.Control
{
    [Serializable]
    sealed partial class TFSControl : ISerializable, IDisposable
    {
        public static string ApplicationName { get; }
        public static string ApplicationPath { get; }
        internal static string AccountStorePath { get; }
        internal static string SettingsPath { get; }
        internal static string DataBasePath { get; }
        internal static string RegeditKey { get; }
        static string NotifyDeSerializetionFailor { get; } = "File:\"{0}\" is incorrect and will be re-created if you close this apllication.";
        private LogPerformer _log;

        static TFSControl()
        {
            CertificateCallback.Initialize();
            AccountStorePath = $"{ApplicationFilePath}.dat";
            SettingsPath = $"{ApplicationFilePath}.xml";
            DataBasePath = $"{ApplicationFilePath}.Data.xml";
            ApplicationName = Assembly.GetEntryAssembly().GetName().Name;
            ApplicationPath = Assembly.GetEntryAssembly().Location;

            string description = "Application implements reading mails, and by their basis creating TFS items.";
            using (RegeditControl regControl = new RegeditControl(ApplicationName))
            {
                if(!description.Equals(regControl["Description"]))
                {
                    regControl["Description"] = description;
                }

                RegeditKey = regControl["Key"].ToString();
                if (RegeditKey.IsNullOrEmptyTrim())
                {
                    RegeditKey = Guid.NewGuid().ToString("D");
                    regControl["Key"] = RegeditKey;
                }
            }
        }

        /// <summary>
        /// Получить TFSControl
        /// </summary>
        /// <returns></returns>
        public static TFSAssist.Control.TFSControl GetControl(LogPerformer log)
        {
            if (log == null)
                throw new ArgumentException(nameof(_log));

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
                    log.OnWriteLog(WarnSeverity.Attention, string.Format(NotifyDeSerializetionFailor, AccountStorePath), ex, true);
                }
            }

            if (tfsControl == null)
                tfsControl = new TFSAssist.Control.TFSControl();

            tfsControl._log = log;

            return tfsControl;
        }

        //-------------------------------------- Reference Instance Controls (Non-Static) -----------------------------------

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
                throw new ArgumentException($"Wrong file configuration from Path=\"{SettingsPath}\". Please check!");

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
                _log.OnWriteLog(WarnSeverity.Attention, string.Format(NotifyDeSerializetionFailor, AccountStorePath), ex, true);
            }
            catch (Exception ex)
            {
                _log.OnWriteLog(WarnSeverity.Attention, string.Format(NotifyDeSerializetionFailor, AccountStorePath), ex, true);
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

        SettingsCollection LoadSettings(string settPath, ref uint tryLoad)
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
                    string bakFileName = $"{settPath}_incorrect.bak";
                    while (File.Exists(bakFileName))
                    {
                        bakFileName = $"{settPath}_incorrect_{++index}.bak";
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

        #region (Serialize - Deserialize) settings and datas

        SettingsCollection DeserializeSettings(string settPath)
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
                _log.OnWriteLog(WarnSeverity.Attention, string.Format(NotifyDeSerializetionFailor, settPath), ex, true);
            }
            return null;
        }

        DataCollection DeserializeDatas(string datasPath)
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
                _log.OnWriteLog(WarnSeverity.Attention, string.Format(NotifyDeSerializetionFailor, datasPath), ex, true);
            }
            return null;
        }

        /// <summary>
        /// Сериализуем (обновляем) конфигурацию с настройками
        /// </summary>
        void SerializeSettings()
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
                _log.OnWriteLog(WarnSeverity.Error, $"Unable to save '{nameof(SettingsCollection)}' to specified path=[{SettingsPath}]", exSer);
            }
        }

        /// <summary>
        /// сериализация приватных данных, паролей и логинов
        /// </summary>
        void SerializePrivateSettings()
        {
            try
            {
                using (FileStream stream = new FileStream(AccountStorePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    new BinaryFormatter().Serialize(stream, this);
                }
            }
            catch (Exception ex)
            {
                _log.OnWriteLog(WarnSeverity.Attention, string.Format(NotifyDeSerializetionFailor, AccountStorePath), ex, true);
            }
        }

        /// <summary>
        /// Сериализуем (обновляем) обработанные данные
        /// </summary>
        void SerializeDatas()
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
                _log.OnWriteLog(WarnSeverity.Error, $"Unable to save '{nameof(DataCollection)}' to specified path=[{DataBasePath}]", exSer);
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

        public void SaveSettings()
        {
            SerializeSettings();
            SerializePrivateSettings();
        }

        void SaveProcessigDatas()
        {
            SerializeSettings();
            SerializeDatas();
        }

        public void Dispose()
        {
            //обязательно удалять асинхронный поток
            _asyncThread?.Abort();

            SerializeSettings();
            SerializePrivateSettings();
            SerializeDatas();
        }
    }
}
