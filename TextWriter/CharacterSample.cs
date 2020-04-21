using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextWriter
{
    class CharacterSample
    {
        public Bitmap Img;
        public Point Begin;
        public Point End;

        public const String ImgFormat = ".png";
        public const String PointFromat = ".txt";

        public CharacterSample(Bitmap image, Point begin, Point end)
        {
            Img = image;
            Begin = begin;
            End = end;
        }
        public CharacterSample(String folder, char character, int sampleInd)
        {
            if (folder[folder.Length - 1] != '\\') folder += "\\";
            Img = (Bitmap)Image.FromFile(folder + character + sampleInd.ToString() + ImgFormat);
            StreamReader reader = new StreamReader(folder + character + sampleInd.ToString() + PointFromat);
            Begin = PointFromString(reader.ReadLine());
            End = PointFromString(reader.ReadLine());
            reader.Close();
        }
        public void Save(String folder, char character, int sampleInd)
        {
            if (folder[folder.Length - 1] != '\\') folder += "\\";
            Img.Save(folder + character + sampleInd.ToString() + ImgFormat, 
                System.Drawing.Imaging.ImageFormat.Png);
            StreamWriter writer = new StreamWriter(folder + character
                + sampleInd.ToString() + PointFromat);
            writer.WriteLine(PointToString(Begin));
            writer.WriteLine(PointToString(End));
            writer.Close();
        }

        public static String PointToString(Point point)
        {
            return point.X.ToString() + ";" + point.Y.ToString();
        }
        public static Point PointFromString(String s)
        {
            String[] spl = s.Split(';');
            return new Point(int.Parse(spl[0]), int.Parse(spl[1]));
        }
    }

    class Character
    {
        public char Symbol;
        public List<CharacterSample> Samples;

        public Character(char symbol)
        {
            Symbol = symbol;
            Samples = new List<CharacterSample>();
        }
        public Character(String folder, char symbol)
        {
            Symbol = symbol;
            Samples = new List<CharacterSample>();
            if (folder[folder.Length - 1] != '\\') folder += "\\";
            for (int i = 0; File.Exists(folder + symbol + i.ToString() + CharacterSample.ImgFormat); i++)
                Samples.Add(new CharacterSample(folder, symbol, i));
        }
        public void Save(String folder)
        {
            if (folder[folder.Length - 1] != '\\') folder += "\\";
            for (int i = 0; i < Samples.Count; i++)
                Samples[i].Save(folder, Symbol, i);
        }

        public void AddSample(CharacterSample sample)
        {
            Samples.Add(sample);
        }
    }
}
