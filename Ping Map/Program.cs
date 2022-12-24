using System.Collections;
using System.Net;
using static Ping_Map.PingIPs;
using static Ping_Map.ImageTest;
using ILGPU;
using ILGPU.Runtime.Cuda;

namespace Ping_Map
{
    internal class Program
    {
        private static Task Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.WriteLine($"CUDA | Threads | Name{Environment.NewLine}");

            var context2 = Context.Create().EnableAlgorithms().Default();
            var context = context2.ToContext();

            DisplayDevices(context);


            var deviceIndex = DeviceMenu(0, context.Devices.Length);

            while (deviceIndex == -1)
            {
                deviceIndex = DeviceMenu(0, context.Devices.Length);
            }

            var device2 = context.Devices[deviceIndex -2];
            var accelerator = device2.CreateAccelerator(context);

            Console.CursorTop = 2;
            DisplayDevices(context);
            Console.CursorTop = deviceIndex;
            Console.Write($">");
            Console.CursorTop = Console.CursorTop = context.Devices.Length + 2;

            Console.WriteLine(Environment.NewLine);

            var resolution = 16;
            int[] resolutionArray = { 16, 16, 16, 16 };

            Console.WriteLine($"Resolution (x^4 / 256^4 Addresses will be scanned, Square numbers only)");
            Console.WriteLine(Environment.NewLine);
            Console.CursorVisible = true;
            Console.WriteLine($"Advanced Selection? (Spacebar for Yes)");

            if (Console.ReadKey().Key == ConsoleKey.Spacebar)
            {
                Console.CursorLeft = 0;
                Console.WriteLine("Each value (of 4 total) gives the resolution for its corresponding number in the IP address");

                for (int i = 0; i < 4; i++)
                { 
                    try
                    {
                        resolutionArray[i] = int.Parse(Console.ReadLine());
                    } catch { }
                    resolution = (int)Math.Pow(resolutionArray[0] * resolutionArray[1] * resolutionArray[2] * resolutionArray[3], (float)1 / 4);
                }
            }
            else
            {

                try
                {
                    resolution = int.Parse(Console.ReadLine());

                }
                catch
                { }

                for (int i = 0; i < 4; i++)
                { 
                    resolutionArray[i] = resolution;
                }

            }

            Console.CursorVisible = false;

            Console.WriteLine($"{Environment.NewLine}");
            /* */

            Progress.Total = (int)Math.Pow(resolution, 4);

            var ipList = GenerateIPlist(resolutionArray);

            var data = PingArray(ipList).Result; //Pings all the ips. this can take very long time

            List<string> working = data[0].Cast<string>().ToList();
            List<int> delay = data[1].Cast<int>().ToList();

            Console.WriteLine($"{Environment.NewLine}");

            CreateImage(ipList, working, delay, resolution, accelerator, resolutionArray); // Maps IPs to image
            
            // Completion
            Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}Process Completed. You may now close this window");

