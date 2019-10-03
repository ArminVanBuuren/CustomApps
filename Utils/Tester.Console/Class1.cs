using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;
using System.Reflection;
//using System.Text;
//using System.Collections.Generic;
//using System.Linq;

namespace Tester.Console
{
    public class FormatFunction : IXsltContextFunction
    {
        public static String get_codelist(string customer, string format, string @params, string splitSign)
        {
            try
            {
                return string.Format(format.Replace("[", "{").Replace("]", "}"), @params.Split(new char[] { splitSign.ToCharArray()[0] }))
                    .Replace("&apos;", "'")
                    .Replace("&quot;", "\"")
                    .Replace("&apos", "'")
                    .Replace("&quot", "\"")
                    .Replace(@"\^", "##1##")
                    .Replace("^", "'")
                    .Replace("##1##", "^")
                    .Replace(@"\@", "##2##")
                    .Replace("@", "\"")
                    .Replace("##2##", "@")
                    .Replace(@"\%", "##3##")
                    .Replace("%", "<")
                    .Replace("##3##", "%");
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public int Minargs
        {
            get { return 2; }
        }

        public int Maxargs
        {
            get { return 2; }
        }

        public XPathResultType ReturnType
        {
            get { return XPathResultType.String; }
        }

        public XPathResultType[] ArgTypes
        {
            get { return new[] { XPathResultType.String, XPathResultType.String }; }
        }

        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length != 2)
                throw new ArgumentException("Function StrFormat() takes only two parameters");

            return string.Format(((string)args[0]).Replace("[", "{").Replace("]", "}").Replace("*", "\"").Replace("^", "'").Replace(@"\r", "\r").Replace(@"\n", "\n"), ((string)args[1]).Split(';'));
        }
    }


    //public class FormatFunction : IXsltContextFunction
    //{
    //    public int Minargs
    //    {
    //        get { return 2; }
    //    }

    //    public int Maxargs
    //    {
    //        get { return 10; }
    //    }

    //    public XPathResultType ReturnType
    //    {
    //        get { return XPathResultType.String; }
    //    }

    //    public XPathResultType[] ArgTypes
    //    {
    //        get
    //        {
    //            return new[]
    //            {
    //                XPathResultType.String, XPathResultType.String, XPathResultType.String, XPathResultType.String, XPathResultType.String, XPathResultType.String, XPathResultType.String, XPathResultType.String, XPathResultType.String, XPathResultType.String
    //            };
    //        }
    //    }

    //    public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
    //    {
    //        if (args == null)
    //            throw new ArgumentNullException("args");
    //        if (args.Length < 2)
    //            throw new ArgumentException("Function Format() takes two or more parameters");

    //        var input = ((string)args[0]).Replace("[", "{").Replace("]", "}");
    //        var @params = args.Skip(1).Take(args.Length - 1).ToArray();
    //        var matches = Regex.Matches(input, @"{(\d+).*?}");
    //        var uniqueParams = matches.OfType<Match>().Select(m => m.Value).Distinct().ToList();
    //        if (uniqueParams.Count <= @params.Length)
    //            return string.Format(input, @params);
    //        else
    //            return FormatOptional(input, string.Empty, @params);
    //    }

    //    public static string FormatOptional(string s, string optional, params object[] param)
    //    {
    //        var result = new StringBuilder();
    //        var index = 0;
    //        var opened = false;

    //        var stack = new Stack<object>(param.Reverse());

    //        foreach (var c in s)
    //        {
    //            if (c == '{')
    //            {
    //                opened = true;
    //                index = result.Length;
    //            }
    //            else if (opened && c == '}')
    //            {
    //                opened = false;
    //                var p = stack.Count > 0 ? stack.Pop() : optional;
    //                var lenToRem = result.Length - index;
    //                result.Remove(index, lenToRem);
    //                result.Append(p);
    //                continue;
    //            }
    //            else if (opened && !char.IsDigit(c))
    //            {
    //                opened = false;
    //            }

    //            result.Append(c);
    //        }

    //        return result.ToString();
    //    }
    //}

    public class getTimeInRangeFunction : IXsltContextFunction
    {
        public int Minargs
        {
            get { return 3; }
        }

        public int Maxargs
        {
            get { return 4; }
        }

        public XPathResultType ReturnType
        {
            get { return XPathResultType.String; }
        }

        public XPathResultType[] ArgTypes
        {
            get { return new XPathResultType[] { XPathResultType.String, XPathResultType.String, XPathResultType.String }; }
        }

        public object Invoke(XsltContext xsltContext,
                             object[] args, XPathNavigator docContext)
        {
            try
            {
                string sTimeZone = args[0].ToString();
                Int32 timeZone;
                if (Int32.TryParse(sTimeZone, out timeZone))
                {
                    timeZone = Convert.ToInt32(sTimeZone);
                }
                else
                {
                    timeZone = 0;
                }
                string sFrom = args[1].ToString();
                string sTo = args[2].ToString();
                int spreadMinutes = (args.Length > 3) ? Convert.ToInt32(args[3].ToString()) : 120;
                //default value of spread is 120 minutes

                DateTime now = DateTime.Now;
                DateTime dFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                dFrom = dFrom + stringToTimeSpan(sFrom);
                dFrom = dFrom + TimeSpan.FromMinutes(timeZone);
                DateTime dTo = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                dTo = dTo + stringToTimeSpan(sTo);
                dTo = dTo + TimeSpan.FromMinutes(timeZone);

                if (now >= dFrom && now <= dTo) return "";

                Random randomShift = new Random();
                if (now < dFrom)
                    return timespanToString(dFrom - now + TimeSpan.FromMinutes(randomShift.Next(spreadMinutes)));
                if (now > dFrom)
                    return
                        timespanToString(dFrom.AddDays(1) - now + TimeSpan.FromMinutes(randomShift.Next(spreadMinutes)));
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        static string timespanToString(TimeSpan timespan)
        {
            return String.Format("0000{0:00}{1:00}{2:00}{3:00}000R", timespan.Days, timespan.Hours, timespan.Minutes, timespan.Seconds);
        }

        static TimeSpan stringToTimeSpan(string span)
        {
            Regex rx = new Regex(@"((?<days>(\d+))\.)?(?<hours>(\d{1,2})):(?<mins>(\d{1,2}))");
            Match match = rx.Match(span);
            GroupCollection groups = match.Groups;
            Int32 days = 0;
            try
            {
                days = Convert.ToInt32(groups["days"].Value);
            }
            catch
            {
                days = 0;
            }
            TimeSpan timeSpan = new TimeSpan(days, Convert.ToInt32(groups["hours"].Value), Convert.ToInt32(groups["mins"].Value), 0);
            return timeSpan;
        }
    }

    public class ToLowerFunction : IXsltContextFunction
    {
        public int Minargs
        {
            get { return 1; }
        }

        public int Maxargs
        {
            get { return 1; }
        }

        public XPathResultType ReturnType
        {
            get { return XPathResultType.String; }
        }

        public XPathResultType[] ArgTypes
        {
            get { return new XPathResultType[] { XPathResultType.String }; }
        }

        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length != 1)
                throw new ArgumentException("Function ToLower() takes only one parameter");

            return ((string)args[0]).ToLower();
        }
    }
}
