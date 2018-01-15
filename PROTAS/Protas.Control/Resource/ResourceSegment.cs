using System;
using System.Collections.Generic;
using Protas.Components.PerformanceLog;
using Protas.Control.Resource.Base;
using Protas.Components.XPackage;

namespace Protas.Control.Resource
{
    internal class ResourceSegment : ShellLog3Net, IDisposable
    {
        delegate string GetResult();
        GetResult _getResult;

        internal string Prefix { get; set; } = string.Empty;
        internal string Postfix { get; set; } = string.Empty;
        public string UniqueId => string.Format("{0}_{1}", Main.UniqueId, Id);
        public int Id { get; }
        public int Block { get; }
        public ResourceKernel Main { get; }
        internal ResourceSegment Parent { get; private set; }
        public ResourceSignature Signature { get; private set; }
        public ResourceHandler Handler { get; private set; }
        public ResourceSegmentCollection ChildSegemnets { get; }
        public bool UseCoreMode { get; private set; } = false;
        public bool IsResource
        {
            get
            {
                if (Handler != null)
                    return Handler.IsResource;
                return false;
            }
        }

        public bool IsCorrectSyntax
        {
            get
            {
                if (Signature != null)
                    return Signature.IsCorrectSyntax;
                return false;
            }
        }
        public bool InBlackList
        {
            get
            {
                if (Signature != null)
                    return Signature.InBlackList;
                return false;
            }
        }
        public bool IsCurrentUniqueType
        {
            get
            {
                //если тип непосредственного ресурса уникален и должен возвращать только уникальные значения
                if (Handler?.Resource?.Type == ResultType.InfinityUnique)
                    return true;
                return false;
            }
        }
        public string Source
        {
            get
            {
                if (ChildSegemnets?.Count > 0)
                    return string.Format("{0}{1}{2}", Prefix, ChildSegemnets.Source, Postfix);
                return string.Format("{0}{1}{2}", Prefix, Signature.Source, Postfix);
            }
        }

        public string Result
        {
            get
            {
                try
                {
                    AddLog(Log3NetSeverity.Max, "Start");
                    //если эвент уже проинициализировался и новый результат уже получен то не необходимости забирать результат еще раз и нашружать процессор
                    //данная метка обеспечивает возврат актуального результат который пришел в метод IsHandlerChanged во время элапсирования эвента
                    if (IsAlreadyElapsed)
                        return string.Format("{0}{1}{2}", Prefix, _prevResult, Postfix);
                    return string.Format("{0}{1}{2}", Prefix, _getResult.Invoke(), Postfix);
                }
                catch(Exception ex)
                {
                    AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.Data);
                    return string.Format("{0}{1}", Prefix, Postfix);
                }
                finally
                {
                    AddLog(Log3NetSeverity.Max, "End");
                }
            }
        }

        /// <param name="block">к какому блоку относится данный сегмент, это учитывается по 0 рангу
        /// к примеру если несколько сегментов: {XXX} {YYY} {ZZZ}=XXX - блок0; YYY - блок1; ZZZ - блок2
        /// </param>
        /// <param name="main">основной обработчик</param>
        /// <param name="id">уникальный номер сегмента</param>
        /// <param name="input">текстовое выражение ресурса или не обычного текста</param>
        public ResourceSegment(int id, string input, int block, ResourceKernel main) : base(main)
        {
            Id = id;
            if (main == null)
                throw new Exception("Machine Resource Is null");
            Block = block;
            Main = main;
            Signature = GetExistOrCreateNewSignature(input);
            AddMessagePrefix(string.Format("SGT:{0}_{1}", block, Id));
            if (Signature.IsCorrectSyntax)
                AddLogForm(Log3NetSeverity.Debug, "Input=\"{4}\" | {0} | IsCorrectSyntax=\"{1}\" | Constructor=\"{2}\" | Props=\"{3}\"", Signature.Shell.ToString(), Signature.IsCorrectSyntax, Signature.Constructor.AsLine, Signature.OutputPath, Signature.Source);
        }

