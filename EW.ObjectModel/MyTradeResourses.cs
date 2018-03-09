using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EW.ObjectModel
{
    [Serializable]
    [DataContract]
    public class MyTradeResourses
    {
        [DataMember] public MyResourses Resourses;

        [DataMember] public readonly IReadOnlyCollection<string> Sectors;

        [DataMember] public readonly IReadOnlyDictionary<ShipType, int> Ships;

        public MyTradeResourses(MyResourses resourses, ICollection<string> sectors, IDictionary<ShipType, int> ships)
        {
            Resourses = resourses;
            Sectors = (IReadOnlyCollection<string>)sectors ?? throw new ArgumentNullException(nameof(sectors));
            Ships = (IReadOnlyDictionary<ShipType, int>)ships ?? throw new ArgumentNullException(nameof(ships));
        }
    }
}