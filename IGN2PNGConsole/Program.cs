using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IGN2PNGConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            double[] masOrig = new double[45]; // !!! 45 - это для поиска jMax !!!
            double[] mas1 = new double[43];

            var culture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            string line;
            string[] w;
            w = new string[43];
            char[] delimiterChars = { ' ', '\t', ';' };
            float fd = 0;

            int W = 43;
            int H = 3199;

            Bitmap p = new Bitmap(W, H);
            Graphics graphPNG = Graphics.FromImage(p);

            graphPNG.Clear(Color.White);

            int n = 0;
            try
            {
                StreamReader sr30 = new StreamReader(@"D:\MyProgect\IGN\1017_спады 30.txt");
                line = sr30.ReadLine();
                while ((line = sr30.ReadLine()) != null)
                {
                    w = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                    if (w.Length > 0)
                    {
                        fd = float.Parse(w[0], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
                        for (int j = 0; j < 43; j++)
                        {
                            masOrig[j] = float.Parse(w[j + 1], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
                        }
                        for (int j = 0; j < 43; j++)
                        {
                            int red = Math.Min((int)(masOrig[j]), 255);
                            double amount = masOrig[j] / 10000;
                            p.SetPixel(j, n, Blend(Color.Red, Color.Green, amount));
                        }
                        n++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //graphPNG.DrawLine(Pens.Black, 0, 0, W, H);

            p.Save(@"D:\MyProgect\IGN\12345.png", ImageFormat.Png);

            p.Dispose();

            Process.Start(@"D:\MyProgect\IGN\12345.png");
        }

        /// <summary>Blends the specified colors together.</summary>
        /// <param name="color">Color to blend onto the background color.</param>
        /// <param name="backColor">Color to blend the other color onto.</param>
        /// <param name="amount">How much of <paramref name="color"/> to keep,
        /// “on top of” <paramref name="backColor"/>.</param>
        /// <returns>The blended colors.</returns>
        private static Color Blend(Color color, Color backColor, double amount)
        {
            byte r = (byte)((color.R * amount) + backColor.R * (1 - amount));
            byte g = (byte)((color.G * amount) + backColor.G * (1 - amount));
            byte b = (byte)((color.B * amount) + backColor.B * (1 - amount));
            return Color.FromArgb(r, g, b);
        }
    }
}
