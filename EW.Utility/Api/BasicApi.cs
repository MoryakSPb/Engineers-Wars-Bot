using EW.ObjectModel;

namespace EW.Utility.Api
{
    internal abstract class MyBasicApi
    {
        protected MyPlayer Sender;

        static internal MyPlayer GetSender(int vk) => MySave.Players.Find(x => x.Vk == vk);

        static internal bool CheckName(string name) => MySave.Factions.Exists(x => x.Name == name) || MySave.Players.Exists(x => x.Name == name) || MySave.Sectors.Exists(x => x.Name == name) || MySave.Scripts.Exists(x => x.Name == name) || MySave.Timers.Exists(x => x.Name == name);
    }
}
