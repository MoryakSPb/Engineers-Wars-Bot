using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    public class MyEventImprovementUpgrated : MyEventImprovementBuilded
    {
        public MyEventImprovementUpgrated(MySector sector, SectorImprovementType improvement) : base(sector, improvement) { }

        public override string ToString() => $"Фракция «{MySave.Factions.Find(x => x.Tag == Sector.Tag).Name}» повысила уровень улучшения \"{MyStrings.GetSectorImprovementType(Improvement)}\" в секторе «{Sector.Name}» до уровня {Sector.Improvement.Level}";
    }
}
