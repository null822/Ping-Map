using System.Collections;
using System.Net;
using System.Net.NetworkInformation;


namespace Ping_Map
{
    static class Progress
    {
        public static int Counter;
        public static int Total;
    }

    public class PingIPs
    {
        public static async Task<List<ArrayList>> PingArray(ArrayList ipList)
        {

            List<IPAddress> pingTargetHostsList = new();

            Console.WriteLine("Loading IPs to Ping");
            foreach (string ipString in ipList)
            {
                IPAddress ip = IPAddress.Parse(ipString);
                pingTargetHostsList.Add(ip);
            }

            IPAddress[] pingTargetHosts = pingTargetHostsList.ToArray();

            Console.WriteLine("Pining IPs");
            var pingTasks = pingTargetHosts.Select(
                host => BufferCall(host)).ToList();

            Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}");


            var pingResults = await Task.WhenAll(pingTasks);


            Console.WriteLine("Removing Failed Replies");
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
            if (reply.Status == IPStatus.Success && !reply.Address.Equals(IPAddress.Parse("8.8.8.8")))
            {
                var workingIP = reply.Address.ToString();
                return Task.FromResult(workingIP);
            }
            
            return Task.FromResult("failed");
        }

        public static Task<int> AnalyzeDataDelay(PingReply reply)
        {
            if (reply.Status == IPStatus.Success && !reply.Address.Equals(IPAddress.Parse("8.8.8.8")))
            {
                var time = reply.RoundtripTime;

                return Task.FromResult((int)time);
            }

            return Task.FromResult(0);
        }


        public static Task<PingReply> Ping(IPAddress host)
        {
            return new Ping().SendPingAsync(host, 2000);
        }

        private static async Task<PingReply?> BufferCall(IPAddress host)
        {
            Progress.Counter++;

            Console.CursorLeft = 0;
            Console.Write("                                                                 ");
            Console.CursorLeft = 0;
            
            Console.Write(Math.Round((float)Progress.Counter / Progress.Total * 100) + "% [ "+ Progress.Counter + " / " + Progress.Total + " ] " + host);

            try
            {
                //Console.WriteLine(host);
                return await Ping(host);
            }
            catch
            {
                return await Ping(IPAddress.Parse("8.8.8.8"));
            }
        }
    }
}
