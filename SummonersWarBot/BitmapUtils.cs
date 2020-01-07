﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SummonersWarBot
{
    public class BitmapUtils
    {
        public static Color GetColor(int x, int y, Bitmap bitmap)
        {
            return bitmap.GetPixel(x, y);
        }

        /// <param name="color">color to compare</param>
        /// <param name="x">location X</param>
        /// <param name="y">location Y</param>
        /// <param name="bitmap">origin Bitmap to compare</param>
        /// <param name="toleranz">0-255</param>
        /// <returns></returns>
        public static bool IsColor(Color color, int x, int y, Bitmap bitmap, byte toleranz = 0)
        {
            Color bitmapColor = GetColor(x, y, bitmap);
            return IsColor(bitmapColor, color, toleranz);
        }

        public static bool IsColor(Color color1, Color color2, byte toleranz)
        {
            return Math.Abs(color1.R - color2.R) <= toleranz && Math.Abs(color1.G - color2.G) <= toleranz && Math.Abs(color1.B - color2.B) <= toleranz && Math.Abs(color1.A - color2.A) <= toleranz;
        }

        public static bool IsColor(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2, byte toleranz)
        {
            return Math.Abs(r1 - r2) <= toleranz && Math.Abs(g1 - g2) <= toleranz && Math.Abs(b1 - b2) <= toleranz;
        }

        public static int GetAbs(Color color1, Color color2)
        {
            return Math.Abs(color1.R - color2.R) + Math.Abs(color1.G - color2.G) + Math.Abs(color1.B - color2.B) + Math.Abs(color1.A - color2.A);
        }

        public static int GetAbs(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
        {
            return Math.Abs(r1 - r2) + Math.Abs(g1 - g2) + Math.Abs(b1 - b2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="bitmap"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="toleranz">toleranz in %</param>
        /// <returns></returns>
        public static bool BitmapQuals(Bitmap origin, Bitmap bitmap, int posX, int posY, float toleranz = 0)
        {
            BitmapData oSrc = origin.LockBits(new Rectangle(0, 0, origin.Width, origin.Height), ImageLockMode.ReadOnly, origin.PixelFormat);
            BitmapData bSrc = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int originBytesPerPixel = Bitmap.GetPixelFormatSize(origin.PixelFormat) / 8;
            int bitmapBytesPerPixel = Bitmap.GetPixelFormatSize(bSrc.PixelFormat) / 8;
            int byteCount = oSrc.Stride * origin.Height; 
            int byteCount2 = bSrc.Stride * bitmap.Height;
            byte[] pixels = new byte[byteCount];
            byte[] pixels2 = new byte[byteCount2];
            IntPtr ptrFirstPixel = oSrc.Scan0;
            IntPtr ptrFirstPixel2 = bSrc.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length); 
            Marshal.Copy(ptrFirstPixel2, pixels2, 0, pixels2.Length);
            int heightInPixels = bSrc.Height;
            int widthInPixels = bSrc.Width;
            int total = 0;
            int max = 0;
            for (int y = 0; y < heightInPixels; y++)
            {
                int originCurrentLine = (y + posY) * oSrc.Stride;
                int bitmapCurrentLine = y * bSrc.Stride;
                for (int x = 0; x < widthInPixels; x++)
                {
                    int originCurrentPixel = originCurrentLine + (x + posX) * originBytesPerPixel; 
                    int bitmapCurrentPixel = bitmapCurrentLine + x * bitmapBytesPerPixel;
                    if (pixels[originCurrentLine + x + 3] == 0)
                        continue;
                    byte r1 = pixels[originCurrentPixel + 2];
                    byte r2 = pixels[bitmapCurrentPixel + 2];
                    byte g1 = pixels[originCurrentPixel + 1];
                    byte g2 = pixels[bitmapCurrentPixel + 1];
                    byte b1 = pixels[originCurrentPixel + 0];
                    byte b2 = pixels[bitmapCurrentPixel + 0];
                    total += GetAbs(r1, g1, b1, r2, g2, b2);
                    max += 255 * 3;
                }
            }
            origin.UnlockBits(oSrc);
            bitmap.UnlockBits(bSrc);
            float equal = ((max - total) / (float)max) * 100;
            Console.WriteLine(equal);
            return equal >= (100 - toleranz);
        }

        public static bool BitmapEquals2(Bitmap origin, Bitmap bitmap, int posX, int posY, float toleranz = 0)
        {
            int total = 0;
            int max = 0;
            for(int x = 0; x < bitmap.Width; x++)
            {
                for(int y = 0; y < bitmap.Height; y++)
                {
                    Color c1 = bitmap.GetPixel(x, y);
                    Color c2 = origin.GetPixel(x + posX, y + posY);
                    max += 255 * 4;
                    total += GetAbs(c1, c2);
                }
            }
            float equal = ((max - total) / (float)max) * 100;
            return equal >= (100 - toleranz);
        }

        public static float GetBitmapEquals(Bitmap origin, Bitmap bitmap, int posX, int posY, float toleranz = 0)
        {
            int total = 0;
            int max = 0;
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color c1 = bitmap.GetPixel(x, y);
                    Color c2 = origin.GetPixel(x + posX, y + posY);
                    max += 255 * 4;
                    total += GetAbs(c1, c2);
                }
            }
            return ((max - total) / (float)max) * 100;
        }

        public static Bitmap GetBitmap(Bitmap origin, int posX, int posY, int width, int height)
        {
            Bitmap newBitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(newBitmap);
            g.DrawImage(origin, -posX, -posY);
            g.Dispose();
            return newBitmap;
        }

        public static Bitmap RemoveColor(Bitmap origin, Color color, Color paint = default(Color), byte toleranz = 0)
        {
            Bitmap img = new Bitmap(origin);
            BitmapData bSrc = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, img.PixelFormat); //Lockt die Bits, damit sie gelesen und beschrieben werden können => kein struct deswegen notwendig
            int bytesPerPixel = Bitmap.GetPixelFormatSize(img.PixelFormat) / 8;
            int byteCount = bSrc.Stride * img.Height; //Insgesamten Bytes der Bitmap
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bSrc.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length); //Kopiert die bytes der Bitmap in einen Array
            int heightInPixels = bSrc.Height;//Höhe
            int widthInBytes = bSrc.Width * bytesPerPixel; //Pixel-Breite in bytes für das durchgehen der For-Schleife
            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * bSrc.Stride;
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    if(IsColor(pixels[currentLine + x + 2], pixels[currentLine + x + 1], pixels[currentLine + x + 0], color.R, color.G, color.B, toleranz))
                    {
                        pixels[currentLine + x + 3] = 0;
                        pixels[currentLine + x + 2] = 0;
                        pixels[currentLine + x + 1] = 0;
                        pixels[currentLine + x + 0] = 0;
                        continue;
                    }
                    pixels[currentLine + x + 3] = paint.A;
                    pixels[currentLine + x + 2] = paint.R;
                    pixels[currentLine + x + 1] = paint.G;
                    pixels[currentLine + x + 0] = paint.B;
                }
            }
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            img.UnlockBits(bSrc);
            return img;
        }

        public static Bitmap ScaleBitmap(Bitmap bitmap, float scale)
        {
            Bitmap newBitmap = new Bitmap((int)(bitmap.Width * scale), (int)(bitmap.Height * scale));
            Graphics g = Graphics.FromImage(newBitmap);
            g.ScaleTransform(scale, scale);
            g.DrawImage(bitmap, 0, 0);
            g.Dispose();
            return newBitmap;
        }
    }
}