            while (true) { Thread.Sleep(1000000000); }
        }

        public static void DisplayDevices(Context context)
        {
            
            foreach (var device in context.Devices)
            {
                for (int i = 0; i < context.Devices.Length; i++)
                {
                    try
                    {
                        if (context.GetCudaDevice(i).Equals(device))
                        {
                            Console.WriteLine($"   1 | {BeautifyInt(device.MaxNumThreads, 7)} | {device.Name}");

                        }
                        else
                        {
                            Console.WriteLine($"   0 | {BeautifyInt(device.MaxNumThreads, 7)} | {device.Name}");
                        }
                    }
                    catch { }
                }
            }
        }

        public static int DeviceMenu(int relIndex, int count)
        {
            var y = Console.GetCursorPosition().Top - relIndex;

            if (y < 2) y = 2;
            if (y > count + 1) y = count + 1;

            Console.CursorLeft = 0;
            Console.Write(" ");
            Console.SetCursorPosition(0, y);
            Console.Write(">");
            Console.CursorLeft = 0;
            ConsoleKey key = Console.ReadKey().Key;

            if (key == ConsoleKey.Enter)
            {
                Console.Write(">Sure?                                                                ");
                Console.CursorLeft = 0;
                return y + relIndex;
            }
            
            if (key == ConsoleKey.UpArrow)
            {
                DeviceMenu(1, count);
                return -1;
            }
            if (key == ConsoleKey.DownArrow)
            {
                DeviceMenu(-1, count);
                return -1;
            }

            DeviceMenu(0, count);
            return -1;
        }
        

        public static ArrayList GenerateIPlist(int[] resolutionArray)
        {
            var resolutionArrayTotal = (int)Math.Pow((resolutionArray[0] * resolutionArray[1] * resolutionArray[2] * resolutionArray[3]), (float)1 / 4);

            ArrayList ipList = new ArrayList();


            // create addValues
            int add1 = 0;
            int add2 = 0;
            int add3 = 0;
            int add4 = 0;

            List<int> addValues = new List<int>
            {
                add1,
                add2,
                add3,
                add4
            };

            int[] sideLen = { 4, 4, 4, 4 };
            int[] scale = { 16, 16, 16, 16};
            for (int i = 0; i < 4; i++)
            {
                sideLen[i] = (int)Math.Sqrt(resolutionArray[i]);
                scale[i] = 256 / resolutionArray[i];
            }
            

            //loop through all the IPs in a specific order
            for (var i5 = 0; i5 <= Math.Pow(resolutionArrayTotal, 2) - 1; i5++)
            {
                add1 = addValues[0];
                add2 = addValues[1];
                add3 = addValues[2];
                add4 = addValues[3];

                for (var i4 = 1 + (add1 * sideLen[0]); i4 <= sideLen[0] + (add1 * sideLen[0]); i4++)
                {

                    for (var i3 = 1 + (add2 * sideLen[1]); i3 <= sideLen[1] + (add2 * sideLen[1]); i3++)
                    {

                        for (var i2 = 1 + (add3 * sideLen[2]); i2 <= sideLen[2] + (add3 * sideLen[2]); i2++)
                        {

                            for (var i1 = 1 + (add4 * sideLen[3]); i1 <= sideLen[3] + (add4 * sideLen[3]); i1++)
                            {

                                string ip = $"{(i4 - 1) * scale[0]}.{(i3 - 1) * scale[1]}.{(i2 - 1) * scale[2]}.{(i1 - 1) * scale[3]}";

                                ipList.Add(ip);
                            }
                        }
                    }
                }

                addValues = NewLine(addValues, sideLen);
            }

            return ipList;
        }

        public static List<int> NewLine(List<int> prev, int[] sideLen)
        {
            // unpack
            var add1 = prev[0];
            var add2 = prev[1];
            var add3 = prev[2];
            var add4 = prev[3];

            // increment
            add4++;

            // overflow logic
            if (add4 >= sideLen[3])
            {
                add4 = 0;
                add3++;
            }

            if (add3 >= sideLen[2])
            {
                add3 = 0;
                add2++;
            }

            if (add2 >= sideLen[1])
            {
                add2 = 0;
                add1++;
            }

            if (add1 >= sideLen[0])
            {
                add1 = 0;
            }

            // repack
            List<int> current = new List<int>
            {
                add1,
                add2,
                add3,
                add4
            };

            return current;
        }

        public static string CharGen(int count, string character)
        {
            string output = "";
            for (int i = 0; i < count; i++)
            {
                output += character;
            }
            return output;
        }

        public static string BeautifyInt(int input, int length)
        {
            var inputLength = input.ToString().Length;
            if (inputLength < length)
            {
                var diff = length - inputLength;
                return CharGen(diff, " ") + input;
            }

            return input.ToString();
            
        }
    }
}
