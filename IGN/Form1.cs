﻿using System;
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

namespace IGN
{
    public partial class Form1 : Form
    {
        Graphics graf;
        Pen pen;
        SolidBrush SolidBrushB;
        Rectangle r;

        int h0;

        float[] depth = new float[3199];
        float[,] data = new float[3199,43];

        PointF[] pf;
        PointF[] pfT;

        double U0 = 1;
        double T2 = 0.5;
        double U0_2;
        double T2_2;
        int M = 0;

        public Form1()
        {
            InitializeComponent();

            h0 = Height / 3 * 2;
            pf = new PointF[43];
            pfT = new PointF[43];

            SolidBrushB = new SolidBrush(BackColor);
            r = new Rectangle(0, 0, Width - 100, h0);

            graf = this.CreateGraphics();
            pen = new Pen(Color.FromArgb(255, 0, 0, 0),2);

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

            trackBar2.Maximum = 42;
            trackBar2.TickFrequency = 5;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            graf.DrawLine(Pens.Black, 0, h0, Width - 50, h0);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = depth[trackBar1.Value].ToString();
            graf.FillRectangle(SolidBrushB, r);

            for (int j = 0; j < 43; j++)
            {
                pf[j].X = j * 20;
                pf[j].Y = h0 - (data[trackBar1.Value, j] / 50);
            }
            graf.DrawLines(pen, pf);

            float d;

            MinSquare(trackBar1.Value);
            for (int j = 0; j < 43; j++)
            {
                pfT[j].X = j * 20;
                d = (float)(U0 * Math.Exp(-(j * 2) / T2));
                pfT[j].Y = h0 - (d / 50);
            }
            graf.DrawLines(Pens.Gray, pfT);

            //MinSquare2(trackBar1.Value);
            //for (int j = 0; j < 43; j++)
            //{
            //    pfT[j].X = j * 20;
            //    d = (float)(U0 * Math.Exp(-(j * 2) / T2));
            //    pfT[j].Y = h0 - (d / 50);
            //}
            //graf.DrawLines(Pens.Green, pfT);

            //MinSquare3(trackBar1.Value);
            //for (int j = 0; j < 43; j++)
            //{
            //    pfT[j].X = j * 20;
            //    d = (float)(U0 * Math.Exp(-(j * 2) / T2));
            //    pfT[j].Y = h0 - (d / 50);
            //}
            //graf.DrawLines(Pens.Red, pfT);

            label2.Text = "M=" + M.ToString();
            label3.Text = trackBar1.Value.ToString();

            graf.DrawLine(Pens.Black, 0, h0, 43 * 20, h0);

            for (int j = 0; j < 43; j++)
            {
                pf[j].X = j * 20 + 150;
                pf[j].Y = h0 - (data[trackBar1.Value, j] / 50) - 150;
            }
            graf.DrawLines(pen, pf);
            graf.DrawLine(Pens.Black, 150, h0 - 150, 43 * 20 + 150, h0 - 150);
        }

        private int MinSquareMas(int n, double[] mas, ref double u, ref double t, ref double s)
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

