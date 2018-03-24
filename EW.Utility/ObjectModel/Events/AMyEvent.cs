using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace EW.Utility.ObjectModel.Events
{
    [DataContract]
    internal abstract class AMyEvent : IEquatable<AMyEvent>
    {
        [IgnoreDataMember] static public DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(AMyEvent), MySave.SerializerSettings);

        [DataMember] internal readonly DateTime Created = DateTime.UtcNow;

        [DataMember] internal bool Sended;

        [DataMember]
        protected abstract int[] Destination { get; }

        protected AMyEvent(bool sended) => Sended = sended;

        public abstract override string ToString();

        public virtual void Send()
        {
            if (Sended) return;
            MyVkApi.LastApi.SendMessage(Destination, ToString(), GetHashCode(), string.Empty);
            Sended = true;
        }


        public override bool Equals(object obj) => Equals(obj as AMyEvent);

        public override int GetHashCode()
        {
            int hashCode = -2047919448;
            hashCode = hashCode * -1521134295 + Created.GetHashCode();
            hashCode = hashCode * -1521134295 + Sended.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<int[]>.Default.GetHashCode(Destination);
            return hashCode;
        }

        static public bool operator ==(AMyEvent event1, AMyEvent event2) => EqualityComparer<AMyEvent>.Default.Equals(event1, event2);

        static public bool operator !=(AMyEvent event1, AMyEvent event2) => !(event1 == event2);

        public bool Equals(AMyEvent other) => other != null && Created == other.Created && Sended == other.Sended && EqualityComparer<int[]>.Default.Equals(Destination, other.Destination);
    }
}
