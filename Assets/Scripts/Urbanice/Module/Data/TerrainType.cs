using System;

namespace Urbanice.Module.Data
{
    [Flags]
    public enum TerrainType
    {
        Plain = 0,
        Hilly = 1,
        Hilltop = 4,
        River = 8,
        Ford = 24,        // Ford requires, Flags River and Ford to be set
        Lake = 32,
        Sea = 64,
    }
    
}