        private void MinSquare(int ind)
        {
            double x, y, m;
            double sum1, sum2, sum3, sum4;

            sum1 = sum2 = sum3 = sum4 = 0;
            m = 0;

            for (int i = 0; i < 43; i++)
            {
                if (data[ind, i] < 0.001) continue;

                x = i * 2;
                y = Math.Log(data[ind, i]);
                m++;

                sum1 += x * y;
                sum2 += x;
                sum3 += y;
                sum4 += x * x;
            }

            double b = (sum2 * sum3 - m * sum1) / (sum2 * sum2 - m * sum4);
            double a = (sum3 - b * sum2) / m;

            U0 = Math.Exp(a);
            T2 = -1 / b;
        }
        private void MinSquare2(int ind)
        {
            double x, y, m;
            double sum1, sum2, sum3, sum4;

            sum1 = sum2 = sum3 = sum4 = 0;
            m = 0;

            for (int i = 0; i < 43; i++)
            {
                if (data[ind, i] < 0.001) break;

                x = i * 2;
                y = Math.Log(data[ind, i]);
                m++;

                sum1 += x * y;
                sum2 += x;
                sum3 += y;
                sum4 += x * x;
            }

            double b = (sum2 * sum3 - m * sum1) / (sum2 * sum2 - m * sum4);
            double a = (sum3 - b * sum2) / m;

            U0 = Math.Exp(a);
            T2 = -1 / b;
        }
        private void MinSquare3(int ind)
        {
            double x, y;
            double sum1, sum2, sum3, sum4;
            int m;
            int z = 0;
            int startInd = 0;

            sum1 = sum2 = sum3 = sum4 = 0;
            m = 0;
            for (int i = 0; i < 42; i++)
            {
                if (data[ind, i] < 0.001)
                {
                    if (data[ind, i + 1] > 0.001)
                    {
                        data[ind, i] = 0.01f;
                    }
                }
            }
            for (int i = 0; i < 42; i++)
            {
                if (data[ind, i] > 0.001f) z++;
                else break;
            }
            
            //if (z > 24) startInd = z - 7;

            for (int i = startInd; i < 42; i++)
            {
                if (data[ind, i] < 0.001) break;

                x = (i - startInd) * 2;
                y = Math.Log(data[ind, i]);
                m++;

                sum1 += x * y;
                sum2 += x;
                sum3 += y;
                sum4 += x * x;
            }

            double b = (sum2 * sum3 - m * sum1) / (sum2 * sum2 - m * sum4);
            double a = (sum3 - b * sum2) / m;

            U0 = Math.Exp(a);
            T2 = -1 / b;
            M = m;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double[] mas = new double[43];
            double[] masT = new double[43];
            double u = 0;
            double t = 0;
            double s = 0;
            double delta = 0;
            int z = 0;

            try
            {
                StreamWriter sw = new StreamWriter(@"D:\MyProgect\IGN\1017_30.txt");
                for (int i = 0; i < depth.Length; i++)
                {
                    MinSquare(i);
                    sw.Write("{0,7:f2} {1,9:f2} {2,7:f2} ", depth[i], U0, T2);

                    for (int j = 0; j < 43; j++)
                    {
                        masT[j] = (float)(U0 * Math.Exp(-(j * 2) / T2));
                        mas[j] = data[i, j];
                    }

                    z = 0;
                    for (int j = 0; j < 43; j++)
                    {
                        delta = data[i, j] - masT[j];
                        if (delta > 0) break;
                        z++;
                    }
                    for (int j = 0; j < 43 - z; j++)
                    {
                        mas[j] = data[i, j + z];
                    }

                    sw.Write("{0,9:f2} {1,3}", delta, z);

                    MinSquareMas(43 - z, mas, ref u, ref t, ref s);
                    sw.Write("  |  ");
                    sw.Write("{0,9:f2} {1,7:f2}", u, t);

                    sw.WriteLine();
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            graf.FillRectangle(SolidBrushB, r);
            
            label4.Text = trackBar2.Value.ToString();

            for (int j = 0; j < 43; j++)
            {
                pf[j].X = j * 20 + 150;
                pf[j].Y = h0 - (data[trackBar1.Value, j] / 50) - 150;
            }
            graf.DrawLines(pen, pf);

            //MinSquare(trackBar1.Value);

            float d;
            int k = 0 - trackBar2.Value;

            PointF[] p = new PointF[43];
            double[] mas = new double[43 - trackBar2.Value];
            double[] masNew = new double[43];
            double u = 0;
            double t = 0;
            double s = 0;

            for (int j = 0; j < 43 - trackBar2.Value; j++)
            {
                mas[j] = data[trackBar1.Value, j + trackBar2.Value];
            }
            
            int ret = MinSquareMas(43 - trackBar2.Value, mas, ref u, ref t, ref s);

            if (ret == -1) return;

            for (int j = 0; j < 43; j++)
            {
                p[j].X = j * 20 + 150;
                d = (float)(u * Math.Exp(-(k * 2) / t));
                k++;
                p[j].Y = h0 - (d / 50) - 150;
            }
            if (trackBar2.Value < 40) graf.DrawLines(Pens.Red, p);

            k = 0 - trackBar2.Value;
            for (int j = 0; j < 43; j++)
            {
                p[j].X = j * 20 + 152;
                d = data[trackBar1.Value, j] - (float)(u * Math.Exp(-(k * 2) / t));
                //if (d < 0) d *= -1;
                k++;
                p[j].Y = h0 - (d / 50) - 152;
                masNew[j] = d;
            }
            if (trackBar2.Value < 40) graf.DrawLines(Pens.Green, p);
            
            ret = MinSquareMas(43, masNew, ref u, ref t, ref s);
            for (int j = 0; j < 43; j++)
            {
                p[j].X = j * 20 + 150;
                d = (float)(u * Math.Exp(-(j * 2) / t));
                p[j].Y = h0 - (d / 50) - 150;
            }
            if (trackBar2.Value < 40) graf.DrawLines(Pens.DarkRed, p);

            for (int j = 0; j < 43; j++)
            {
                pfT[j].X = j * 20 + 150;
                d = (float)(U0 * Math.Exp(-(j * 2) / T2));
                pfT[j].Y = h0 - (d / 50) - 148;
            }
            graf.DrawLines(Pens.Gray, pfT);
        }
    }
}
