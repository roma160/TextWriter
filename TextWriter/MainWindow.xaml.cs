using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace TextWriter
{
    public partial class MainWindow : Window
    {
        BitmapImage bitmap;
        OpenFileDialog open = new OpenFileDialog();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            open.Title = "Open image:";
            open.Filter = "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif";
            if(open.ShowDialog() == true)
            {
                bitmap = new BitmapImage(new Uri(open.FileName));
                Preview.Source = bitmap;
            }
        }

        private void Scroller_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                Scroller.ScrollToHorizontalOffset(Scroller.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                PreviewScale.ScaleX += e.Delta/100;
                PreviewScale.ScaleY += e.Delta/100;
                e.Handled = true;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Scroller.HorizontalOffset.ToString());
        }

        System.Windows.Point convertToPixels(System.Windows.Point p)
        {
            System.Windows.Point ret = p;
            ret.X *= bitmap.PixelWidth;
            ret.X /= Preview.ActualWidth;
            ret.Y *= bitmap.PixelHeight;
            ret.Y /= Preview.ActualHeight;
            return ret;
        }

        private void Preview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point pos = convertToPixels(e.GetPosition(Preview));
            double x = pos.X;
            double y = pos.Y;

            int n_x = bitmap.PixelWidth * 4;
            int siz = n_x * bitmap.PixelHeight;
            byte[] pixels = new byte[siz];
            bitmap.CopyPixels(pixels, n_x, 0);
            int i = (int)y * n_x + (int)x * 4;
            MessageBox.Show(pixels[i] + " " + pixels[i+1] + " " + pixels[i+2]);
        }
    }
}
