using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;
using static Ping_Map.PingIPs;

namespace Ping_Map
{
    internal class ImageTest
    {
        public static void CreateImage(ArrayList ipList, List<string> working, List<int> delay, int resolution)
        {
            Int32 size = (Int32) Math.Pow(resolution, 2);

            using (Image<Rgba32> image = new(size, size))
            {
                //Set Background
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                        
                        for (int x = 0; x < pixelRow.Length; x++)
                        {
                            ref Rgba32 pixel = ref pixelRow[x];
                            if (pixel.A == 0)
                            {
                                pixel = Color.Black;
                            }
                        }
                    }
                });

                ArrayList ipListMapped = GenerateMappedIPlist(resolution);

                Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}");

                Console.WriteLine("|    IP Address   |" + SpcGen(size.ToString().Length / 4) + "X " + SpcGen(size.ToString().Length / 4) + "|" +SpcGen(size.ToString().Length / 4) + "Y " + SpcGen(size.ToString().Length / 4) + "| Delay | Brightness");
                Console.WriteLine("|                 |" + SpcGen(size.ToString().Length / 4) + "  " + SpcGen(size.ToString().Length / 4) + "|" +SpcGen(size.ToString().Length / 4) + "  " + SpcGen(size.ToString().Length / 4) + "|       |     |");

                foreach (String ip in working)
                {
                    float indexMapped = ipListMapped.IndexOf(ip);
                    float index = ipList.IndexOf(ip);

                    int y = (int)Math.Floor(indexMapped / size);
                    int x = (int)indexMapped - y * size;

                    long ipDelay = delay[(int)index];

                    try
                    {
                        int brightness = (int)Math.Log(ipDelay / 7.8125, 2.1) * 32;

                        Console.WriteLine($"| {IPbeautify(ip)} | {BeautifyInt(x.ToString(), size.ToString().Length)} | {BeautifyInt(y.ToString(), size.ToString().Length)} |  {BeautifyInt(ipDelay.ToString(), 4)} | {BeautifyInt(brightness.ToString(), 3)} |");

                        image[x, y] = Color.FromRgb((byte)brightness, (byte)brightness, (byte)brightness);
                    }
                    catch
                    {
                        Console.WriteLine($"Error writing pixel for {ip}");
                    }
                }
                
                image.Save("map_" + resolution +".png");
            }
        }
        public static ArrayList GenerateMappedIPlist(int resolution)
        {
            ArrayList ipList = new ArrayList();

            Console.WriteLine($"|     TREE     |    IP Address   |");
            Console.WriteLine($"|              |                 |");

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

            for (var i5 = 0; i5 <= Math.Pow(resolution, 2)-1; i5++)
            {
                add1 = addValues[0];
                add2 = addValues[1];
                add3 = addValues[2];
                add4 = addValues[3];

                for (var i4 = 1 + (add1 * sideLen); i4 <= sideLen + (add1 * sideLen); i4++) {
                    
                    for (var i3 = 1 + (add2 * sideLen); i3 <= sideLen + (add2 * sideLen); i3++) {
                        
                        for (var i2 = 1 + (add3 * sideLen); i2 <= sideLen + (add3 * sideLen); i2++) {
                            
                            for (var i1 = 1 + (add4 * sideLen); i1 <= sideLen + (add4 * sideLen); i1++) {
                                
                                String ip = $"{(i4 - 1) * scale}.{(i3 - 1) * scale}.{(i2 - 1) * scale}.{(i1 - 1) * scale}";

                                ipList.Add(ip);
                                Console.WriteLine("| " + BeautifyInt(i5.ToString(), (Math.Pow(resolution, 2) - 1).ToString().Length) + " | " + BeautifyInt((i4 - 1).ToString(), resolution.ToString().Length) + " " + BeautifyInt((i3 - 1).ToString(), resolution.ToString().Length) + " " + BeautifyInt((i2 - 1).ToString(), resolution.ToString().Length) + " " + BeautifyInt((i1 - 1).ToString(), resolution.ToString().Length) + " | " + IPbeautify(ip) + " |");

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
    }
}
