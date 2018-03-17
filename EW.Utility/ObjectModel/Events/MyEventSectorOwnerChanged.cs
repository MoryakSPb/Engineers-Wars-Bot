using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    class MyEventSectorOwnerChanged : AMyEvent
    {
        internal override int[] Destination => MySave.Players.FindAll(x => x.AllowedMessages == MessagesType.All).Select(x => x.Vk).ToArray();
        internal readonly MyFaction OldOwner;
        internal readonly MyFaction NewOwner;
        internal readonly MySector Sector;
        private readonly ReasonEnum _reason;

        public MyEventSectorOwnerChanged(MyFaction oldOwner, MyFaction newOwner, MySector sector, ReasonEnum reason) : base(false)
        {
            OldOwner = oldOwner;
            NewOwner = newOwner ?? throw new ArgumentNullException(nameof(newOwner));
            Sector = sector ?? throw new ArgumentNullException(nameof(sector));
            _reason = reason;
        }

        public ReasonEnum Reason => OldOwner is null ? ReasonEnum.Nobody : _reason;


        public override string ToString()
        {
            switch (Reason)
            {
                case ReasonEnum.Default: throw new ArgumentOutOfRangeException();
                case ReasonEnum.Fight: return $"Фракция «{NewOwner.Name}» отбила в бою сектор «{Sector.Name}» у фракции «{OldOwner.Name}»";
                case ReasonEnum.Offer: return $"Фракция «{NewOwner.Name}» получила сектор «{Sector.Name}» в результате договора с фракцией «{OldOwner.Name}»";
                case ReasonEnum.Nobody: return $"Фракция «{NewOwner.Name}» заняла ничейный сектор «{Sector.Name}»";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public override bool Equals(object obj)
        {
            MyEventSectorOwnerChanged changed = obj as MyEventSectorOwnerChanged;
            return changed != null &&
                   EqualityComparer<int[]>.Default.Equals(Destination, changed.Destination) &&
                   EqualityComparer<MyFaction>.Default.Equals(OldOwner, changed.OldOwner) &&
                   EqualityComparer<MyFaction>.Default.Equals(NewOwner, changed.NewOwner) &&
                   EqualityComparer<MySector>.Default.Equals(Sector, changed.Sector) &&
                   Reason == changed.Reason;
        }

        public override int GetHashCode()
        {
            int hashCode = -709297640;
            hashCode = hashCode * -1521134295 + EqualityComparer<int[]>.Default.GetHashCode(Destination);
            hashCode = hashCode * -1521134295 + EqualityComparer<MyFaction>.Default.GetHashCode(OldOwner);
            hashCode = hashCode * -1521134295 + EqualityComparer<MyFaction>.Default.GetHashCode(NewOwner);
            hashCode = hashCode * -1521134295 + EqualityComparer<MySector>.Default.GetHashCode(Sector);
            hashCode = hashCode * -1521134295 + Reason.GetHashCode();
            return hashCode;
        }

        internal enum ReasonEnum
        {
            Default,
            Fight,
            Offer,
            Nobody = -1,
        }

        public static bool operator ==(MyEventSectorOwnerChanged changed1, MyEventSectorOwnerChanged changed2)
        {
            return EqualityComparer<MyEventSectorOwnerChanged>.Default.Equals(changed1, changed2);
        }

        public static bool operator !=(MyEventSectorOwnerChanged changed1, MyEventSectorOwnerChanged changed2)
        {
            return !(changed1 == changed2);
        }
    }
}
