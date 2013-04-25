using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace ComputerSystemSim
{
    /// <summary>
    /// 32-bit Multiplicative Congruential Pseudo Random Number Generator
    /// Microsoft XAML/C# Version
    /// </summary>
    class PseudoRandomGenerator
    {
        #region Variables (private)

        /// <summary>
        /// The seed used by randomNumberGenerator
        /// </summary>      
        private static long gv_lRandomNumberSeed = 1;

        /// <summary>
        /// 2^31
        /// </summary>
        private static double twoPow = Math.Pow(2, 31);

        /// <summary>
        /// 7^5
        /// </summary>
        private static double sevenPow = Math.Pow(7, 5);

        #endregion

        /// <summary>
        /// Uses the forumla we derived in class, z_i = 7^5 * z_(i-1) (mod 2^31 - 1)        
        /// </summary>
        /// <param name="plSeed">Pointer to the seed value to use; pass by reference so it updates each time used</param>
        /// <returns>Returns a random number between 0.0 and 1.0.</returns>
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

        /// <summary>
        /// Wrapper method for random number gen.
        /// </summary>
        /// <returns>Returns a random number between 0.0 and 1.0.</returns>
        public static double RandomNumberGenerator()
        {
            return RandomNumberGenerator(ref gv_lRandomNumberSeed);
        }
        
        /// <summary>
        /// Returns a random variate from an exponential probability distribution with the given mean value of dMean.
        /// </summary>
        /// <param name="dMean">Given mean for distribution</param>
        /// <returns>Random value with mean centered at dMean</returns>
        public static double ExponentialRVG(double dMean)
        {
            return (-dMean * Math.Log(RandomNumberGenerator(ref gv_lRandomNumberSeed)));
        } 
    }
}


