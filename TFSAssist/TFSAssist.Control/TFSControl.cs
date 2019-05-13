using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TFSAssist.Control.DataBase.Datas;
using TFSAssist.Control.DataBase.Settings;
using Utils;

namespace TFSAssist.Control
{
    // Необходимо подложить библиотеки Microsoft.WITDataStore32.dll и Microsoft.WITDataStore64.dll в bin для AnyCpu платформы
    // Необходимо подложить библиотеку Microsoft.WITDataStore32.dll в bin для x86 платформы
    // Необходимо подложить библиотеку Microsoft.WITDataStore64.dll в bin для x64 платформы

    struct StatusString
    {
        public const string Initialization = "Initialization...";
        public const string ConnectingMail = "Connecting to Mail-server...";
        public const string ConnectingTFS = "Connecting to TFS-server...";
        public const string Processing = "Processing...";
        public const string Sleeping = "Sleeping...";
        public const string Completed = "Completed";
        public const string Aborted = "Aborted";
        public const string ProcessingError = "Processing Error";
        public const string FatalExeption = "Fatal Exception!";
    }

    public sealed partial class TFSControl
    {
        public event EventHandler IsCompleted;
        private const RegexOptions _default = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase;
        const RegexOptions _filterRegOpt = RegexOptions.IgnoreCase | RegexOptions.Compiled;

        Func<string, string, bool> _mailFilter;
        private TfsTeamProjectCollection _tfsService;
        private WorkItemStore _workItemStore;
        private ExchangeService _exchangeService;
        private FolderId _exchangeFolder;

        private readonly object _objectLock = new object();
        Regex ParceSubject { get; set; }
        Regex ParceBody { get; set; }
        static readonly Regex _checkEmailAddress = new Regex(@"^[A-Z0-9a-z._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,64}$", RegexOptions.Compiled);
        private MailItem? _tempLastMailItem;

        void StartPerforming()
        {
            lock (_objectLock)
            {
                try
                {
                    //=========================================================
                    _tempLastMailItem = null;
                    _log.LogItemId = 0;
                    _log.OnStatusChanged(StatusString.Initialization);
                    _log.OnWriteLog($"ThreadID=[{Thread.CurrentThread.ManagedThreadId}] Priority=[{Thread.CurrentThread.Priority}]");

                    string mailFilterFrom = Settings.MailOption.FilterMailFrom.Value.Trim();
                    string mailFilterSubject = Settings.MailOption.FilterSubject.Value.Trim();
                    string logFilterRules;

                    if (!string.IsNullOrEmpty(mailFilterFrom) && !string.IsNullOrEmpty(mailFilterSubject))
                    {
                        _mailFilter = (from, subject) => Regex.IsMatch(from, mailFilterFrom, _filterRegOpt) &&
                                                         Regex.IsMatch(subject, mailFilterSubject, _filterRegOpt);
                        logFilterRules = $"From=[{mailFilterFrom}] And Subject=[{mailFilterSubject}] Options=[{_filterRegOpt:G}]";
                    }
                    else if (!string.IsNullOrEmpty(mailFilterFrom))
                    {
                        _mailFilter = (from, subject) => Regex.IsMatch(from, mailFilterFrom, _filterRegOpt);
                        logFilterRules = $"From=[{mailFilterFrom}] Options=[{_filterRegOpt:G}]";
                    }
                    else if (!string.IsNullOrEmpty(mailFilterSubject))
                    {
                        _mailFilter = (from, subject) => Regex.IsMatch(subject, mailFilterSubject, _filterRegOpt);
                        logFilterRules = $"Subject=[{mailFilterSubject}] Options=[{_filterRegOpt:G}]";
                    }
                    else
                    {
                        _mailFilter = (from, subject) => true;
                        logFilterRules = "No filter will be use.";
                    }

                    _log.OnWriteLog($"Regex pattern for filter mails - {logFilterRules}");
                    ParceSubject = new Regex(Settings.MailOption.ParceSubject[0].Value, _default);
                    _log.OnWriteLog($"Regex pattern for parsing subject of mail. Pattern=[{Settings.MailOption.ParceSubject[0].Value}] Options=[{ParceSubject.Options:G}]");
                    ParceBody = new Regex(Settings.MailOption.ParceBody[0].Value, _default | RegexOptions.Multiline);
                    _log.OnWriteLog($"Regex pattern for parsing body of mail. Pattern=[{Settings.MailOption.ParceBody[0].Value}] Options=[{ParceBody.Options:G}]");
                    //=========================================================
                    ConnectToMailServer();
                    ConnectToTFSServer();
                    //=========================================================
                    StartProcess();
                    //=========================================================
                    _log.OnStatusChanged(StatusString.Completed);
                }
                catch (ThreadAbortException)
                {
                    //Thread.ResetAbort(); - отменить аборт
                    InProgress = false;
                }
                catch (Exception fatal)
                {
                    _log.OnStatusChanged(StatusString.FatalExeption);
                    _log.OnWriteLog(WarnSeverity.Error, $"Process stopped.", fatal, true);
                }
                finally
                {
                    if (!InProgress)
                    {
                        _log.OnStatusChanged(StatusString.Aborted);
                    }
                    else
                    {
                        InProgress = false;
                    }
                    IsCompleted?.Invoke(this, EventArgs.Empty);
                    SaveProcessigDatas();
                }
            }
        }

