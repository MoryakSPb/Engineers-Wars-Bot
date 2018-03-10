using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EW.ObjectModel
{
    [Serializable]
    [DataContract]
    [KnownType(typeof(ShipType))]
    [KnownType(typeof(string))]
    public class MyTradeResourses
    {
        [DataMember] public readonly ICollection<string> Sectors;

        [DataMember] public readonly IDictionary<ShipType, int> Ships;
        [DataMember] public MyResourses Resourses;

        public MyTradeResourses(MyResourses resourses, ICollection<string> sectors, IDictionary<ShipType, int> ships)
        {
            Resourses = resourses;
            Sectors = sectors.ToList() ?? throw new ArgumentNullException(nameof(sectors));
            Ships = ships ?? throw new ArgumentNullException(nameof(ships));
        }
    }
}
