using System;
using System.Text.RegularExpressions;

namespace TFSGeneration.Control.DataBase
{
	public static class Utils
	{
		//string _example = "---% now %---% now ( + 8 : hour ) %---% now ( + 1 : day ) %---% now ( + 1 : workday ) %---% now ( + 1 : month ) %---% now ( + 1 : year ) %---";
		static Regex GetSimpleFunc = new Regex(@"%\s*(?<Func>[A-z]+?)\s*%", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static Regex GetNowWithOpt = new Regex(@"%\s*(?<Func>.+?)\s*\(\s*(?<Operator>\+|\-)\s*(?<Num>\d+)\s*\:\s*(?<Operation>.+?)\)\s*%", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public static string GetCustomFuncResult(string source)
		{
			string _source = source;

			MatchEvaluator evaluatorOtherwise = GetCustomFunc;
			_source = GetSimpleFunc.Replace(_source, evaluatorOtherwise);

			MatchEvaluator evaluator = SetNowOptions;
			_source = GetNowWithOpt.Replace(_source, evaluator);

			return _source;
		}

		static string GetCustomFunc(Match match)
		{
			GroupCollection groups = match.Groups;
		    if (groups["Func"].Success)
		    {
		        if (groups["Func"].Value.Trim().Equals("now", StringComparison.CurrentCultureIgnoreCase))
		            return DateTime.Now.ToString("G");
		        if (groups["Func"].Value.Trim().Equals("^M", StringComparison.CurrentCultureIgnoreCase))
		            return Environment.NewLine;
            }
		    return match.Value;
		}

		static string SetNowOptions(Match match)
		{
			GroupCollection groups = match.Groups;
			if (groups["Func"].Success && groups["Func"].Value.Trim().Equals("now", StringComparison.CurrentCultureIgnoreCase) && groups.Count > 4)
			{
				DateTime now = DateTime.Now;

				int numberOf = int.Parse(match.Groups["Num"].Value.Trim());
				if (numberOf == 0)
					return now.ToString("G");


				string @operator = match.Groups["Operator"].Value.Trim();
				if (@operator == "-")
					numberOf = -numberOf;

				string operation = match.Groups["Operation"].Value.Trim().ToLower();
				switch (operation)
				{
					case "workday": return now.AddWorkdays(numberOf).ToString("G");
					case "day": return now.AddDays(numberOf).ToString("G");
					case "week": return now.AddDays(numberOf * 7).ToString("G");
					case "month": return now.AddMonths(numberOf).ToString("G");
					case "year": return now.AddYears(numberOf).ToString("G");
					case "hour": return now.AddHours(numberOf).ToString("G");
					case "minute": return now.AddMinutes(numberOf).ToString("G");
					case "second": return now.AddSeconds(numberOf).ToString("G");
					default: return now.ToString("G");
				}
			}

			return match.Value;
		}

		public static DateTime AddWorkdays(this DateTime originalDate, int workDays)
		{
			DateTime tmpDate = originalDate;
			if (workDays > 0)
			{
				while (workDays > 0)
				{
					tmpDate = tmpDate.AddDays(1);
					if (tmpDate.DayOfWeek < DayOfWeek.Saturday && tmpDate.DayOfWeek > DayOfWeek.Sunday && !tmpDate.IsHoliday())
						workDays--;
				}
			}
			else if (workDays < 0)
			{
				while (workDays < 0)
				{
					tmpDate = tmpDate.AddDays(-1);
					if (tmpDate.DayOfWeek < DayOfWeek.Saturday && tmpDate.DayOfWeek > DayOfWeek.Sunday && !tmpDate.IsHoliday())
						workDays++;
				}
			}
			return tmpDate;
		}

		static bool IsHoliday(this DateTime originalDate)
		{
			switch (originalDate.ToString("dd.MM"))
			{
				case "01.01": return true;
				case "07.01": return true;
				case "08.03": return true;
				case "01.05": return true;
				case "09.05": return true;
				case "03.07": return true;
				case "07.11": return true;
				case "25.12": return true;
			}
			return false;
		}
	}
}
