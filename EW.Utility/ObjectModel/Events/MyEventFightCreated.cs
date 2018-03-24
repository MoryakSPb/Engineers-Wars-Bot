using System;
using System.Globalization;
using System.Linq;
using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    internal class MyEventFightCreated : AMyEvent
    {
        internal readonly AMyFight Fight;

        protected override int[] Destination => MySave.Players.FindAll(x => x.AllowedMessages == MessagesType.All && (x.Tag == Fight.AttackersTag || x.Tag == Fight.DefendersTag)).Select(x => x.Vk).ToArray();
        public MyEventFightCreated(AMyFight fight) : base(false) => Fight = fight ?? throw new ArgumentNullException(nameof(fight));

        public override string ToString()
        {
            switch (Fight)
            {
                case MySectorFight _: return $"{Fight.StartTime.ToString(CultureInfo.CurrentUICulture)} произойдет битва фракций «{MySave.Factions.Find(x => x.Tag == Fight.AttackersTag)}» и «{MySave.Factions.Find(x => x.Tag == Fight.AttackersTag)}» за сектор {((MySectorFight) Fight).Sector}\r\nДля просмотра списка битв введите \"bot fights\"";
                case MyTradeShipFight _: return $"{Fight.StartTime.ToString(CultureInfo.CurrentUICulture)} произойдет битва фракций «{MySave.Factions.Find(x => x.Tag == Fight.AttackersTag)}» и «{MySave.Factions.Find(x => x.Tag == Fight.AttackersTag)}» за торговый корабль\r\nДля просмотра списка битв введите \"bot fights\"";
                default: return $"{Fight.StartTime.ToString(CultureInfo.CurrentUICulture)} произойдет битва фракций «{MySave.Factions.Find(x => x.Tag == Fight.AttackersTag)}» и «{MySave.Factions.Find(x => x.Tag == Fight.AttackersTag)}»\r\nДля просмотра списка битв введите \"bot fights\"";
            }
        }
    }
}
