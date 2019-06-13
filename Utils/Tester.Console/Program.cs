﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeleSharp.TL;
using TLSharp.Core;
using Utils;
using Utils.CollectionHelper;
using Utils.ConditionEx;
using Utils.Crypto;
using Utils.Handles;
using Utils.Telegram;

namespace Tester.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Stopwatch stw = new Stopwatch();
                //while (true)
                //{
                //    stw.Start();
                //    string concat = string.Empty;
                //    for (int j = 0; j < 5; j++)
                //    {
                //        concat += STRING.RandomString(1);
                //    }
                //    stw.Stop();
                //    System.Console.WriteLine($"Concat=[{concat}]. Miliseconds={stw.ElapsedMilliseconds} Ticks={stw.ElapsedTicks}");
                //    stw.Reset();

                //    stw.Start();
                //    StringBuilder builder = new StringBuilder();
                //    for (int j = 0; j < 5; j++)
                //    {
                //        builder.Append(STRING.RandomString(1));
                //    }
                //    stw.Stop();
                //    System.Console.WriteLine($"Builder=[{builder.ToString()}]. Miliseconds={stw.ElapsedMilliseconds} Ticks={stw.ElapsedTicks}");
                //    stw.Reset();

                //    System.Console.WriteLine(new string('=',20));
                //    Thread.Sleep(1000);
                //}

                //string ss1 = XML.NormalizeXmlValueFast("&amp;quot;&amp;lt;&amp;gt; &lt;=  &gt;=");
                //string ss2 = XML.NormalizeXmlValueFast("&&&quot; fefe &quot;&apos; & 111&quot");
                //string ss3 = XML.NormalizeXmlValueFast("& fefe \"' 111", XML.XMLValueEncoder.Encode);

                var dd1 = 197.IsParity();
                var dd2 = 266.IsParity();

                List<long> temp = new List<long>();
                
                int i = 0;
                //while (i < 200)
                //{
                //    stw.Start();




                //    stw.Stop();
                //    temp.Add(stw.ElapsedMilliseconds);
                //    stw.Reset();


                //    i++;
                //}

                //DynamicObject dynObj = new DynamicObject(GetResult);
                //DuplicateDictionary<string, bool> res = new DuplicateDictionary<string, bool>();
                //var res1 = ExpressionBuilder.Calculate("('1'='1' and '2'>'${random}' and '5'>='4' and 'fff'='fff' and 'sasDDDddd'^='DDD' and 'rrrrr'!='aaaaa')", dynObj);
                //while (i < 200)
                //{
                //    stw.Start();

                //    res.Add(res1.StringResult, res1.ConditionResult);
                //    dynObj.Elapsed();

                //    stw.Stop();
                //    temp.Add(stw.ElapsedMilliseconds);
                //    stw.Reset();

                //    i++;
                //}


                //double dd = 855555;
                //using (var responceBody = WEB.GetHttpWebResponse("https://www.whatismyip.com/ip-address-lookup/"))
                //{
                //    if (responceBody != null && responceBody.StatusCode == HttpStatusCode.OK)
                //    {
                //        var httpDoc = responceBody.GetHtmlDocument();
                //    }
                //}



                //using (RegeditControl regedit = new RegeditControl("TFSAssist"))
                //{
                //    using (var stream = new FileStream(nameof(TLControl) + "Session", FileMode.CreateNew))
                //    {
                //        byte[] bytes = (byte[])regedit[nameof(TLControl) + "Session", RegistryValueKind.Binary];
                //        stream.Write(bytes, 0, bytes.Length);
                //    }
                //}


                //!!!!!!
                //string dd = TLControl.SessionName + ".code";
                //if(File.Exists(dd))
                //    File.Delete(dd);

                //string ss = AES.EncryptStringAES("InCommon-2.0", nameof(TLControl));
                //using (var stream = new FileStream(TLControl.SessionName + ".code", FileMode.OpenOrCreate))
                //{
                //    byte[] logsBytes = new UTF8Encoding(true).GetBytes(ss);
                //    stream.Write(logsBytes, 0, logsBytes.Length);
                //}

                //DisplayData();

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }

            System.Console.WriteLine(@"Complete");
            System.Console.ReadLine();
        }

        static string GetResult(string input)
        {
            if (input.IndexOf("${random}", StringComparison.CurrentCultureIgnoreCase) == -1)
                return input;

            var res = new Random().Next(0, 9);
            return input.Replace("${random}", res.ToString());
        }

        static void DisplayData()
        {
            var reader = OleDbEnumerator.GetRootEnumerator();

            var list = new List<String>();
            while (reader.Read())
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    //if (reader.GetName(i) == "SOURCES_NAME")
                    {
                        list.Add(reader.GetValue(i).ToString());
                    }
                }
                System.Console.WriteLine("{0} = {1}", reader.GetName(0), reader.GetValue(0));
            }
            reader.Close();
        }

        //static void ParceOptions()
        //{
        //    string isCommand = "30609:";
        //    //string tlMessage = "30609:cam ";
        //    string tlMessage = "30609:cam ( par1 =d '1111,=(2222)' , par2 =q '5555,=(6666)' ) ";

        //    string command = tlMessage.Substring(isCommand.Length, tlMessage.Length - isCommand.Length);
        //    Dictionary<string, string> options = null;
        //    int optStart = command.IndexOf('(');
        //    int optEnd = command.IndexOf(')');
        //    if (optStart != -1 && optEnd != -1 && optEnd > optStart)
        //    {
        //        string strOptions = command.Substring(optStart, command.Length - optStart);
        //        options = ReadOptionParams(strOptions);
        //        command = command.Substring(0, optStart);
        //    }

        //    command = command.ToLower().Trim();
        //}

        static void TelegramTester()
        {
            //using (var stream = new FileStream(@"C:\!MyRepos\CustomApp\Utils\Tester.Console\bin\Debug\session.dat", FileMode.Open))
            //{
            //    var buffer = new byte[2048];
            //    stream.Read(buffer, 0, 2048);
            //    using (RegeditControl regedit = new RegeditControl(ASSEMBLY.ApplicationName))
            //    {
            //        regedit[nameof(TLControl) + "Session", RegistryValueKind.Binary] = buffer;
            //    }
            //}


            //DateTime d1 = DateTime.ParseExact("20.03.2019 15:28:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            //DateTime d2 = DateTime.ParseExact("20.03.2019 21:34:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            //var dd = d2.Subtract(d1);
            //TimeSpan ss = d1.Subtract(mdt);

            var mdt = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime newDt1 = mdt.AddSeconds(1553084912).ToLocalTime();
            DateTime newDt1Temp = mdt.AddSeconds(1553084912);
            var strUs = newDt1Temp.ToString(new CultureInfo("en-US"));
            var strGb = newDt1Temp.ToString(new CultureInfo("en-GB"));
            DateTime newDt2 = mdt.AddSeconds(1553106864).ToLocalTime();

            var ddddd = DateTime.Now;
            var ddddd1 = DateTime.Now.ToUniversalTime();


            DateTime newd1 = DateTime.ParseExact("20.03.2019 15:28:32", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
            DateTime newd2 = DateTime.ParseExact("20.03.2019 21:34:24", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
            newd1 = newd1.ToUniversalTime();
            newd2 = newd2.ToUniversalTime();
            double start = newd1.Subtract(mdt).TotalSeconds;
            double end = newd2.Subtract(mdt).TotalSeconds;


            //TimeSpan span = DateTime.Now.Subtract(DateTime.Parse("07.02.2018 00:00:00"));
            TLControl control = new TLControl(770122, "8bf0b952100c9b22fd92499fc329c27e");
            Process(control);
        }

        static async void Process(TLControl control)
        {
           // await control.ConnectAsync();
            //var user1 = await control.GetUserAsync("+79113573202");
            //var user2 = await control.GetUserByUserNameAsync("MexicanCactus");
            //var chat1 = await control.GetChatAsync("Ацацоц");
            //var chat2 = await control.GetChatAsync("Тухлый сыр");
            //var user2 = await control.GetUserByUserNameAsync("TFSAssistbot");
            //await control.GetMessagesAsync(user2.Destination);
            
            //DateTime lastDate = DateTime.ParseExact("19.03.2019 00:00:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
            
            //!!!!!!!!!!!DateTime lastDate = DateTime.ParseExact("18.03.2019 20:17:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
            DateTime lastDate = DateTime.ParseExact("15.03.2019 00:00:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
            while (true)
            {
                //DateTime currentDate = DateTime.Now;
                List<TLMessage> newMessages = await control.GetDifference(control.UserHost.User, control.UserHost.Destination, lastDate);
                
                TLMessage message = newMessages?.LastOrDefault();
                if (message != null)
                    lastDate = TLControl.ToDate(message.Date);
                //if (isChanged)
                //{
                //    var res = await control.GetMessagesAsync(control.CurrentUser.Destination, lastDate, null, 50);
                //    lastDate = currentDate;
                //}
                await Task.Delay(1000);
            }


        }
    }
}
