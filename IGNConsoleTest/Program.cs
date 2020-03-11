using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IGNConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            double[] masOrig = new double[45];
            double[] mas1 = new double[45];
            int offset = 5;

            for (int j = 0; j < 45; j++) masOrig[j] = j;
            for (int j = 0; j < 45 - offset; j++) mas1[j] = masOrig[j + offset];

            File30();
        }
        private static void File30()
        {
            var culture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            string line;
            string[] w;
            w = new string[43];
            char[] delimiterChars = { ' ', '\t', ';' };
            float fd = 0;
            double u = 0;
            double t = 0;
            double s = 0;
            int l = 0;
            int n = 0;

            double[] masOrig = new double[43];
            double[] mas1 = new double[43];
            double[] masWork = new double[43];
            double[] masU0 = new double[43];
            double[] masT2 = new double[43];
            double[] masS = new double[43];

            double U0_O = 0;
            double T2_O = 0;
            double S_O = 0;
            double U0_W = 0;
            double T2_W = 0;
            double S_W = 0;

            try
            {
                StreamReader sr30 = new StreamReader(@"D:\MyProgect\IGN\1017_спады 30.txt");
                StreamWriter sw = new StreamWriter(@"D:\MyProgect\IGN\1017_30_Test.txt");
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

                        int offset = 0;
                        int tailLen = 7;
                        double U0 = 0, T2 = 0, S = 0;
                        double U0_L = 0, T2_L = 0, S_L = 0;
                        for (n = 0; n < 43 - tailLen; n++)
                        {
                            for (int j = 0; j < tailLen; j++) mas1[j] = masOrig[j + offset];
                            if (-1 == MinSquareMas(tailLen, mas1, ref U0, ref T2, ref S)) break;
                            U0_O = U0;
                            T2_O = T2;
                            S_O = S;
                            if (-1 == MinSquareMasLin(tailLen, mas1, ref U0, ref T2, ref S)) break;
                            U0_L = U0;
                            T2_L = T2;
                            S_L = S;
                            offset++;
                        }
                        
                        int jMax = offset;

                        for (int j = 0; j < tailLen; j++) mas1[j] = masOrig[j + jMax - tailLen];
                        if (-1 == MinSquareMas(offset, mas1, ref U0, ref T2, ref S)) { Console.WriteLine("Error. -1 {0}", fd); sw.WriteLine("Error"); return; }

                        sw.Write("{0,-5}", l);
                        sw.Write("{0,7:f2} ", fd);
                        sw.Write("{0,4} ", jMax);
                        //sw.Write("{0,7:f2} {1,7:f2} {2,7:f2} {3,7:f2} | ", T2_O, T2_L, S_O, S_L);
                        sw.Write("({0,5:f2} {1,7:f4})", T2, S);
                        for (int j = 0; j < tailLen; j++) sw.Write("{0,7:f2} ", mas1[j]);
                        sw.WriteLine();
                        l++;
                    }
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static int MinSquareMas(int n, double[] mas, ref double u, ref double t, ref double s)
        {
            double x, y, m;
            double sum1, sum2, sum3, sum4;

            sum1 = sum2 = sum3 = sum4 = 0;
            m = 0;

            for (int i = 0; i < n; i++)
            {
                if (mas[i] < 0.001) continue;

                x = i * 2;
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
            for (int i = 0; i < n; i++)
            {
                if (mas[i] < 0.001) continue;

                x = i * 2;
                y = Math.Log(mas[i]);
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

        private static int MinSquareMasLin(int n, double[] mas, ref double u, ref double t, ref double s)
        {
            double x, y, m;
            double sum1, sum2, sum3, sum4;

            sum1 = sum2 = sum3 = sum4 = 0;
            m = 0;

            for (int i = 0; i < n; i++)
            {
                if (mas[i] < 0.001) continue;

                x = i * 2;
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

                x = i * 2;
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
    }
}
