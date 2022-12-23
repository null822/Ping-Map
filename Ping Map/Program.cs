using System.Collections;
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


            var index = DeviceMenu(0, context.Devices.Length);

            while (index == -1)
            {
                index = DeviceMenu(0, context.Devices.Length);
            }

            var device2 = context.Devices[index-2];
            var accelerator = device2.CreateAccelerator(context);

            Console.CursorTop = 2;
            DisplayDevices(context);
            Console.CursorTop = index;
            Console.Write($">");
            Console.CursorTop = Console.CursorTop = context.Devices.Length + 2;


            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Resolution (x^4 / 256^4 Addresses will be scanned, Square numbers are recommended)");
            var resolution = 16;
            Console.CursorVisible = true;
            try
            {
                resolution = int.Parse(Console.ReadLine());
            } catch { }
            Console.CursorVisible = false;

            Console.WriteLine($"{Environment.NewLine}");
            /* */

            Progress.Total = (int)Math.Pow(resolution, 4);

            var ipList = GenerateIPlist(resolution);

            var data = PingArray(ipList).Result; //Pings all the ips. this can take very long time

            List<string> working = data[0].Cast<string>().ToList();
            List<int> delay = data[1].Cast<int>().ToList();

            Console.WriteLine($"{Environment.NewLine}");

            CreateImage(ipList, working, delay, resolution, accelerator); // Maps IPs to image
            
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
        

        public static ArrayList GenerateIPlist(int resolution)
        {
            ArrayList ipList = new ArrayList();

            var sideLen = (int)Math.Sqrt(resolution);

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

            var scale = 256 / resolution;

            //loop through all the IPs in a specific order

            for (var i5 = 0; i5 <= Math.Pow(resolution, 2) - 1; i5++)
            {
                add1 = addValues[0];
                add2 = addValues[1];
                add3 = addValues[2];
                add4 = addValues[3];

                for (var i4 = 1 + (add1 * sideLen); i4 <= sideLen + (add1 * sideLen); i4++)
                {

                    for (var i3 = 1 + (add2 * sideLen); i3 <= sideLen + (add2 * sideLen); i3++)
                    {

                        for (var i2 = 1 + (add3 * sideLen); i2 <= sideLen + (add3 * sideLen); i2++)
                        {

                            for (var i1 = 1 + (add4 * sideLen); i1 <= sideLen + (add4 * sideLen); i1++)
                            {

                                string ip = $"{(i4 - 1) * scale}.{(i3 - 1) * scale}.{(i2 - 1) * scale}.{(i1 - 1) * scale}";

                                ipList.Add(ip);
                            }
                        }
                    }
                }

                addValues = NewLine(addValues, sideLen);
            }

            return ipList;
        }

        public static List<int> NewLine(List<int> prev, int sideLen)
        {
            // unpack
            var add1 = prev[0];
            var add2 = prev[1];
            var add3 = prev[2];
            var add4 = prev[3];

            // increment
            add4++;

            // overflow logic
            if (add4 >= sideLen)
            {
                add4 = 0;
                add3++;
            }

            if (add3 >= sideLen)
            {
                add3 = 0;
                add2++;
            }

            if (add2 >= sideLen)
            {
                add2 = 0;
                add1++;
            }

            if (add1 >= sideLen)
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
