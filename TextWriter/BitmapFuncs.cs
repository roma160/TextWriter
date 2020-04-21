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
    }
}
