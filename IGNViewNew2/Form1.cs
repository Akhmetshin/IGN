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

namespace IGNViewNew2
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
        bool flag1 = false;
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

            //trackBar2.Minimum = 3;
            trackBar2.Maximum = 40;
            trackBar2.TickFrequency = 5;
        }
        private void MainDraw()
        {
            double[] masOrig = new double[45]; // !!! 45 - это для поиска jMax !!!
            double[] mas1 = new double[43];
            double[] masDelta = new double[43];

            label1.Text = depth[trackBar1.Value].ToString();

            graf.FillRectangle(SolidBrushB, r);

            for (int j = 0; j < 45; j++) masOrig[j] = 0;
            for (int j = 0; j < 43; j++) masOrig[j] = data[trackBar1.Value, j];

            double U0_O = 0;
            double T2_O = 0;
            double S_O = 0;
            double[] masU0 = new double[43];
            double[] masT2 = new double[43];

            int offset = 0;
            int tailLen = 7;
            double U0 = 0, T2 = 0, S = 0;
            double U0_L = 0, T2_L = 0, S_L = 0;
            string strT2 = "";
            string strT2_Lin = "";
            for (int n = 0; n < 43 - tailLen; n++)
            {
                for (int j = 0; j < tailLen; j++) mas1[j] = masOrig[j + offset];
                if (-1 == MinSquareMas(tailLen, mas1, ref U0, ref T2, ref S)) break;
                U0_O = U0;
                T2_O = T2;
                strT2 += string.Format("{0,5:f2} ", T2);
                masU0[n] = U0;
                masT2[n] = T2;
                S_O = S;
                if (-1 == MinSquareMasLin(tailLen, mas1, ref U0, ref T2, ref S)) break;
                U0_L = U0;
                T2_L = T2;
                strT2_Lin += string.Format("{0,5:f2} ", T2);
                S_L = S;
                if (S_O > S_L) break;
                offset++;
            }
            label4.Text = strT2;
            label5.Text = strT2_Lin;
            int jMax = offset;

            trackBar2.Maximum = jMax;

            label3.Text = jMax.ToString();
            MyDrawLines(jMax, masOrig, pen1);

            strT2_Lin = "";
            int indMaxT2 = 0;
            if(flag1)
            {
                for (int n = 1; n < 43 - tailLen; n++)
                {
                    if (masU0[n] > masOrig[n]) { indMaxT2++; continue; }
                    if (masT2[n] < masT2[n + 1])
                    {
                        strT2_Lin += string.Format("{0,5:f2} ", masT2[n]);
                        indMaxT2++;
                    }
                    else { indMaxT2++; break; }
                }
                label5.Text = strT2_Lin;
                trackBar2.Value = indMaxT2;
            }
            flag1 = false;

            int v = trackBar2.Value;
            label2.Text = trackBar2.Value.ToString();
            label6.Text = string.Format("{0,5:f2} ", masT2[v]);
            MyDrawLinesT2U(jMax, mas1, Pens.Red, v, masU0[v], masT2[v]);

            for (int j = 0; j < jMax; j++) masDelta[j] = masOrig[j] - mas1[j];
            MyDrawLines(jMax, masDelta, pen2);
        }
        private void MyDrawLinesT2U(int n, double[] mas, Pen p, int v, double U0, double T2)
        {
            if (n - v < 2) return;

            for (int j = 0; j < n; j++) mas[j] = (U0 * Math.Exp(-((j - v) * 2) / T2));

            PointF[] pf = new PointF[n];
            for (int j = 0; j < n; j++)
            {
                pf[j].X = j * 20;
                pf[j].Y = (float)(h0 - mas[j] / 50);
            }
            graf.DrawLines(p, pf);
        }
        private void MyDrawLines(int n, double[] mas, Pen p)
        {
            PointF[] pf = new PointF[n];
            for (int j = 0; j < n; j++)
            {
                pf[j].X = j * 20;
                pf[j].Y = (float)(h0 - mas[j] / 50);
            }
            graf.DrawLines(p, pf);
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

        private void button1_Click(object sender, EventArgs e)
        {
            flag1 = true;
            MainDraw();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double[] masOrig = new double[45]; // !!! 45 - это для поиска jMax !!!
            double[] mas1 = new double[43];
            double[] masFirstTeor = new double[43];
            double[] masTailTeor = new double[43];
            double[] masDelta = new double[43];
            double[] masNearTeor = new double[43];
            double[] masFarTeor = new double[43];

            double[] masU0 = new double[43];
            double[] masT2 = new double[43];
            double[] masS = new double[43];

            double U0_O = 0;
            double T2_O = 0;
            double S_O = 0;
            double U0_W = 0;
            double T2_W = 0;
            double S_W = 0;

            int tailLen = 7;

            try
            {
                StreamWriter sw = new StreamWriter(@"D:\MyProgect\IGN\1017.txt");
                for (int cyrDepth = 0; cyrDepth < depth.Length; cyrDepth++)
                {
                    sw.Write("{0,7:f2} ", depth[cyrDepth]);

                    for (int j = 0; j < 45; j++) masOrig[j] = 0;
                    for (int j = 0; j < 43; j++) masOrig[j] = data[cyrDepth, j];

                    double U0 = 0, T2 = 0, S = 0;
                    double U0_L = 0, T2_L = 0, S_L = 0;
                    MinSquareMas(3, masOrig, ref U0, ref T2, ref S);
                    for (int j = 0; j < 43; j++) masFirstTeor[j] = (U0 * Math.Exp(-(j * 2) / T2)); // теор эксп по первым 3 точкам
                    sw.Write(" |{0,8:f2} {1,7:f2} {2,7:f5} |", U0, T2, S);

                    int offset = 0;
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
                    offset = 0;

                    MinSquareMas(jMax, masOrig, ref U0, ref T2, ref S);
                    sw.Write("{0,8:f2} {1,7:f2} {2,7:f2} ", U0, T2, S);
                    MinSquareMas(jMax - 1, masOrig, ref U0, ref T2, ref S);
                    sw.Write("{0,8:f2} {1,7:f2} {2,7:f2} ", U0, T2, S);

                    // на каждой точке глубины
                    double SFar = 0, SNear = 0;
                    double[] SNew = new double[jMax];
                    double sum1 = 0;
                    for (int n = 0; n < jMax; n++)
                    {
                        for (int j = 0; j < tailLen; j++) mas1[j] = masOrig[j + offset]; // двигаю хвост направо
                        if (-1 == MinSquareMas(tailLen, mas1, ref U0, ref T2, ref S)) break; // рассчёт U0, T2, S
                        for (int j = 0; j < jMax; j++) masTailTeor[j] = (U0 * Math.Exp(-((j - n) * 2) / T2)); // рассчёт теоретической кривой (экспоненты) для хвоста
                        for (int j = 0; j < jMax; j++) masDelta[j] = masFirstTeor[j] - masTailTeor[j]; // дельта между эксп по трём первым точкам и текущей теор эксп
                        if (-1 == MinSquareMas(tailLen, masDelta, ref U0, ref T2, ref SNear)) break; // U0, T2, S для ближней (быстрой) эксп.
                        for (int j = 0; j < jMax; j++) masNearTeor[j] = (U0 * Math.Exp(-(j * 2) / T2)); // рассчёт теоретической Ближней экспоненты
                        for (int j = 0; j < jMax; j++) masDelta[j] = masOrig[j] - masNearTeor[j]; // дельта между замером и ближней теоретической эксп
                        if (-1 == MinSquareMas(tailLen, masDelta, ref U0, ref T2, ref SFar)) break; // U0, T2, S для дальней (медленной) эксп.
                        for (int j = 0; j < jMax; j++) masFarTeor[j] = (U0 * Math.Exp(-(j * 2) / T2)); // рассчёт теоретической Дальней экспоненты
                        sum1 = 0;
                        for (int j = 0; j < jMax; j++)
                        {
                            sum1 += (masOrig[j] - (masNearTeor[j] + masFarTeor[j])) * (masOrig[j] - (masNearTeor[j] + masFarTeor[j]));
                        }
                        
                        SNew[n] = sum1 / (jMax - 2);

                        masU0[n] = U0;
                        masT2[n] = T2;
                        masS[n] = S;

                        offset++;
                    }

                    sw.WriteLine();
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
