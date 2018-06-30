using System;
using System.Runtime.Serialization;

namespace EW.ObjectModel
{
    [Serializable]
    [DataContract]
    abstract public class AMyGameObject : IComparable<AMyGameObject>
    {
        /// <summary>
        ///     Имя объекта
        /// </summary>
        [DataMember]
        readonly public string Name;

        [DataMember]
        protected string MTag;

        /// <summary>
        ///     Тег объекта
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Проверить аргументы или открытые методы", MessageId = "0")]
        [IgnoreDataMember]
        public virtual string Tag
        {
            get => MTag;
            set
            {
                if (value is null) throw new ArgumentNullException(nameof(value));
                if (value.Length == 3 || string.IsNullOrEmpty(value))
                    MTag = value;
                else
                    throw new ArgumentException("Некорректный тег", nameof(value));
            }
        }

        protected AMyGameObject(string name, string tag)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            MTag = tag ?? throw new ArgumentNullException(nameof(tag));
        }

        public bool Equals(AMyGameObject other) => Equals(this, other);

        public override int GetHashCode() => Name.GetHashCode();

        public override string ToString() => string.IsNullOrWhiteSpace(MTag) ? Name : MTag + "." + Name;

        public virtual int CompareTo(AMyGameObject other) => string.Compare(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
    }
}
