using System;

namespace Urbanice.Generators._1D.Random
{
    /// <summary>
    /// this class contains the PRNG which can be instantiated
    /// </summary>
    public class PseudoRandom
    {
        private int _seed;
        private System.Random _rnd;
        private bool _initialzed = false;

        public bool Initialzed => _initialzed;
        public string SeedString;

        public int Seed {
            get
            {
                _seed = SeedString.GetHashCode();
                return _seed;
            }
        }
        public PseudoRandom(string seedString)
        {
            _initialzed = true;
            
            SeedString = seedString;
            _rnd = new System.Random(Seed);
        }

        /// <summary>
        /// Returns a random number [0f,1f) with linear probability
        /// </summary>
        public float Next(RandomDistribution distribution = RandomDistribution.Linear)
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

        /// <summary>
        /// Returns random numbers with linear probability
        /// </summary>
        private float Linear()
        {
            return (float)_rnd.NextDouble();
        }
        /// <summary>
        /// Returns random numbers with easeinout probability
        /// </summary>
        private float EaseInEaseOut(int exp)
        {
            var x = Next();
            var val = (float) ((float)(Math.Pow(x,exp)) / (Math.Pow(x,exp) + Math.Pow(1-x,exp)));
            return val;
        }
        /// <summary>
        /// Returns random numbers with cubic probability
        /// </summary>
        private float Cubic()
        {
            var x = Next();
            var val = 6 * Math.Pow(x - .47f, 3) - (x*x)/2f + .62f;
            return (float) val;
        }
    }
}