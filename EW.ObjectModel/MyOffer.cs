using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EW.ObjectModel
{
    [Serializable]
    [DataContract]
    [KnownType(typeof(MyTradeResourses))]
    [KnownType(typeof(ShipType))]
    [KnownType(typeof(bool?))]
    [KnownType(typeof(List<string>))]
    public class MyOffer
    {
        [DataMember] public readonly DateTime CreateTime = DateTime.UtcNow;

        [DataMember] public readonly bool Creator;

        [DataMember] public readonly (string, string) Factions;

        [DataMember] public readonly MyOfferType OfferType;

        [DataMember] public readonly MyOfferOptions Options;
        [DataMember] public readonly int PactTurns;
        [DataMember] public bool Confirmed;

        [DataMember] public (MyTradeResourses, MyTradeResourses) Deal;

        [DataMember]
        public (bool?, bool?) Confirm { get; set; }

        public MyOffer((string, string) factions, bool creator, (bool?, bool?) confirm, MyOfferType offerType, MyOfferOptions options, (MyTradeResourses, MyTradeResourses) deal, int pactTurns)
        {
            Factions = factions;
            Creator = creator;
            Confirm = confirm;
            OfferType = offerType;
            Options = options;
            if (OfferType == MyOfferType.WarToNeutral || OfferType == MyOfferType.NeutralToAlly || OfferType == MyOfferType.Default) Deal = deal;
            else Deal = default;
            if (Options == MyOfferOptions.CreatePact) PactTurns = pactTurns;
            if (OfferType != MyOfferType.NeutralToWar && OfferType != MyOfferType.AllyToNeutral) return;
            Confirm = (true, true);
        }

        public MyOffer((string, string) factions, bool creator, (bool?, bool?) confirm, MyOfferType offerType, MyOfferOptions options, int pactTurns, (int, int, int, int, int, int) resourses1, ICollection<string> sectors1, IDictionary<ShipType, int> ships1, (int, int, int, int, int, int) resourses2, ICollection<string> sectors2, IDictionary<ShipType, int> ships2) : this(factions, creator, confirm, offerType, options, (new MyTradeResourses(new MyResourses(resourses1), sectors1, ships1), new MyTradeResourses(new MyResourses(resourses2), sectors2, ships2)), pactTurns)
        {
        }
    }

    public enum MyOfferType
    {
        Default,
        WarToNeutral,
        NeutralToAlly,
        AllyToNeutral,
        NeutralToWar
    }

    public enum MyOfferOptions
    {
        Trade,
        CreatePact,
        ChangeUnion
    }
}