        /// <summary>
        /// Коннект к MAIL
        /// </summary>
        void ConnectToMailServer()
        {
            _log.OnStatusChanged(StatusString.ConnectingMail);

            _exchangeService = new ExchangeService(ExchangeVersion.Exchange2013_SP1)
            {
                TraceListener = new TraceListener(_log.OnWriteLog),
                TraceFlags = TraceFlags.All,
                TraceEnabled = true,
                KeepAlive = true,
                Timeout = int.Parse(Settings.MailOption.AuthorizationTimeout.Value) * 1000
            };

            var _mailPassword = new SecureString();
            if (Settings.MailOption.Password.Value != null)
                foreach (char ch in Settings.MailOption.Password.Value)
                    _mailPassword.AppendChar(ch);

            if(_mailPassword.Length == 0)
                throw new ArgumentException($"You must enter password.");

            _log.OnWriteLog($"Address=[{Settings.MailOption.Address.Value}] UserName=[{Settings.MailOption.UserName.Value}] Exchange URI=[{Settings.MailOption.ExchangeUri.Value}]");

            if (!Settings.MailOption.ExchangeUri.Value.IsNullOrEmptyTrim() && !Settings.MailOption.UserName.Value.IsNullOrEmptyTrim())
            {
                var domain_username = Settings.MailOption.UserName.Value.Split('\\');
                if (domain_username.Length == 2)
                    _exchangeService.Credentials = new NetworkCredential(domain_username[1].Trim(), _mailPassword, domain_username[0].Trim());
                else
                    _exchangeService.Credentials = new NetworkCredential(Settings.MailOption.UserName.Value, _mailPassword);

                _exchangeService.Url = new Uri(Settings.MailOption.ExchangeUri.Value);
                //AlternateIdBase response = _exchangeService.ConvertId(new AlternateId(IdFormat.EwsId, "Placeholder", domain_username[1].Trim()), IdFormat.EwsId);
            }
            else if (!Settings.MailOption.Address.Value.IsNullOrEmptyTrim() && _checkEmailAddress.IsMatch(Settings.MailOption.Address.Value)) // Необходим только Email Address и пароль, т.к. вызывается другой способ подключения
            {
                _exchangeService.Credentials = new NetworkCredential(Settings.MailOption.Address.Value, _mailPassword);
                _exchangeService.AutodiscoverUrl(Settings.MailOption.Address.Value, RedirectionUrlValidationCallback);
                if (!Settings.MailOption.ExchangeUri.Value.Equals(_exchangeService.Url.AbsoluteUri))
                {
                    _log.OnWriteLog($"New Exchange URI=[{_exchangeService.Url.AbsoluteUri}]");
                    Settings.MailOption.ExchangeUri.Value = _exchangeService.Url.AbsoluteUri; // обновляем ссылку на путь до Exchange сервера почты
                }
            }
            else
            {
                throw new ArgumentException($"Authorization data is incorrect! Please check mail's authorization fields.");
            }

            // проверяем коннект к почтовому серверу через считывание папки Входящие
            try
            {
                _exchangeFolder = new FolderId(WellKnownFolderName.Inbox);
                Folder.Bind(_exchangeService, _exchangeFolder);
            }
            catch (ServiceRequestException ex)
            {
                throw new Exception("Connection error to Exchange server! Please check your authorization data.", ex);
            }
            finally
            {
                _exchangeService.TraceEnabled =
                    false; // во время дальнейшей обработки данная опция уже бсполезна, т.к. после получения ошибок при подключении к серверу в дебагe уже ничего не пишется об этом, он просто будет спамить что получил от сервера и передал
            }


            // если указан фильтр по папке, то найти папку на почте и обрабатывать письмо только из этой папки
            var folderName = Settings.MailOption.SourceFolder.Value.Trim();
            Folder folderFilter = null;
            if (!folderName.IsNullOrEmpty())
            {
                var inboxFolders = _exchangeService.FindFolders(WellKnownFolderName.MsgFolderRoot, new FolderView(int.MaxValue)
                {
                    Traversal = FolderTraversal.Deep
                });

                if (GetFolderId(inboxFolders, folderName, out folderFilter))
                {
                    _exchangeFolder = folderFilter.Id;
                }
            }

            _log.OnWriteLog($"Successful connected to Exchange server. Reading folder:[{(folderFilter.DisplayName.IsNullOrEmpty() ? "Inbox" : folderFilter.DisplayName)}]");
        }

