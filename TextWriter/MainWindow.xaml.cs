using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;

namespace TextWriter
{
    public partial class MainWindow : Window
    {
        OpenFileDialog open = new OpenFileDialog();
        double scaleFactor = 300.0;
        enum NowAction
        {
            WaitingForAction = 0,
            SettingStartPoint = 1,
            SettingFinishPoint = 2,
            SettingBeginPoint = 3,
            SettingEndPoint = 4,
            ClearingBack = 5
        }
        public static float PointPreviewScale = 10f;

        const String NeededChars = "абвгґдеєжзиіїйклмнопрстуфхцчшщьюяАБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ.-!?:,«»";
        const String Words = "абвгґдеєжзиіїйклмнопрстуфхцчшщьюяАБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ";
        const String cachedPreviewName = "Cache/preview_cache.png";
        const String cacheFolderName = "Cache";
        List<Character> Characters;
        int selectedSampleIndex = -1;
        int selectedCharIndex = -1;
        bool isImageOpened = false;
        NowAction action = NowAction.WaitingForAction;
        System.Drawing.Size previewSize = new System.Drawing.Size(0, 0);

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
        void onBitmapChanged(Bitmap newBitmap)
        {
            Preview.Source = BitmapFuncs.fromBitmap(newBitmap);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            open.Title = "Open image:";
            open.Filter = "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif";
            if(open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Bitmap bitmap = (Bitmap)Image.FromFile(open.FileName);
                Directory.CreateDirectory(cacheFolderName);
                bitmap.Save(cachedPreviewName, System.Drawing.Imaging.ImageFormat.Png);
                previewSize = bitmap.Size;
                onBitmapChanged(bitmap);
                OpenImage.IsEnabled = false;
                isImageOpened = true;
                bitmap.Dispose();
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
                if(selectedSampleIndex != -1 && selectedCharIndex != -1)
                    showControls(Characters[selectedCharIndex].Samples[selectedSampleIndex]);
                e.Handled = true;
            }
        }

        System.Windows.Point convertToPixels(System.Windows.Point p)
        {
            System.Windows.Point ret = p;
            ret.X *= previewSize.Width;
            ret.X /= Preview.ActualWidth;
            ret.Y *= previewSize.Height;
            ret.Y /= Preview.ActualHeight;
            return ret;
        }
        System.Drawing.Point convertToPixelsD(System.Windows.Point p)
        {
            System.Windows.Point buff = convertToPixels(p);
            return new System.Drawing.Point((int)buff.X, (int)buff.Y);
        }
        System.Windows.Point convertFromPixels(System.Windows.Point p)
        {
            System.Windows.Point ret = p;
            ret.X *= Preview.ActualWidth;
            ret.X /= previewSize.Width;
            ret.Y *= Preview.ActualWidth;
            ret.Y /= previewSize.Width;
            return ret;
        }
        System.Windows.Point wpoint(float x, float y)
        {
            return new System.Windows.Point(x, y);
        }
        Line createLine(float x1, float y1, float x2, float y2)
        {
            Line ret = new Line();
            ret.X1 = x1;
            ret.X2 = x2;
            ret.Y1 = y1;
            ret.Y2 = y2;
            return ret;
        }

        private void Preview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point pos = convertToPixels(e.GetPosition(Preview));
            int x = (int)pos.X;
            int y = (int)pos.Y;
            CharacterSample sample = Characters[selectedCharIndex].Samples[selectedSampleIndex];
            System.Drawing.Point point = new System.Drawing.Point(x, y);
            System.Drawing.Point startPoint = new System.Drawing.Point(sample.bitmapLoc.X,
                sample.bitmapLoc.Y + sample.Img.Height);
            System.Drawing.Point finishPoint = new System.Drawing.Point(sample.bitmapLoc.X +
                sample.Img.Width, sample.bitmapLoc.Y);
            System.Drawing.Point beginPoint = new System.Drawing.Point(
                sample.Begin.X + sample.bitmapLoc.X, sample.Begin.Y + sample.bitmapLoc.Y);
            System.Drawing.Point endPoint = new System.Drawing.Point(
                sample.End.X + sample.bitmapLoc.X, sample.End.Y + sample.bitmapLoc.Y);