        public ResourceSegment(int id, ResourceSegmentCollection childRes, int block, ResourceKernel main) : base(main)
        {
            Id = id;
            if (main == null)
                throw new Exception("Machine Resource Is null");
            Block = block;
            Main = main;
            ChildSegemnets = new ResourceSegmentCollection(this);
            ChildSegemnets = childRes;
            foreach (ResourceSegment rs in ChildSegemnets)
            {
                rs.Parent = this;
            }
            Signature = GetExistOrCreateNewSignature(ChildSegemnets.Result);
            AddMessagePrefix(string.Format("SGT:{0}_{1}", block, Id));
            AddLogForm(Log3NetSeverity.Max, "[Input=\"{0}\"(Childs.Count=\"{3}\" Childs.Source=\"{1}\")]{2}", ChildSegemnets[0].Prefix,
                ChildSegemnets.Source.Substring(ChildSegemnets[0].Prefix.Length - 1, ChildSegemnets.Source.Length - ChildSegemnets[0].Prefix.Length - childRes[ChildSegemnets.Count - 1].Postfix.Length),
                childRes[ChildSegemnets.Count - 1].Postfix,
                ChildSegemnets.Count);
        }

        /// <summary>
        /// начальная инициализация сегмента, происходит только после полного распарпарсивания строки с ресурсами
        /// </summary>
        public void PrimaryInitializer()
        {
            try
            {
                
                if (ChildSegemnets?.Count > 0)
                {
                    AddLogForm(Log3NetSeverity.Debug, "Start Init Signature=\"{0}\". Processing On Childs", Signature.Source);
                    //очищаем всякое дерьмо которое было необходимо при формировании сегментов в классе ResourceEx.ParceExpression 
                    //имея дочернии сегменты, но сейчас это дерьмо не нужно по этому удаляем
                    Main.SignatureCollection.Remove(Signature);
                    Signature = null;

                    //присваиваем делегату результат вызов метода для получения результата из дочерних сегментов
                    //а затем после формирования новой(или текущей) - сигнатуры и хэндлера возвращать результат непосредственного ресурса
                    _getResult = GetResultBasedByChild;
                }
                else
                {
                    AddLogForm(Log3NetSeverity.Debug, "Start Init Signature=\"{0}\" Processing On Single", Signature.Source);

                    //если текущая сигнатура находится в блэклисте
                    if (InBlackList)
                    {
                        AddLogForm(Log3NetSeverity.Debug, "Resource:\"{0}\" In BlackList. Skipped", Signature.Source);
                        _getResult = GetResultIfInBlacklist;
                        return;
                    }

                    //если новосозданной сигнатура некорректная то возвращаем источник
                    if (!IsCorrectSyntax)
                    {
                        AddLogForm(Log3NetSeverity.Debug, "Signature:\"{0}\" Is Incorrect!", Signature.Source);
                        _getResult = GetResultIfIncorrectSignatureOrHandler;
                        return;
                    }

                    SetNewOrExistingHandlerByNewSignature(Signature);
                    //если новый или уже существующий из списка созданных, хэндлер некорректый исходя из новосозданной сигатуры
                    if (!IsResource)
                    {
                        AddLogForm(Log3NetSeverity.Debug, "Signature:\"{0}\" Not Resource!", Signature.Source);
                        _getResult = GetResultIfIncorrectSignatureOrHandler;
                        return;
                    }

                    //если везде все корректно и непоредственный ресусрс может возвращать определенные значения
                    _getResult = GetFlawlessResult;
                }
            }
            catch(Exception ex)
            {
                AddLogForm(Log3NetSeverity.Error, "Exception In Primary Initialization Signature=\"{0}\".\r\n{0}\r\n{1}", Signature.Source, ex.Message, ex.Data);
            }
            finally
            {
                AddLogForm(Log3NetSeverity.Debug, "End Init Signature=\"{0}\"", Signature.Source);
            }
        }

        /// <summary>
        /// Если имеются дочерне сегменты. То сначала получаем результат из дочерних сегментов, далее формируем новую(или используем текущую) - сигнатуру и хэндлер. 
        /// Затем возвращаем результат непосредственного ресурса или в случае от определенной ошибки соответвующий результат
        /// </summary>
        /// <returns></returns>
        string GetResultBasedByChild()
        {
            //не нарушать порядок в этом свойстве!!!
            //инициируем дочерние сегменты
            if (IsGetNewSignatureByChildSegments())
            {
                //если новосозданной сигнатура находится в блэклисте
                if (InBlackList)
                {
                    AddLogForm(Log3NetSeverity.Debug, "Resource:\"{0}\" In BlackList. Skipped", Signature.Source);
                    return GetResultIfInBlacklist();
                }

                //если новосозданной сигнатура некорректная то возвращаем источник
                if (!IsCorrectSyntax)
                {
                    AddLogForm(Log3NetSeverity.Debug, "Signature:\"{0}\" Is Incorrect!", Signature.Source);
                    return ChildSegemnets.Source;
                }

                SetNewOrExistingHandlerByNewSignature(Signature);
                //если новый или уже существующий из списка созданных, хэндлер некорректый исходя из новосозданной сигатуры
                if (!IsResource)
                {
                    AddLogForm(Log3NetSeverity.Debug, "Signature:\"{0}\" Not Resource!", Signature.Source);
                    return ChildSegemnets.Source;
                }
                
            }
            else if (InBlackList)
                return GetResultIfInBlacklist();
            //если некорректный синтаксис объявления ресурса или не существует сигнатра или не найден непосредственный ресурс сигнатруры то возвращаем исходник
            else if (!IsCorrectSyntax || !IsResource)
                return ChildSegemnets.Source;

            return GetFlawlessResult();
        }

