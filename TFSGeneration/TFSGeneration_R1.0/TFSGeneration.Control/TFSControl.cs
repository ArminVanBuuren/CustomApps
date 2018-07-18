using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TFSAssist.Control.DataBase;
using TFSAssist.Control.DataBase.Datas;
using TFSAssist.Control.DataBase.Settings;

namespace TFSAssist.Control
{
    public sealed partial class TFSControl
    {
        private const string STR_INITIALIZATION = "Initialization...";
        private const string STR_CONNECTING = "Connecting...";
        private const string STR_PROCESSING = "Processing...";
        private const string STR_SLEEPING = "Sleeping...";
        private const string STR_COMPLETED = "Completed";
        private const string STR_ABORTED = "Aborted";
        private const string STR_PROC_ERROR = "Processing Error";



        public event EventHandler IsCompleted;
        private const RegexOptions _default = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase;

        Func<string, string, bool> _validation;
        private TfsTeamProjectCollection _tfsService;
        private WorkItemStore _workItemStore;
        private ExchangeService _exchangeService;
        private FolderId _exchangeFolder;

        private object _objectLock = new object();
        private Thread _workerThread;
        Regex ParceSubject { get; set; }
        Regex ParceBody { get; set; }
        static Regex _checkEmailAddress = new Regex(@"^[A-Z0-9a-z._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,64}$", RegexOptions.Compiled);

        void StartPerforming()
        {
            lock (_objectLock)
            {
                try
                {
                    //=========================================================
                    InProgress = true;
                    NotifyUserCurrentStatus(STR_INITIALIZATION);
                    _workerThread = Thread.CurrentThread;

                    string mailFilterFrom = Settings.MailOption.FilterMailFrom.Value.Trim();
                    string mailFilterSubject = Settings.MailOption.FilterSubject.Value.Trim();

                    const RegexOptions regOpt = RegexOptions.IgnoreCase | RegexOptions.Compiled;
                    if (!string.IsNullOrEmpty(mailFilterFrom) && !string.IsNullOrEmpty(mailFilterSubject))
                    {
                        _validation = (from, subject) => Regex.IsMatch(from, mailFilterFrom, regOpt) &&
                                                         Regex.IsMatch(subject, mailFilterSubject, regOpt);
                    }
                    else if (!string.IsNullOrEmpty(mailFilterFrom))
                        _validation = (from, subject) => Regex.IsMatch(from, mailFilterFrom, regOpt);
                    else if (!string.IsNullOrEmpty(mailFilterSubject))
                        _validation = (from, subject) => Regex.IsMatch(subject, mailFilterSubject, regOpt);
                    else
                        _validation = (from, subject) => true;

                    ParceSubject = new Regex(Settings.MailOption.ParceSubject[0].Value, _default);
                    ParceBody = new Regex(Settings.MailOption.ParceBody[0].Value, _default | RegexOptions.Multiline);
                    //=========================================================
                    Connect();
                    //=========================================================
                    StartProcess();
                    //=========================================================
                    NotifyUserCurrentStatus(STR_COMPLETED);
                }
                catch (ThreadAbortException)
                {
                    //Thread.ResetAbort();
                    NotifyUserCurrentStatus(STR_ABORTED);
                }
                catch (Exception fatal)
                {
                    NotifyUserCurrentStatus(fatal.Message);
                    NotifyUserIfHasError(WarnSeverity.Error,
                        string.Format("Processing was stopped. {0}{1}", Environment.NewLine, fatal.Message), fatal,
                        true);
                }
                finally
                {
                    InProgress = false;
                    IsCompleted?.Invoke(this, EventArgs.Empty);
                    SerializeDatas();
                }
            }
        }

