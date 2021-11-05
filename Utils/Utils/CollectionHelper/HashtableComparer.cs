using System;
using System.Collections;

namespace Utils.CollectionHelper
{
    /// <summary>
    /// Summary description for HashtableComparer.
    /// </summary>
    public static class HashtableComparer
    {
        public static int Compare(Hashtable table1, Hashtable table2)
        {
            var de = table1.GetEnumerator();
            while (de.MoveNext())
            {
                if (table2.ContainsKey(de.Key) == false)
                    return 1;

                if (!(de.Value is IComparable comp))
                    throw new Exception();

                if (comp.CompareTo(table2[de.Key]) != 0)
                    return 1;
            }

            return 0;
        }
    }
}