        /// <summary>
        /// если сигнатура находится в блэклисте
        /// </summary>
        /// <returns></returns>
        string GetResultIfInBlacklist()
        {
            return string.Empty;
        }

        /// <summary>
        /// если был неверно написан из возможных вариантов:
        /// 1) некорректный синтаксис для сформированной сигнатуры
        /// 2) Не существует сигнатра или  то возвращаем исходник
        /// 3) Не найдена сущность ресурса ResourceEntity при формировании сигнатуры
        /// 3) Ошибка при инициализация ресурса
        /// 4) После инициализации непосредственного ресурса возникли неполадки и IResource.IsCorrect = false
        /// </summary>
        /// <returns></returns>
        string GetResultIfIncorrectSignatureOrHandler()
        {
            return Signature.Source;
        }

        /// <summary>
        /// Если все идеально корректно (безупречно)
        /// </summary>
        /// <returns></returns>
        string GetFlawlessResult()
        {
            return GetHandlerResult(Handler.GetResult());
        }


        string GetHandlerResult(XPack input)
        {
            string result = GetXPackResultByPath(input, Signature.OutputPath);
            IsInputStringInnerResource(ref result);
            return result;
        }
        public static string GetXPackResultByPath(XPack input, string path)
        {
            List<XPack> collectionResult = input?[path];
            XPack resultByExpression = null;
            if (collectionResult?.Count > 0)
                resultByExpression = collectionResult[0];
            if (resultByExpression == null)
                return string.Empty;
            return resultByExpression.Value;
        }
        /// <summary>
        /// Если имееются ресурсы в полученном значении ресурса.
        /// Если значение из ресурса (это в восновном являются макросы) в которых присутсвует динамическое использование ресурсов
        /// </summary>
        /// <param name="value"></param>
        bool IsInputStringInnerResource(ref string value)
        {
            bool result = false;
            if (Main.Main == null)
                return result;
            //создаем блек лист и добавляем юзающую сигнатуру в данном сегменте, чтобы не получился бесконечный цикл
            List<ResourceShell> newBlackList = new List<ResourceShell>();
            newBlackList.AddRange(Main.BlackList);
            newBlackList.Add(Signature.Shell);
            ResourceKernel possibleRes = Main.Main.GetInnerResource(value, newBlackList, Main.Contexts);
            if (possibleRes != null && possibleRes.IsResource)
            {
                string newResult = possibleRes.Result;
                AddLogForm(Log3NetSeverity.MaxErr, "Input String=\"{0}\" Is Resource. Final Result=\"{1}\"", value, newResult);
                value = newResult;
                result = true;
            }
            //possibleRes.Dispose();
            return result;
        }




        string _prevChildsResult;
        /// <summary>
        /// Если имеются дочерние сегменты с ресурсами то в первую очередь выполняются они, а затем уже текущий сегмент
        /// </summary>
        bool IsGetNewSignatureByChildSegments()
        {
            string childResult = ChildSegemnets.Result;
            if (!string.Equals(_prevChildsResult, childResult))
            {
                Signature = GetExistOrCreateNewSignature(childResult);
                AddLogForm(Log3NetSeverity.Max, "Prev Signature:{0} | New Signature:{1}", _prevChildsResult, childResult);
                _prevChildsResult = childResult;
                return true;
            }
            return false;
        }


