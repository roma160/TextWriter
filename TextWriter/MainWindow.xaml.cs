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
        Bitmap bitmap;
        Bitmap buffBitmap;
        Graphics graphics;
        OpenFileDialog open = new OpenFileDialog();
        double scaleFactor = 300.0;

        const String NeededChars = "абвгґдеєжзиіїйклмнопрстуфхцчшщьюяАБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ.-!?:,«»";
        List<Character> Characters;
        public MainWindow()
        {
            InitializeComponent();
            Characters = new List<Character>();
        }
        int checkIfAllCharactersAdded()
        {
            if (Characters.Count < NeededChars.Length) 
                return Characters.Count;
            else return -1;
        }
        BitmapSource fromBitmap(Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
        }
        void onBitmapChanged()
        {
            Preview.Source = fromBitmap(bitmap);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            open.Title = "Open image:";
            open.Filter = "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif";
            if(open.ShowDialog() == true)
            {
                bitmap = (Bitmap)Image.FromFile(open.FileName);
                buffBitmap = bitmap;
                onBitmapChanged();
                graphics = Graphics.FromImage(bitmap);

                
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
                double k = Math.Pow(2, (double)e.Delta / scaleFactor);
                PreviewScale.ScaleX *= k;
                PreviewScale.ScaleY *= k;
                e.Handled = true;
            }
        }

        System.Windows.Point convertToPixels(System.Windows.Point p)
        {
            System.Windows.Point ret = p;
            ret.X *= bitmap.Width;
            ret.X /= Preview.ActualWidth;
            ret.Y *= bitmap.Height;
            ret.Y /= Preview.ActualHeight;
            return ret;
        }

        private void Preview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point pos = convertToPixels(e.GetPosition(Preview));
            int x = (int)pos.X;
            int y = (int)pos.Y;

            graphics.FillEllipse(System.Drawing.Brushes.White, x, y, 50, 50);
            onBitmapChanged();
        }

        private void add_character_button_Click(object sender, RoutedEventArgs e)
        {
            int res = checkIfAllCharactersAdded();
            if (res == -1) MessageBox.Show("You have added all characters");
            else if (buffBitmap != null)
            {
                Character newC = new Character(NeededChars[res]);
                newC.AddSample(new CharacterSample(null,
                    new System.Drawing.Point(0, 0), new System.Drawing.Point(0, 0)));
                Characters.Add(newC);
                updateLists();
            }
            else
                MessageBox.Show("Firstly open image.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
        }
        void updateLists()
        {
            if (Characters.Count > characters_listbox.Items.Count)
            {
                characters_listbox.Items.Clear();
                for (int i = 0; i < Characters.Count; i++)
                    characters_listbox.Items.Add(Characters[i].Symbol);
                characters_listbox.SelectedIndex = Characters.Count - 1;
            }
            else
                characters_listbox_SelectionChanged(null, null);
        }

        private void variants_listbox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void characters_listbox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(characters_listbox.Items.Count > 0)
            {
                Character selected = Characters[characters_listbox.SelectedIndex];
                variants_listbox.Items.Clear();
                for (int i = 0; i < selected.Samples.Count; i++)
                    variants_listbox.Items.Add(i);
            }
        }
    }
}
