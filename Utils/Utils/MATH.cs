using System.Data;
using System.Text;

namespace Utils
{
    public static class MATH
    {
        public static bool IsMathExpression(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var temp = new StringBuilder();
            var charIn = input.ToCharArray(0, input.Length);
            var nextIsNum = 0;
            var bkt = 0;

            foreach (var ch in charIn)
            {
                if (ch == ' ')
                    continue;

                if ((ch == '+' || ch == '-' || ch == '*' || ch == '/') && nextIsNum == 0)
                {
                    if (!NUMBER.IsNumber(temp.ToString()))
                        return false;
                    temp.Remove(0, temp.Length);
                    nextIsNum++;
                }
                else switch (ch)
                {
                    case '(':
                        bkt++;
                        break;
                    case ')':
                        bkt--;
                        break;
                    default:
                        temp.Append(ch);
                        nextIsNum = 0;
                        break;
                }
            }

            return NUMBER.IsNumber(temp.ToString()) && bkt == 0;
        }

        public static double Calculate(string expression)
        {
            try
            {
                var exp = expression.Replace(",", ".");
                var table = new DataTable();
                table.Columns.Add("expression", typeof(string), exp);
                var row = table.NewRow();
                table.Rows.Add(row);
                return double.Parse((string)row["expression"]);
            }
            catch
            {
                return double.NaN;
            }
        }
    }
}