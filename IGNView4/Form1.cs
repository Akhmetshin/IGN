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

namespace IGNView4
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
            r = new Rectangle(0, 0, Width - 100, h0 + 10);

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
        }
        private void MainDraw()
        {
            double[] masOrig = new double[45]; // !!! 45 - это для поиска jMax !!!
            double[] mas1 = new double[43];
            double[] mas2 = new double[43];

            label1.Text = depth[trackBar1.Value].ToString();

            graf.FillRectangle(SolidBrushB, r);

            for (int j = 0; j < 45; j++) masOrig[j] = 0;
            for (int j = 0; j < 43; j++) mas1[j] = 0;
            for (int j = 0; j < 43; j++) mas2[j] = j;
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
                offset++;
            }

            int jMax = offset - 1;
            
            label2.Text = jMax.ToString();

            PointF[] pf1 = new PointF[jMax];
            PointF[] pf2 = new PointF[jMax];
            for (int j = 0; j < jMax; j++)
            {
                pf1[j].X = j * 20;
                pf1[j].Y = (float)(h0 - masOrig[j] / 50);
                mas2[j] = j + 1;
            }
            graf.DrawLines(pen1, pf1);

            double a = 0;
            double b = 0;
            double R = 0;

            GipReg(jMax, mas2, masOrig, ref a, ref b, ref R);

            label5.Text = a.ToString("F4");
            label7.Text = b.ToString("F4");
            label9.Text = R.ToString("F4");

            if (double.IsNaN(a)) return;

            for (int j = 0; j < jMax; j++)
            {
                double yt = a + b / mas2[j];
                pf2[j].X = j * 20;
                pf2[j].Y = (float)(h0 - yt / 50);
            }

            graf.DrawLines(Pens.Red, pf2);

            ExpReg(jMax, mas2, masOrig, ref a, ref b, ref R);

            label16.Text = a.ToString("F4");
            label14.Text = b.ToString("F4");
            label12.Text = R.ToString("F4");

            if (double.IsNaN(a)) return;

            for (int j = 0; j < jMax; j++)
            {
                double yt = Math.Exp(a + b * mas2[j]);
                pf2[j].X = j * 20;
                pf2[j].Y = (float)(h0 - yt / 50);
            }
            graf.DrawLines(Pens.Green, pf2);

            PowReg(jMax, mas2, masOrig, ref a, ref b, ref R);

            //label16.Text = a.ToString("F4");
            //label14.Text = b.ToString("F4");
            //label12.Text = R.ToString("F4");

            if (double.IsNaN(a)) return;

            for (int j = 0; j < jMax; j++)
            {
                double yt = a * Math.Pow(mas2[j], b);
                pf2[j].X = j * 20;
                pf2[j].Y = (float)(h0 - yt / 50);
            }
            graf.DrawLines(Pens.Blue, pf2);

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

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            MainDraw();
        }
        private static int LinReg(int n, double[] x, double[] y, ref double a, ref double b, ref double r)
        {
            double sx = 0;
            double sy = 0;
            double sxy = 0;
            double sx2 = 0;
            double sy2 = 0;

            for (int i = 0; i < n; i++)
            {
                sx += x[i];
                sy += y[i];
                sxy += x[i] * y[i];
                sx2 += x[i] * x[i];
                sy2 += y[i] * y[i];
            }

            a = ((sx * sy) - n * sxy) / (sx * sx - n * sx2);
            b = (sx * sxy - sx2 * sy) / (sx * sx - n * sx2);
            r = (n * sxy - sx * sy) / Math.Sqrt((n * sx2 - sx * sx) * (n * sy2 - sy * sy));

            return 0;
        }

        /// Степенная регрессия
        private static int PowReg(int n, double[] x, double[] y, ref double a, ref double b, ref double R)
        {
            double sx = 0;
            double sy = 0;
            double slx = 0;
            double slx2 = 0;
            double sly = 0;
            double slxy = 0;

            for (int i = 0; i < n; i++)
            {
                sx += x[i];
                sy += y[i];
                slx += Math.Log(x[i]);
                slx2 += Math.Log(x[i]) * Math.Log(x[i]);
                sly += Math.Log(y[i]);
                slxy += Math.Log(x[i]) * Math.Log(y[i]);
            }

            b = (n * slxy - slx * sly) / (n * slx2 - slx * slx);
            a = Math.Exp(1.0 / n * sly - b / n * slx);

            double averY = sy / n;

            double syr1 = 0;
            double syr2 = 0;
            for (int i = 0; i < n; i++)
            {
                double yt = y[i] - a * Math.Pow(x[i], b);
                syr1 += yt * yt;
                syr2 += (y[i] - averY) * (y[i] - averY);
            }

            R = Math.Sqrt(1 - syr1 / syr2);

            return 0;
        }

        /// Показательная регрессия
        private static int SigReg(int n, double[] x, double[] y, ref double a, ref double b, ref double R)
        {
            double sx = 0;
            double sy = 0;
            double sx2 = 0;
            double sly = 0;
            double sxly = 0;

            for (int i = 0; i < n; i++)
            {
                sx += x[i];
                sy += y[i];
                sx2 += x[i] * x[i];
                sly += Math.Log(y[i]);
                sxly += x[i] * Math.Log(y[i]);
            }

            b = Math.Exp((n * sxly - sx * sly) / (n * sx2 - sx * sx));
            a = Math.Exp(1.0 / n * sly - Math.Log(b) / n * sx);

            double averY = sy / n;

            double syr1 = 0;
            double syr2 = 0;
            for (int i = 0; i < n; i++)
            {
                double yt = y[i] - a * Math.Pow(b, x[i]);
                syr1 += yt * yt;
                syr2 += (y[i] - averY) * (y[i] - averY);
            }

            R = Math.Sqrt(1 - syr1 / syr2);

            return 0;
        }

        private static int LogReg(int n, double[] x, double[] y, ref double a, ref double b, ref double R)
        {
            double sx = 0;
            double sy = 0;
            double slx = 0;
            double slx2 = 0;
            double sylx = 0;

            for (int i = 0; i < n; i++)
            {
                sx += x[i];
                sy += y[i];
                slx += Math.Log(x[i]);
                slx2 += Math.Log(x[i]) * Math.Log(x[i]);
                sylx += y[i] * Math.Log(x[i]);
            }

            b = (n * sylx - slx * sy) / (n * slx2 - slx * slx);
            a = 1.0 / n * sy - b / n * slx;

            double averY = sy / n;

            double syr1 = 0;
            double syr2 = 0;
            for (int i = 0; i < n; i++)
            {
                double yt = y[i] - (a + b * Math.Log(x[i]));
                syr1 += yt * yt;
                syr2 += (y[i] - averY) * (y[i] - averY);
            }

            R = Math.Sqrt(1 - syr1 / syr2);

            return 0;
        }

        /// Гиперболическая регрессия
        private static int GipReg(int n, double[] x, double[] y, ref double a, ref double b, ref double R)
        {
            double sx = 0;
            double sy = 0;
            double sdx = 0;
            double sdx2 = 0;
            double sydx = 0;

            for (int i = 0; i < n; i++)
            {
                sx += x[i];
                sy += y[i];
                sdx += 1 / x[i];
                sdx2 += 1 / (x[i] * x[i]);
                sydx += y[i] / x[i];
            }

            b = (n * sydx - sdx * sy) / (n * sdx2 - sdx * sdx);
            a = 1.0 / n * sy - b / n * sdx;

            if (double.IsNaN(a)) return -1;

            double averY = sy / n;

            double syr1 = 0;
            double syr2 = 0;
            for (int i = 0; i < n; i++)
            {
                double yt = y[i] - (a + b / x[i]);
                syr1 += yt * yt;
                syr2 += (y[i] - averY) * (y[i] - averY);
            }

            R = Math.Sqrt(1 - syr1 / syr2);

            return 0;
        }

        /// Экспоненциальная регрессия
        private static int ExpReg(int n, double[] x, double[] y, ref double a, ref double b, ref double R)
        {
            double sx = 0;
            double sy = 0;
            double sx2 = 0;
            double sly = 0;
            double sxly = 0;

            for (int i = 0; i < n; i++)
            {
                sx += x[i];
                sy += y[i];
                sx2 += x[i] * x[i];
                sly += Math.Log(y[i]);
                sxly += x[i] * Math.Log(y[i]);
            }

            b = (n * sxly - sx * sly) / (n * sx2 - sx * sx);
            a = 1.0 / n * sly - b / n * sx;

            double averY = sy / n;

            double syr1 = 0;
            double syr2 = 0;
            for (int i = 0; i < n; i++)
            {
                double yt = y[i] - Math.Exp(a + b * x[i]);
                syr1 += yt * yt;
                syr2 += (y[i] - averY) * (y[i] - averY);
            }

            R = Math.Sqrt(1 - syr1 / syr2);

            return 0;
        }

        private static int CubReg(int n, double[] x, double[] y, ref double a, ref double b, ref double c, ref double d, ref double R, ref double ae)
        {
            double[,] M0 = new double[4, 4];

            double[,] M1 = new double[3, 3];
            double[,] M2 = new double[3, 3];
            double[,] M3 = new double[3, 3];
            double[,] M4 = new double[3, 3];

            double sx = 0;
            double sy = 0;
            double sx2 = 0;
            double sx3 = 0;
            double sx4 = 0;
            double sx5 = 0;
            double sx6 = 0;
            double sxy = 0;
            double sx2y = 0;
            double sx3y = 0;

            double X1 = 0;
            double X2 = 0;
            double X3 = 0;
            double X4 = 0;

            for (int i = 0; i < n; i++)
            {
                sx += x[i];
                sy += y[i];
                sx2 += x[i] * x[i];
                sx3 += x[i] * x[i] * x[i];
                sx4 += x[i] * x[i] * x[i] * x[i];
                sx5 += x[i] * x[i] * x[i] * x[i] * x[i];
                sx6 += x[i] * x[i] * x[i] * x[i] * x[i] * x[i];
                sxy += x[i] * y[i];
                sx2y += x[i] * x[i] * y[i];
                sx3y += x[i] * x[i] * x[i] * y[i];
            }
            X1 = sy;
            X2 = sxy;
            X3 = sx2y;
            X4 = sx3y;

            M0[0, 0] = sx3; M0[0, 1] = sx2; M0[0, 2] = sx; M0[0, 3] = n;
            M0[1, 0] = sx4; M0[1, 1] = sx3; M0[1, 2] = sx2; M0[1, 3] = sx;
            M0[2, 0] = sx5; M0[2, 1] = sx4; M0[2, 2] = sx3; M0[2, 3] = sx2;
            M0[3, 0] = sx6; M0[3, 1] = sx5; M0[3, 2] = sx4; M0[3, 3] = sx3;

            M1[0, 0] = M0[1, 1]; M1[0, 1] = M0[1, 2]; M1[0, 2] = M0[1, 3];
            M1[1, 0] = M0[2, 1]; M1[1, 1] = M0[2, 2]; M1[1, 2] = M0[2, 3];
            M1[2, 0] = M0[3, 1]; M1[2, 1] = M0[3, 2]; M1[2, 2] = M0[3, 3];

            M2[0, 0] = M0[1, 0]; M2[0, 1] = M0[1, 2]; M2[0, 2] = M0[1, 3];
            M2[1, 0] = M0[2, 0]; M2[1, 1] = M0[2, 2]; M2[1, 2] = M0[2, 3];
            M2[2, 0] = M0[3, 0]; M2[2, 1] = M0[3, 2]; M2[2, 2] = M0[3, 3];

            M3[0, 0] = M0[1, 0]; M3[0, 1] = M0[1, 1]; M3[0, 2] = M0[1, 3];
            M3[1, 0] = M0[2, 0]; M3[1, 1] = M0[2, 1]; M3[1, 2] = M0[2, 3];
            M3[2, 0] = M0[3, 0]; M3[2, 1] = M0[3, 1]; M3[2, 2] = M0[3, 3];

            M4[0, 0] = M0[1, 0]; M4[0, 1] = M0[1, 1]; M4[0, 2] = M0[1, 2];
            M4[1, 0] = M0[2, 0]; M4[1, 1] = M0[2, 1]; M4[1, 2] = M0[2, 2];
            M4[2, 0] = M0[3, 0]; M4[2, 1] = M0[3, 1]; M4[2, 2] = M0[3, 2];

            double dt1 = M1[0, 0] * M1[1, 1] * M1[2, 2] + M1[0, 1] * M1[1, 2] * M1[2, 0] + M1[0, 2] * M1[1, 0] * M1[2, 1] - M1[0, 2] * M1[1, 1] * M1[2, 0] - M1[0, 0] * M1[1, 2] * M1[2, 1] - M1[0, 1] * M1[1, 0] * M1[2, 2];
            double dt2 = M2[0, 0] * M2[1, 1] * M2[2, 2] + M2[0, 1] * M2[1, 2] * M2[2, 0] + M2[0, 2] * M2[1, 0] * M2[2, 1] - M2[0, 2] * M2[1, 1] * M2[2, 0] - M2[0, 0] * M2[1, 2] * M2[2, 1] - M2[0, 1] * M2[1, 0] * M2[2, 2];
            double dt3 = M3[0, 0] * M3[1, 1] * M3[2, 2] + M3[0, 1] * M3[1, 2] * M3[2, 0] + M3[0, 2] * M3[1, 0] * M3[2, 1] - M3[0, 2] * M3[1, 1] * M3[2, 0] - M3[0, 0] * M3[1, 2] * M3[2, 1] - M3[0, 1] * M3[1, 0] * M3[2, 2];
            double dt4 = M4[0, 0] * M4[1, 1] * M4[2, 2] + M4[0, 1] * M4[1, 2] * M4[2, 0] + M4[0, 2] * M4[1, 0] * M4[2, 1] - M4[0, 2] * M4[1, 1] * M4[2, 0] - M4[0, 0] * M4[1, 2] * M4[2, 1] - M4[0, 1] * M4[1, 0] * M4[2, 2];

            double D = M0[0, 0] * dt1 - M0[0, 1] * dt2 + M0[0, 2] * dt3 - M0[0, 3] * dt4;

            M0[0, 0] = X1;
            M0[1, 0] = X2;
            M0[2, 0] = X3;
            M0[3, 0] = X4;

            M1[0, 0] = M0[1, 1]; M1[0, 1] = M0[1, 2]; M1[0, 2] = M0[1, 3];
            M1[1, 0] = M0[2, 1]; M1[1, 1] = M0[2, 2]; M1[1, 2] = M0[2, 3];
            M1[2, 0] = M0[3, 1]; M1[2, 1] = M0[3, 2]; M1[2, 2] = M0[3, 3];

            M2[0, 0] = M0[1, 0]; M2[0, 1] = M0[1, 2]; M2[0, 2] = M0[1, 3];
            M2[1, 0] = M0[2, 0]; M2[1, 1] = M0[2, 2]; M2[1, 2] = M0[2, 3];
            M2[2, 0] = M0[3, 0]; M2[2, 1] = M0[3, 2]; M2[2, 2] = M0[3, 3];

            M3[0, 0] = M0[1, 0]; M3[0, 1] = M0[1, 1]; M3[0, 2] = M0[1, 3];
            M3[1, 0] = M0[2, 0]; M3[1, 1] = M0[2, 1]; M3[1, 2] = M0[2, 3];
            M3[2, 0] = M0[3, 0]; M3[2, 1] = M0[3, 1]; M3[2, 2] = M0[3, 3];

            M4[0, 0] = M0[1, 0]; M4[0, 1] = M0[1, 1]; M4[0, 2] = M0[1, 2];
            M4[1, 0] = M0[2, 0]; M4[1, 1] = M0[2, 1]; M4[1, 2] = M0[2, 2];
            M4[2, 0] = M0[3, 0]; M4[2, 1] = M0[3, 1]; M4[2, 2] = M0[3, 2];

            //dt1 = M1[0, 0] * M1[1, 1] * M1[2, 2] + M1[0, 1] * M1[1, 2] * M1[2, 0] + M1[0, 2] * M1[1, 0] * M1[2, 1] - M1[0, 2] * M1[1, 1] * M1[2, 0] - M1[0, 0] * M1[1, 2] * M1[2, 1] - M1[0, 1] * M1[1, 0] * M1[2, 2];
            dt2 = M2[0, 0] * M2[1, 1] * M2[2, 2] + M2[0, 1] * M2[1, 2] * M2[2, 0] + M2[0, 2] * M2[1, 0] * M2[2, 1] - M2[0, 2] * M2[1, 1] * M2[2, 0] - M2[0, 0] * M2[1, 2] * M2[2, 1] - M2[0, 1] * M2[1, 0] * M2[2, 2];
            dt3 = M3[0, 0] * M3[1, 1] * M3[2, 2] + M3[0, 1] * M3[1, 2] * M3[2, 0] + M3[0, 2] * M3[1, 0] * M3[2, 1] - M3[0, 2] * M3[1, 1] * M3[2, 0] - M3[0, 0] * M3[1, 2] * M3[2, 1] - M3[0, 1] * M3[1, 0] * M3[2, 2];
            dt4 = M4[0, 0] * M4[1, 1] * M4[2, 2] + M4[0, 1] * M4[1, 2] * M4[2, 0] + M4[0, 2] * M4[1, 0] * M4[2, 1] - M4[0, 2] * M4[1, 1] * M4[2, 0] - M4[0, 0] * M4[1, 2] * M4[2, 1] - M4[0, 1] * M4[1, 0] * M4[2, 2];

            a = (X1 * dt1 - M0[0, 1] * dt2 + M0[0, 2] * dt3 - M0[0, 3] * dt4) / D;

            M0[0, 0] = sx3;
            M0[1, 0] = sx4;
            M0[2, 0] = sx5;
            M0[3, 0] = sx6;

            M0[0, 1] = X1;
            M0[1, 1] = X2;
            M0[2, 1] = X3;
            M0[3, 1] = X4;

            M1[0, 0] = M0[1, 1]; M1[0, 1] = M0[1, 2]; M1[0, 2] = M0[1, 3];
            M1[1, 0] = M0[2, 1]; M1[1, 1] = M0[2, 2]; M1[1, 2] = M0[2, 3];
            M1[2, 0] = M0[3, 1]; M1[2, 1] = M0[3, 2]; M1[2, 2] = M0[3, 3];

            M2[0, 0] = M0[1, 0]; M2[0, 1] = M0[1, 2]; M2[0, 2] = M0[1, 3];
            M2[1, 0] = M0[2, 0]; M2[1, 1] = M0[2, 2]; M2[1, 2] = M0[2, 3];
            M2[2, 0] = M0[3, 0]; M2[2, 1] = M0[3, 2]; M2[2, 2] = M0[3, 3];

            M3[0, 0] = M0[1, 0]; M3[0, 1] = M0[1, 1]; M3[0, 2] = M0[1, 3];
            M3[1, 0] = M0[2, 0]; M3[1, 1] = M0[2, 1]; M3[1, 2] = M0[2, 3];
            M3[2, 0] = M0[3, 0]; M3[2, 1] = M0[3, 1]; M3[2, 2] = M0[3, 3];

            M4[0, 0] = M0[1, 0]; M4[0, 1] = M0[1, 1]; M4[0, 2] = M0[1, 2];
            M4[1, 0] = M0[2, 0]; M4[1, 1] = M0[2, 1]; M4[1, 2] = M0[2, 2];
            M4[2, 0] = M0[3, 0]; M4[2, 1] = M0[3, 1]; M4[2, 2] = M0[3, 2];

            dt1 = M1[0, 0] * M1[1, 1] * M1[2, 2] + M1[0, 1] * M1[1, 2] * M1[2, 0] + M1[0, 2] * M1[1, 0] * M1[2, 1] - M1[0, 2] * M1[1, 1] * M1[2, 0] - M1[0, 0] * M1[1, 2] * M1[2, 1] - M1[0, 1] * M1[1, 0] * M1[2, 2];
            dt2 = M2[0, 0] * M2[1, 1] * M2[2, 2] + M2[0, 1] * M2[1, 2] * M2[2, 0] + M2[0, 2] * M2[1, 0] * M2[2, 1] - M2[0, 2] * M2[1, 1] * M2[2, 0] - M2[0, 0] * M2[1, 2] * M2[2, 1] - M2[0, 1] * M2[1, 0] * M2[2, 2];
            dt3 = M3[0, 0] * M3[1, 1] * M3[2, 2] + M3[0, 1] * M3[1, 2] * M3[2, 0] + M3[0, 2] * M3[1, 0] * M3[2, 1] - M3[0, 2] * M3[1, 1] * M3[2, 0] - M3[0, 0] * M3[1, 2] * M3[2, 1] - M3[0, 1] * M3[1, 0] * M3[2, 2];
            dt4 = M4[0, 0] * M4[1, 1] * M4[2, 2] + M4[0, 1] * M4[1, 2] * M4[2, 0] + M4[0, 2] * M4[1, 0] * M4[2, 1] - M4[0, 2] * M4[1, 1] * M4[2, 0] - M4[0, 0] * M4[1, 2] * M4[2, 1] - M4[0, 1] * M4[1, 0] * M4[2, 2];

            b = (M0[0, 0] * dt1 - X1 * dt2 + M0[0, 2] * dt3 - M0[0, 3] * dt4) / D;

            M0[0, 1] = sx2;
            M0[1, 1] = sx3;
            M0[2, 1] = sx4;
            M0[3, 1] = sx5;

            M0[0, 2] = X1;
            M0[1, 2] = X2;
            M0[2, 2] = X3;
            M0[3, 2] = X4;

            M1[0, 0] = M0[1, 1]; M1[0, 1] = M0[1, 2]; M1[0, 2] = M0[1, 3];
            M1[1, 0] = M0[2, 1]; M1[1, 1] = M0[2, 2]; M1[1, 2] = M0[2, 3];
            M1[2, 0] = M0[3, 1]; M1[2, 1] = M0[3, 2]; M1[2, 2] = M0[3, 3];

            M2[0, 0] = M0[1, 0]; M2[0, 1] = M0[1, 2]; M2[0, 2] = M0[1, 3];
            M2[1, 0] = M0[2, 0]; M2[1, 1] = M0[2, 2]; M2[1, 2] = M0[2, 3];
            M2[2, 0] = M0[3, 0]; M2[2, 1] = M0[3, 2]; M2[2, 2] = M0[3, 3];

            M3[0, 0] = M0[1, 0]; M3[0, 1] = M0[1, 1]; M3[0, 2] = M0[1, 3];
            M3[1, 0] = M0[2, 0]; M3[1, 1] = M0[2, 1]; M3[1, 2] = M0[2, 3];
            M3[2, 0] = M0[3, 0]; M3[2, 1] = M0[3, 1]; M3[2, 2] = M0[3, 3];

            M4[0, 0] = M0[1, 0]; M4[0, 1] = M0[1, 1]; M4[0, 2] = M0[1, 2];
            M4[1, 0] = M0[2, 0]; M4[1, 1] = M0[2, 1]; M4[1, 2] = M0[2, 2];
            M4[2, 0] = M0[3, 0]; M4[2, 1] = M0[3, 1]; M4[2, 2] = M0[3, 2];

            dt1 = M1[0, 0] * M1[1, 1] * M1[2, 2] + M1[0, 1] * M1[1, 2] * M1[2, 0] + M1[0, 2] * M1[1, 0] * M1[2, 1] - M1[0, 2] * M1[1, 1] * M1[2, 0] - M1[0, 0] * M1[1, 2] * M1[2, 1] - M1[0, 1] * M1[1, 0] * M1[2, 2];
            dt2 = M2[0, 0] * M2[1, 1] * M2[2, 2] + M2[0, 1] * M2[1, 2] * M2[2, 0] + M2[0, 2] * M2[1, 0] * M2[2, 1] - M2[0, 2] * M2[1, 1] * M2[2, 0] - M2[0, 0] * M2[1, 2] * M2[2, 1] - M2[0, 1] * M2[1, 0] * M2[2, 2];
            dt3 = M3[0, 0] * M3[1, 1] * M3[2, 2] + M3[0, 1] * M3[1, 2] * M3[2, 0] + M3[0, 2] * M3[1, 0] * M3[2, 1] - M3[0, 2] * M3[1, 1] * M3[2, 0] - M3[0, 0] * M3[1, 2] * M3[2, 1] - M3[0, 1] * M3[1, 0] * M3[2, 2];
            dt4 = M4[0, 0] * M4[1, 1] * M4[2, 2] + M4[0, 1] * M4[1, 2] * M4[2, 0] + M4[0, 2] * M4[1, 0] * M4[2, 1] - M4[0, 2] * M4[1, 1] * M4[2, 0] - M4[0, 0] * M4[1, 2] * M4[2, 1] - M4[0, 1] * M4[1, 0] * M4[2, 2];

            c = (M0[0, 0] * dt1 - M0[0, 1] * dt2 + X1 * dt3 - M0[0, 3] * dt4) / D;

            M0[0, 2] = sx;
            M0[1, 2] = sx2;
            M0[2, 2] = sx3;
            M0[3, 2] = sx4;

            M0[0, 3] = X1;
            M0[1, 3] = X2;
            M0[2, 3] = X3;
            M0[3, 3] = X4;

            M1[0, 0] = M0[1, 1]; M1[0, 1] = M0[1, 2]; M1[0, 2] = M0[1, 3];
            M1[1, 0] = M0[2, 1]; M1[1, 1] = M0[2, 2]; M1[1, 2] = M0[2, 3];
            M1[2, 0] = M0[3, 1]; M1[2, 1] = M0[3, 2]; M1[2, 2] = M0[3, 3];

            M2[0, 0] = M0[1, 0]; M2[0, 1] = M0[1, 2]; M2[0, 2] = M0[1, 3];
            M2[1, 0] = M0[2, 0]; M2[1, 1] = M0[2, 2]; M2[1, 2] = M0[2, 3];
            M2[2, 0] = M0[3, 0]; M2[2, 1] = M0[3, 2]; M2[2, 2] = M0[3, 3];

            M3[0, 0] = M0[1, 0]; M3[0, 1] = M0[1, 1]; M3[0, 2] = M0[1, 3];
            M3[1, 0] = M0[2, 0]; M3[1, 1] = M0[2, 1]; M3[1, 2] = M0[2, 3];
            M3[2, 0] = M0[3, 0]; M3[2, 1] = M0[3, 1]; M3[2, 2] = M0[3, 3];

            M4[0, 0] = M0[1, 0]; M4[0, 1] = M0[1, 1]; M4[0, 2] = M0[1, 2];
            M4[1, 0] = M0[2, 0]; M4[1, 1] = M0[2, 1]; M4[1, 2] = M0[2, 2];
            M4[2, 0] = M0[3, 0]; M4[2, 1] = M0[3, 1]; M4[2, 2] = M0[3, 2];

            dt1 = M1[0, 0] * M1[1, 1] * M1[2, 2] + M1[0, 1] * M1[1, 2] * M1[2, 0] + M1[0, 2] * M1[1, 0] * M1[2, 1] - M1[0, 2] * M1[1, 1] * M1[2, 0] - M1[0, 0] * M1[1, 2] * M1[2, 1] - M1[0, 1] * M1[1, 0] * M1[2, 2];
            dt2 = M2[0, 0] * M2[1, 1] * M2[2, 2] + M2[0, 1] * M2[1, 2] * M2[2, 0] + M2[0, 2] * M2[1, 0] * M2[2, 1] - M2[0, 2] * M2[1, 1] * M2[2, 0] - M2[0, 0] * M2[1, 2] * M2[2, 1] - M2[0, 1] * M2[1, 0] * M2[2, 2];
            dt3 = M3[0, 0] * M3[1, 1] * M3[2, 2] + M3[0, 1] * M3[1, 2] * M3[2, 0] + M3[0, 2] * M3[1, 0] * M3[2, 1] - M3[0, 2] * M3[1, 1] * M3[2, 0] - M3[0, 0] * M3[1, 2] * M3[2, 1] - M3[0, 1] * M3[1, 0] * M3[2, 2];
            dt4 = M4[0, 0] * M4[1, 1] * M4[2, 2] + M4[0, 1] * M4[1, 2] * M4[2, 0] + M4[0, 2] * M4[1, 0] * M4[2, 1] - M4[0, 2] * M4[1, 1] * M4[2, 0] - M4[0, 0] * M4[1, 2] * M4[2, 1] - M4[0, 1] * M4[1, 0] * M4[2, 2];

            d = (M0[0, 0] * dt1 - M0[0, 1] * dt2 + M0[0, 2] * dt3 - X1 * dt4) / D;

            double yAver = sy / n;
            double ya1 = 0;
            double ya2 = 0;
            for (int i = 0; i < n; i++)
            {
                double t = a * x[i] * x[i] * x[i] + b * x[i] * x[i] + c * x[i] + d;
                ya1 += (y[i] - t) * (y[i] - t);
                ya2 += (y[i] - yAver) * (y[i] - yAver);
            }
            if (ya1 != 0) R = Math.Sqrt(1 - ya1 / ya2);

            ya1 = 0;
            for (int i = 0; i < n; i++)
            {
                double t = a * x[i] * x[i] * x[i] + b * x[i] * x[i] + c * x[i] + d;
                if (0 != y[i]) ya1 += Math.Abs((y[i] - t) / y[i]);
            }

            ae = ya1 / n * 100;

            return 0;
        }

        private static int SqrReg(int n, double[] x, double[] y, ref double a, ref double b, ref double c, ref double R, ref double ae)
        {
            double[,] M = new double[3, 3];
            double[,] DA = new double[3, 3];
            double[,] DB = new double[3, 3];
            double[,] DC = new double[3, 3];

            double sx = 0;
            double sy = 0;
            double sx2 = 0;
            double sx3 = 0;
            double sx4 = 0;
            double sxy = 0;
            double sx2y = 0;

            double X1 = 0;
            double X2 = 0;
            double X3 = 0;

            for (int i = 0; i < n; i++)
            {
                sx += x[i];
                sy += y[i];
                sx2 += x[i] * x[i];
                sx3 += x[i] * x[i] * x[i];
                sx4 += x[i] * x[i] * x[i] * x[i];
                sxy += x[i] * y[i];
                sx2y += x[i] * x[i] * y[i];
            }

            X1 = sy;
            X2 = sxy;
            X3 = sx2y;

            M[0, 0] = sx2;
            M[0, 1] = sx;
            M[0, 2] = n;
            M[1, 0] = sx3;
            M[1, 1] = sx2;
            M[1, 2] = sx;
            M[2, 0] = sx4;
            M[2, 1] = sx3;
            M[2, 2] = sx2;

            DA[0, 0] = DB[0, 0] = DC[0, 0] = M[0, 0];
            DA[0, 1] = DB[0, 1] = DC[0, 1] = M[0, 1];
            DA[0, 2] = DB[0, 2] = DC[0, 2] = M[0, 2];
            DA[1, 0] = DB[1, 0] = DC[1, 0] = M[1, 0];
            DA[1, 1] = DB[1, 1] = DC[1, 1] = M[1, 1];
            DA[1, 2] = DB[1, 2] = DC[1, 2] = M[1, 2];
            DA[2, 0] = DB[2, 0] = DC[2, 0] = M[2, 0];
            DA[2, 1] = DB[2, 1] = DC[2, 1] = M[2, 1];
            DA[2, 2] = DB[2, 2] = DC[2, 2] = M[2, 2];

            DA[0, 0] = X1; DA[1, 0] = X2; DA[2, 0] = X3;
            DB[0, 1] = X1; DB[1, 1] = X2; DB[2, 1] = X3;
            DC[0, 2] = X1; DC[1, 2] = X2; DC[2, 2] = X3;

            double D =
                M[0, 0] * M[1, 1] * M[2, 2]
              + M[0, 1] * M[1, 2] * M[2, 0]
              + M[0, 2] * M[1, 0] * M[2, 1]
              - M[0, 2] * M[1, 1] * M[2, 0]
              - M[0, 0] * M[1, 2] * M[2, 1]
              - M[0, 1] * M[1, 0] * M[2, 2];


            a = (
                DA[0, 0] * DA[1, 1] * DA[2, 2]
              + DA[0, 1] * DA[1, 2] * DA[2, 0]
              + DA[0, 2] * DA[1, 0] * DA[2, 1]
              - DA[0, 2] * DA[1, 1] * DA[2, 0]
              - DA[0, 0] * DA[1, 2] * DA[2, 1]
              - DA[0, 1] * DA[1, 0] * DA[2, 2]) / D;

            b = (
                DB[0, 0] * DB[1, 1] * DB[2, 2]
              + DB[0, 1] * DB[1, 2] * DB[2, 0]
              + DB[0, 2] * DB[1, 0] * DB[2, 1]
              - DB[0, 2] * DB[1, 1] * DB[2, 0]
              - DB[0, 0] * DB[1, 2] * DB[2, 1]
              - DB[0, 1] * DB[1, 0] * DB[2, 2]) / D;

            c = (
                DC[0, 0] * DC[1, 1] * DC[2, 2]
              + DC[0, 1] * DC[1, 2] * DC[2, 0]
              + DC[0, 2] * DC[1, 0] * DC[2, 1]
              - DC[0, 2] * DC[1, 1] * DC[2, 0]
              - DC[0, 0] * DC[1, 2] * DC[2, 1]
              - DC[0, 1] * DC[1, 0] * DC[2, 2]) / D;

            double yAver = sy / n;
            double ya1 = 0;
            double ya2 = 0;
            for (int i = 0; i < n; i++)
            {
                double t = a * x[i] * x[i] + b * x[i] + c;
                ya1 += (y[i] - t) * (y[i] - t);
                ya2 += (y[i] - yAver) * (y[i] - yAver);
            }
            if (ya1 != 0) R = Math.Sqrt(1 - ya1 / ya2);

            ya1 = 0;
            for (int i = 0; i < n; i++)
            {
                double t = a * x[i] * x[i] + b * x[i] + c;
                if (0 != y[i]) ya1 += Math.Abs((y[i] - t) / y[i]);
            }

            ae = ya1 / n * 100;

            return 0;
        }
    }
}
