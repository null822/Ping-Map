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
    internal class Image
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
            //Array ipList = Array.CreateInstance(typeof(String), (Int32)Math.Pow(resolution, 4) + 1);
            ArrayList ipList = new ArrayList();


            Array ipList4 = Array.CreateInstance(typeof(Array), resolution);
            for (var i1 = 0; i1 <= resolution - 1; i1++)
            {
                //part #1
                var ip1 = (i1 * (256 / resolution)).ToString();
                Array ipList3 = Array.CreateInstance(typeof(Array), resolution);
                for (var i2 = 0; i2 <= resolution - 1; i2++)
                {
                    //part #2
                    var ip2 = ip1 + "." + i2 * (256 / resolution);
                    Array ipList2 = Array.CreateInstance(typeof(Array), resolution);
                    for (var i3 = 0; i3 <= resolution - 1; i3++)
                    {
                        //part #3
                        var ip3 = ip2 + "." + i3 * (256 / resolution);
                        Array ipList1 = Array.CreateInstance(typeof(String), resolution);
                        for (var i4 = 0; i4 <= resolution - 1; i4++)
                        {
                            //part #4
                            var ip = ip3 + "." + i4 * (256 / resolution);

                            //add IPs to Lists
                            ipList1.SetValue(ip, i4);
                        }
                        ipList2.SetValue(ipList1, i3);
                    }
                    ipList3.SetValue(ipList2, i2);
                }
                ipList4.SetValue(ipList3, i1);
            }

            /*
            foreach (Array ipList3 in ipList4)
            {
                foreach (Array ipList2 in ipList3)
                {
                    foreach (Array ipList1 in ipList2)
                    {
                        foreach (String ip in ipList1)
                        {
                            ipList.Add(ip);
                            Console.WriteLine(ip);
                        }
                    }
                }
            }
            */


            for (var i5 = 0; i5 <= Math.Pow(resolution, 2); i5++)
            {
                var row = i5;

                for (var i4 = 1; i4 <= Math.Sqrt(resolution) * 2; i4++) {
                    Array ipList3 = Array.CreateInstance(typeof(Array), (Int32)Math.Sqrt(resolution));
                    try {
                        ipList3 = (Array)ipList4.GetValue(i4 + row - 1);
                    } catch { }

                    for (var i3 = 1; i3 <= Math.Sqrt(resolution) * 2; i3++) {
                        Array ipList2 = Array.CreateInstance(typeof(Array), (Int32)Math.Sqrt(resolution));
                        try {
                            ipList2 = (Array)ipList3.GetValue(i3 + row - 1);
                        } catch { }

                        for (var i2 = 1; i2 <= Math.Sqrt(resolution) * 2; i2++) {
                            Array ipList1 = Array.CreateInstance(typeof(String), (Int32)Math.Sqrt(resolution) + 1);
                            try {
                            ipList1 = (Array)ipList2.GetValue(i2 + row - 1);
                            } catch { }

                            if (ipList1 == null)
                            {
                                ipList1.SetValue("", 0);
                            }
                            for (var i1 = 1; i1 <= Math.Sqrt(resolution) * 2; i1++) {
                                try {
                                    String ip = (String)ipList1.GetValue(i1 + row - 1);
                                    
                                ipList.Add(ip);
                                Console.WriteLine($"IP (i5->i1) | {i5} {i4-1} {i3-1} {i2-1} {i1-1} | IP: {ip}");
                                } catch { }
                            }
                        }
                    }
                }
            }


            return ipList;

        }
    }
}
