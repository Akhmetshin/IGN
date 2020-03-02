using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IGNConsole
{
    class Program
    {
        static double[] mas = new double[43];
        static double[] mas2 = new double[43];
        static double[] masU = new double[43];
        static double[] masT = new double[43];
        static double[] masS = new double[43];

        static void Main(string[] args)
        {
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
            int i = 0;
            float fd = 0;
            double u = 0;
            double t = 0;
            double s = 0;

            try
            {
                StreamReader sr30 = new StreamReader(@"D:\MyProgect\IGN\1017_спады 30.txt");
                StreamWriter sw = new StreamWriter(@"D:\MyProgect\IGN\1017_30_UTS.txt");
                line = sr30.ReadLine();
                while ((line = sr30.ReadLine()) != null)
                {
                    w = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                    if (w.Length > 0)
                    {
                        fd = float.Parse(w[0], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
                        for (int j = 0; j < 43; j++)
                        {
                            mas[j] = float.Parse(w[j + 1], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
                        }
                        sw.Write("{0,7:f2} ", fd);

                        MinSquareMas(43, mas, ref u, ref t, ref s);
                        sw.Write("({0,8:f2} {1,7:f2} {2,7:f3}) ", u, t, s);

                        for (int j = 0; j < 43; j++)
                        {
                            for (int m = 0; m < 42 - j; m++) mas2[m] = mas[m + 1 + j];
                            if (-1 == MinSquareMas(43 - j, mas2, ref u, ref t, ref s)) break;
                            sw.Write("({0,8:f2} {1,7:f2} {2,7:f3}) ", u, t, s);
                        }

                        sw.WriteLine();
                        i++;
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

            if(m>2)
            {
                s = sum1 / (m - 2);
            }
            else { s = 0; }

            return 0;
        }

    }
}
