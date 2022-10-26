using Microsoft.CSharp;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using static ParallellProcess.Processor;

namespace Main
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine($"Resolution e.g. 3 scans 3/256 of the internet");
            var resolution = 16;
            resolution = int.Parse(Console.ReadLine());
            Console.WriteLine($"{Environment.NewLine}");


            var ipList = generateIPlist(resolution);


            ArrayList ipList1 = new ArrayList();
            ArrayList ipList2 = new ArrayList();
            ArrayList ipList3 = new ArrayList();
            ArrayList ipList4 = new ArrayList();
            ArrayList ipList5 = new ArrayList();
            ArrayList ipList6 = new ArrayList();
            ArrayList ipList7 = new ArrayList();
            ArrayList ipList8 = new ArrayList();

            var sectorSize = ipList.Count / 8;

            foreach (String ip in ipList)
            {
                if (ipList.IndexOf(ip) <= sectorSize)
                {
                    ipList1.Add(ip);
                }
                else if (ipList.IndexOf(ip) <= sectorSize * 2)
                {
                    ipList2.Add(ip);
                }
                else if (ipList.IndexOf(ip) <= sectorSize * 3)
                {
                    ipList3.Add(ip);
                }
                else if (ipList.IndexOf(ip) <= sectorSize * 4)
                {
                    ipList4.Add(ip);
                }
                else if (ipList.IndexOf(ip) <= sectorSize * 5)
                {
                    ipList5.Add(ip);
                }
                else if (ipList.IndexOf(ip) <= sectorSize * 6)
                {
                    ipList6.Add(ip);
                }
                else if (ipList.IndexOf(ip) <= sectorSize * 7)
                {
                    ipList7.Add(ip);
                }
                else if (ipList.IndexOf(ip) <= sectorSize * 8)
                {
                    ipList8.Add(ip);
                }

            }

            ArrayList working = new ArrayList();

            //Process(es)
            var Process1 = Task.Factory.StartNew(() => Scan(ipList1));
            var Process2 = Task.Factory.StartNew(() => Scan(ipList2));
            var Process3 = Task.Factory.StartNew(() => Scan(ipList3));
            var Process4 = Task.Factory.StartNew(() => Scan(ipList4));
            var Process5 = Task.Factory.StartNew(() => Scan(ipList5));
            var Process6 = Task.Factory.StartNew(() => Scan(ipList6));
            var Process7 = Task.Factory.StartNew(() => Scan(ipList7));
            var Process8 = Task.Factory.StartNew(() => Scan(ipList8));

            Task.WaitAll(Process1, Process2, Process3, Process4, Process5, Process6, Process7, Process8);

            foreach (String IP in Process1.Result) if (!working.Contains(IP)) working.Add(IP);
            foreach (String IP in Process2.Result) if (!working.Contains(IP)) working.Add(IP);
            foreach (String IP in Process3.Result) if (!working.Contains(IP)) working.Add(IP);
            foreach (String IP in Process4.Result) if (!working.Contains(IP)) working.Add(IP);
            foreach (String IP in Process5.Result) if (!working.Contains(IP)) working.Add(IP);
            foreach (String IP in Process6.Result) if (!working.Contains(IP)) working.Add(IP);
            foreach (String IP in Process7.Result) if (!working.Contains(IP)) working.Add(IP);
            foreach (String IP in Process8.Result) if (!working.Contains(IP)) working.Add(IP);


            Console.WriteLine($"{Environment.NewLine}Working IPs:{Environment.NewLine}");
            foreach (String IP in working)
            {
                Console.WriteLine($"{IP}");
            }
        }

        public static Boolean Ping(String IP)
        {
            Ping ping = new Ping();
            try
            {
                var sentPing = ping.Send(IP);
                if (sentPing.Status == IPStatus.Success)
                {
                    return true;
                }
            }
            catch { }

            return false;

        }

        public static ArrayList generateIPlist(int resolution)
        {
            ArrayList IPlist = new ArrayList();

            for (var i1 = 0; i1 <= resolution - 1; i1++)
            {
                var IP1 = (i1 * (256 / resolution)).ToString();

                for (var i2 = 0; i2 <= resolution - 1; i2++)
                {
                    var IP2 = IP1 + "." + (i2 * (256 / resolution)).ToString();

                    for (var i3 = 0; i3 <= resolution - 1; i3++)
                    {
                        var IP3 = IP2 + "." + (i3 * (256 / resolution)).ToString();

                        for (var i4 = 0; i4 <= resolution - 1; i4++)
                        {
                            var IP = IP3 + "." + (i4 * (256 / resolution)).ToString();

                            if (IP1 != "0")
                            {
                                IPlist.Add(IP);
                            }

                        }
                    }
                }
            }

            return IPlist;

        }
    }
}
