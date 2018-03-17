using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    [DataContract]
    internal class MyEventShipCompleted : AMyEvent
    {
        static public new DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(MyEventShipCompleted), MySave.SerializerSettings);

        public MyEventShipCompleted(MyFaction faction, ShipType ship) : base(false)
        {
            Faction = faction;
            Ship = ship;
        }

        internal override int[] Destination => MySave.Players.FindAll(x => x.AllowedMessages == MessagesType.All).Select(x => x.Vk).ToArray();
        [DataMember]
        public readonly MyFaction Faction;
        [DataMember]
        public readonly ShipType Ship;
        public override string ToString() => $"Фракция «{Faction.Name}» построила {MyStrings.GetShipNameOnce(Ship).ToLowerInvariant()}";
    }
}