            if(action == NowAction.WaitingForAction)
            {
                if (checkIfInCircle(startPoint, point))
                    action = NowAction.SettingStartPoint;
                else if (checkIfInCircle(finishPoint, point))
                    action = NowAction.SettingFinishPoint;
                else if (checkIfInCircle(beginPoint, point))
                    action = NowAction.SettingBeginPoint;
                else if (checkIfInCircle(endPoint, point))
                    action = NowAction.SettingEndPoint;
            }
            else if(action == NowAction.SettingStartPoint)
            {
                Bitmap buffBitmap = (Bitmap)Image.FromFile(cachedPreviewName);
                int X = Math.Min(x, finishPoint.X);
                int Y = Math.Min(y, finishPoint.Y);
                int W = Math.Abs(finishPoint.X - x);
                int H = Math.Abs(y - finishPoint.Y);
                Characters[selectedCharIndex].Samples[selectedSampleIndex].Img =
                    BitmapFuncs.cropBitmap(buffBitmap, new System.Drawing.Rectangle(X, Y, W, H));
                Characters[selectedCharIndex].Samples[selectedSampleIndex].bitmapLoc =
                    new System.Drawing.Point(X, Y);
                showControls(Characters[selectedCharIndex].Samples[selectedSampleIndex]);
                action = NowAction.WaitingForAction;
            }
            else if (action == NowAction.SettingFinishPoint)
            {
                Bitmap buffBitmap = (Bitmap)Image.FromFile(cachedPreviewName);
                int X = Math.Min(x, startPoint.X);
                int Y = Math.Min(y, startPoint.Y);
                int W = Math.Abs(startPoint.X - x);
                int H = Math.Abs(y - startPoint.Y);
                Characters[selectedCharIndex].Samples[selectedSampleIndex].Img =
                    BitmapFuncs.cropBitmap(buffBitmap, new System.Drawing.Rectangle(X, Y, W, H));
                Characters[selectedCharIndex].Samples[selectedSampleIndex].bitmapLoc =
                    new System.Drawing.Point(X, Y);
                showControls(Characters[selectedCharIndex].Samples[selectedSampleIndex]);
                action = NowAction.WaitingForAction;
            }
            else if(action == NowAction.SettingEndPoint)
            {
                endPoint = new System.Drawing.Point(
                    x - sample.bitmapLoc.X, y - sample.bitmapLoc.Y);
                Characters[selectedCharIndex].Samples[selectedSampleIndex].End =
                    endPoint;
                showControls(Characters[selectedCharIndex].Samples[selectedSampleIndex]);
                action = NowAction.WaitingForAction;
            }
            else if (action == NowAction.SettingBeginPoint)
            {
                beginPoint = new System.Drawing.Point(
                    x - sample.bitmapLoc.X, y - sample.bitmapLoc.Y);
                Characters[selectedCharIndex].Samples[selectedSampleIndex].Begin =
                    beginPoint;
                showControls(Characters[selectedCharIndex].Samples[selectedSampleIndex]);
                action = NowAction.WaitingForAction;
            }
            else if(action == NowAction.ClearingBack)
            {
                Bitmap bitmap = (Bitmap)Image.FromFile(cachedPreviewName);
                System.Drawing.Color pixel = bitmap.GetPixel(x, y);
                Characters[selectedCharIndex].Samples[selectedSampleIndex].Img =
                    BitmapFuncs.replaceColor(Characters[selectedCharIndex].Samples[selectedSampleIndex].Img,
                    pixel, System.Drawing.Color.Transparent, FillStrenght.Value);
                showControls(Characters[selectedCharIndex].Samples[selectedSampleIndex]);
                action = NowAction.WaitingForAction;
            }
        }

