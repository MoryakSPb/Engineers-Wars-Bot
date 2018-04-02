using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EW.ObjectModel
{
    [DataContract]
    public class MyFaction : AMyGameObject
    {
        [DataMember] public (TimeSpan start, TimeSpan finish) ActiveInterval;
        [DataMember] public bool Attack;
        [DataMember] public int BulidPoints;
        [DataMember] public MyResourses ChangesResourses;
        [DataMember] public int CurrentShipBuild;

        [DataMember] public FactionType FactionType;
        [DataMember] public (int MonolithCharges, int ShipSlots, int Production) MaxResourses;
        [DataMember] public ICollection<string> MInvitesNicks = new List<string>();
        [DataMember] public MyResourses Resourses;
        [DataMember] public MyResourses SavedResourses1;
        [DataMember] public MyResourses SavedResourses2;
        [DataMember] public ShipType? ShipBuild;
        [DataMember] public IDictionary<ShipType, int> Ships = SMyEconomyConsts.GetNewEmptyShipDictionary();
        [DataMember] public TradeShipStatus TradeShipStatus;
        [DataMember] public string VkUrl;

        [IgnoreDataMember]
        public override string Tag
        {
            get => MTag;
            set
            {
                if (value is null) throw new ArgumentNullException(nameof(value));
                if (value.Length == 3) MTag = value;
                else throw new ArgumentException("Некорректный тег", nameof(value));
            }
        }

        [IgnoreDataMember]
        public int TotalShipBuild => ShipBuild.HasValue ? SMyEconomyConsts.Ships[ShipBuild.Value].Cost.Production : 0;

        public MyFaction(string name, string tag, FactionType factionType, (TimeSpan start, TimeSpan finish) activeInterval) : base(name, tag)
        {
            FactionType = factionType;
            ActiveInterval = activeInterval;
        }

        public override int CompareTo(AMyGameObject other) => string.Compare(base.Tag, other.Tag, StringComparison.Ordinal);
    }


    /// <summary>
    ///     Перечисление, представляющие типы фракций
    /// </summary>
    public enum FactionType
    {
        /// <summary>
        ///     Военная
        /// </summary>
        Military,

        /// <summary>
        ///     Переселенческая
        /// </summary>
        Resettlement,

        /// <summary>
        ///     Исследовательская
        /// </summary>
        Research,

        /// <summary>
        ///     Индустриальная (строительная)
        /// </summary>
        Industrial,

        /// <summary>
        ///     Коммерчерская (торговая)
        /// </summary>
        Commercial,

        /// <summary>
        ///     Пиратская (бандитская)
        /// </summary>
        Pirate,

        /// <summary>
        ///     Авантюриская
        /// </summary>
        Adventurous
    }

    public enum TradeShipStatus
    {
        None,
        Started,
        InWay,
        Attacked
    }
}
