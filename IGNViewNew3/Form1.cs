using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        Pen pen1;
        Pen pen2;
        SolidBrush SolidBrushB;
        Rectangle r;

        float[] depth = new float[3199];
        float[,] data = new float[3199, 43];

        int h0;
        public Form1()
        {
            InitializeComponent();

            h0 = Height / 3 * 2;

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
            label2.Text = trackBar2.Value.ToString();

            graf.FillRectangle(SolidBrushB, r);

            for (int j = 0; j < 45; j++) masOrig[j] = 0;
            for (int j = 0; j < 43; j++) mas1[j] = 0;
            for (int j = 0; j < 43; j++) masOrig[j] = data[trackBar1.Value, j];

            double U0_O = 0;
            double T2_O = 0;
            double S_O = 0;
            double U0_W = 0;
            double T2_W = 0;
            double S_W = 0;

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

            int jMax = offset;
            trackBar2.Maximum = jMax - 3;
            label3.Text = jMax.ToString();

            for (int j = 0; j < trackBar2.Value; j++) mas1[j] = masOrig[j];
            int l = 0;
            for (int j = trackBar2.Value - 1; j < jMax; j++) { mas2[l] = masOrig[j]; l++; }

            PointF[] pf1 = new PointF[trackBar2.Value];
            PointF[] pf2 = new PointF[jMax - trackBar2.Value];

            for (int j = 0; j < trackBar2.Value; j++)
            {
                pf1[j].X = j * 20;
                pf1[j].Y = (float)(h0 - mas1[j] / 50);
            }
            graf.DrawLines(pen1, pf1);

            for (int j = 0; j < jMax - trackBar2.Value; j++)
            {
                pf2[j].X = (j + trackBar2.Value - 1) * 20;
                pf2[j].Y = (float)(h0 - mas2[j] / 50);
            }
            if (jMax - trackBar2.Value > 1) graf.DrawLines(pen2, pf2);
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            MainDraw();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            MainDraw();
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
