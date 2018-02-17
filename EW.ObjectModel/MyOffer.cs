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

        [DataMember] public (MyTradeResourses, MyTradeResourses) Deal;

        [DataMember]
        public (bool?, bool?) Confirm { get; set; }

        public MyOffer((string, string) factions, bool creator, (bool, bool) confirm, MyOfferType offerType, MyOfferOptions options, (MyTradeResourses, MyTradeResourses) deal)
        {
            Factions = factions;
            Creator = creator;
            Confirm = confirm;
            OfferType = offerType;
            Options = options;
            Deal = deal;
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