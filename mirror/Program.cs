using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace mirror
{
    class Program
    {
        static bool verbose = false;
        static int port = 8123;
        struct _histst
        {
            public _net._rec rec;
            public DateTime time_received;
        }
        static List<_histst> history = new List<_histst>();
        static _histst last = new _histst();
        static long totalreq = 0;
        static void Main(string[] args)
        {
            if (!checkargs(args))
                return;
            DateTime starttimestamp = DateTime.Now;
            Log("Program started. Port: " + port + " Verbose: " + (verbose ? "True" : "False"));
            _net net = new _net();
            while (true)
            {
                try
                {
                    clear_old_history();
                    display(starttimestamp);
                    _net._rec temp = net.receive();
                    
                    //catalog received message
                    _histst rec = new _histst();
                    rec.rec.ip = temp.ip;
                    rec.rec.message = temp.message;
                    rec.time_received = DateTime.Now;
                    history.Add(rec);
                    last = rec;
                    totalreq++;
                    Log("Received connection from " + rec.rec.ip + ": " + rec.rec.message);

                    //Send capitalized reply
                    net.send(rec.rec.ip, rec.rec.message);
                    Log("Sent connection to " + rec.rec.ip + ": " + rec.rec.message.ToUpper());
                }
                catch (Exception e)
                {
                    Log("Error: " + e.Message, logtype.error);
                    Log(e.StackTrace, logtype.error);
                    Log("Error caused program to terminate. Check error logs for details.");
                    break;
                }
            }
        }
        static void clear_old_history()
        {
            for (int i = 0; i != history.Count; i++)
            {
                if (history[i].time_received.AddMinutes(1) < DateTime.Now)
                {
                    history.Remove(history[i]);
                    i--;
                }
            }
        }
        static void display(DateTime st)
        {
            Console.Clear();
            Console.WriteLine("Listener started: " + st.ToShortDateString() + " " + st.ToLongTimeString());
            Console.WriteLine("Port: " + port + " | Total Requests served: " + totalreq + " | # Requests served < 5 min: " + history.Count);
            if (history.Count > 0)
                Console.WriteLine("Last message: " + last.time_received.ToShortDateString() + " " + last.time_received.ToLongTimeString() + " | " + last.rec.ip + " | " + last.rec.message);
        }
        static bool checkargs(string[] args)
        {
            for (int i = 0; i != args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-v":
                        verbose = true;
                        break;
                    case "-p":
                        try
                        {
                            port = Int16.Parse(args[++i]);
                        }
                        catch
                        {
                            Log("unable to read \"" + args[i - 1] + " " + args[i] + "\".", logtype.error);
                        }
                        break;
                }
            }
            return true;
        }
        enum logtype { error, log, console_only }
        static void Log(string errormsg, logtype lt = logtype.log)
        {
            switch (lt)
            {
                case logtype.console_only:
                    Console.WriteLine(DateTime.Now.ToLongTimeString() + " | " + errormsg);
                    break;
                case logtype.log:
                    string[] templog = { DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - " + errormsg };
                    File.AppendAllLines("Mirror.log", templog);
                    if (verbose)
                        Console.WriteLine(templog[0]);
                    break;
                case logtype.error:
                    string[] temperr = { DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - " + errormsg };
                    File.AppendAllLines("Error.log", temperr);
                    if (verbose)
                        Console.WriteLine(temperr[0]);
                    break;
            }
        }
    }
}