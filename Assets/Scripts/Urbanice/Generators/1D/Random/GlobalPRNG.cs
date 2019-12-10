using System;
using UnityEngine;

namespace Urbanice.Generators._1D.Random
{
    /// <summary>
    /// The global PRNG as a singleton
    /// </summary>
    public class GlobalPRNG
    {
        private static GlobalPRNG _instance;
        
        private static int _seed;
        private static System.Random _rnd;
        private static bool _initialzed = false;

        public static bool Initialzed => _initialzed;
        public static string SeedString;

        public static int Seed {
            get
            {
                _seed = SeedString.GetHashCode();
                return _seed;
            }
        }
        public static void Init(string seedString)
        {
            _initialzed = true;
            
            SeedString = seedString;
            _rnd = new System.Random(Seed);
        }

        /// <summary>
        /// Returns a random number [0f,1f) with linear probability
        /// </summary>
        public static float Next(RandomDistribution distribution = RandomDistribution.Linear)
        {
            if (!_initialzed)
            {
                throw new Exception($"{typeof(GlobalPRNG).Name} needs to be initialized. Call Init() first");
            }

            switch (distribution)
            {
                case RandomDistribution.Linear:
                    return Linear();
                    break;
                case RandomDistribution.EaseInEaseOut:
                    return EaseInEaseOut(2);
                    break;
                case RandomDistribution.EaseInEaseOutCubic:
                    return EaseInEaseOut(3);

                    break;
                case RandomDistribution.Cubic:
                    return Cubic();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(distribution), distribution, null);
            }
        }

        private static float Linear()
        {
            return (float)_rnd.NextDouble();
        }
        /// <summary>
        /// Returns values that are more likely close to 0 and 1
        /// </summary>
        private static float EaseInEaseOut(int exp)
        {
            var x = Next();
            var val = (float) ((float)(Math.Pow(x,exp)) / (Math.Pow(x,exp) + Math.Pow(1-x,exp)));
            return val;
        }
        /// <summary>
        /// Returns values that are more likely close to 0.5
        /// </summary>
        private static float Cubic()
        {
            var x = Next();
            var val = 6 * Math.Pow(x - .47f, 3) - (x*x)/2f + .62f;
            Debug.Log($"x: {x} => {val}");
            return (float) val;
        }
    }
}