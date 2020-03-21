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
            double[] masMax = new double[5];
            //masMax[0] = masMax[1] = masMax[2] = masMax[3] = masMax[4] = -1;
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
                            masOrig[j] = Math.Log(float.Parse(w[j + 1], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture));
                        }
                        for (int j = 0; j < 43; j++)
                        {
                            if (masMax[0] < masOrig[j]) masMax[0] = masOrig[j];
                            Array.Sort(masMax);
                        }
                        n++;
                    }
                }
                sr30.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            int W = 43;
            int H = n;

            Bitmap p = new Bitmap(W, H);
            Graphics graphPNG = Graphics.FromImage(p);

            graphPNG.Clear(Color.White);

            //graphPNG.DrawLine(Pens.Black, 0, 0, W, H);
            double g4 = masMax[3] / 4 * 3;
            double g3 = masMax[3] / 4 * 2;
            double g2 = masMax[3] / 4;
            double g1 = masMax[3] / 5;

            double amount;
            //n = 0;
            n--;
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
                            masOrig[j] = Math.Log(float.Parse(w[j + 1], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture));
                        }

                        for (int j = 0; j < 43; j++)
                        {
                            if (masOrig[j] > g1)
                            {
                                if (masOrig[j] > g2)
                                {
                                    if (masOrig[j] > g3)
                                    {
                                        if (masOrig[j] > g4)
                                        {
                                            amount = (masOrig[j] - g4) / (masMax[3] - g4);
                                            p.SetPixel(j, n, Blend(Color.Red, Color.Yellow, amount));
                                        }
                                        else
                                        {
                                            amount = (masOrig[j] - g3) / (g4 - g3);
                                            p.SetPixel(j, n, Blend(Color.Yellow, Color.Green, amount));
                                        }
                                    }
                                    else
                                    {
                                        amount = (masOrig[j] - g2) / (g3 - g2);
                                        p.SetPixel(j, n, Blend(Color.Green, Color.Blue, amount));
                                    }
                                }
                                else
                                {
                                    amount = (masOrig[j] - g1) / (g2 - g1);
                                    p.SetPixel(j, n, Blend(Color.Blue, Color.Black, amount));
                                }
                            }
                            else
                            {
                                //amount = masOrig[j] / g1;
                                //p.SetPixel(j, n, Blend(Color.Gray, Color.White, amount));
                                p.SetPixel(j, n, Color.White);
                            }
                        }
                        n--;
                    }
                }
                sr30.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            p.Save(@"D:\MyProgect\IGN\123456.png", ImageFormat.Png);

            p.Dispose();

            Process.Start(@"D:\MyProgect\IGN\123456.png");
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
