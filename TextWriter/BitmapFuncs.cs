using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TextWriter
{
    class BitmapFuncs
    {
        public static Bitmap cropBitmap(Bitmap bitmap, Rectangle cropArea)
        {
            Bitmap ret = new Bitmap(bitmap);
            return ret.Clone(cropArea, 
                System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
        }
        public static Bitmap cropBitmap(Bitmap bitmap, Point begin, Point end)
        {
            Point start = new Point(Math.Min(begin.X, end.X),
                Math.Min(begin.Y, end.Y));
            Size size = new Size(Math.Abs(begin.X - end.X),
                Math.Abs(begin.Y - end.Y));
            return cropBitmap(bitmap, new Rectangle(start, size));
        }
        public static BitmapSource fromBitmap(Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
        }
        public static Bitmap replaceColor(Bitmap source, Color from, Color to, double strength)
        {
            for(int x = 0; x < source.Width; x++)
            {
                for(int y = 0; y < source.Height; y++)
                {
                    Color pixel = source.GetPixel(x, y);
                    if (ColourDistance(from, pixel) <= strength)
                        source.SetPixel(x, y, to);
                }
            }
            return source;
        }
        public static float ColourDistance(Color a, Color b)
        {
            double R = Math.Pow(a.R - b.R, 2);
            double G = Math.Pow(a.G - b.G, 2);
            double B = Math.Pow(a.B - b.B, 2);
            return (float)Math.Sqrt(R + G + B) * 100f / (255f * (float)Math.Sqrt(3));
        }
    }
}