        static bool GetFolderId(FindFoldersResults inboxFolders, string folderName, out Folder folderOut)
        {
            folderOut = null;
            foreach (Folder folder in inboxFolders)
            {
                if (!folder.DisplayName.Equals(folderName, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                folderOut = folder;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Коннект к TFS
        /// </summary>
        void ConnectToTFSServer()
        {
            _log.OnStatusChanged(StatusString.ConnectingTFS);

            // Если в настройках xml файла нет указаний на создание каких либо TFS
            if (Settings.TFSOption.TFSCreate.TeamProjects == null || Settings.TFSOption.TFSCreate.TeamProjects.Length == 0)
                throw new TFSFieldsException("TeamProjects Not Found! Please check config file.");

            Uri collectionUri = new Uri(Settings.TFSOption.TFSUri.Value);

            // Коннект по кастомному логину и паролю
            if (!Settings.TFSOption.TFSUserName.Value.IsNullOrEmptyTrim())
            {
                string[] tfs_domain_username = Settings.TFSOption.TFSUserName.Value.Split('\\');
                if (tfs_domain_username.Length != 2 || tfs_domain_username[0].IsNullOrEmpty() || tfs_domain_username[1].IsNullOrEmpty())
                    throw new ArgumentException("You must add Domain and UserName for TFS-server like: \"Domain\\Username\"");

                var _tfsUserPassword = new SecureString();
                if (Settings.TFSOption.TFSUserPassword.Value != null)
                    foreach (char ch in Settings.TFSOption.TFSUserPassword.Value)
                        _tfsUserPassword.AppendChar(ch);

                NetworkCredential credential = new NetworkCredential(tfs_domain_username[1].Trim(), _tfsUserPassword, tfs_domain_username[0].Trim());
                _tfsService = new TfsTeamProjectCollection(collectionUri, credential);

                _log.OnWriteLog($"TFS authorization=[{tfs_domain_username[0].Trim()}\\{tfs_domain_username[1].Trim()}]");
            }
            else
            {
                _tfsService = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(collectionUri);
                _log.OnWriteLog($"TFS authorization=[Auto Mode]");
            }

            _tfsService.Authenticate();
            _tfsService.EnsureAuthenticated();
            _workItemStore = _tfsService.GetService<WorkItemStore>();

            _log.OnWriteLog($"Successful connected to TFS server.");
        }




        /// <summary>
        /// The following is a basic redirection validation callback method. It 
        /// inspects the redirection URL and only allows the Service object to 
        /// follow the redirection link if the URL is using HTTPS. 
        /// </summary>
        /// <param name="redirectionUrl"></param>
        /// <returns></returns>
        internal static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                return true;
            }

            // The default for the validation callback is to reject the URL.
            return false;
        }


        /// <summary>
        /// бесконечный процесс обработки, пока не возникнет эксепшн, в том числе аборт процесса
        /// </summary>
        void StartProcess()
        {
            while (InProgress)
            {
                Processing();
                _log.OnStatusChanged(StatusString.Sleeping, WarnSeverity.StatusRegular);
                Thread.Sleep(Settings.Interval.Value * 1000);
            }
        }

        /// <summary>
        /// Получаем все письма из папки folderId
        /// </summary>
        /// <param name="service"></param>
        /// <param name="folderId"></param>
        /// <param name="numberOfMessages"></param>
        /// <param name="numberOfAttempts"></param>
        /// <returns></returns>
        public MailItem[] GetUnreadMailFromInbox(ExchangeService service, FolderId folderId, int numberOfMessages, ref int numberOfAttempts)
        {
            try
            {
                numberOfAttempts++;
                //https://stackoverflow.com/questions/36069801/ews-read-mail-plain-text-body-getting-serviceobjectpropertyexception
                PropertySet GetItemsPropertySet = new PropertySet(BasePropertySet.FirstClassProperties, EmailMessageSchema.From, EmailMessageSchema.ToRecipients);
                GetItemsPropertySet.RequestedBodyType = BodyType.Text;

                Folder inbox = Folder.Bind(service, folderId);
                FindItemsResults<Item> findResults = inbox.FindItems(new ItemView(numberOfMessages));

                ServiceResponseCollection<GetItemResponse> items = service.BindToItems(findResults.Select(item => item.Id), GetItemsPropertySet);

                return items.Select(item =>
                                    {
                                        string[] uniqueId = item.Item.Id.UniqueId.Split('+');
                                        return new MailItem {
                                                                //ID = item.Item.Id.ChangeKey + item.Item.Id.UniqueId,
                                                                ID = uniqueId[uniqueId.Length - 1],
                                                                From = ((EmailAddress) item.Item[EmailMessageSchema.From]).Address,
                                                                Recipients = ((EmailAddressCollection) item.Item[EmailMessageSchema.ToRecipients])
                                                                        .Select(recipient => recipient.Address)
                                                                        .ToArray(),
                                                                Subject = item.Item.Subject,
                                                                ReceivedDate = item.Item.DateTimeReceived,
                                                                Body = item.Item.Body.Text
                                                            };
                                    }).ToArray();
            }
            catch (ServiceRequestException ex)
            {
                return ExchangeExceptionHandle(service, folderId, numberOfMessages, ref numberOfAttempts, ex);
            }
            catch (ServiceResponseException ex)
            {
                return ExchangeExceptionHandle(service, folderId, numberOfMessages, ref numberOfAttempts, ex);
            }
        }

        MailItem[] ExchangeExceptionHandle(ExchangeService service, FolderId folderId, int numberOfMessages, ref int numberOfAttempts, Exception ex)
        {
            if (numberOfAttempts >= 5)
                throw new Exception($"Error connecting to Exchange server! {numberOfAttempts} connection attempts were made.", ex);

            Thread.Sleep(30 * 1000);
            return GetUnreadMailFromInbox(service, folderId, numberOfMessages, ref numberOfAttempts);
        }

        
        /// <summary>
        /// Основная обработка писем и создания TFS заявки
        /// </summary>
        void Processing()
        {
            DateTime lastProcessingDate = DateTime.Parse(Settings.MailOption.StartDate.Value);

            _log.OnStatusChanged(StatusString.Processing, WarnSeverity.StatusRegular);
            
            int countExeptions = 0;
            int numberOfAttempts = 0;

            // получаем последнее письмо которые к нам пришло (самое свежее и проверяем дату)
            MailItem[] items = GetUnreadMailFromInbox(_exchangeService, _exchangeFolder, 100, ref numberOfAttempts);

            // если ничего не найденно то сразу завершаем выполнение метода
            if (items.Length <= 0)
                return;

            // получаем дату самого свежего письма, если дата заданного пользователем или последней обработки больше чем дата последнего письма в новой обработки то можно сразу же завершать действие, т.к. даже самое первое письмо не будет попадать под условие, остальные более тем более
            MailItem firstMailItem = items[0];
            if (lastProcessingDate > firstMailItem.ReceivedDate || _tempLastMailItem != null && _tempLastMailItem.Equals(firstMailItem))
                return;
            _tempLastMailItem = firstMailItem;

            int countOfProcessing = 0;
            foreach (MailItem item in items)
            {
                //Если дата письма меньше чем дата начала то завершаем обработку
                if (lastProcessingDate > item.ReceivedDate)
                    break;

                _log.LogItemId++;
                _log.OnWriteLog($"Received new item. ReceivedDate=[{item.ReceivedDate:G}] From=[{item.From}] Subject=[{item.Subject}]", true);

                //Если валидация по адресату или теме письма не совпадает с регулярным выражением то пропускаем обработку
                if (!_mailFilter.Invoke(item.From, item.Subject))
                {
                    _log.OnWriteLog($"Ignored. Rejected by filter.", true);
                    continue;
                }

                //Если это письмо уже обрабатывалось, то проверяем на валидность создания TFS
                TMData task = Datas.IsExist(item);
                if (task != null)
                {
                    //Если успешно он раньше был создан то пропускаем обработку
                    if (task.Status == ProcessingStatus.Created || task.Status == ProcessingStatus.Skipped)
                    {
                        _log.OnWriteLog($"TFS already exist - [{task.Executed.TFSID}]", true);
                        continue;
                    }

                    Datas.Remove(task);
                }

                // если валидация прошла успешно и поступило новое письмо или в предыдущей обработке таск был создан ошибочно, то будет пересоздаваться
                countOfProcessing++;

                task = new TMData {
                                      ID = item.ID,
                                      From = item.From,
                                      ReceivedDate = item.ReceivedDate.ToString("G")
                                  };

                try
                {
                    //Распарсиваем необходимые нам данные из тела и темы письма
                    List<DataMail> parced = ParceBodyAndSubject(item.Subject, item.Body);

                    if (parced.Count > 0)
                    {
                        task.Executed = new ItemExecuted {
                                                             MailParcedItems = parced
                                                         };
                        //создаем ТФС
                        bool resultCreateTFS = CheckAndCreateTFSItem(task.Executed, out var tfsId);
                        task.Executed.TFSID = tfsId;
                        //Если уже TFS создавался то пропускаем обработку, и записываем ранее созданный TFSID
                        task.Status = resultCreateTFS ? ProcessingStatus.Created : ProcessingStatus.Skipped;
                    }
                    else
                    {
                        task.Status = ProcessingStatus.Skipped;
                    }

                    Datas.Add(task);
                }
                catch (TFSFieldsException ex)
                {
                    //Если в настройках что то было неверно заполненно
                    task.Status = ProcessingStatus.Failure;
                    Datas.Add(task);

                    countExeptions++;

                    string logProcessing = $"Processing Error!\r\n";
                    Exception detailedEx = ex;
                    while (true)
                    {
                        logProcessing += $"{detailedEx.Message}\r\n";

                        if (detailedEx.InnerException != null)
                        {
                            detailedEx = detailedEx.InnerException;
                            continue;
                        }
                        break;
                    }

                    _log.OnWriteLog(logProcessing);
                }
                catch (Exception)
                {
                    //Фатальная ошибка если ее выкинул сам TFS обработчик при поиске и создания TFS
                    task.Status = ProcessingStatus.FatalException;
                    Datas.Add(task);

                    throw;
                }
            }

            //Обновляем данные в форме приложения и указываем что будем искать следующие письма от даты последнего письма, не важно попал ли он в валидацию или нет, т.к. мы его уже считали и все более старые от этого письма тоже
            if (firstMailItem.ReceivedDate > lastProcessingDate)
                Settings.MailOption.StartDate.Value = firstMailItem.ReceivedDate.ToString("G");

            if (countExeptions > 0)
            {
                //отправляем список ошибок которые возможно связанны с настроками конфига
                _log.OnStatusChanged(StatusString.ProcessingError);
                _log.OnWriteLog(WarnSeverity.Warning, $"Catched {countExeptions} processing exceptions.");
                Thread.Sleep(10 * 1000);
            }


            if (countOfProcessing > 0) //обновляем файл с обработанными данными
                SaveProcessigDatas();
        }

        List<DataMail> ParceBodyAndSubject(string subject, string body)
        {
            List<DataMail> parced = new List<DataMail>();

            GroupCollection fromSubject = ParceSubject.Match(subject).Groups;
            _log.OnWriteLog($"Start parsing content of Subject=[{subject}].", true);

            foreach (string match in ParceSubject.GetGroupNames())
            {
                if (!match.IsNumber())
                {
                    DataMail dm = new DataMail
                    {
                        Name = $"{nameof(OptionMail.ParceSubject)}_{match}",
                        Value = fromSubject[match].Value.Trim()
                    };
                    parced.Add(dm);

                    _log.OnWriteLog($"{dm}", true);
                }
            }


            GroupCollection fromBody = ParceBody.Match(body).Groups;
            _log.OnWriteLog($"Start parsing content of Body=[{body}]", true);

            foreach (string match in ParceBody.GetGroupNames())
            {
                if (!match.IsNumber())
                {
                    DataMail dm = new DataMail
                    {
                        Name = $"{nameof(OptionMail.ParceBody)}_{match}",
                        Value = fromBody[match].Value.Trim()
                    };
                    parced.Add(dm);

                    _log.OnWriteLog($"{dm}", true);
                }
            }

            return parced;
        }

        bool CheckAndCreateTFSItem(ItemExecuted itemExec, out string createdTfsId)
        {
            createdTfsId = string.Empty;
            //запускаем скрипт по поиску дублей
            string query = itemExec.ReplaceParcedValues(Settings.TFSOption.GetDublicateTFS[0].Value);
            if (!query.IsNullOrEmptyTrim())
            {
                _log.OnWriteLog($"Check duplicates. Query=[{query}]", true);
                WorkItemCollection workQuery = _workItemStore.Query(query);

                if (workQuery.Count > 0)
                {
                    foreach (WorkItem queryResult in workQuery)
                    {
                        createdTfsId += queryResult.Id + ";";
                    }

                    _log.OnWriteLog($"Query result: [{createdTfsId}]. Processing skipped.", true);
                    return false;
                }
            }
            else
            {
                _log.OnWriteLog("Query of check dublicates is null. Processing continues.", true);
            }
            return CreateTFSItem(itemExec, ref createdTfsId);
        }

        bool CreateTFSItem(ItemExecuted itemExec, ref string createdTfsId)
        {
            _log.OnWriteLog("Start searching TeamProject...", true);
            foreach (TeamProjectCondition teamProj in Settings.TFSOption.TFSCreate.TeamProjects)
            {
                //проверяем условие в аттрибуте Condition, но сначала выполняется функция getParcedValue для замены необходимых спарсенных данных
                if (!teamProj.GetConditionResult(itemExec.ReplaceParcedValues, _log))
                    continue;

                //если не указана имя проекта TFS
                if (teamProj.Value.IsNullOrEmpty())
                    throw new TFSFieldsException("TeamProject's attribute=[Value] mustn't be empty!");
                if (teamProj.WorkItems == null || teamProj.WorkItems.Length == 0)
                {
                    _log.OnWriteLog($"Not found any workItems from TeamProject=[{teamProj.Value}]", true);
                    continue;
                }

                _log.OnWriteLog($"Found TeamProject=[{teamProj.Value}]. Start searching WorkItems...", true);
                Project teamProject = _workItemStore.Projects[teamProj.Value];

                foreach (WorkItemCondition workItem in teamProj.WorkItems)
                {
                    //проверяем условие в аттрибуте Condition, но сначала выполняется функция getParcedValue для замены необходимых спарсенных данных
                    if (!workItem.GetConditionResult(itemExec.ReplaceParcedValues, _log))
                        continue;

                    //если не указан тип создания заявки TFS
                    if (workItem.Value.IsNullOrEmpty())
                        throw new TFSFieldsException("WorkItem's attribute=[Value] mustn't be empty!");
                    if (workItem.Fields == null || workItem.Fields.Length == 0)
                    {
                        _log.OnWriteLog($"Not found any fields from workItem=[{workItem.Value}]", true);
                        continue;
                    }

                    _log.OnWriteLog($"Found WorkItem=[{workItem.Value}]. Start reading Fields...", true);

                    WorkItemType workItemType = teamProject.WorkItemTypes[workItem.Value];
                    string displayForm = workItemType.DisplayForm;
                    WorkItem tfsWorkItem = new WorkItem(workItemType);
                    string failedFieldsStr = string.Empty;

                    try
                    {
                        //заполняем все поля которые были указаны в конфиге для обпределенного типа заявки TFS
                        foreach (FieldCondition field in workItem.Fields)
                        {
                            string getFrmFieldValue = field.GetSwitchValue(itemExec.ReplaceParcedValues, _log.OnWriteLog);
                            _log.OnWriteLog($"{field.ToString()} FinalValue=[{getFrmFieldValue}]", true);

                            //кастомный аттрибуты Control.XXX
                            if (field.Name.Equals("Control.AssignedTo", StringComparison.CurrentCultureIgnoreCase))
                            {
                                //"Control.AssignedTo", если не заполнен то подставляется текущий пользователь
                                tfsWorkItem.Fields["Assigned To"].Value = getFrmFieldValue.Trim().IsNullOrEmpty()
                                    ? _tfsService.AuthorizedIdentity.DisplayName
                                    : getFrmFieldValue;
                                continue;
                            }
                            if (field.Name.Equals("Control.Links", StringComparison.CurrentCultureIgnoreCase))
                            {
                                //кастомный аттрибут "Control.Links", если он не заполнен то к этому item линкуются все возможные таски
                                foreach (string link in getFrmFieldValue.Split(';',','))
                                {
                                    if(link.Trim().IsNullOrEmpty())
                                        continue;

                                    int linkTfsId;
                                    if (int.TryParse(link, out linkTfsId))
                                    {
                                        tfsWorkItem.Links.Add(new RelatedLink(linkTfsId));
                                    }
                                    else
                                    {
                                        throw new TFSFieldsException($"Incorrect Value=[{link}] from Filed=[{field.Name}]. Please check Fields => TeamProject Condition=[{teamProj.Condition}] => WorkItem Condition=[{workItem.Condition}] in your config.");
                                    }
                                }
                                continue;
                            }

                            tfsWorkItem.Fields[field.Name].Value = getFrmFieldValue;
                        }

                        try
                        {
                            tfsWorkItem.Save();
                        }
                        catch (Exception)
                        {
                            var failedFields = tfsWorkItem.Validate();
                            foreach (Field failFld in failedFields)
                            {
                                failedFieldsStr += $"Name=[{failFld.Name}] Value=[{failFld.Value}] Status=[{failFld.Status:G}] ReferenceName=[{failFld.ReferenceName}] Description=[{failFld.FieldDefinition.HelpText}]\r\n";
                            }
                            throw;
                        }
                        
                        createdTfsId += tfsWorkItem.Id + ";";
                        _log.OnWriteLog($"Successfully created {workItem.Value}=[{createdTfsId}] in TeamProject=[{teamProj.Value}]", true);
                    }
                    catch (ValidationException ex) // ошибка если не все обязательные поля были заполнены или были указаны неверные значения
                    {
                        //получаем все обязательные поля для заполнения, чтобы в случае эксепшена знать какие поля необходимо заполнить
                        StringBuilder reqFieldsStr = tfsWorkItem.Fields.OfType<Field>().Where(field => field.IsRequired).Aggregate(new StringBuilder(), (current, field) => AddParams(current, field));
                        string res = reqFieldsStr.ToString();
                        throw new TFSFieldsException($"Error when create [{workItem.Value}] in TFS [{teamProj.Value}]! Please check Fields => TeamProject Condition=[{teamProj.Condition}] => WorkItem Condition=[{workItem.Condition}] in your config.\r\nFailed Fields:\r\n{failedFieldsStr}{new string('=', 50)}\r\nAll Required Fields:\r\n{res}{new string('=', 50)}", ex);
                    }
                    catch (Exception)
                    {
                        if (!displayForm.IsNullOrEmpty())
                            _log.OnWriteLog($"Detailed display form for [{teamProj.Value}:{workItem.Value}]=[{displayForm}]");
                        throw;
                    }
                }
            }

            return !createdTfsId.IsNullOrEmpty();
        }

        StringBuilder AddParams(StringBuilder builder, Field field)
        {
            builder.Append($"Name=[{field.Name}] Value=[{field.Value}] Descritpion=[{field.FieldDefinition?.HelpText}]\r\n");
            //builder.Append(FormatParam("Value", field.Value == null ? "Null" : field.Value.ToString(), 37));
            //builder.Append(FormatParam("ReferenceName", field.ReferenceName, 35));
            //builder.Append($"Descritpion=[{field.FieldDefinition?.HelpText}]\r\n");
            return builder;
        }

        string FormatParam(string paramName, string paramValue, int maxSpaces = 15)
        {
            string spaces = " ";
            if (maxSpaces - paramValue?.Length > 0)
                spaces = new string(' ', maxSpaces - paramValue.Length);

            return $"{paramName}=[{paramValue}]{spaces}";
        }
    }
}