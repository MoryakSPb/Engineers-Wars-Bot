using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    class MyEventRelationsChanged : AMyEvent
    {
        internal override int[] Destination => MySave.Players.FindAll(x => x.AllowedMessages == MessagesType.All).Select(x => x.Vk).ToArray();
        internal readonly MyFaction Faction1;
        internal readonly MyFaction Faction2;
        internal readonly MyOfferType Relations;

        public MyEventRelationsChanged(MyFaction faction1, MyFaction faction2, MyOfferType relations) : base(false)
        {
            Faction1 = faction1 ?? throw new ArgumentNullException(nameof(faction1));
            Faction2 = faction2 ?? throw new ArgumentNullException(nameof(faction2));
            Relations = relations;
        }

        public override string ToString()
        {
            switch (Relations)
            {
                case MyOfferType.WarToNeutral:
                    return $"Фракции «{Faction1.Name}» и «{Faction2.Name}» заключили мирный договор";
                case MyOfferType.NeutralToAlly:
                    return $"Фракции «{Faction1.Name}» и «{Faction2.Name}» перешли к сотрудничеству";
                case MyOfferType.AllyToNeutral:
                    return $"Фракции «{Faction1.Name}» и «{Faction2.Name}» больше не сотрудничают";
                case MyOfferType.NeutralToWar:
                    return $"Фракции «{Faction1.Name}» и «{Faction2.Name}» теперь находятся в состоянии войны";
                default: throw new ArgumentException(nameof(Relations));
            }
        }
    }
}
