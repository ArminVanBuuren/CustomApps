using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PROTAS
{
    public class trapinfo
    {
        public string info;
        public string ip;
        public string cause;
    }

    public class trap
    {
        public delegate void myDelegate(trapinfo t);
        public myDelegate del;

        trapinfo info = new trapinfo();
        public void AddCallback(myDelegate m)
        {
            del += m;
        }

        public void RemoveCallback(myDelegate m)
        {
            del -= m;
        }

        //or
        public static trap operator +(trap x, myDelegate m)
        {
            x.AddCallback(m);
            return x;
        }
        public static trap operator -(trap x, myDelegate m)
        {
            x.RemoveCallback(m);
            return x;
        }

        //usage  

        //t.AddCallback(new trap.myDelegate(notify));

        public void run()
        {

            //While(true) 
            // If a trap occurred, notify the subscriber
            for (; ; )
            {
                Thread.Sleep(500);
                foreach (myDelegate d in del.GetInvocationList())
                {
                    info.cause = "Shut Down";
                    info.ip = "192.168.0.1";
                    info.info = "Test";
                    d.Invoke(info);
                }
            }
        }
    }

    public class machine
    {
        private int _occuredtime = 0;

        public trapinfo info = new trapinfo();
        public void notify(trapinfo t)
        {
            ++_occuredtime;
            info.cause = t.cause;
            info.info = t.info;
            info.ip = t.ip;
            getInfo();
        }
        public void subscribe(trap t)
        {
            //t.del += new trap.myDelegate(notify);
            t += new trap.myDelegate(notify);
        }
        public void getInfo()
        {
            Console.WriteLine("<Alert>: cauese/{0}, info/ {1}, ip/{2}, time/{3}",
                              info.cause, info.info, info.ip, _occuredtime);
        }


    }
}
