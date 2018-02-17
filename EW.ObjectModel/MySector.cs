using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EW.ObjectModel
{
    [DataContract]
    public class MySector : AMyGameObject
    {
        [DataMember] public List<string> Contacts;

        [DataMember]
        public SectorType SectorType { get; set; }

        [DataMember]
        public (SectorImprovementType Type, int Level) Improvement { get; set; }

        [IgnoreDataMember]
        public MyResourses Service => SMyEconomyConsts.Sectors[SectorType].Service + SMyEconomyConsts.SectorImprovements[Improvement].Service;

        [IgnoreDataMember]
        public MyResourses? UpgradeCost => SMyEconomyConsts.SectorImprovements.ContainsKey((Improvement.Type, Improvement.Level + 1)) && SMyEconomyConsts.SectorImprovements[(Improvement.Type, Improvement.Level + 1)].buyable ? SMyEconomyConsts.SectorImprovements[(Improvement.Type, Improvement.Level + 1)].Cost : (MyResourses?) null;

        [IgnoreDataMember]
        public bool Improvementable => SectorType != SectorType.Monolith && Improvement.Type != SectorImprovementType.Headquarters;

        public MySector(string name, string tag, SectorType sectorType, (SectorImprovementType Type, int Level) improvement, IEnumerable<string> contacts) : base(name, tag)
        {
            SectorType = sectorType;
            Improvement = improvement;
            Contacts = contacts.ToList() ?? throw new ArgumentNullException(nameof(contacts));
        }

        public MySector(string name, SectorType sectorType, IEnumerable<string> contacts) : base(name, string.Empty)
        {
            SectorType = sectorType;
            Improvement = (SectorImprovementType.None, 0);
            Contacts = contacts.ToList() ?? throw new ArgumentNullException(nameof(contacts));
        }
    }
}