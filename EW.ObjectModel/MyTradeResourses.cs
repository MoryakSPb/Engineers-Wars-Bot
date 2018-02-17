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

        [DataMember] public ICollection<string> Sectors;

        [DataMember] public IDictionary<ShipType, int> Ships;

        public MyTradeResourses(MyResourses resourses, ICollection<string> sectors, IDictionary<ShipType, int> ships)
        {
            Resourses = resourses;
            Sectors = sectors ?? throw new ArgumentNullException(nameof(sectors));
            Ships = ships ?? throw new ArgumentNullException(nameof(ships));
        }
    }
}