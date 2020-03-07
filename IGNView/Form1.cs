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

namespace IGNView
{
    public partial class IGNView : Form
    {
        Graphics graf;
        Pen pen;
        SolidBrush SolidBrushB;
        Rectangle r;

        float[] depth = new float[3199];
        float[,] data = new float[3199, 43];

        int h0;
        public IGNView()
        {
            InitializeComponent();

            h0 = Height / 3 * 2;

            SolidBrushB = new SolidBrush(BackColor);
            r = new Rectangle(0, 0, Width - 100, h0 + 1);

            graf = this.CreateGraphics();
            pen = new Pen(Color.FromArgb(255, 0, 0, 0), 2);

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

            //trackBar2.Maximum = 42;
            //trackBar2.TickFrequency = 5;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            float[] masOrig = new float[43];
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

            label1.Text = depth[trackBar1.Value].ToString();
            graf.FillRectangle(SolidBrushB, r);
            
            for (int j = 0; j < 43; j++) masOrig[j] = data[trackBar1.Value, j];
            DrawExp(43, masOrig, pen);

            MinSquareMas(43, masOrig, ref U0_O, ref T2_O, ref S_O);
            DrawExp(43, masWork, Pens.Red, U0_O, T2_O);
            
            int n = 0;
        }
        private void DrawExp(int n, float[] mas, Pen p, double u, double t)
        {
            PointF[] pf = new PointF[43];
            for (int j = 0; j < n; j++)
            {
                mas[j]= (float)(u * Math.Exp(-(j * 2) / t));
            }
            for (int j = 0; j < n; j++)
            {
                pf[j].X = j * 20;
                pf[j].Y = h0 - mas[j] / 50;
            }
            graf.DrawLines(p, pf);
        }
        private void DrawExp(int n, float[] mas, Pen p)
        {
            PointF[] pf = new PointF[43];

            for (int j = 0; j < n; j++)
            {
                pf[j].X = j * 20;
                pf[j].Y = h0 - mas[j] / 50;
            }
            graf.DrawLines(p, pf);
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
