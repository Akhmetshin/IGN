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

        PointF[] pf;
        PointF[] pfT;

        int h0;
        public IGNView()
        {
            InitializeComponent();

            h0 = Height / 3 * 2;
            pf = new PointF[43];
            pfT = new PointF[43];

            SolidBrushB = new SolidBrush(BackColor);
            r = new Rectangle(0, 0, Width - 100, h0);

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
            label1.Text = depth[trackBar1.Value].ToString();
            graf.FillRectangle(SolidBrushB, r);

            for (int j = 0; j < 43; j++)
            {
                pf[j].X = j * 20;
                pf[j].Y = h0 - (data[trackBar1.Value, j] / 50);
            }
            graf.DrawLines(pen, pf);

        }
    }
}
