using System;
using System.Runtime.Serialization;

namespace EW.ObjectModel
{
    [Serializable]
    [DataContract]
    public class MyPlayer : AMyGameObject
    {
        [DataMember] public (TimeSpan, TimeSpan) Activity;

        [DataMember] public MessagesType AllowedMessages;

        [DataMember] protected bool MIsFactionLeader;

        /// <summary>
        ///     ID страницы ВКонтакте
        /// </summary>
        [DataMember]
        public int Vk { get; protected set; }

        /// <summary>
        ///     Steam64ID профиля игрока
        /// </summary>
        [DataMember]
        public ulong Steam { get; protected set; }

        /// <summary>
        ///     Статус игрока в проекте
        /// </summary>
        [DataMember]
        public PlayerStatus Status { get; set; }

        /// <summary>
        ///     Определяет возможность пользоватся командами, требуюие повышенные привилегии
        /// </summary>
        [DataMember]
        public bool IsAdmin { get; set; }

        /// <summary>
        ///     Определяет блокировку игрока
        /// </summary>
        [DataMember]
        public bool IsBanned { get; set; }

        /// <summary>
        ///     Определяет руководителя фракции
        /// </summary>
        [IgnoreDataMember]
        public bool IsFactionLeader
        {
            get => MIsFactionLeader && Status == PlayerStatus.FactionMember;
            set => MIsFactionLeader = value;
        }

        public MyPlayer(string name, int vk, ulong steam) : base(name, string.Empty)
        {
            Vk = vk;
            Steam = steam;
            Status = PlayerStatus.Guest;
            IsAdmin = false;
            IsBanned = false;
            MIsFactionLeader = false;
        }

        public MyPlayer(string name, string tag, int vk, ulong steam, PlayerStatus status, bool isAdmin, bool isBanned, bool isFactionLeader) : base(name, tag)
        {
            Vk = vk;
            Steam = steam;
            Status = status;
            IsAdmin = isAdmin;
            IsBanned = isBanned;
            MIsFactionLeader = isFactionLeader;
        }
    }


    /// <summary>
    ///     Перечисление, представляющие статус игрока
    /// </summary>
    public enum PlayerStatus
    {
        Guest,
        Mercenary,
        FactionMember
    }

    [Flags]
    public enum MessagesType : byte
    {
        None = 0,
        All = 1
    }
}
