using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace IGNConsoleFon
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
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

            float[] masOrig = new float[45]; // !!! 45 - это для поиска jMax !!!
            float[] masWork = new float[43];
            float[] masFar = new float[43];
            float[] masNear = new float[43];
            float[] masFAN = new float[43];

            double U0_O = 0;
            double T2_O = 0;
            double S_O = 0;
            double U0_W = 0;
            double T2_W = 0;
            double S_W = 0;
            double U0_F = 0;
            double T2_F = 0;
            double S_F = 0;
            double U0_N = 0;
            double T2_N = 0;
            double S_N = 0;
            double U0_FAN = 0;
            double T2_FAN = 0;
            double S_FAN = 0;

            try
            {
                StreamReader sr30 = new StreamReader(@"D:\MyProgect\IGN\1017_спады 30.txt");
                StreamWriter sw = new StreamWriter(@"D:\MyProgect\IGN\1017_30_fon.txt");
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
                        sw.Write("{0,7:f2} ", fd);

                        int jMax = 0;
                        for (int j = 0; j < 43; j++)
                        {
                            if (masOrig[j] < 1)
                            {
                                if (masOrig[j + 1] < 1 || masOrig[j + 2] < 1) break;
                            }
                            jMax++;
                        }

                        int indL = 0;
                        int indR = 43;

                        bool flagL = false;
                        bool flagR = false;

                        while (true)
                        {
                            if (masOrig[indL] >= masOrig[indL + 1]) indL++;
                            else flagL = true;
                            if (masOrig[indR] >= masOrig[indR - 1]) indR--;
                            else flagR = true;

                            if (indL >= indR) break;
                            if (flagL && flagR) { indL++; indR--; flagL = false; flagR = false; }
                            if (indL >= indR) break;
                        }

                        sw.Write("{0,3} {1,3} {2,3} {3,4} {4,4}", jMax, indL, indR, masOrig[indL], masOrig[indR]);
                        sw.WriteLine();
                    }
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static int MinSquareMas(int n, float[] mas, ref double u, ref double t, ref double s)
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

    }
}
