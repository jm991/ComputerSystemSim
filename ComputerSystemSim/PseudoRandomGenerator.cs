using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace ComputerSystemSim
{
    /**
     * 32-bit Multiplicative Congruential Pseudo Random Number Generator
     * Microsoft XAML/C# Version
     */
    class PseudoRandomGenerator
    {        
        /**
         * The seed used by randomNumberGenerator.
         */
        private static long gv_lRandomNumberSeed = 1;

        private static double twoPow = Math.Pow(2, 31);

        private static double sevenPow = Math.Pow(7, 5);

        /**
         * Returns a random number between 0.0 and 1.0.
         * z_i = 7^5 * z_(i-1) (mod 2^31 - 1)
         * plSeed - Pointer to the seed value to use; pass by reference so it updates each time used
         */
        private static double RandomNumberGenerator(ref long plSeed)
        {
            double dZ;
            double dQuot;
            long lQuot;

            dZ = plSeed * sevenPow;
            dQuot = dZ / twoPow;
            lQuot = (long) Math.Floor(dQuot);
            dZ -= lQuot * twoPow;
            plSeed = (long) Math.Floor(dZ);

            return (dZ / twoPow);
        }

        public static double RandomNumberGenerator()
        {
            return RandomNumberGenerator(ref gv_lRandomNumberSeed);
        }
        
        /**
         * Returns a random variate from an exponential probability
         * distribution with the given mean value of dMean.
         */
        public static double ExponentialRVG(double dMean)
        {
            return (-dMean * Math.Log(RandomNumberGenerator(ref gv_lRandomNumberSeed)));
        } 
    }
}


