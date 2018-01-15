using System.Collections.Generic;
using Protas.Components.PerformanceLog;
using Protas.Components.XPackage;
using Protas.Control.Resource;
using Protas.Control.Resource.Base;

namespace Protas.Control.ProcessFrame.Handlers
{

    class Caser : ConditionExShell, IHandler
    {
        List<Caser> ChildsCases { get; } = new List<Caser>();
        public string CallName { get; }
        public IHandler Calling { get; }
        public bool IsCorrect { get; } = false;

        public Caser(XPack pack, Process process) : base(pack, process)
        {
            string callName;
            if (!pack.Attributes.TryGetValue("call", out callName))
            {
                AddLog(Log3NetSeverity.Error, "Attribute \"Call\" Not Found");
                return;
            }

            switch(pack.Name.ToLower())
            {
                case "Case": Calling = process.GetIHandler(callName); break;
                case "Remote": Calling = process.GetRemoteBinding(callName); break;
                default: AddLogForm(Log3NetSeverity.Error, "{0} Is Unknown", pack.Name); break;
            }

            if (Calling == null)
            {
                AddLogForm(Log3NetSeverity.Error, "Handler \"{0}\" Not Found On Process=\"{1}\"", callName, process.Name);
                return;
            }
            
            CallName = callName;

            foreach (XPack childCasePack in pack.ChildPacks)
            {
                Caser childCase = new Caser(childCasePack, process);
                if (childCase.IsCorrect)
                    ChildsCases.Add(childCase);
            }


            IsCorrect = true;
        }

        public XPack Run(ResourceComplex complex)
        {
            if (CheckConditionVoid != null)
            {
                KeyValuePair<bool, XPack> checkCondition = CheckConditionVoid.Invoke(complex);
                if (!checkCondition.Key)
                    return checkCondition.Value;
            }

            XPack parentResult = Calling.Run(complex);
            foreach(Caser cs in ChildsCases)
            {
                parentResult.ChildPacks.Add(cs.Run(complex));
            }

            return parentResult;
        }

        public XPack ForceStop()
        {
            return null;
        }
    }
}
