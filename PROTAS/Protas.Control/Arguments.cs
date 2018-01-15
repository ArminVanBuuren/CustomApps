using Protas.Components.PerformanceLog;

namespace Protas.Control
{
    internal enum Processor
    {
        MainControl = 0,
        Trigger = 1,
        Timer = 2,
        Task = 3,
        Handler = 4
    }
    internal interface IShellBase
    {
        void Start();
        void Stop();
    }
    internal class ShellProcessor : ShellLog3Net
    {
        public ShellProcessor(Log3Net log) : base(log)
        {

        }
    }


    class CItemProperties
    {

    }

}
