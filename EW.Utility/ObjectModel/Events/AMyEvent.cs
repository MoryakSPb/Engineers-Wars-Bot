using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    [DataContract]
    abstract public class AMyEvent : IEquatable<AMyEvent>
    {
        [IgnoreDataMember]
        static public DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(AMyEvent), MySave.SerializerSettings);

        [DataMember]
        readonly internal DateTime Created = DateTime.UtcNow;

        [DataMember]
        internal bool Sended;

        [DataMember]
        protected virtual int[] Destination => MySave.Players.FindAll(x => x.AllowedMessages == MessagesType.All).Select(x => x.Vk).ToArray();

        protected AMyEvent(bool sended) => Sended = sended;

        abstract public override string ToString();

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