        /// <summary>
        /// Коннект к TFS и Mail
        /// </summary>
        void Connect()
        {
            NotifyUserCurrentStatus(STR_CONNECTING);

            _exchangeService = new ExchangeService(ExchangeVersion.Exchange2013_SP1);
            _exchangeService.TraceListener = new TraceListener(OnWriteLog);
            _exchangeService.TraceFlags = TraceFlags.All;
            _exchangeService.TraceEnabled = Settings.MailOption.DebugLogging.Value;
            _exchangeService.KeepAlive = true;
            _exchangeService.Timeout = int.Parse(Settings.MailOption.AuthorizationTimeout.Value) * 1000;

            SecureString _mailPassword = new SecureString();
            if (Settings.MailOption.Password.Value != null)
                foreach (char ch in Settings.MailOption.Password.Value)
                    _mailPassword.AppendChar(ch);


            if (!TM.IsNullOrEmptyTrim(Settings.MailOption.ExchangeUri.Value) && !TM.IsNullOrEmptyTrim(Settings.MailOption.UserName.Value))
            {
                string[] domain_username = Settings.MailOption.UserName.Value.Split('\\');
                if (domain_username.Length != 2 || domain_username[0].IsNullOrEmpty() || domain_username[1].IsNullOrEmpty())
                    throw new ArgumentException("You must add UserName and Domain like: \"Domain\\Username\"");

                _exchangeService.Credentials = new NetworkCredential(domain_username[1].Trim(), _mailPassword, domain_username[0].Trim());
                _exchangeService.Url = new Uri(Settings.MailOption.ExchangeUri.Value);
                //AlternateIdBase response = _exchangeService.ConvertId(new AlternateId(IdFormat.EwsId, "Placeholder", domain_username[1].Trim()), IdFormat.EwsId);
            }
            else if (!TM.IsNullOrEmptyTrim(Settings.MailOption.Address.Value) && _checkEmailAddress.IsMatch(Settings.MailOption.Address.Value)) // Необходим только Email Address и пароль, т.к. вызывается другой способ подключения
            {
                _exchangeService.Credentials = new NetworkCredential(Settings.MailOption.Address.Value, _mailPassword);
                _exchangeService.AutodiscoverUrl(Settings.MailOption.Address.Value, RedirectionUrlValidationCallback);
                OnWriteLog(string.Format("Absolute Uri to Exchange Server - \"{0}\"", _exchangeService.Url.AbsoluteUri));
                Settings.MailOption.ExchangeUri.Value =
                    _exchangeService.Url.AbsoluteUri; // обновляем ссылку на путь до Exchange сервера почты
            }
            else
            {
                throw new ArgumentException(string.Format(
                    "Mail Address=[{0}] Or Domain\\Username=[{1}] With ExchangeUri=[{2}] is Incorrect! Please check fields.",
                    TM.ToStringIsNullOrEmptyTrim(Settings.MailOption.Address.Value),
                    TM.ToStringIsNullOrEmptyTrim(Settings.MailOption.UserName.Value),
                    TM.ToStringIsNullOrEmptyTrim(Settings.MailOption.ExchangeUri.Value)));
            }

            // проверяем коннект к почтовому серверу
            try
            {
                _exchangeFolder = new FolderId(WellKnownFolderName.Inbox);
                Folder checkConnect = Folder.Bind(_exchangeService, _exchangeFolder);
            }
            catch (ServiceRequestException ex)
            {
                throw new Exception("Error connecting to Exchange Server! Please check your authorization data.", ex);
            }
            finally
            {
                _exchangeService.TraceEnabled = false; // во время дальнейшей обработки данная опция уже бсполезна, т.к. после получения ошибок при подключении к серверу в дебагe уже ничего не пишется об этом, он просто будет спамить что получил от сервера и передал
            }


            // если указан фильтр по папке, то найти папку на почте и обрабатывать письмо только из этой папки
            string findFolder = Settings.MailOption.SourceFolder.Value.Trim();
            if (!findFolder.IsNullOrEmpty())
            {
                //находим все дочение папки из папки Входящие (Inbox)
                FindFoldersResults inboxFolders = _exchangeService.FindFolders(WellKnownFolderName.Inbox,
                    new FolderView(int.MaxValue) {Traversal = FolderTraversal.Deep});
                if (inboxFolders != null)
                {
                    foreach (Folder folder in inboxFolders)
                    {
                        if (!folder.DisplayName.Equals(findFolder, StringComparison.CurrentCultureIgnoreCase))
                            continue;
                        _exchangeFolder = folder.Id;
                        break;
                    }
                }
            }

            // Если в настройках xml файла нет указаний на создание каких либо TFS
            if (Settings.TFSOption.TFSCreate.TeamProjects == null || Settings.TFSOption.TFSCreate.TeamProjects.Length == 0)
                throw new TFSFieldsException("TeamProjects Not Found! Please check config file.");

            Uri collectionUri = new Uri(Settings.TFSOption.TFSUri.Value);

            // Коннект по кастомному логину и паролю
            if (!TM.IsNullOrEmptyTrim(Settings.TFSOption.TFSUserName.Value))
            {
                string[] tfs_domain_username = Settings.TFSOption.TFSUserName.Value.Split('\\');
                if (tfs_domain_username.Length != 2 || tfs_domain_username[0].IsNullOrEmpty() || tfs_domain_username[1].IsNullOrEmpty())
                    throw new ArgumentException("You must add TFS-UserName and TFS-Domain like: \"Domain\\Username\"");

                SecureString _tfsUserPassword = new SecureString();
                if (Settings.TFSOption.TFSUserPassword.Value != null)
                    foreach (char ch in Settings.TFSOption.TFSUserPassword.Value)
                        _tfsUserPassword.AppendChar(ch);

                NetworkCredential credential = new NetworkCredential(tfs_domain_username[1].Trim(), _tfsUserPassword, tfs_domain_username[0].Trim());
                _tfsService = new TfsTeamProjectCollection(collectionUri, credential);
            }
            else
            {
                _tfsService = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(collectionUri);
            }

            _tfsService.Authenticate();
            _tfsService.EnsureAuthenticated();
            _workItemStore = _tfsService.GetService<WorkItemStore>();

            //NotifyUserIfHasError(WarnSeverity.Warning, string.Format("Please see log tab. Catched: {0} processing errors!", 10), "1111");
            //Test1();
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
            while (true)
            {
                Processing();
                NotifyUserCurrentStatus(STR_SLEEPING);
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
                    return new MailItem
                    {
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
                throw new Exception(string.Format("Error connecting to Exchange Server! {0} connection attempts were made to the server.", numberOfAttempts), ex);

            Thread.Sleep(30 * 1000);
            return GetUnreadMailFromInbox(service, folderId, numberOfMessages, ref numberOfAttempts);
        }

        /// <summary>
        /// Основная обработка писем и создания TFS заявки
        /// </summary>
        void Processing()
        {
            DateTime lastProcessingDate = DateTime.Parse(Settings.MailOption.StartDate.Value);

            // из за культуры могут возникнуть проблемы, т.к. текущая дата может быть одна, а дата на почте может быть совсем другая и обработка будет не корректной
            // если выставленная дата больше чем текущая дата, то ждать когда эта дата наступит - данная модификация не верна, если только можно сделать синхронизацю культуры
            //if (lastProcessingDate > DateTime.Now)
            //    return;


            NotifyUserCurrentStatus(STR_PROCESSING);
            string logProcessing = string.Empty;
            int countErrors = 0;
            int numberOfAttempts = 0;

            // получаем последнее письмо которые к нам пришло (самое свежее и проверяем дату)
            MailItem[] items = GetUnreadMailFromInbox(_exchangeService, _exchangeFolder, 100, ref numberOfAttempts);

            // если ничего не найденно то сразу завершаем выполнение метода
            if (items.Length <= 0)
                return;

            // получаем дату последнего письма, если дата заданного пользователем или последней обработки больше чем дата последнего письма в новой обработки то можно сразу же завершать действие, т.к. даже самое первое письмо не будет попадать под условие, остальные более тем более
            DateTime dateOfFirstMailItem = items[0].ReceivedDate;
            if (lastProcessingDate > dateOfFirstMailItem)
                return;

            int countOfProcessing = 0;
            foreach (MailItem item in items)
            {

                //Если дата письма меньше чем дата начала то завершаем обработку
                if (lastProcessingDate > item.ReceivedDate)
                    break;

                //Если валидация по адресату или теме письма не совпадает с регулярным выражением то пропускаем обработку
                if (!_validation.Invoke(item.From, item.Subject))
                    continue;

                //Если это письмо уже обрабатывалось, то проверяем на валидность создания TFS
                TMData _task = Datas.IsExist(item);
                if (_task != null)
                {
                    //Если успешно он раньше был создан то пропускаем обработку
                    if (_task.Status == ProcessingStatus.Created || _task.Status == ProcessingStatus.Skipped)
                        continue;
                    Datas.Remove(_task);
                }

                // если валидация прошла успешно и пустопуло новое письмо или в предыдущей обработке таск был создан ошибочно, то будет пересоздаваться
                countOfProcessing++;

                _task = new TMData
                {
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
                        _task.Executed = new ItemExecuted
                        {
                            MailParcedItems = parced
                        };
                        string tfsId;
                        //создаем ТФС
                        bool resultCreateTFS = CheckAndCreateTFSItem(_task.Executed.ReplaceParcedValues, out tfsId);
                        _task.Executed.TFSID = tfsId;
                        //Если уже TFS создавался то пропускаем обработку, и записываем ранее созданный TFSID
                        _task.Status = resultCreateTFS ? ProcessingStatus.Created : ProcessingStatus.Skipped;
                    }
                    else
                    {
                        _task.Status = ProcessingStatus.Skipped;
                    }

                    Datas.Add(_task);
                }
                catch (TFSFieldsException ex)
                {
                    //Если в настройках что то было неверно заполненно
                    _task.Status = ProcessingStatus.Failure;
                    Datas.Add(_task);

                    countErrors++;
                    logProcessing += string.Format("Processing Error!{0}ReceivedDate=[{1}] Subject=[{2}]{0}{3}{0}{4}{0}{5}{0}", Environment.NewLine, item.ReceivedDate, item.Subject, ex.Message, ex.StackTrace, new string('=', 47));
                }
                catch (Exception)
                {
                    //Фатальная ошибка если ее выкинул сам TFS обработчик при поиске и создания TFS
                    _task.Status = ProcessingStatus.FatalException;
                    Datas.Add(_task);

                    throw;
                }
            }

            //Обновляем данные в форме приложения и указываем что будем искать следующие письма от даты последнего письма, не важно попал ли он в валидацию или нет, т.к. мы его уже считали и все более старые от этого письма тоже
            if (dateOfFirstMailItem > lastProcessingDate)
                Settings.MailOption.StartDate.Value = dateOfFirstMailItem.ToString("G");

            if (countErrors > 0)
            {
                //отправляем список ошибок которые возможно связанны с настроками конфига
                NotifyUserCurrentStatus(STR_PROC_ERROR);
                NotifyUserIfHasError(WarnSeverity.Warning, string.Format("Please see log tab. Catched: {0} processing errors!", countErrors),
                    logProcessing.Trim());
                Thread.Sleep(10 * 1000);
            }


            if (countOfProcessing > 0) //обновляем файл с обработанными данными
                SerializeDatas();
        }

        List<DataMail> ParceBodyAndSubject(string subject, string body)
        {
            GroupCollection fromSubject = ParceSubject.Match(subject).Groups;
            GroupCollection fromBody = ParceBody.Match(body).Groups;
            List<DataMail> parced = new List<DataMail>();

            foreach (string match in ParceSubject.GetGroupNames())
            {
                parced.Add(new DataMail
                {
                    Name = string.Format("{0}_{1}", nameof(OptionMail.ParceSubject), match),
                    Value = fromSubject[match].Value
                });
            }


            foreach (string match in ParceBody.GetGroupNames())
            {
                parced.Add(new DataMail
                {
                    Name = string.Format("{0}_{1}", nameof(OptionMail.ParceBody), match),
                    Value = fromBody[match].Value
                });

            }

            return parced;
        }



        bool CheckAndCreateTFSItem(GetParcedValue getParcedValue, out string createdTfsId)
        {
            createdTfsId = string.Empty;
            //запускаем скрипт по поиску дублей
            string query = getParcedValue(Settings.TFSOption.GetDublicateTFS[0].Value);
            WorkItemCollection workQuery = _workItemStore.Query(query);

            if (workQuery.Count > 0)
            {
                foreach (WorkItem queryResult in workQuery)
                {
                    createdTfsId += queryResult.Id + ";";
                }
                return false;
            }

            return CreateTFSItem(getParcedValue, ref createdTfsId);
        }

        bool CreateTFSItem(GetParcedValue getParcedValue, ref string createdTfsId)
        {
            foreach (TeamProjectCondition teamProj in Settings.TFSOption.TFSCreate.TeamProjects)
            {
                //проверяем условие в аттрибуте Condition, но сначала выполняется функция getParcedValue для замены необходимых спарсенных данных
                if (!teamProj.GetConditionResult(getParcedValue))
                    continue;

                //если не указана имя проекта TFS
                if (teamProj.Value.IsNullOrEmpty())
                    throw new TFSFieldsException("TeamProject's Attribute=[Value] Must Not Be Empty!");
                if (teamProj.WorkItems == null || teamProj.WorkItems.Length == 0)
                    throw new TFSFieldsException("Not Found WorkItems From Project=[{0}]!", teamProj.Value);

                Project teamProject = _workItemStore.Projects[teamProj.Value];

                foreach (WorkItemCondition workItem in teamProj.WorkItems)
                {
                    //проверяем условие в аттрибуте Condition, но сначала выполняется функция getParcedValue для замены необходимых спарсенных данных
                    if (!workItem.GetConditionResult(getParcedValue))
                        continue;

                    //если не указан тип создания заявки TFS
                    if (workItem.Value.IsNullOrEmpty())
                        throw new TFSFieldsException("WorkItem's Attribute=[Value] Must Not Be Empty!");
                    if (workItem.Fields == null || workItem.Fields.Length == 0)
                        throw new TFSFieldsException("Not Found Fields From WorkItem=[{0}]!", workItem.Value);


                    WorkItemType workItemType = teamProject.WorkItemTypes[workItem.Value];
                    WorkItem tfsWorkItem = new WorkItem(workItemType);

                    try
                    {
                        //заполняем все поля которые были указаны в конфиге для обпределенного типа заявки TFS
                        foreach (FieldCondition field in workItem.Fields)
                        {
                            //кастомный аттрибут "Control.AssignedTo", если он не заполнен то подставляется текущий пользователь
                            if (field.Name.Equals("Control.AssignedTo", StringComparison.CurrentCultureIgnoreCase))
                            {
                                string getAssignTo = field.GetSwitchValue(getParcedValue).Trim();
                                tfsWorkItem.Fields["Assigned To"].Value = getAssignTo.IsNullOrEmpty()
                                    ? _tfsService.AuthorizedIdentity.DisplayName
                                    : getAssignTo;
                                continue;
                            }
                            tfsWorkItem.Fields[field.Name].Value = field.GetSwitchValue(getParcedValue);
                        }

                        tfsWorkItem.Save();
                        createdTfsId += tfsWorkItem.Id + ";";
                    }
                    catch (ValidationException ex) // ошибка если не все обязательные поля были заполнены
                    {
                        //получаем все обязательные поля для заполнения, чтобы в случае эксепшена знать какие поля необходимо заполнить
                        string reqFields = tfsWorkItem.Fields.Cast<Field>().Where(field => field.IsRequired)
                                                      .Aggregate(string.Empty, (current, field) => current + (field.ReferenceName + Environment.NewLine)).Trim();

                        throw new TFSFieldsException("Error in creating TFS item! Please check workitem's fields in config.",
                                                     new TFSFieldsException(string.Format("Error in {0}=[{1}] / {2}=[{3}]{4}Required fields:{4}{5}", teamProj.Value, teamProj.Condition, workItem.Value, workItem.Condition, Environment.NewLine, reqFields), ex));
                    }
                }
            }

            return !createdTfsId.IsNullOrEmpty();
        }



        void Test1()
        {
            string query = @"SELECT [System.Id]
            FROM WorkItems
            WHERE [System.ID] = '1373710'";
            WorkItemCollection workQuery = _workItemStore.Query(query);
            foreach (WorkItem item in workQuery)
            {
                foreach (Field field in item.Fields)
                {
                    string fefe = field.Name;
                    object fefe123 = field.Value;
                }
            }


            TMData _task = new TMData
            {
                ID = "1221",
                From = "JIRA",
                ReceivedDate = DateTime.Now.ToString("G")
            };

            List<DataMail> parced = ParceBodyAndSubject(@"[JIRA]  (ITHD-268244) Приоритет: [Высокий] Отв. группа: [МедиаТел(HD)]", @"
     [ http://jira:8090/browse/ITHD-268244?page=com.atlassian.jira.plugin.system.issuetabpanels:all-tabpanel ]

	Здравствуйте!
	На вашу группу [ МедиаТел(HD) ] назначена новая заявка.

	Сообщения в счета

	Код: ITHD-268244
	URL: http://jira:8090/browse/ITHD-268244?page=com.atlassian.jira.plugin.system.issuetabpanels:all-tabpanel
	Проект: ХелпДеск
	Тип запроса: Ошибка

-- 
This message is automatically generated by JIRA.
");

            _task.Executed = new ItemExecuted
            {
                MailParcedItems = parced
            };

            //string temp = _task.Executed.ReplaceParcedValues(Settings.TFSOption.GetDublicateTFS[0].Value);

            string idResult = string.Empty;
            CreateTFSItem(_task.Executed.ReplaceParcedValues, ref idResult);
        }

        void Test2()
        {
            //WorkItemStore workItemStore = _tfsService.GetService<WorkItemStore>();
            Project teamProject = _workItemStore.Projects["Support"];
            WorkItemType workItemType = teamProject.WorkItemTypes["Task"];
            WorkItem tfsTask = new WorkItem(workItemType);
            tfsTask.Fields["Title"].Value = "MyTest";
            tfsTask.Fields["Activity"].Value = "Other";
            tfsTask.Fields["Assigned To"].Value = _tfsService.AuthorizedIdentity.DisplayName;
            tfsTask.Fields["Severity"].Value = @"High";
            tfsTask.Fields["System.AreaPath"].Value = @"Support\Resource Management Domain\SPA";
            tfsTask.Fields["System.IterationPath"].Value = @"Support\Release 4.7.1";
            tfsTask.Fields["Sitronics.RND.Region"].Value = @"MTS.Bel";
            tfsTask.Fields["Sitronics.RND.StsProject"].Value = @"B13002_MTSMinsk_TS_ALL_in_2017";
            tfsTask.Save();
        }

    }
}