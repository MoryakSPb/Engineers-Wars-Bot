using System;
using System.Collections.Generic;
using System.Linq;
using EW.ObjectModel;

namespace EW.Utility.Api
{
    internal sealed class MyBotApi : MyBasicApi
    {
        internal MyBotApi(int sender)
        {
            Sender = Player(sender);
            if (Sender is null) throw new ArgumentNullException("Игрок не зарегистрирован", nameof(sender));
            if (Sender.IsBanned) throw new ArgumentException("Игрок заблокирован", nameof(sender));
        }

        internal MyBotApi(MyPlayer sender)
        {
            Sender = sender;
            if (Sender is null) throw new ArgumentNullException("Игрок не зарегистрирован", nameof(sender));
            if (Sender.IsBanned) throw new ArgumentException("Игрок заблокирован", nameof(sender));
        }

        internal IEnumerable<string> Players() => MySave.Players.Select(x => x.Name);

        internal IEnumerable<string> Factions() => MySave.Factions.Select(x => x.Name);

        internal IEnumerable<string> Sectors() => MySave.Sectors.Select(x => x.Name);

        internal IEnumerable<AMyFight> Fights() => MySave.Fights.Where(x => x.StartTime >= DateTime.UtcNow && !x.ResultRegistered);

        internal IEnumerable<AMyFight> AllFights() => MySave.Fights.AsEnumerable();

        internal MyPlayer Player(string name) => MySave.Players.Find(x => x.Name == name);

        internal MyPlayer Player(int vk) => MySave.Players.Find(x => x.Vk == vk);

        internal MyPlayer Player(ulong steam64) => MySave.Players.Find(x => x.Steam == steam64);

        internal MyPlayer Status() => Sender;

        internal MyFaction Faction()
        {
            if (string.IsNullOrWhiteSpace(Sender.Tag)) return null;
            return Faction(Sender.Tag);
        }

        internal MyFaction Faction(string tag) => MySave.Factions.Find(x => x.Tag == tag);

        internal MySector Sector(string name) => MySave.Sectors.Find(x => x.Name == name);

        internal BotJoinResult Join(int fightIndex, int team)
        {
            AMyFight fight;
            List<MyPlayer> attackers = new List<MyPlayer>();
            List<MyPlayer> defenders = new List<MyPlayer>();
            try
            {
                fight = AllFights().ToList()[fightIndex];
            }
            catch (IndexOutOfRangeException)
            {
                return BotJoinResult.InvalidIndex;
            }

            fight.AttackersPlayers.ForEach(item => attackers.Add(MySave.Players.Find(x => x.Name == item)));
            fight.DefendersPlayers.ForEach(item => defenders.Add(MySave.Players.Find(x => x.Name == item)));
            if (attackers.Contains(Sender) || defenders.Contains(Sender)) return BotJoinResult.Joined;
            switch (Sender.Status)
            {
                case PlayerStatus.Guest: return BotJoinResult.Guest;
                case PlayerStatus.Mercenary:
                {
                    if (team == 0)
                    {
                        int attackersMercCount = attackers.Count(x => x.Status == PlayerStatus.Mercenary);
                        if (attackersMercCount < fight.AttackersMercSlots)
                        {
                            fight.AttackersPlayers.Add(Sender.Name);
                            return BotJoinResult.Ok;
                        }

                        return BotJoinResult.NoMercSlots;
                    }

                    if (team == 1)
                    {
                        int defendersMercCount = defenders.Count(x => x.Status == PlayerStatus.Mercenary);
                        if (defendersMercCount < fight.DefendersMercSlots)
                        {
                            fight.DefendersPlayers.Add(Sender.Name);
                            return BotJoinResult.Ok;
                        }

                        return BotJoinResult.NoMercSlots;
                    }

                    return BotJoinResult.InvalidTeam;
                }
                case PlayerStatus.FactionMember:
                {
                    if (fight.AttackersTag == Sender.Tag)
                    {
                        fight.AttackersPlayers.Add(Sender.Name);
                        return BotJoinResult.Ok;
                    }

                    if (fight.DefendersTag == Sender.Tag)
                    {
                        fight.DefendersPlayers.Add(Sender.Name);
                        return BotJoinResult.Ok;
                    }

                    return BotJoinResult.NonYourFaction;
                }
                default: return BotJoinResult.Error;
            }
        }

        internal BotLeaveResult Leave(int fightIndex)
        {
            AMyFight fight;
            List<MyPlayer> attackers = new List<MyPlayer>();
            List<MyPlayer> defenders = new List<MyPlayer>();
            try
            {
                fight = ((List<AMyFight>) AllFights())[fightIndex];
            }
            catch (IndexOutOfRangeException)
            {
                return BotLeaveResult.InvalidIndex;
            }

            fight.AttackersPlayers.ForEach(item => attackers.Add(MySave.Players.Find(x => x.Name == item)));
            fight.DefendersPlayers.ForEach(item => defenders.Add(MySave.Players.Find(x => x.Name == item)));
            if (attackers.Remove(Sender)) return BotLeaveResult.Ok;
            if (defenders.Remove(Sender)) return BotLeaveResult.Ok;
            return BotLeaveResult.NotFinded;
        }

        internal void SetActivity((TimeSpan, TimeSpan) interval) => Sender.Activity = interval;

        internal enum BotJoinResult
        {
            Ok,
            Guest = -1,
            NoMercSlots = -2,
            InvalidIndex = -3,
            InvalidTeam = -4,
            Joined = -5,
            NonYourFaction = -6,
            Error = int.MinValue
        }

        internal enum BotLeaveResult
        {
            Ok,
            InvalidIndex = -1,
            NotFinded = -2,
            Error = int.MinValue
        }
    }
}
