using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Utils.Crypto;
using Utils.Handles;
using Utils.Telegram;

namespace Tester.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //using (var stream = new FileStream(@"C:\!MyRepos\CustomApp\Utils\Tester.Console\bin\Debug\session.dat", FileMode.Open))
            //{
            //    var buffer = new byte[2048];
            //    stream.Read(buffer, 0, 2048);
            //    using (RegeditControl regedit = new RegeditControl(ASSEMBLY.ApplicationName))
            //    {
            //        regedit[nameof(TLControl)+ "Session", RegistryValueKind.Binary] = buffer;
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
            var ddddd1  = DateTime.Now.ToUniversalTime();

            
            DateTime newd1 = DateTime.ParseExact("20.03.2019 15:28:32", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
            DateTime newd2 = DateTime.ParseExact("20.03.2019 21:34:24", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
            newd1 = newd1.ToUniversalTime();
            newd2 = newd2.ToUniversalTime();
            double start = newd1.Subtract(mdt).TotalSeconds;
            double end = newd2.Subtract(mdt).TotalSeconds;


            
            //TimeSpan span = DateTime.Now.Subtract(DateTime.Parse("07.02.2018 00:00:00"));
            TLControlNew control = new TLControlNew(770122, "8bf0b952100c9b22fd92499fc329c27e", "+375333866536");
            Process(control);


            System.Console.ReadLine();
        }

        static async void Process(TLControlNew control)
        {
            await control.ConnectAsync();
            //var user1 = await control.GetUserAsync("+79113573202");
            //var user2 = await control.GetUserByUserNameAsync("MexicanCactus");
            //var chat1 = await control.GetChatAsync("Ацацоц");
            //var chat2 = await control.GetChatAsync("Тухлый сыр");
            //var user2 = await control.GetUserByUserNameAsync("TFSAssistbot");
            //await control.GetMessagesAsync(user2.Destination);
            
            //DateTime lastDate = DateTime.ParseExact("19.03.2019 00:00:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
            
            DateTime lastDate = DateTime.ParseExact("18.03.2019 20:17:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
            while (true)
            {
                //DateTime currentDate = DateTime.Now;
                bool isChanged = await control.GetDifference(control.CurrentUser.Destination, lastDate);
                //if (isChanged)
                //{
                //    var res = await control.GetMessagesAsync(control.CurrentUser.Destination, lastDate, null, 50);
                //    lastDate = currentDate;
                //}
                //await Task.Delay(1000);
            }


        }
    }

    public class TLControlNew : TLControl
    {
        public TLControlNew(int appiId, string apiHash, string numberToAthenticate = null, string passwordToAuthenticate = null):base(appiId, apiHash, numberToAthenticate, passwordToAuthenticate)
        {

        }
    }
}
