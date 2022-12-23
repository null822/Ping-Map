using System.Collections;
using ILGPU;
using ILGPU.Runtime;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = SixLabors.ImageSharp.Color;

namespace Ping_Map
{
    internal class ImageTest
    {
        public static void CreateImage(ArrayList ipArrayList, List<string> working, List<int> delay, int resolution, Accelerator accelerator)
        {
            List<string> ipList = new List<string>();
            foreach (string ip in ipArrayList)
            {
                ipList.Add(ip);
            }

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



                Console.WriteLine("Calculating Image Data: ");
                
                Console.WriteLine("    Coords");
                List<int[]> imageDataCoords = ImageDataCoords(accelerator, size);

                Console.WriteLine("    Brightness");
                int[] imageDataB = ImageDataBrightness(accelerator, delay.ToArray(), size);

                // unpack the coords
                int[] imageDataY = imageDataCoords[0];
                int[] imageDataX = imageDataCoords[1];


                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Writing Pixels to Image");
                for (int i = 0; i < ipList.Count; i++)
                {
                    int x = imageDataX[i];
                    int y = imageDataY[i];
                    int b = imageDataB[i];
                    
                    image[x, y] = Color.FromRgb((byte)b, (byte)b, (byte)b);
                }
                
                Console.WriteLine($"{Environment.NewLine}");
                Console.WriteLine("Saving Image");

                image.SaveAsync("map_" + resolution +".png");
            }
        }
        
        #region GPU Compute

        #region Coords
        
        static List<int[]> ImageDataCoords(Accelerator accelerator, int size)
        {

            var kernel = accelerator.LoadAutoGroupedStreamKernel<
                Index1D,
                int,
                ArrayView1D<int, Stride1D.Dense>,
                ArrayView1D<int, Stride1D.Dense>
            >(ImageDataCoordsKernel);

            using var yCoordsOutBuffer = accelerator.Allocate1D<int>((long)Math.Pow(size, 2));
            using var xCoordsOutBuffer = accelerator.Allocate1D<int>((long)Math.Pow(size, 2));

            kernel(yCoordsOutBuffer.Extent.ToIntIndex(), size, yCoordsOutBuffer.View, xCoordsOutBuffer.View);

            List<int[]> outputPackage = new List<int[]> (2)
            {
                yCoordsOutBuffer.GetAsArray1D(),
                xCoordsOutBuffer.GetAsArray1D()
            };
            // Reads data from the GPU buffer into a new CPU array
            return outputPackage;
        }
        
        // The kernel that runs on the accelerated device
        static void ImageDataCoordsKernel(
            Index1D index,
            int size,
            ArrayView1D<int, Stride1D.Dense> yCoords,
            ArrayView1D<int, Stride1D.Dense> xCoords
        )
        {
            int y = (int)Math.Floor((float)index.Size / size);
            int x = index.Size - y * size;

            yCoords[index] = y;
            xCoords[index] = x;
        }

        #endregion


        #region Brightness Values
        
        static int[] ImageDataBrightness(Accelerator accelerator, int[] delay, int size)
        {
            var kernel = accelerator.LoadAutoGroupedStreamKernel<
                Index1D,
                //Index1D,
                int,
                ArrayView1D<int, Stride1D.Dense>,
                ArrayView1D<int, Stride1D.Dense>
            >(ImageDataBrightnessKernel);
            
            using var delayBuffer = accelerator.Allocate1D<int>((long)Math.Pow(size, 2));
            using var bValueOutBuffer = accelerator.Allocate1D<int>((long)Math.Pow(size, 2));

            delayBuffer.CopyFromCPU(delay);

            kernel(delayBuffer.Extent.ToIntIndex(), size, delayBuffer.View, bValueOutBuffer.View);

            // Reads data from the GPU buffer into a new CPU array
            
            return bValueOutBuffer.GetAsArray1D();
        }

        // The kernel that runs on the accelerated device
        static void ImageDataBrightnessKernel(
            Index1D index1D,
            //Index2D index2D,
            int size,
            ArrayView1D<int, Stride1D.Dense> delay,
            ArrayView1D<int, Stride1D.Dense> bValueOut
        )
        {
            int b = (int)(Math.Log(delay[index1D] / (float)7.8125, 2.1) * 32);

            bValueOut[index1D] = b;
        }

        #endregion

        #endregion
    }
}
