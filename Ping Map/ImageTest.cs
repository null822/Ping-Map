using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;

namespace Ping_Map
{
    internal class ImageTest
    {
        public static void CreateImage(ArrayList working, int resolution)
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

                ArrayList ipList = GenerateMappedIPlist(resolution);
                //ArrayList ipList = new ArrayList();
                //foreach (var ip in ipArray) ipList.Add(ip);
                
                foreach (String ip in working)
                {
                    float index = ipList.IndexOf(ip);
                    int y = (int)Math.Floor(index / size);
                    int x = (int)index - y * size;

                    Console.WriteLine($"IP: {ip}, X: {x}, Y: {y}");
                    try
                    {
                        image[x, y] = Color.White;
                    }
                    catch
                    {
                        Console.WriteLine($"Could not write pixel for IP {ip}");
                    }
                }
                
                image.Save("map_" + resolution +".png");
            }
        }
        public static ArrayList GenerateMappedIPlist(int resolution)
        {
            ArrayList ipList = new ArrayList();

            Console.WriteLine($" |   TREE      | IP");

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
                var row = i5;

                add1 = addValues[0];
                add2 = addValues[1];
                add3 = addValues[2];
                add4 = addValues[3];

                Console.WriteLine(" | NEWLINE ---- [ " + add1 + " | " + add2 + " | " + add3 + " | " + add4 + " | ]");


                for (var i4 = 1 + (add1 * sideLen); i4 <= sideLen + (add1 * sideLen); i4++) {
                    
                    for (var i3 = 1 + (add2 * sideLen); i3 <= sideLen + (add2 * sideLen); i3++) {
                        
                        for (var i2 = 1 + (add3 * sideLen); i2 <= sideLen + (add3 * sideLen); i2++) {
                            
                            for (var i1 = 1 + (add4 * sideLen); i1 <= sideLen + (add4 * sideLen); i1++) {
                                
                                String ip = $"{(i4 - 1) * scale}.{(i3 - 1) * scale}.{(i2 - 1) * scale}.{(i1 - 1) * scale}";

                                ipList.Add(ip);
                                Console.WriteLine($" | {i5} | {i4 - 1} {i3 - 1} {i2 - 1} {i1 - 1} | IP: {ip}");

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






        /*      BACKUP
        public static ArrayList GenerateMappedIPlist(int resolution)
        {
            //Array ipList = Array.CreateInstance(typeof(String), (Int32)Math.Pow(resolution, 4) + 1);
            ArrayList ipList = new ArrayList();

            Console.WriteLine($" |           | IP");

            for (var i5 = 0; i5 <= Math.Pow(resolution, 2); i5++)
            {
                var row = i5;

                for (var i4 = 1; i4 <= Math.Sqrt(resolution) * 2; i4++)
                {

                    for (var i3 = 1; i3 <= Math.Sqrt(resolution) * 2; i3++)
                    {

                        for (var i2 = 1; i2 <= Math.Sqrt(resolution) * 2; i2++)
                        {

                            for (var i1 = 1; i1 <= Math.Sqrt(resolution) * 2; i1++)
                            {

                                String ip = $"{i4 * resolution}:{i3 * resolution}:{i2 * resolution}:{i1 * resolution}";

                                ipList.Add(ip);
                                Console.WriteLine($" | {i5} {i4 - 1} {i3 - 1} {i2 - 1} {i1 - 1} | IP: {ip}");

                            }
                        }
                    }
                }
            }


            return ipList;

        }
        */
    }
}
