using System.Collections.Generic;

namespace EW.ObjectModel
{
    static public class SMyEconomyConsts
    {
        static readonly public IReadOnlyDictionary<ShipType, (MyResourses Cost, MyResourses Service)> Ships = new Dictionary<ShipType, (MyResourses Cost, MyResourses Service)>(2)
        {
            {
                ShipType.Fighter, (new MyResourses(125, 25, 15, 0, 0, 1), new MyResourses(-20, -1, -2, 0, -1, 0))
            },
            {
                ShipType.Corvette, (new MyResourses(950, 80, 70, 0, 0, 5), new MyResourses(-100, -10, -8, 0, -5, 0))
            }
        };

        static readonly public IReadOnlyDictionary<SectorType, (MyResourses Service, bool Improvementable)> Sectors = new Dictionary<SectorType, (MyResourses Service, bool Improvementable)>(2)
        {
            {
                SectorType.Default, (new MyResourses(150, 0, 0, 0, 0, 0), true)
            },
            {
                SectorType.Monolith, (new MyResourses(0, 0, 0, 1, 0, 0), false)
            }
        };

        static readonly public IReadOnlyDictionary<(SectorImprovementType Type, int Level), (bool buyable, MyResourses Cost, MyResourses Service)> SectorImprovements = new Dictionary<(SectorImprovementType Type, int Level), (bool buyable, MyResourses Cost, MyResourses Service )>(20)
        {
            {(SectorImprovementType.None, 0), (false, new MyResourses(), new MyResourses())},
            {
                (SectorImprovementType.Headquarters, 1), (false, new MyResourses(), new MyResourses(250, 15, 15, 0, 10, 2))
            },
            {
                (SectorImprovementType.Mine, 1), (true, new MyResourses(250, 15, 0, 0, 0, 0), new MyResourses(150, -5, 0, 0, 0, 0))
            },
            {
                (SectorImprovementType.Mine, 2), (true, new MyResourses(350, 20, 0, 0, 0, 0), new MyResourses(250, -10, 0, 0, 0, 0))
            },
            {
                (SectorImprovementType.Mine, 3), (true, new MyResourses(500, 15, 0, 1, 0, 0), new MyResourses(250, -20, 0, 0, 0, 0))
            },
            {
                (SectorImprovementType.Factory, 1), (true, new MyResourses(350, 5, 0, 0, 0, 0), new MyResourses(-150, -5, 0, 0, 0, 1))
            },
            {
                (SectorImprovementType.Factory, 2), (true, new MyResourses(350, 5, 0, 0, 0, 0), new MyResourses(-250, -10, 0, 0, 0, 3))
            },
            {
                (SectorImprovementType.Factory, 3), (true, new MyResourses(350, 5, 0, 1, 0, 0), new MyResourses(-250, -20, 0, 0, 0, 5))
            },
            {
                (SectorImprovementType.Storage, 1), (true, new MyResourses(400, 15, 5, 0, 0, 0), new MyResourses(-100, -5, 15, 0, 0, 0))
            },
            {
                (SectorImprovementType.Storage, 2), (true, new MyResourses(600, 20, 5, 0, 0, 0), new MyResourses(-125, -5, 20, 0, 0, 0))
            },
            {
                (SectorImprovementType.Storage, 3), (true, new MyResourses(900, 10, 5, 1, 0, 0), new MyResourses(-250, -10, 30, 0, 0, 0))
            },
            {
                (SectorImprovementType.Hangar, 1), (true, new MyResourses(400, 15, 15, 0, 0, 0), new MyResourses(-50, -5, -5, 0, 1, 0))
            },
            {
                (SectorImprovementType.Hangar, 2), (true, new MyResourses(600, 25, 25, 0, 0, 0), new MyResourses(-100, -10, -5, 0, 3, -1))
            },
            {
                (SectorImprovementType.Hangar, 3), (true, new MyResourses(900, 20, 20, 1, 0, 0), new MyResourses(-125, -15, -5, 0, 5, -2))
            },
            {
                (SectorImprovementType.Powerstation, 1), (true, new MyResourses(350, 0, 0, 0, 0, 0), new MyResourses(-125, 25, 0, 0, 0, 0))
            },
            {
                (SectorImprovementType.Powerstation, 2), (true, new MyResourses(450, 0, 0, 0, 0, 0), new MyResourses(-175, 35, 0, 0, 0, 0))
            },
            {
                (SectorImprovementType.Powerstation, 3), (true, new MyResourses(550, 0, 0, 1, 0, 0), new MyResourses(-235, 45, 0, 0, 0, 0))
            },
            {
                (SectorImprovementType.Outpost, 1), (true, new MyResourses(450, 15, 5, 0, 0, 0), new MyResourses(-150, -5, -5, 0, 0, 0))
            },
            {
                (SectorImprovementType.Outpost, 2), (true, new MyResourses(600, 15, 15, 0, 0, 0), new MyResourses(-250, -10, -10, 0, 0, 0))
            },
            {
                (SectorImprovementType.Outpost, 3), (true, new MyResourses(800, 15, 25, 1, 0, 0), new MyResourses(-300, -10, -10, 0, 0, 0))
            }
        };

        static public Dictionary<ShipType, int> GetNewEmptyShipDictionary()
        {
            Dictionary<ShipType, int> dict = new Dictionary<ShipType, int>(Ships.Count);
            foreach (KeyValuePair<ShipType, (MyResourses Cost, MyResourses Service)> item in Ships) dict.Add(item.Key, 0);
            return dict;
        }
    }

    public enum ShipType
    {
        Fighter = 0,
        Corvette = 3
    }

    public enum SectorType
    {
        Default,
        Monolith
    }

    public enum SectorImprovementType
    {
        None,
        Headquarters,
        Mine,
        Factory,
        Storage,
        Hangar,
        Powerstation,
        Outpost
    }
}
