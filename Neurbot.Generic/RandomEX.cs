using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurbot.Generic
{
    public static class RandomEx
    {
        public static double NextNormal(this Random rand)
        {
            // http://mathworld.wolfram.com/Box-MullerTransformation.html
            // get two uniform(0,1] random doubles
            // can't be 0 otherwise log(0) will result in NaN
            double x1 = 1.0 - rand.NextDouble();
            double x2 = 1.0 - rand.NextDouble();

            // Convert to random normal with mean 0 and variance 1
            return Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Sin(2.0 * Math.PI * x2);
        }

        public static double NextNormal(this Random rand, double mean, double variance)
        {
            //random normal(mean,stdDev^2)
            return mean + Math.Sqrt(variance) * rand.NextNormal();
        }
    }
}
