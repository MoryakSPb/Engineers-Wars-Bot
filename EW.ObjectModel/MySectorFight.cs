using System;
using System.Runtime.Serialization;

namespace EW.ObjectModel
{
    [DataContract]
    public class MySectorFight : AMyFight
    {
        [DataMember] public readonly string Sector;

        public bool ImprovementDestroyed { get; set; }

        public MySectorFight(string attackersTag, string defendersTag, DateTime startTime, string sectorName) : base(attackersTag, defendersTag, startTime) => Sector = sectorName;
    }
}
