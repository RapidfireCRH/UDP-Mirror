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
            public IPAddress ip;
            public string message;
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
            while (true)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("Listener started: " + starttimestamp.ToShortDateString() + " " + starttimestamp.ToLongTimeString());
                    Console.WriteLine("Port: " + port + " | Total Requests served: " + totalreq + " | # Requests served < 5 min: " + history.Count);
                    if (history.Count > 0)
                        Console.WriteLine("Last message: " + last.time_received.ToShortDateString() + " " + last.time_received.ToLongTimeString() + " | " + last.ip + " | " + last.message);
                    //create connection
                    UdpClient udpClient = new UdpClient(port);
                    IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    string receivedstr = Encoding.ASCII.GetString(udpClient.Receive(ref remoteIPEndPoint));

                    //catalog received message
                    _histst rec = new _histst();
                    rec.ip = remoteIPEndPoint.Address;
                    rec.message = receivedstr;
                    rec.time_received = DateTime.Now;
                    history.Add(rec);
                    last = rec;
                    Log("Received connection from " + remoteIPEndPoint.Address.ToString() + ": " + receivedstr);

                    //Send capitalized reply
                    udpClient.Send(Encoding.ASCII.GetBytes(receivedstr.ToUpper()), Encoding.ASCII.GetBytes(receivedstr).Length, remoteIPEndPoint);
                    Log("Sent connection to " + remoteIPEndPoint.Address.ToString() + ": " + receivedstr.ToUpper());
                }
                catch(Exception e)
                {
                    Log("Error: " + e.Message, logtype.error);
                    Log(e.StackTrace, logtype.error);
                    Log("Error caused program to terminate. Check error logs for details.");
                    break;
                }
            }
        }
        static bool checkargs(string[] args)
        {
            for(int i = 0;i!= args.Length;i++)
            {
                switch(args[i].ToLower())
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
                            Log("unable to read \"" + args[i - 1] + " " + args[i] + "\".",logtype.error);
                        }
                        break;
                }
            }
            return true;
        }
        enum logtype { error, log, console_only}
        static void Log(string errormsg, logtype lt = logtype.log)
        {
            switch(lt)
            {
                case logtype.console_only:
                    Console.WriteLine(DateTime.Now.ToLongTimeString() + " | " + errormsg);
                    break;
                case logtype.log:
                    string[] templog = { DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - " + errormsg };
                    File.WriteAllLines("Mirror.log", templog);
                    if (verbose)
                        Console.WriteLine(templog[0]);
                    break;
                case logtype.error:
                    string[] temperr = { DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - " + errormsg };
                    File.WriteAllLines("Error.log",temperr);
                    if (verbose)
                        Console.WriteLine(temperr[0]);
                    break;
            }
        }
    }
}
