using System;
using System.Collections.Generic;
using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    internal abstract class AMyEvent : IEquatable<AMyEvent>
    {
        internal readonly DateTime Created = DateTime.UtcNow;
        internal bool Sended;
        internal abstract int[] Destination { get; }

        public abstract override string ToString();
        public abstract void Send();

        public override bool Equals(object obj)
        {
            return Equals(obj as AMyEvent);
        }

        public bool Equals(AMyEvent other)
        {
            return other != null &&
                   Created == other.Created &&
                   Sended == other.Sended &&
                   EqualityComparer<int[]>.Default.Equals(Destination, other.Destination);
        }

        public override int GetHashCode()
        {
            var hashCode = -2047919448;
            hashCode = hashCode * -1521134295 + Created.GetHashCode();
            hashCode = hashCode * -1521134295 + Sended.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<int[]>.Default.GetHashCode(Destination);
            return hashCode;
        }

        public static bool operator ==(AMyEvent event1, AMyEvent event2)
        {
            return EqualityComparer<AMyEvent>.Default.Equals(event1, event2);
        }

        public static bool operator !=(AMyEvent event1, AMyEvent event2)
        {
            return !(event1 == event2);
        }
    }


}
