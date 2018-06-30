using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EW.ObjectModel
{
    [Serializable]
    [DataContract]
    abstract public class AMyFight : IComparable<AMyFight>
    {
        [DataMember]
        public int AttackersMercSlots;

        [DataMember]
        public List<string> AttackersPlayers = new List<string>();

        [DataMember]
        public Dictionary<ShipType, int> AttackersСasualties = SMyEconomyConsts.GetNewEmptyShipDictionary();

        [DataMember]
        public int DefendersMercSlots;

        [DataMember]
        public List<string> DefendersPlayers = new List<string>();

        [DataMember]
        public Dictionary<ShipType, int> DefendersСasualties = SMyEconomyConsts.GetNewEmptyShipDictionary();

        [DataMember]
        protected string MAttackersTag;

        [DataMember]
        protected string MDefendersTag;

        [IgnoreDataMember]
        public string AttackersTag
        {
            get => MAttackersTag;
            set
            {
                if (value is null) throw new ArgumentNullException(nameof(value));
                if (value.Length == 3 || string.IsNullOrEmpty(value))
                    MAttackersTag = value;
                else
                    throw new ArgumentException("Некорректный тег", nameof(value));
            }
        }

        public string DefendersTag
        {
            get => MDefendersTag;
            set
            {
                if (value is null) throw new ArgumentNullException(nameof(value));
                if (value.Length == 3 || string.IsNullOrEmpty(value))
                    MDefendersTag = value;
                else
                    throw new ArgumentException("Некорректный тег", nameof(value));
            }
        }

        [DataMember]
        public bool ResultRegistered { get; set; }

        [DataMember]
        public FightResult Result { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        internal AMyFight(string attackersTag, string defendersTag, DateTime startTime)
        {
            AttackersTag = attackersTag;
            DefendersTag = defendersTag;
            StartTime = startTime;
        }

        public virtual int CompareTo(AMyFight other) => StartTime.CompareTo(other.StartTime);
    }

    public enum FightResult
    {
        NoResult,
        AttackersWin,
        DefendersWin,
        Stalemate
    }
}
