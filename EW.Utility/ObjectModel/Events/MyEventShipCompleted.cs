using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    [DataContract]
    internal class MyEventShipCompleted : AMyEvent
    {
        static public new DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(MyEventShipCompleted), MySave.SerializerSettings);

        [DataMember] public readonly MyFaction Faction;

        [DataMember] public readonly ShipType Ship;

        protected override int[] Destination => MySave.Players.FindAll(x => x.AllowedMessages == MessagesType.All).Select(x => x.Vk).ToArray();

        public MyEventShipCompleted(MyFaction faction, ShipType ship) : base(false)
        {
            Faction = faction;
            Ship = ship;
        }

        public override string ToString() => $"Фракция «{Faction.Name}» построила {MyStrings.GetShipNameOnce(Ship).ToLowerInvariant()}";
    }
}
