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
        static double ed = 100;
        static void Main(string[] args)
        {
            double[] masOrig = new double[45]; // !!! 45 - это для поиска jMax !!!
            double[] mas1 = new double[43];
            double[] mas1Delta = new double[43];

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
                            masOrig[j] = float.Parse(w[j + 1], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
                        }

                        int jMax = GetJMax(masOrig, mas1);

                        double U0 = 0, T2 = 0, S = 0;
                        if (-1 == MinSquareMas(jMax, masOrig, ref U0, ref T2, ref S))
                        {
                            Console.WriteLine("if (-1 == MinSquareMas(jMax, masOrig, ref U0, ref T2, ref S))");
                            return;
                        }
                        for (int j = 0; j < jMax; j++)
                        {
                            mas1Delta[j] = Math.Log(Math.Sqrt((masOrig[j] - (U0 * Math.Exp(-(j * ed) / T2))) * (masOrig[j] - (U0 * Math.Exp(-(j * ed) / T2)))));
                        }
                        for (int j = 0; j < jMax; j++)
                        {
                            if (masMax[0] < mas1Delta[j]) masMax[0] = mas1Delta[j];
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

            double g4 = masMax[3] / 5 * 4;
            double g3 = masMax[3] / 5 * 3;
            double g2 = masMax[3] / 5 * 2;
            double g1 = masMax[3] / 5;

            double amount = 0;
            //n = 0;
            n--;
            try
            {
                StreamReader sr30 = new StreamReader(@"D:\MyProgect\IGN\1017_спады 30.txt");
                StreamWriter sw = new StreamWriter(@"D:\MyProgect\amount.txt");

                line = sr30.ReadLine();
                while ((line = sr30.ReadLine()) != null)
                {
                    w = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                    if (w.Length > 0)
                    {
                        fd = float.Parse(w[0], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
                        sw.Write("{0,7:f2} ", fd);

                        for (int j = 0; j < 43; j++)
                        {
                            masOrig[j] = float.Parse(w[j + 1], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
                        }

                        int jMax = GetJMax(masOrig, mas1);

                        double U0 = 0, T2 = 0, S = 0;
                        if (-1 == MinSquareMas(jMax, masOrig, ref U0, ref T2, ref S))
                        {
                            Console.WriteLine("if (-1 == MinSquareMas(jMax, masOrig, ref U0, ref T2, ref S))");
                            return;
                        }
                        for (int j = 0; j < jMax; j++)
                        {
                            mas1Delta[j] = Math.Log(Math.Sqrt((masOrig[j] - (U0 * Math.Exp(-(j * ed) / T2))) * (masOrig[j] - (U0 * Math.Exp(-(j * ed) / T2)))));
                        }

                        for (int j = 0; j < jMax; j++)
                        {
                            if (mas1Delta[j] > g1)
                            {
                                if (mas1Delta[j] > g2)
                                {
                                    if (mas1Delta[j] > g3)
                                    {
                                        if (mas1Delta[j] > g4)
                                        {
                                            amount = (mas1Delta[j] - g4) / (masMax[3] - g4);
                                            p.SetPixel(j, n, Blend(Color.Red, Color.Yellow, amount));
                                        }
                                        else
                                        {
                                            amount = (mas1Delta[j] - g3) / (g4 - g3);
                                            p.SetPixel(j, n, Blend(Color.Yellow, Color.Green, amount));
                                        }
                                    }
                                    else
                                    {
                                        amount = (mas1Delta[j] - g2) / (g3 - g2);
                                        p.SetPixel(j, n, Blend(Color.Green, Color.LightBlue, amount));
                                    }
                                }
                                else
                                {
                                    amount = (mas1Delta[j] - g1) / (g2 - g1);
                                    p.SetPixel(j, n, Blend(Color.LightBlue, Color.Blue, amount));
                                }
                            }
                            else
                            {
                                amount = -1;
                                p.SetPixel(j, n, Blend(Color.Blue, Color.Gray, mas1Delta[j]));
                            }
                            sw.Write("{0,7:f2} {1,7:f2}  ", mas1Delta[j], amount);
                        }
                        n--;
                        sw.WriteLine();
                    }
                }
                sw.Close();
                sr30.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            p.Save(@"D:\MyProgect\123456.png", ImageFormat.Png);

            p.Dispose();

            //Process.Start(@"D:\MyProgect\123456.png");

            Bitmap pS = new Bitmap(250, 10);
            Graphics graphPNG_S = Graphics.FromImage(pS);

            graphPNG_S.Clear(Color.White);
            for(int i=0;i<10;i++)
            {
                for (int j = 0; j < 50; j++)    pS.SetPixel(j, i, Blend(Color.Red, Color.Yellow, 1 - 0.02 * j));
                for (int j = 50; j < 100; j++)  pS.SetPixel(j, i, Blend(Color.Yellow, Color.Green, 1 - 0.02 * (j - 50)));
                for (int j = 100; j < 150; j++) pS.SetPixel(j, i, Blend(Color.Green, Color.LightBlue, 1 - 0.02 * (j - 100)));
                for (int j = 150; j < 200; j++) pS.SetPixel(j, i, Blend(Color.LightBlue, Color.Blue, 1 - 0.02 * (j - 150)));
                for (int j = 200; j < 250; j++) pS.SetPixel(j, i, Blend(Color.Blue, Color.Gray, 1 - 0.02 * (j - 200)));
            }

            pS.Save(@"D:\MyProgect\123456_Scale.png", ImageFormat.Png);

            pS.Dispose();
        }

        private static int GetJMax(double[] masOrig, double[] mas1)
        {
            int offset = 0;
            int tailLen = 7;
            double U0 = 0, T2 = 0, S = 0;
            double U0_O = 0, T2_O = 0, S_O = 0;
            double U0_L = 0, T2_L = 0, S_L = 0;
            for (int i = 0; i < 43 - tailLen; i++)
            {
                for (int j = 0; j < tailLen; j++) mas1[j] = masOrig[j + offset];
                if (-1 == MinSquareMas(tailLen, mas1, ref U0, ref T2, ref S)) break;
                S_O = S;
                if (-1 == MinSquareMasLin(tailLen, mas1, ref U0, ref T2, ref S)) break;
                S_L = S;
                if (S_O > S_L) break;
                offset++;
            }
            return offset - 1;
        }

        private static int MinSquareMas(int n, double[] mas, ref double u, ref double t, ref double s)
        {
            double x, y, m;
            double sum1, sum2, sum3, sum4;
            //double ed = 2;

            sum1 = sum2 = sum3 = sum4 = 0;
            m = 0;

            for (int i = 0; i < n; i++)
            {
                if (mas[i] < 0.001) continue;

                x = i * ed;
                y = Math.Log(mas[i]);
                m++;

                sum1 += x * y;
                sum2 += x;
                sum3 += y;
                sum4 += x * x;
            }

            if (m < 3)
            {
                u = -1;
                t = -1;
                s = -1;
                return -1;
            }

            double b = (sum2 * sum3 - m * sum1) / (sum2 * sum2 - m * sum4);
            double a = (sum3 - b * sum2) / m;

            u = Math.Exp(a);
            t = -1 / b;

            if (double.IsInfinity(u)) return -1;
            if (double.IsInfinity(t)) return -1;

            if (0 > u) return -1;
            if (0 > t) return -1;

            sum1 = 0;
            m = 0;
            for (int i = 0; i < n; i++)
            {
                if (mas[i] < 0.001) continue;

                x = i * ed;
                y = Math.Log(mas[i]);
                //y = mas[i];
                m++;
                sum1 += (y - a - b * x) * (y - a - b * x);
                //sum1 += (y - (u * Math.Exp(-x / t))) * (y - (u * Math.Exp(-x / t)));

            }

            if (m > 2)
            {
                s = sum1 / (m - 2);
            }
            else { s = 0; }

            return 0;
        }
        private static int MinSquareMasLin(int n, double[] mas, ref double u, ref double t, ref double s)
        {
            double x, y, m;
            double sum1, sum2, sum3, sum4;
            //double ed = 2;
            sum1 = sum2 = sum3 = sum4 = 0;
            m = 0;

            for (int i = 0; i < n; i++)
            {
                if (mas[i] < 0.001) continue;

                x = i * ed;
                //y = Math.Log(mas[i]);
                y = mas[i];
                m++;

                sum1 += x * y;
                sum2 += x;
                sum3 += y;
                sum4 += x * x;
            }

            if (m < 3)
            {
                u = -1;
                t = -1;
                s = -1;
                return -1;
            }

            double b = (sum2 * sum3 - m * sum1) / (sum2 * sum2 - m * sum4);
            double a = (sum3 - b * sum2) / m;

            //u = Math.Exp(a);
            u = a;
            t = -1 / b;

            if (double.IsInfinity(u)) return -1;
            if (double.IsInfinity(t)) return -1;

            if (0 > u) return -1;
            if (0 > t) return -1;

            sum1 = 0;
            for (int i = 0; i < n; i++)
            {
                if (mas[i] < 0.001) continue;

                x = i * ed;
                //y = Math.Log(mas[i]);
                y = mas[i];
                m++;
                sum1 += (y - a - b * x) * (y - a - b * x);
            }

            if (m > 2)
            {
                s = sum1 / (m - 2);
            }
            else { s = 0; }

            return 0;
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
