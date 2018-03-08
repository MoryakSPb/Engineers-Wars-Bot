using System;
using System.Runtime.Serialization;

namespace EW.ObjectModel
{
    [Serializable]
    [DataContract]
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

        public MyOffer((string, string) factions, bool creator, (bool, bool) confirm, MyOfferType offerType, MyOfferOptions options, (MyTradeResourses, MyTradeResourses) deal, int pactTurns)
        {
            Factions = factions;
            Creator = creator;
            Confirm = confirm;
            OfferType = offerType;
            Options = options;
            if (OfferType == MyOfferType.WarToNeutral || OfferType == MyOfferType.NeutralToAlly || OfferType == MyOfferType.Default) Deal = deal;
            else Deal = default;
            PactTurns = pactTurns;
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