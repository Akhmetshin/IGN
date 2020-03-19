using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IGNViewNew3
{
    public partial class Form1 : Form
    {
        Graphics graf;
        readonly Pen pen1;
        readonly Pen pen2;
        readonly SolidBrush SolidBrushB;
        Rectangle r;

        float[] depth = new float[3199];
        float[,] data = new float[3199, 43];

        int h0;
        double ed;
        public Form1()
        {
            InitializeComponent();

            h0 = Height / 3 * 2;
            ed = 100;

            SolidBrushB = new SolidBrush(BackColor);
            r = new Rectangle(0, 0, Width - 100, h0 + 1);

            graf = this.CreateGraphics();
            pen1 = new Pen(Color.FromArgb(255, 0, 0, 0), 2);
            pen2 = new Pen(Color.FromArgb(255, 0, 255, 255), 2);

            File();
        }

        private void File()
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
            try
            {
                StreamReader sr = new StreamReader(@"D:\MyProgect\IGN\1017_спады 30.txt");
                line = sr.ReadLine();
                while ((line = sr.ReadLine()) != null)
                {
                    w = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                    if (w.Length > 0)
                    {
                        fd = float.Parse(w[0], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
                        depth[i] = fd;
                        for (int j = 0; j < 43; j++)
                        {
                            fd = float.Parse(w[j + 1], NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
                            data[i, j] = fd;
                        }
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            trackBar1.Maximum = i - 1;
            trackBar1.TickFrequency = 50;

            trackBar2.Minimum = 3;
            trackBar2.Maximum = 40;
            trackBar2.TickFrequency = 5;
        }
        private void MainDraw()
        {
            double[] masOrig = new double[45]; // !!! 45 - это для поиска jMax !!!
            double[] mas1 = new double[43];
            double[] mas2 = new double[43];

            label1.Text = depth[trackBar1.Value].ToString();
            
            int v = trackBar2.Value;
            label2.Text = v.ToString();

            graf.FillRectangle(SolidBrushB, r);

            for (int j = 0; j < 45; j++) masOrig[j] = 0;
            for (int j = 0; j < 43; j++) mas1[j] = 0;
            for (int j = 0; j < 43; j++) masOrig[j] = data[trackBar1.Value, j];

            double U0_O = 0;
            double T2_O = 0;
            double S_O = 0;

            int offset = 0;
            int tailLen = 7;
            double U0 = 0, T2 = 0, S = 0;
            double U0_L = 0, T2_L = 0, S_L = 0;
            for (int n = 0; n < 43 - tailLen; n++)
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
                if (S_O > S_L) break;
                offset++;
            }

            int jMax = offset - 1;
            trackBar2.Maximum = jMax - 3;
            label3.Text = jMax.ToString();

            for (int j = 0; j <= v; j++) mas1[j] = masOrig[j];
            int l = 0;
            for (int j = v; j < jMax; j++) { mas2[l] = masOrig[j]; l++; }

            PointF[] pf1 = new PointF[v + 1];
            PointF[] pf2 = new PointF[jMax - v];

            for (int j = 0; j <= v; j++)
            {
                pf1[j].X = j * 20;
                pf1[j].Y = (float)(h0 - mas1[j] / 50);
            }
            graf.DrawLines(pen1, pf1);

            for (int j = 0; j < jMax - v; j++)
            {
                pf2[j].X = (j + v) * 20;
                pf2[j].Y = (float)(h0 - mas2[j] / 50);
            }
            if (jMax - v > 1) graf.DrawLines(pen2, pf2);

            double U0_1 = 0, T2_1 = 0, S_1 = 0;
            double U0_2 = 0, T2_2 = 0, S_2 = 0;
            if (-1 == MinSquareMas2UT(v, jMax - v - 1, masOrig, ref U0_1, ref T2_1, ref S_1, ref U0_2, ref T2_2, ref S_2)) return;

            double[] mas1Teor = new double[43];
            double[] mas2Teor = new double[43];
            //double ed = 2;
            for (int j = 0; j <= v; j++) mas1Teor[j] = (U0_1 * Math.Exp(-(j * ed) / T2_1));
            for (int j = 0; j < jMax - v; j++) mas2Teor[j] = (U0_2 * Math.Exp(-(j * ed) / T2_2));
            PointF[] pf1Teor = new PointF[v + 1];
            PointF[] pf2Teor = new PointF[jMax - v];
            for (int j = 0; j <= v; j++)
            {
                pf1Teor[j].X = j * 20;
                pf1Teor[j].Y = (float)(h0 - mas1Teor[j] / 50);
            }
            graf.DrawLines(Pens.Red, pf1Teor);

            for (int j = 0; j < jMax - v; j++)
            {
                pf2Teor[j].X = (j + v) * 20;
                pf2Teor[j].Y = (float)(h0 - mas2Teor[j] / 50);
            }
            graf.DrawLines(Pens.Blue, pf2Teor);
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            MainDraw();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            MainDraw();
        }
        private int MinSquareMas(int n, double[] mas, ref double u, ref double t, ref double s)
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
        private int MinSquareMas2UT(int n1, int n2, double[] mas, ref double u, ref double t, ref double s, ref double u2, ref double t2, ref double s2)
        {
            double x, y, m;
            double sum1, sum2, sum3, sum4;

            sum1 = sum2 = sum3 = sum4 = 0;
            m = 0;

            double x2, y2, m2;
            double sum1_2, sum2_2, sum3_2, sum4_2;
            //double ed = 10;

            sum1_2 = sum2_2 = sum3_2 = sum4_2 = 0;
            m2 = 0;

            for (int i = 0; i <= n1; i++)
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

            if (0 > u) return -2;
            if (0 > t) return -2;

            sum1 = 0;
            m = 0;
            for (int i = 0; i <= n1; i++)
            {
                if (mas[i] < 0.001) continue;

                x = i * ed;
                //y = mas[i];
                y = Math.Log(mas[i]);
                m++;
                //sum1 += (y - (u * Math.Exp(-x / t))) * (y - (u * Math.Exp(-x / t)));
                sum1 += (y - a - b * x) * (y - a - b * x);
            }

            if (m > 2)
            {
                s = sum1 / (m - 2);
            }
            else { s = 0; }

            //********************************************************************************
            for (int i = 0; i < n2; i++)
            {
                if (mas[i + n1] < 0.001) continue;

                x2 = i * ed;
                y2 = Math.Log(mas[i + n1]);
                m2++;

                sum1_2 += x2 * y2;
                sum2_2 += x2;
                sum3_2 += y2;
                sum4_2 += x2 * x2;
            }

            if (m2 < 3)
            {
                u2 = -1;
                t2 = -1;
                s2 = -1;
                return -1;
            }

            double b2 = (sum2_2 * sum3_2 - m2 * sum1_2) / (sum2_2 * sum2_2 - m2 * sum4_2);
            double a2 = (sum3_2 - b2 * sum2_2) / m2;

            u2 = Math.Exp(a2);
            t2 = -1 / b2;

            if (double.IsInfinity(u2)) return -1;
            if (double.IsInfinity(t2)) return -1;

            if (0 > u2) return -2;
            if (0 > t2) return -2;

            sum1_2 = 0;
            m2 = 0;
            for (int i = 0; i < n2; i++)
            {
                if (mas[i + n1] < 0.001) continue;

                x2 = i * ed;
                //y2 = mas[i + n1];
                y2 = Math.Log(mas[i + n1]);
                m2++;
                //sum1_2 += (y2 - (u2 * Math.Exp(-x2 / t2))) * (y2 - (u2 * Math.Exp(-x2 / t2)));
                sum1_2 += (y2 - a2 - b2 * x2) * (y2 - a2 - b2 * x2);
            }

            if (m2 > 2)
            {
                s2 = sum1_2 / (m2 - 2);
            }
            else { s2 = 0; }

            return 0;
        }
        private int MinSquareMasLin(int n, double[] mas, ref double u, ref double t, ref double s)
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

        private void button1_Click(object sender, EventArgs e)
        {
            double[] masOrig = new double[45]; // !!! 45 - это для поиска jMax !!!
            double[] mas1 = new double[43];

            int v = trackBar2.Value;
            try
            {
                StreamWriter sw = new StreamWriter(@"D:\MyProgect\IGN\1017UTS.txt");

                for (int cyrDepth = 0; cyrDepth < depth.Length; cyrDepth++)
                {
                    sw.Write("{0,7:f2} ", depth[cyrDepth]);

                    for (int j = 0; j < 45; j++) masOrig[j] = 0;
                    for (int j = 0; j < 43; j++) masOrig[j] = data[cyrDepth, j];
                    for (int j = 0; j < 43; j++) mas1[j] = 0;

                    double U0_O = 0;
                    double T2_O = 0;
                    double S_O = 0;

                    int offset = 0;
                    int tailLen = 7;
                    double U0 = 0, T2 = 0, S = 0;
                    double U0_L = 0, T2_L = 0, S_L = 0;
                    for (int n = 0; n < 43 - tailLen; n++)
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
                        if (S_O > S_L) break;
                        offset++;
                    }

                    int jMax = offset - 1;

                    if (-1 == MinSquareMas(jMax, masOrig, ref U0, ref T2, ref S))
                    {
                        MessageBox.Show("if (-1 == MinSquareMasLin(tailLen, mas1, ref U0, ref T2, ref S))");
                        return;
                    }
                    //sw.Write("{0,8:f2} {1,7:f2} {2,7:f3} ", U0, T2, S);
                    sw.Write("{0,8:f2} {1,7:f2} ", U0, T2, S);

                    double U0_1 = 0, T2_1 = 0, S_1 = 0;
                    double U0_2 = 0, T2_2 = 0, S_2 = 0;
                    double[] masU0_1 = new double[43];
                    double[] masT2_1 = new double[43];
                    double[] masS_1 = new double[43];
                    double[] masU0_2 = new double[43];
                    double[] masT2_2 = new double[43];
                    double[] masS_2 = new double[43];
                    double sMin = 10000;
                    int indSMin = 0;
                    for (int n = 3; n < jMax - tailLen; n++) // начинаем с 3-х точек для первой эксп.
                    {
                        int ret = MinSquareMas2UT(n, jMax - n - 1, masOrig, ref U0_1, ref T2_1, ref S_1, ref U0_2, ref T2_2, ref S_2);
                        if (-1 == ret)
                        {
                            MessageBox.Show("if (-1 == MinSquareMas2UT(v, jMax - v - 1, masOrig, ref U0_1, ref T2_1, ref S_1, ref U0_2, ref T2_2, ref S_2))");
                            return;
                        }
                        if (-2 == ret) continue;

                        masU0_1[n] = U0_1;
                        masT2_1[n] = T2_1;
                        masS_1[n] = S_1;
                        masU0_2[n] = U0_2;
                        masT2_2[n] = T2_2;
                        masS_2[n] = S_2;
                    }
                    for (int n = 3; n < jMax - tailLen; n++)
                    {
                        if (sMin > masS_1[n] + masS_2[n]) { sMin = masS_1[n] + masS_2[n]; indSMin = n; }
                    }
                    //sw.Write("{0,7:f3} {1,7:f3} {2,7:f3} {3,7:f3} {4}", masT2_1[indSMin], masS_1[indSMin], masT2_2[indSMin], masS_2[indSMin], indSMin);
                    sw.Write("{0,7:f3}  {2,7:f3}", masT2_1[indSMin], masS_1[indSMin], masT2_2[indSMin], masS_2[indSMin], indSMin);

                    sw.WriteLine();
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int W = 43;
            int H = depth.Length;

            double[] masOrig = new double[45]; // !!! 45 - это для поиска jMax !!!
            double[] mas1 = new double[43];

            int v = trackBar2.Value;

            Bitmap p = new Bitmap(W, H);
            Graphics graphPNG = Graphics.FromImage(p);

            graphPNG.Clear(Color.White);

            graphPNG.DrawLine(Pens.Black, 0, 0, 50, H);
            for (int cyrDepth = 0; cyrDepth < depth.Length; cyrDepth++)
            {
                for (int j = 0; j < 45; j++) masOrig[j] = 0;
                for (int j = 0; j < 43; j++) masOrig[j] = data[cyrDepth, j];
                for (int j = 0; j < 43; j++) mas1[j] = 0;

                double U0_O = 0;
                double T2_O = 0;
                double S_O = 0;

                int offset = 0;
                int tailLen = 7;
                double U0 = 0, T2 = 0, S = 0;
                double U0_L = 0, T2_L = 0, S_L = 0;
                for (int n = 0; n < 43 - tailLen; n++)
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
                    if (S_O > S_L) break;
                    offset++;
                }

                int jMax = offset - 1;

                if (-1 == MinSquareMas(jMax, masOrig, ref U0, ref T2, ref S))
                {
                    MessageBox.Show("if (-1 == MinSquareMasLin(tailLen, mas1, ref U0, ref T2, ref S))");
                    return;
                }

                double U0_1 = 0, T2_1 = 0, S_1 = 0;
                double U0_2 = 0, T2_2 = 0, S_2 = 0;
                double[] masU0_1 = new double[43];
                double[] masT2_1 = new double[43];
                double[] masS_1 = new double[43];
                double[] masU0_2 = new double[43];
                double[] masT2_2 = new double[43];
                double[] masS_2 = new double[43];
                double sMin = 10000;
                int indSMin = 0;
                for (int n = 3; n < jMax - tailLen; n++) // начинаем с 3-х точек для первой эксп.
                {
                    int ret = MinSquareMas2UT(n, jMax - n - 1, masOrig, ref U0_1, ref T2_1, ref S_1, ref U0_2, ref T2_2, ref S_2);
                    if (-1 == ret)
                    {
                        MessageBox.Show("if (-1 == MinSquareMas2UT(v, jMax - v - 1, masOrig, ref U0_1, ref T2_1, ref S_1, ref U0_2, ref T2_2, ref S_2))");
                        return;
                    }
                    if (-2 == ret) continue;

                    masU0_1[n] = U0_1;
                    masT2_1[n] = T2_1;
                    masS_1[n] = S_1;
                    masU0_2[n] = U0_2;
                    masT2_2[n] = T2_2;
                    masS_2[n] = S_2;
                }
                for (int n = 3; n < jMax - tailLen; n++)
                {
                    if (sMin > masS_1[n] + masS_2[n]) { sMin = masS_1[n] + masS_2[n]; indSMin = n; }
                }
            }

            p.Save(@"D:\MyProgect\IGN\123.png", ImageFormat.Png);

            p.Dispose();
        }
    }
}
