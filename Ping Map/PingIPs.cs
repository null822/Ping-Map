using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using static Ping_Map.Program;


namespace Ping_Map
{
    static class Progress
    {
        public static int Counter = 0;
        public static int Failed = 0;
        public static int Total = 0;
    }

    public class PingIPs
    {
        public static async Task<ArrayList> PingArrayAsync(ArrayList ipList)
        {
            Console.WriteLine("Approx. Statistics: ");

            ArrayList working = new ArrayList();


            List<IPAddress> pingTargetHostsList = new();

            foreach (String ipString in ipList)
            {
                IPAddress ip = IPAddress.Parse(ipString);
                pingTargetHostsList.Add(ip);
            }

            IPAddress[] pingTargetHosts = pingTargetHostsList.ToArray();

            var pingTasks = pingTargetHosts.Select(
                host => BufferCall(host)).ToList();


            var pingResults = await Task.WhenAll(pingTasks);

            pingResults = pingResults.Where(val => val != null).ToArray();

            Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}");

            Console.WriteLine("Statuses: ");

            Console.WriteLine($"{Environment.NewLine}");

            foreach (PingReply reply in pingResults)
            {
                Console.WriteLine(reply.Status);
            }
            
            foreach (PingReply reply in pingResults)
            {
                if (reply.Status == IPStatus.Success)
                {
                    var workingIP = reply.Address.ToString();
                    if (!working.Contains(workingIP))
                    {
                        working.Add(workingIP);
                    }
                }
            }
            
            Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}Working IPs:");

            foreach (String ip in working)
            {
                Console.WriteLine(ip);
            }

            return working;
        }

        public static Task<PingReply> Ping(IPAddress host)
        {
            Progress.Counter++;
            return new Ping().SendPingAsync(host, 2000);
        }

        private static async Task<PingReply?> BufferCall(IPAddress host)
        {
            Console.CursorLeft = 0;
            Console.Write("                                                           ");
            Console.CursorLeft = 0;
            Console.Write(Progress.Counter / Progress.Total + "% | " + Progress.Counter + " / " + Progress.Total + " | Failed: " + Progress.Failed + " | IP: " + host);
            try
            {
                //Console.WriteLine(host);
                return await Ping(host);
            }
            catch
            {
                Progress.Failed++;
                return null;
            }
        }
    }
}
