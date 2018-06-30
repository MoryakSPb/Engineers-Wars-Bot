using System.Linq;
using EW.ObjectModel;

namespace EW.Utility.ObjectModel.Events
{
    internal class MyEventNextTurn : AMyEvent
    {
        protected override int[] Destination => MySave.Players.Where(x => x.AllowedMessages == MessagesType.All && x.Status != PlayerStatus.Guest).Select(x => x.Vk).ToArray();

        internal MyEventNextTurn() : base(false) { }

        public override string ToString() => "==========\r\nХод завершен\r\n==========";
    }
}