        private void add_character_button_Click(object sender, RoutedEventArgs e)
        {
            int res = checkIfAllCharactersAdded();
            if (res == -1) System.Windows.MessageBox.Show("You have added all characters");
            else if (isImageOpened)
            {
                Bitmap buffBitmap = (Bitmap)Image.FromFile(cachedPreviewName);
                Character newC = new Character(NeededChars[res]);

                System.Drawing.Point screenStart = convertToPixelsD(new System.Windows.Point(
                    Scroller.HorizontalOffset, Scroller.VerticalOffset));
                System.Drawing.Point startPoint = screenStart;
                System.Drawing.Point finishPoint = new System.Drawing.Point(screenStart.X + 100, screenStart.Y + 100);
                System.Drawing.Point beginPoint = new System.Drawing.Point(25, 25);
                System.Drawing.Point endPoint = new System.Drawing.Point(50, 50);
                newC.AddSample(new CharacterSample(BitmapFuncs.cropBitmap(buffBitmap, startPoint, finishPoint),
                    beginPoint, endPoint));
                newC.Samples[0].bitmapLoc = startPoint;
                Characters.Add(newC);
                updateLists();
                buffBitmap.Dispose();
            }
            else
                System.Windows.MessageBox.Show("Firstly open image.", "Error", 
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
            if(variants_listbox.Items.Count > 0)
            {
                Character selectedChar = Characters[characters_listbox.SelectedIndex];
                CharacterSample sample = selectedChar.Samples[variants_listbox.SelectedIndex];
                selectedCharIndex = characters_listbox.SelectedIndex;
                selectedSampleIndex = variants_listbox.SelectedIndex;
                showControls(sample);
            }
        }
        void showControls(CharacterSample sample)
        {
            System.Drawing.Point startPoint = new System.Drawing.Point(sample.bitmapLoc.X,
                sample.bitmapLoc.Y + sample.Img.Height);
            System.Drawing.Point finishPoint = new System.Drawing.Point(sample.bitmapLoc.X +
                sample.Img.Width, sample.bitmapLoc.Y);
            System.Drawing.Point beginPoint = new System.Drawing.Point(
                sample.Begin.X + sample.bitmapLoc.X, sample.Begin.Y + sample.bitmapLoc.Y);
            System.Drawing.Point endPoint = new System.Drawing.Point(
                sample.End.X + sample.bitmapLoc.X, sample.End.Y + sample.bitmapLoc.Y);

            ControlsCanvas.Children.Clear();
            /*bitmap = (Bitmap)buffBitmap.Clone();
            graphics = Graphics.FromImage(bitmap);*/
            drawPointControl(startPoint, Colors.Black);
            drawPointControl(finishPoint, Colors.Black);
            drawPointControl(beginPoint, Colors.Green);
            drawPointControl(endPoint, Colors.Red);
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            Line firstLine = createLine(startPoint.X, finishPoint.Y, finishPoint.X, finishPoint.Y);
            Line secondLine = createLine(finishPoint.X, finishPoint.Y, finishPoint.X, startPoint.Y);
            Line thirdLine = createLine(finishPoint.X, startPoint.Y, startPoint.X, startPoint.Y);
            Line fourthLine = createLine(startPoint.X, startPoint.Y, startPoint.X, finishPoint.Y);
            firstLine.Stroke = secondLine.Stroke = thirdLine.Stroke = fourthLine.Stroke = brush;
            ControlsCanvas.Children.Add(firstLine);
            ControlsCanvas.Children.Add(secondLine);
            ControlsCanvas.Children.Add(thirdLine);
            ControlsCanvas.Children.Add(fourthLine);
            /*System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black, 
                1f / (float)PreviewScale.ScaleX);
            graphics.DrawLine(pen, startPoint.X, finishPoint.Y, finishPoint.X, finishPoint.Y);
            graphics.DrawLine(pen, finishPoint.X, finishPoint.Y, finishPoint.X, startPoint.Y);
            graphics.DrawLine(pen, finishPoint.X, startPoint.Y, startPoint.X, startPoint.Y);
            graphics.DrawLine(pen, startPoint.X, startPoint.Y, startPoint.X, finishPoint.Y);
            onBitmapChanged();*/

            SymbolPreview.Source = BitmapFuncs.fromBitmap(sample.Img); 
        }
        void drawPointControl(System.Drawing.Point point, System.Windows.Media.Color color)
        {
            float a = PointPreviewScale / (float)PreviewScale.ScaleX;
            float x = point.X;
            float y = point.Y;
            /*System.Drawing.Pen pen = new System.Drawing.Pen(color, a/10f);
            System.Drawing.Brush brush = new SolidBrush(color);
            graphics.DrawLine(pen, x - a, y, x + a, y);
            graphics.DrawLine(pen, x, y - a, x, y + a);
            graphics.FillEllipse(brush, x, y, 1.5f * a, 1.5f * a);*/
            SolidColorBrush brush = new SolidColorBrush(color);
            Line firstLine = createLine(x - a, y, x + a, y);
            firstLine.StrokeThickness = a / 10f;
            firstLine.Stroke = brush;
            Line secondLine = createLine(x, y - a, x, y + a);
            secondLine.StrokeThickness = a / 10f;
            secondLine.Stroke = brush;
            Ellipse ellipse = new Ellipse();
            ellipse.Width = 1.5f * a;
            ellipse.Height = 1.5f * a;
            ellipse.Margin = new Thickness(x, y, 0, 0);
            ellipse.Fill = brush;
            ellipse.MouseDown += Preview_MouseDown;
            ControlsCanvas.Children.Add(firstLine);
            ControlsCanvas.Children.Add(secondLine);
            ControlsCanvas.Children.Add(ellipse);
        }
        bool checkIfInCircle(System.Drawing.Point circle, System.Drawing.Point point)
        {
            float a = PointPreviewScale / (float)PreviewScale.ScaleX;
            float x = (float)point.X - (float)circle.X - 1.5f * a/2;
            float y = (float)point.Y - (float)circle.Y - 1.5f * a/2;
            return Math.Pow(x, 2) + Math.Pow(y, 2) <= Math.Pow(1.5f * a/2, 2);
        }

        private void characters_listbox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(characters_listbox.Items.Count > 0)
            {
                Character selected = Characters[characters_listbox.SelectedIndex];
                variants_listbox.Items.Clear();
                for (int i = 0; i < selected.Samples.Count; i++)
                    variants_listbox.Items.Add(i);
                CharacterPreview.Content = selected.Symbol;
            }
        }

