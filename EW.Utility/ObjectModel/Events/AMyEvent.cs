using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    [DataContract]
    internal abstract class AMyEvent : IEquatable<AMyEvent>
    {
        [IgnoreDataMember]
        static public DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(AMyEvent), MySave.SerializerSettings);

        [DataMember]
        internal readonly DateTime Created = DateTime.UtcNow;
        [DataMember]
        internal bool Sended;

        protected AMyEvent(bool sended)
        {
            Sended = sended;
        }

        [DataMember]
        internal abstract int[] Destination { get; }

        public abstract override string ToString();
        public virtual void Send()
        {
            if (Sended) return;
            MyVkApi.LastApi.SendMessage(Destination, ToString(), GetHashCode(), string.Empty);
            Sended = true;
        }


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
