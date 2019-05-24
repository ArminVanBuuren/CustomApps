using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Utils.TYPES;

namespace Utils
{
    public static class MATH
    {
        /// <summary>
        /// Выполняет операции складивания, деление и т.д.
        /// </summary>
        public static double Evaluate(string expression)
        {
            if (IsMathExpression(expression))
                return Evaluate(expression, TypeParam.MathEx);
            return double.NaN;
        }

        public static double Evaluate(string expression, TypeParam pType)
        {
            try
            {
                if (pType != TypeParam.MathEx)
                {
                    return double.NaN;
                }

                string exp = expression.Replace(",", ".");
                var table = new DataTable();
                table.Columns.Add("expression", typeof(string), exp);
                DataRow row = table.NewRow();
                table.Rows.Add(row);
                double result = double.Parse((string)row["expression"]);
                return result;
            }
            catch
            {
                return double.NaN;
            }
        }
    }
}