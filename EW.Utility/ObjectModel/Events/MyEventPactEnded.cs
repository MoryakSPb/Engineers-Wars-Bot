using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    public class MyEventPactEnded : AMyEvent
    {
        internal readonly MyPolitic Politic;

        public MyEventPactEnded(MyPolitic politic) : base(false) => Politic = politic;

        public override string ToString() => 
            $"Закончился мирный договор между фракциями «{MySave.Factions.Find(x=> x.Tag == Politic.Factions.Item1).Name}» и «{MySave.Factions.Find(x => x.Tag == Politic.Factions.Item2).Name}»";
    }
}
