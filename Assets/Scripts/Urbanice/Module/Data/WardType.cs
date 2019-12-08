namespace Urbanice.Module.Data
{
    public enum WardType
    {
        Invalid = 0,
        Any = 1,
        // Market types
        Market = 10,
        Chappel = 11,
        Cathedral = 12,
        Merchant = 15,
        TownHall = 16,
        // Military types
        Castle = 20,

        // Craftsmen types
        Saddler = 31,
        Goldshmith = 32,
        // Highclass types
        Patrician = 21,
        Weaver = 33,
        Carpenter = 34,
        Mason = 35,
        Baker = 36,
        // Craftsmen types
        // Farmland types
        Farm = 60,
        Farmfields = 61,
        Pasture = 62,
        // HeavyIndustry types / 70
        // Slum types / 100
        Common = 100,
        Begger = 101,
        Graveyard = 102,
    }
}