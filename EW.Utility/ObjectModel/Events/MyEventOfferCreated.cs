using System;
using System.Linq;
using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    public class MyEventOfferCreated : AMyEvent
    {
        public MyEventOfferCreated(MyOffer offer) : base(false) => Offer = offer;
        protected override int[] Destination => MySave.Players.Where(x => x.IsAdmin || x.IsFactionLeader && x.Tag == Offer.Factions.Item1 ^ x.Tag == Offer.Factions.Item2).Select(x => x.Vk).ToArray();
        public readonly MyOffer Offer;
        private MyFaction Faction1 => MySave.Factions.Find(x => x.Tag == Offer.Factions.Item1);
        private MyFaction Faction2 => MySave.Factions.Find(x => x.Tag == Offer.Factions.Item2);
        public override string ToString() => !(Offer.Confirm.Item1 == true && Offer.Confirm.Item1 == true) ? (Offer.Creator ? $"Фракция «{Faction2.Name}» отправила договор фракции «{Faction1.Name}»" : $"Фракция «{Faction1.Name}» отправила договор фракции «{Faction2.Name}»") : string.Empty;
    }
}
