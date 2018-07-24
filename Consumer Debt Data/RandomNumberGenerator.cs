namespace FluidTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Security.Cryptography;


    /// <summary>
    /// Random number generator which exposes a constructor allowing the consumer to use 
    /// a time-basesd seed or not.
    /// </summary>
    class RandomNumberGenerator : Random
    {
        private static int seedValue;

        /// <summary>
        /// Random Number Generator:  If we use a time based seed, ignore the seed paramater value, 
        /// otherwise use the seed parameter
        /// </summary>
        /// <param name="useTimeBasedSeed"></param>
        /// <param name="seed"></param>
        public RandomNumberGenerator(Boolean useTimeBasedSeed, int seed)
             : base(seedValue = useTimeBasedSeed ? (int)(DateTime.Now.Ticks) : seed)
        {
           
        }

        /// <summary>
        /// Random Number Generator
        /// </summary>
        /// <param name="seed"></param>
        public RandomNumberGenerator(int seed) : base(seed)
        {
            seedValue = seed;
        }

        /// <summary>
        /// Value of the seedValue used to instantiate this class
        /// </summary>
        public int Seed
        {
            get{ return seedValue;}
        }


        /// <summary>
        /// testing...
        /// </summary>
        internal static void test()
        {
            
            RandomNumberGenerator r1 = new RandomNumberGenerator(true,0);
            RandomNumberGenerator r2 = new RandomNumberGenerator(false,0);

            StringBuilder s1 = new StringBuilder();
            for (int lcv = 1; lcv <= 10; lcv++)
            {
                s1.Append(r1.Next(5));
                s1.Append(" ");
            }

            StringBuilder s2 = new StringBuilder();
            for (int lcv = 1; lcv <= 10; lcv++)
            {
                s2.Append(r2.Next(5));
                s2.Append(" ");
            }
        
        }
    }


}
