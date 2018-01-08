using System.Collections.Generic;

namespace Script.Control.Handlers.Timesheet.WriteData
{
    public class SemiNumericComparer : IComparer<double>
    {
        public int Compare(double num1, double num2)
        {
            if (num1 < num2) return 1;
            if (num1 > num2) return -1;
            return 0;
        }
    }
}
