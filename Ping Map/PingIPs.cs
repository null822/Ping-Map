using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
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
        public static async Task<List<ArrayList>> PingArray(ArrayList ipList)
        {
            Console.WriteLine("Pining IPs");

            List<IPAddress> pingTargetHostsList = new();

            foreach (String ipString in ipList)
            {
                IPAddress ip = IPAddress.Parse(ipString);
                pingTargetHostsList.Add(ip);
            }

            IPAddress[] pingTargetHosts = pingTargetHostsList.ToArray();

            var pingTasks = pingTargetHosts.Select(
                host => BufferCall(host)).ToList();

            Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}");

            var pingResults = await Task.WhenAll(pingTasks);

            pingResults = pingResults.Where(val => val != null).ToArray();


            Console.WriteLine("Retrieving Status Data");
            var processStatusTasks = pingResults.Select(
                reply => AnalyzeDataStatus(reply, pingResults)).ToList();
            ArrayList status = new ArrayList(await Task.WhenAll(processStatusTasks));


            Console.WriteLine("Retrieving Delay Data");
            var processDelayTasks = pingResults.Select(
                reply => AnalyzeDataDelay(reply)).ToList();
            ArrayList delay = new ArrayList(await Task.WhenAll(processDelayTasks));

            ArrayList working = new ArrayList();

            Console.WriteLine("Copying Working IPs");

            foreach (string ip in status)
            {
                if (ip != "failed")
                {
                    working.Add(ip);
                }
            }

            List<ArrayList> data = new()
            {
                working,
                delay
            };
            return data;
        }

        public static Task<string> AnalyzeDataStatus(PingReply reply, PingReply[] pingResults)
        {
            /*if (reply.Address.ToString() != "0.0.0.0" && reply.Address.ToString() != "8.8.8.8" && reply.RoundtripTime != 0)
            {
                Console.WriteLine(BeautifyInt(Array.IndexOf(pingResults, reply).ToString(), pingResults.Length.ToString().Length) + " | " + IPbeautify(reply.Address.ToString()));
            }
            */
            if (reply.Status == IPStatus.Success && !reply.Address.Equals(IPAddress.Parse("8.8.8.8")))
            {
                var workingIP = reply.Address.ToString();
                return Task.FromResult(workingIP);
            }
            
            return Task.FromResult("failed");
        }

        public static Task<int> AnalyzeDataDelay(PingReply reply)
        {
            /*if (reply.Address.ToString() != "0.0.0.0" && reply.Address.ToString() != "8.8.8.8" && reply.RoundtripTime != 0)
            {
                Console.WriteLine(IPbeautify(reply.Address.ToString()) + " | " + BeautifyInt(reply.RoundtripTime.ToString(), 4));
            }*/

            if (reply.Status == IPStatus.Success && !reply.Address.Equals(IPAddress.Parse("8.8.8.8")))
            {
                var time = reply.RoundtripTime;

                return Task.FromResult((int)time);
            }

            return Task.FromResult(0);
        }


        public static Task<PingReply> Ping(IPAddress host)
        {
            Progress.Counter++;
            return new Ping().SendPingAsync(host, 2000);
        }

        private static async Task<PingReply?> BufferCall(IPAddress host)
        {
            Console.CursorLeft = 0;
            Console.Write("                                                                 ");
            Console.CursorLeft = 0;
            
            Console.Write("IP: " + IPbeautify(host.ToString()) + " | " + Progress.Counter + " / " + Progress.Total + " |");

            try
            {
                //Console.WriteLine(host);
                return await Ping(host);
            }
            catch
            {
                Progress.Failed++;
                return await Ping(IPAddress.Parse("8.8.8.8"));
            }
        }

        public static string IPbeautify(string ip)
        {
            var searchIP = ip;

            var index1 = searchIP.IndexOf('.');
            searchIP = searchIP.Substring(0, index1) + searchIP.Substring(index1 + 1);


            var index2 = searchIP.IndexOf('.');
            searchIP = searchIP.Substring(0, index2) + searchIP.Substring(index2 + 1);


            var index3 = searchIP.IndexOf('.');


            //account for removed '.'s
            index2 += 1;
            index3 += + 2;

            var num1 = ip.Substring(0, index1);
            var num2 = ip.Substring(index1+1, index2 - index1 - 1);
            var num3 = ip.Substring(index2+1, index3 - index2 - 1);
            var num4 = ip.Substring(index3+1);

            string[] numArray = 
            {
                num1,
                num2,
                num3,
                num4
            };

            var zeroTally = 0;

            foreach (var num in numArray)
            {
                if (num.Length < 3)
                {
                    if (num.Length < 2)
                    {
                        zeroTally += 2;
                    }
                    else
                    {
                        zeroTally+= 1;
                    }
                }
            }


            var outputIP = ip + SpcGen(zeroTally-1);

            return outputIP;
        }

        public static string SpcGen(int count)
        {
            var output = "";
            for (int i = 0; i <= count; i++)
            {
                output = output + " ";
            }

            return output;
        }


        public static string BeautifyInt(string integer, int length)
        {
            var dif = length - integer.Length - 1;

            return SpcGen(dif) + integer;

        }
    }
}
