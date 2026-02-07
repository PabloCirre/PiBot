using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public class PngToIco
{
    public static void Main(string[] args)
    {
        if (args.Length < 2) return;
        string input = args[0];
        string output = args[1];

        try {
            using (Bitmap source = new Bitmap(input)) {
                // Resize to standard 256x256 for better Win32 support
                using (Bitmap resized = new Bitmap(source, new Size(256, 256))) {
                    using (MemoryStream ms = new MemoryStream()) {
                        resized.Save(ms, ImageFormat.Png);
                        byte[] pngData = ms.ToArray();

                        using (FileStream fs = new FileStream(output, FileMode.Create)) {
                            using (BinaryWriter bw = new BinaryWriter(fs)) {
                                // ICONDIR
                                bw.Write((short)0); // Reserved
                                bw.Write((short)1); // Type (Icon)
                                bw.Write((short)1); // Image count

                                // ICONDIRENTRY
                                bw.Write((byte)0); // Width (0 = 256)
                                bw.Write((byte)0); // Height (0 = 256)
                                bw.Write((byte)0); // Colors
                                bw.Write((byte)0); // Reserved
                                bw.Write((short)1); // Color planes
                                bw.Write((short)32); // Bits per pixel
                                bw.Write(pngData.Length); // Size
                                bw.Write(22); // Offset (Header size 6 + entry size 16)

                                bw.Write(pngData);
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Done.");
        } catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
    }
}
