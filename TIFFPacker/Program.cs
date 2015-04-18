using FreeImageAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIFFPacker
{
    class Program
    {
        static void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("    TIFFPacker.exe <inputfile> <outputfile>");
            Console.WriteLine("Example:");
            Console.WriteLine("    TIFFPacker.exe test.tiff rg.png");
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintHelp();
                return;
            }

            try
            {
                string inputFile = args[0];
                string outputFile = args[1];

                Console.WriteLine(String.Format("Loading file: {0}", inputFile));
                FIBITMAP image = FreeImage.LoadEx(inputFile);

                // Make sure the image is one of the supported types
                FREE_IMAGE_TYPE imgType = FreeImage.GetImageType(image);
                switch (imgType)
                {
                    case FREE_IMAGE_TYPE.FIT_INT16:
                        Console.WriteLine("Detected 16-bit short");
                        break;
                    case FREE_IMAGE_TYPE.FIT_INT32:
                        Console.WriteLine("Detected 32-bit int");
                        break;
                    case FREE_IMAGE_TYPE.FIT_UINT16:
                        Console.WriteLine("Detected 16-bit ushort");
                        break;
                    case FREE_IMAGE_TYPE.FIT_UINT32:
                        Console.WriteLine("Detected 32-bit uint");
                        break;
                    default:
                        Console.WriteLine(String.Format("Unsupported file type: {0}", imgType.ToString()));
                        return;
                }

                uint width = FreeImage.GetWidth(image);
                uint height = FreeImage.GetHeight(image);
                uint fileBPP = FreeImage.GetBPP(image);

                // Allocate new RGB Image
                FIBITMAP newMap = FreeImage.Allocate((int)width, (int)height, 8 /*BitsPerPixel*/ * 3);
                RGBQUAD outQuad = new RGBQUAD();

                // Multiplier for the byte offset into the scaline
                int iterations = 0;
                int lateralMultipler = fileBPP == 16 ? 2 : 4;
                for (uint x = 0; x < width; ++x, ++iterations)
                {
                    float progress = ((float)(iterations)) / ((float)(width * height));
                    Console.Write(String.Format("\rProgress {0:000.0}%     ", progress*100.0f));
                    for (uint y = 0; y < height; ++y, ++iterations)
                    {
                        IntPtr line = FreeImage.GetScanLine(image, (int)y);
                        if (fileBPP >= 16)
                        {
                            line = new IntPtr(line.ToInt64() + lateralMultipler * x);
                            if (imgType == FREE_IMAGE_TYPE.FIT_UINT16)
                            {
                                ushort value = (ushort)System.Runtime.InteropServices.Marshal.ReadInt16(line);
                                outQuad.rgbRed = (byte)(value / 256);
                                outQuad.rgbGreen = (byte)(value % 256);
                            }
                            else if (imgType == FREE_IMAGE_TYPE.FIT_UINT32)
                            {
                                uint value = (uint)System.Runtime.InteropServices.Marshal.ReadInt32(line);
                                outQuad.rgbRed = (byte)(value / 256);
                                outQuad.rgbGreen = (byte)(value % 256);
                            }
                            else if (imgType == FREE_IMAGE_TYPE.FIT_INT16)
                            {
                                short value = (short)System.Runtime.InteropServices.Marshal.ReadInt16(line);
                                outQuad.rgbRed = (byte)(value / 256);
                                outQuad.rgbGreen = (byte)(value % 256);
                            }
                            else if (imgType == FREE_IMAGE_TYPE.FIT_INT32)
                            {
                                int value = (int)System.Runtime.InteropServices.Marshal.ReadInt32(line);
                                outQuad.rgbRed = (byte)(value / 256);
                                outQuad.rgbGreen = (byte)(value % 256);
                            }
                            FreeImage.SetPixelColor(newMap, x, y, ref outQuad);
                        }
                        else
                        {

                        }
                    }
                }
                Console.WriteLine(" "); //empty space
                Console.WriteLine(String.Format("Writing file: {0}", outputFile));
                if (FreeImage.SaveEx(newMap, outputFile))
                    Console.WriteLine("Finished");
                else
                    Console.WriteLine("ERROR: Failed to write file");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: ");
                Console.Write(ex.Message);
            }
        }
    }
}
