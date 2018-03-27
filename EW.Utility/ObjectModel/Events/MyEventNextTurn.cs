using System.Globalization;
using System.Linq;
using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    internal class MyEventNextTurn : AMyEvent
    {
        internal MyEventNextTurn() : base(false){}

        protected override int[] Destination => MySave.Players.Where(x => x.AllowedMessages == MessagesType.All && x.Status != PlayerStatus.Guest).Select(x => x.Vk).ToArray();

        public override string ToString() => "Ход завершен";
    }
}