        bool _prevHandlerIsCore = false;
        /// <summary>
        /// инициализируем текущий сегмент, находит хэндлер удовлетворяющий условию сигнатуры или создает новый, присваивает эвенты или удаляет старый хэндлер с удалением эвентов
        /// </summary>
        void SetNewOrExistingHandlerByNewSignature(ResourceSignature sign)
        {
            lock (this)
            {
                ResourceHandler _newHandler = GetExistsOrCreateNewHandler(sign);
                if (_newHandler != null)
                {
                    //если экземпляры хэндлеров равны, то мы проверям режим выполнения связанных хэндлеров (Возможно у нас был инициирован изменения режима в базовом классе и новая переинициализация сегментов)
                    if (ReferenceEquals(Handler, _newHandler))
                    {
                        //если предыдущий режим был в статичном состоянии, то переводим его в ядро
                        if (!_prevHandlerIsCore && (Main.ProcessingMode == ResourceMode.Core))
                        {
                            AssignHandlerToCoreMode();
                        }
                        //если предыдущий режим был в состоянии ядра, то переводим в статичный
                        else if (_prevHandlerIsCore && (Main.ProcessingMode == ResourceMode.External))
                        {
                            AssignHandlerToExternalMode();
                        }
                    }
                    else
                    {
                        //отвязываем текущий(старый) хендлер от сегмента
                        RemoveOldHandler();

                        //привязываем новый хэндлер к сегменту
                        Handler = _newHandler;
                        if (Main.ProcessingMode == ResourceMode.Core)
                        {
                            AssignHandlerToCoreMode();
                            _prevHandlerIsCore = true;
                        }
                        else
                            _prevHandlerIsCore = false;
                    }
                }
                else
                {
                    if (Handler != null)
                    {
                        if (_prevHandlerIsCore)
                            AssignHandlerToExternalMode();
                        Handler = null;
                    }
                }
            }
        }
        /// <summary>
        /// Отвязываем текущий(старый) хендлер от сегмента
        /// </summary>
        void RemoveOldHandler()
        {
            if (Handler == null)
                return;
            if (_prevHandlerIsCore)
            {
                AssignHandlerToExternalMode();
            }
        }

        /// <summary>
        /// Если текущий сегмент не имеет дочерних элементов или ни один из дочерних сегментов не в режиме ядра, то именно этот сегмент (хендлер) должен работать в режиме ядра.
        /// Для актуальности данных которые используют родительские элементы
        /// </summary>
        bool IsLowestRank => (ChildSegemnets == null || (ChildSegemnets != null && !ChildSegemnets.UseCoreMode));
        void AssignHandlerToCoreMode()
        {
            if (IsLowestRank)
            {
                AddLogForm(Log3NetSeverity.MaxErr, "Try Set Handler({0}) On Core Mode", Handler.TraceMessagePrefix);
                Handler.HandlerChanged += new RSegmentEvent.CallRSegment(IsBindingHandlerChanged);
                Handler.UseCoreMode++;
                UseCoreMode = true;
            }
        }
        void AssignHandlerToExternalMode()
        {
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!проверить
            //if (IsLowestRank)
            {
                AddLogForm(Log3NetSeverity.MaxErr, "Try Set Handler({0}) On External Mode", Handler.TraceMessagePrefix);
                Handler.HandlerChanged -= new RSegmentEvent.CallRSegment(IsBindingHandlerChanged);
                Handler.UseCoreMode--;
                UseCoreMode = false;
            }
        }





        /// <summary>
        /// создаем или находим готовые распарсенные линки с названиями ресурсов, конструкторами и пути к выходным свойствам
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        ResourceSignature GetExistOrCreateNewSignature(string input)
        {
            lock (Main.SignatureCollection)
            {
                foreach (ResourceSignature existsSignature in Main.SignatureCollection)
                {
                    if (string.Equals(existsSignature.Source, input))
                    {
                        //если найденный Shell уже есть в списке блэклиста
                        if (existsSignature.Shell != null && Main.BlackList.Contains(existsSignature.Shell))
                            return null;
                        return existsSignature;
                    }
                }
                ResourceSignature newSignature = new ResourceSignature(Main, input);
                Main.SignatureCollection.Add(newSignature);
                return newSignature;
            }
        }

        /// <summary>
        /// создаем или находим сигнатуры привязанные к непосредственным ресурсам
        /// </summary>
        /// <returns></returns>
        ResourceHandler GetExistsOrCreateNewHandler(ResourceSignature sign)
        {
            ResourceHandler existingHandler = Main.HandlerCollection.GetExistingHandler(this, sign);
            if (existingHandler == null)
            {
                ResourceHandler newHandler = Main.HandlerCollection.Add(sign, Main.MainLog3Net);
                AddLogForm(Log3NetSeverity.Max, "Created New:{0}", newHandler.Header);
                return newHandler;
            }
            AddLogForm(Log3NetSeverity.Max, "Getted Existing:{0}", existingHandler.Header);
            return existingHandler;
        }










