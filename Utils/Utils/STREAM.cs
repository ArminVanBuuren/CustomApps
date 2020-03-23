using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class STREAM
    {
        public static void GarbageCollect()
        {
            for (int i = 0; i < 3; i++)
            {
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }
            GC.GetTotalMemory(true);
        }
    }
}
