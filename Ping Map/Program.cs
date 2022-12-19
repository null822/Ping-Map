using Microsoft.CSharp;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using static Ping_Map.PingIPs;
using static Ping_Map.ImageTest;

namespace Ping_Map
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine($"Resolution (e.g. 16 scans 16^4 / 256^4 of all IP Addresses)");
            var resolution = 16;
            var ipPerProcess = 4;
            try
            {
                resolution = int.Parse(Console.ReadLine());
            } catch { }

            Console.WriteLine($"{Environment.NewLine}");
            /* */

            Progress.Total = (int)Math.Pow(resolution, 4);

            var ipList = GenerateIPlist(resolution);

            var data = PingArrayAsync(ipList).Result; //Pings all the ips. this can take very long time

            List<string> working = data[0].Cast<string>().ToList();
            List<int> delay = data[1].Cast<int>().ToList();

            Console.WriteLine($"{Environment.NewLine}");

            CreateImage(ipList, working, delay, resolution);

            // Completion
            Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}Done Pinging IPs, analyzing data, and saving the image. You may now close this window");

            while (true) { Thread.Sleep(1000000000); }
        }
        
        public static ArrayList GenerateIPlist(int resolution)
        {
            ArrayList ipList = new ArrayList();

            for (var i1 = 0; i1 <= resolution - 1; i1++)
            {
                var ip1 = (i1 * (256 / resolution)).ToString();

                for (var i2 = 0; i2 <= resolution - 1; i2++)
                {
                    var ip2 = ip1 + "." + i2 * (256 / resolution);

                    for (var i3 = 0; i3 <= resolution - 1; i3++)
                    {
                        var ip3 = ip2 + "." + i3 * (256 / resolution);

                        for (var i4 = 0; i4 <= resolution - 1; i4++)
                        {
                            var ip = ip3 + "." + i4 * (256 / resolution);

                            if (!(i1 == 0 && i2 == 0 && i3 == 0 && i4 == 0))
                            {
                                ipList.Add(ip);
                            }
                        }
                    }
                }
            }

            return ipList;

        }
    }
}
