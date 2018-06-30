using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    public class MyEventImprovementBuilded : AMyEvent
    {
        readonly internal SectorImprovementType Improvement;
        readonly internal MySector Sector;

        public MyEventImprovementBuilded(MySector sector, SectorImprovementType improvement) : base(false)
        {
            Sector = sector;
            Improvement = improvement;
        }

        public override string ToString() => Improvement != SectorImprovementType.None ? $"Фракция «{MySave.Factions.Find(x => x.Tag == Sector.Tag).Name}» построила в секторе «{Sector.Name}» улучшение \"{MyStrings.GetSectorImprovementType(Improvement)}\"" : $"В секторе «{Sector.Name}» теперь нет улучшения сектора";
    }
}