        private void SaveCharactersButton_Click(object sender, RoutedEventArgs e)
        {
            using(FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select empty folder for saving";
                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (Directory.GetFiles(dialog.SelectedPath).Length > 0)
                        System.Windows.MessageBox.Show("Select empty folder!!!");
                    else
                    {
                        Image.FromFile(cachedPreviewName).Save(dialog.SelectedPath + "\\preview.png",
                            System.Drawing.Imaging.ImageFormat.Png);
                        StreamWriter writer = new StreamWriter(dialog.SelectedPath + "\\chars.txt");
                        foreach (Character character in Characters)
                        {
                            character.Save(dialog.SelectedPath);
                            writer.Write(character.Symbol);
                        }
                        writer.Close();
                        System.Windows.MessageBox.Show("Saved succesfuly.");
                    }
                }
            }
        }

        private void RemoveBackButton_Click(object sender, RoutedEventArgs e)
        {
            action = NowAction.ClearingBack;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowCharacter_Click(object sender, RoutedEventArgs e)
        {
            CharacterSample sample = Characters[selectedCharIndex].Samples[selectedSampleIndex];
            Scroller.ScrollToHorizontalOffset(sample.bitmapLoc.X);
            Scroller.ScrollToVerticalOffset(sample.bitmapLoc.Y);
        }

        private void add_variant_button_Click(object sender, RoutedEventArgs e)
        {
            Bitmap buffBitmap = (Bitmap)Image.FromFile(cachedPreviewName);
            System.Drawing.Point screenStart = convertToPixelsD(new System.Windows.Point(
                    Scroller.HorizontalOffset, Scroller.VerticalOffset));
            System.Drawing.Point startPoint = screenStart;
            System.Drawing.Point finishPoint = new System.Drawing.Point(screenStart.X + 100, screenStart.Y + 100);
            System.Drawing.Point beginPoint = new System.Drawing.Point(25, 25);
            System.Drawing.Point endPoint = new System.Drawing.Point(50, 50);
            Characters[selectedCharIndex].AddSample(new CharacterSample(BitmapFuncs.cropBitmap(buffBitmap, startPoint, finishPoint),
                    beginPoint, endPoint));
            Characters[selectedCharIndex].Samples.Last().bitmapLoc = startPoint;
            updateLists();
            buffBitmap.Dispose();
        }
    }
}