        /// <summary>
        /// Если пришел элапс от привязанного хэндлера, нам нужно добавить текущий сегмент с самим результатом в очередь выполнения в основной класс, для очередной обработки
        /// </summary>
        /// <param name="provokingHandler">элапсирующий хендлер</param>
        /// <param name="result">результат выполнения непосредственного ресурса</param>
        void IsBindingHandlerChanged(ResourceHandler provokingHandler, XPack result)
        {
            if (Main.ProcessingMode == ResourceMode.External)
                return;
            ResourceKernel.REsPAck temp = new ResourceKernel.REsPAck(this, provokingHandler, result);
            //проверяем если текущий сегмент с тем же хендлером и результатом уже присутсвует в очереди, то не добавляем
            if (Parent == null && Main.QueueInitiation.Contains(temp))
                return;
            //добавляем в очередль обработки
            Main.QueueInitiation.Enqueue(temp);
        }

        /// <summary>
        /// Как только очередь из основного класса дошла до текущего сегмента, начинаем процесс проверки результат и элапсации опять таки основного класса
        /// если непосредственный ресурс был изменен и он инициирует обработку эвента в свзяи с чем  привязанная сигнатура (Handler) инициирует обработку эвента данного сегмента
        /// </summary>
        internal void IsBindingHandlerChanged2(ResourceHandler provokingHandler, XPack result)
        {
            if (Handler == null || !ReferenceEquals(Handler, provokingHandler))
                return;
            AddLogForm(Log3NetSeverity.Max, "Start | {0}", this);
            //если тип непосредственного ресурса яаляется вечно новым значением (ResourceType.Infinity) то проверяем на уникальность значения свойства
            IsHandlerChanged(Handler, result);
            AddLogForm(Log3NetSeverity.Max, "End | {0}", this);
        }

        string _prevResult;
        bool IsAlreadyElapsed { get; set; } = false;
        /// <summary>
        /// проверяем изменился ли данный сегмент, точнее выходные свойства props из результата привязанной сигнатуры, если изменились то инициируем эвенты родительских сигнатур или базового класса ресурсов
        /// </summary>
        /// <returns></returns>
        internal void IsHandlerChanged(ResourceHandler initialHandler, XPack result)
        {
            try
            {
                AddLog(Log3NetSeverity.Max, "Start");
                //не нарушать порядок выполнения в этом методе!!!

                //ставим метку что был запущен элапсирование эвента и в свействе Result будет подставляться актуальный результат который пришел в этот метод
                IsAlreadyElapsed = true;

                //если тип самого первого дочернего инициатора эвента (непосредственный ресурс => сигнатура) является уникальным значением        
                if (IsCurrentUniqueType)
                {
                    string current = (result == null) ? _getResult.Invoke() : GetHandlerResult(result);
                    if (current.Equals(_prevResult))
                    {
                        AddLogForm(Log3NetSeverity.Max, "Segment Not Changed. Prev Value Is Identical To Current:{0}", current);
                        return;
                    }
                    _prevResult = current;
                }
                else
                {
                    //обязательно необходим для свойства Result, там чтоит проверка на поле _segmentEventElapsed чтобы там не городить дополнительные проверки
                    _prevResult = (result == null) ? _getResult.Invoke() : GetHandlerResult(result);
                }



                if (Parent == null)
                {
                    //если данный сегмент не является дочерним сегментом, то инициируем эвент ка базовому ресурсному классу
                    Main?.IsResourceChanged(this, initialHandler);
                }
                else
                {
                    //если данный сегмент является дочерним сегментом, то инициируем жвент на родительский сегмент
                    //и передаем значение если тип самого первого дочернего инициатора эвента (непосредственный ресурс => сигнатура) является уникальным значением
                    Parent.IsHandlerChanged(initialHandler, null);
                }
            }
            catch (Exception ex)
            {
                AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                AddLog(Log3NetSeverity.Max, "End");
                IsAlreadyElapsed = false;
            }
        }

        public override string ToString()
        {
            return Source;
        }

        public void Dispose()
        {
            _prevResult = null;
        }
    }
}