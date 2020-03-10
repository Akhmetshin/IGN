using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            MinSqr(40, mas1);
        }

        static void MinSqr(int n, double[] mas)
        {
            for (int j = 0; j < n; j++)
            {
                Console.WriteLine("{0}", mas[j]);
            }
        }
    }
}
