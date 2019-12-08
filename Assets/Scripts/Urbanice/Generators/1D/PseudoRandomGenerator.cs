using UnityEngine;
using Urbanice.Generators._1D.Random;

namespace Urbanice.Generators._1D
{
    /// <summary>
    /// This class instantiates the pseudorandom number generator
    /// </summary>
    [CreateAssetMenu(menuName = "Urbanice/Generators/Values/Pseudo Random", fileName = "new Random Generator", order = 1)]
    public class PseudoRandomGenerator : FloatGenerator
    {
        public string Seed = "Urbanice";
        private PseudoRandom prng;

        public override void Init()
        {
            prng = new PseudoRandom(Seed);
        }
        public override float Generate()
        {
            return prng.Next();
        }
    }
}