using System.Text;

namespace Protas.Control.Resource
{
    class ResourceParcer
    {
        int _segmentsId = -1;
        int _block = 0;
        ResourceKernel Resource { get; }
        public ResourceParcer(ResourceKernel resource)
        {
            Resource = resource;
        }
        public ResourceSegmentCollection GetByExpression(string input)
        {
            ResourceSegmentCollection result = GetByExpression(input, 0);
            _segmentsId = -1;
            _block = 0;
            return result;
        }
        ResourceSegmentCollection GetByExpression(string input, int under)
        {
            string procString = input;
            int undSegm = under;
            int skiplines = 0;
            int segmCount = 0;
            int lines = 0;
            bool breakLoop = false;
            ResourceSegmentCollection parentSegments = new ResourceSegmentCollection(Resource);
            ResourceSegmentCollection childSegments = new ResourceSegmentCollection(Resource);
            StringBuilder Static = new StringBuilder();
            foreach (char ch in procString)
            {
                lines++;
                if (skiplines > 0)
                {
                    skiplines--;
                    continue;
                }
                switch (ch)
                {
                    case '{':
                        if (segmCount == 0)
                        {
                            segmCount = segmCount + 1;
                            if (Static.Length > 0)
                            {
                                _segmentsId++;
                                parentSegments.Add(new ResourceSegment(_segmentsId, Static.ToString(), _block, Resource));
                                if (under == 0)
                                    _block++;
                                Static.Remove(0, Static.Length);
                            }
                        }
                        else if (segmCount > 0)
                        {
                            undSegm++;
                            string getAfter = procString.Substring(lines - 1, procString.Length - lines + 1);
                            childSegments.AddRange(GetByExpression(getAfter, undSegm));
                            string packStatic = childSegments[childSegments.Count - 1].Source;
                            //добавляем префикс к посленему дочернему сегменту от родительского сегмента (еще не созданного) например {now(' (если один дочерний ресурс) или ',' или ')$ (если конструкторов или свойств больше одного) - то что дальше после ковычки дочерний ресурс
                            childSegments[childSegments.Count - 1].Prefix = Static.ToString();
                            Static.Remove(0, Static.Length);
                            skiplines = (packStatic.Length == 0) ? 0 : packStatic.Length - 1;
                            undSegm--;
                            continue;
                        }
                        break;
                    case '}':
                        Static.Append(ch);
                        if (childSegments.Count > 0)
                        {
                            //добавляем окончание к последнему дочернуму сегменту - например ')$day}
                            //т.е. Префиксы родительского уже добавлены в первый дочерний (или где-то посередине если их много) то что выше - {now(' + суффик подбирается из дочерних сегментов + завершение родительского сегментта ')$day}
                            //если в оригинале например был {now('{rand('a-z')}')$day} - из этого выражения {rand('a-z')} является дочерним по отношению к сегменту который сейчас создастся как родительский
                            //или s: {now('{rand('a-c')}','{rand('d-f')}')$day} {rand('a-c')}
                            childSegments[childSegments.Count - 1].Postfix = Static.ToString();
                            _segmentsId++;
                            parentSegments.Add(new ResourceSegment(_segmentsId, childSegments, _block, Resource));
                            childSegments = new ResourceSegmentCollection(Resource);
                        }
                        else
                        {
                            _segmentsId++;
                            parentSegments.Add(new ResourceSegment(_segmentsId, Static.ToString(), _block, Resource));
                        }
                        Static.Remove(0, Static.Length);
                        segmCount = (segmCount > 0) ? segmCount - 1 : 0;
                        if (undSegm >= 1)
                        {
                            breakLoop = true;
                            break;
                        }
                        continue;
                }
                if (breakLoop)
                    break;
                Static.Append(ch);
            }

            if (Static.Length > 0)
            {
                _segmentsId++;
                if (under == 0)
                    _block++;
                parentSegments.Add(new ResourceSegment(_segmentsId, Static.ToString(), _block, Resource));
                Static.Remove(0, Static.Length);
            }
            return parentSegments;
        }
    }
}
