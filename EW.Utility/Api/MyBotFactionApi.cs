using System;
using System.Collections.Generic;
using System.Linq;
using EW.ObjectModel;

namespace EW.Utility.Api
{
    internal sealed class MyBotFactionApi : MyBasicApi
    {
        internal readonly MyFaction Faction;

        internal string Tag => Faction.Tag;

        internal MyBotFactionApi(int sender)
        {
            Sender = GetSender(sender);
            if (Sender is null) throw new ArgumentNullException("Игрок не зарегистрирован", nameof(sender));
            if (Sender.IsBanned) throw new ArgumentException("Игрок заблокирован", nameof(sender));
            Faction = MySave.Factions.Find(x => x.Tag == Sender.Tag);
            if (Faction is null || !Sender.IsFactionLeader)
                throw new InvalidOperationException("Игрок не состоит во фракции или не является ее лидером");
        }

        internal MyBotFactionApi(MyPlayer sender)
        {
            Sender = sender;
            if (Sender is null) throw new ArgumentNullException("Игрок не зарегистрирован", nameof(sender));
            if (Sender.IsBanned) throw new ArgumentException("Игрок заблокирован", nameof(sender));
            Faction = MySave.Factions.Find(x => x.Tag == Sender.Tag);
            if (Faction is null || !Sender.IsFactionLeader)
                throw new InvalidOperationException("Игрок не состоит во фракции или не является ее лидером");
        }

        static internal List<string> GetTradeShips()
        {
            List<string> factions = new List<string>();
            foreach (MyFaction item in MySave.Factions)
                if (item.TradeShipStatus == TradeShipStatus.InWay)
                    factions.Add(item.Tag);
            return factions;
        }

        internal MyTradeShipAttackResult AttackTradeShip(string tag, TimeSpan time, out MyTradeShipFight fight)
        {
            fight = null;
            if (!Faction.Attack) return MyTradeShipAttackResult.NoAttack;
            if (Faction.Tag == tag) return MyTradeShipAttackResult.YourShip;
            string factionTag = GetTradeShips().Find(x => x == tag);
            if (string.IsNullOrWhiteSpace(factionTag)) return MyTradeShipAttackResult.NotFound;
            MyFaction enemy = MySave.Factions.Find(x => x.Tag == tag);
            if (enemy is null) return MyTradeShipAttackResult.NotFound;
            MyPolitic policits = MySave.Politics.Find(x => x.Factions.Item1 == Faction.Tag && (x.Factions.Item2 == enemy.Tag) ^ (x.Factions.Item2 == Faction.Tag) && x.Factions.Item1 == enemy.Tag) ?? throw new ArgumentException("Данные отношений не найдены", nameof(Faction));
            if (!(policits.Status == MyPoliticStatus.War || Faction.FactionType == FactionType.Pirate)) return MyTradeShipAttackResult.NotInWar;

            if (MySave.BotSettings.ActivityTime.Item1 > time || MySave.BotSettings.ActivityTime.Item2 < time) return MyTradeShipAttackResult.InvalidAdminTime;
            if (Faction.ActiveInterval.start > time || Faction.ActiveInterval.finish < time) return MyTradeShipAttackResult.InvalidYourTime;
            if (enemy.ActiveInterval.start > time || enemy.ActiveInterval.finish < time) return MyTradeShipAttackResult.InvalidEnemyTime;

            if (enemy.Ships.Values.Sum() == 0)
            {
                enemy.TradeShipStatus = TradeShipStatus.None;
                Faction.Resourses %= Faction.FactionType == FactionType.Pirate ? 250 : 125;
                Faction.Attack = false;
                return MyTradeShipAttackResult.OkNoFight;
            }

            enemy.TradeShipStatus = TradeShipStatus.Attacked;
            DateTime date = DateTime.UtcNow.Date.AddDays(1.0) + time;
            if (date < DateTime.UtcNow) date = date.AddDays(1.0);
            fight = new MyTradeShipFight(Faction.Tag, enemy.Tag, date);
            MySave.Fights = MySave.Fights.Add(fight);
            return MyTradeShipAttackResult.Ok;
        }

        internal MyFaction Status() => Faction;

        internal bool GrindShip(ShipType ship, int count = 1)
        {
            if (Faction.Ships[ship] <= 0) return false;

            Faction.Ships[ship] -= count;
            Faction.Resourses += (SMyEconomyConsts.Ships[ship].Cost / 2 + SMyEconomyConsts.Ships[ship].Service) * count;
            return true;
        }

        internal MySetBuildResult SetBuild(ShipType type)
        {
            if (Faction.Resourses.Production >= SMyEconomyConsts.Ships[type].Cost.Production)
            {
                Faction.Resourses -= SMyEconomyConsts.Ships[type].Cost;
                ++Faction.Ships[type];
                return MySetBuildResult.Built;
            }

            if (!Faction.ShipBuild.HasValue)
            {
                Faction.ShipBuild = type;
                Faction.CurrentShipBuild = 0;
                return MySetBuildResult.Ok;
            }

            return MySetBuildResult.QueueIsBusy;
        }

        internal bool CancelBuild()
        {
            if (Faction.ShipBuild.HasValue)
            {
                Faction.Resourses += SMyEconomyConsts.Ships[Faction.ShipBuild.Value].Cost.Basic / 2;
                Faction.ShipBuild = null;
                return true;
            }

            return false;
        }

        internal bool StartTradeShip()
        {
            if (!(Faction.Attack || Faction.TradeShipStatus == TradeShipStatus.None)) return false;
            Faction.TradeShipStatus = TradeShipStatus.Started;
            Faction.Attack = false;
            return true;
        }

        internal bool CancelTradeShip()
        {
            if (Faction.TradeShipStatus == TradeShipStatus.Started)
            {
                Faction.TradeShipStatus = TradeShipStatus.None;
                Faction.Attack = true;
            }

            return false;
        }

        internal List<MyOffer> Offers() => MySave.Offers.FindAll(x => !x.Confirmed && (x.Factions.Item1 == Faction.Tag) ^ (x.Factions.Item2 == Faction.Tag)).ToList();

        internal MyOffer Offer(int index) => Offers().ElementAtOrDefault(index);

        internal bool SetOffer(int index, bool accept)
        {
            MyOffer offer = Offer(index);
            if (offer is null) return false;
            offer.Confirm = accept ? (true, true) : (false, false);
            return true;
        }

        internal MyBuildImprovementResult BuildImprovement(MySector sector, SectorImprovementType impId)
        {
            if (impId == SectorImprovementType.None) return MyBuildImprovementResult.UseDestroyImprovement;
            if (impId == SectorImprovementType.Headquarters) return MyBuildImprovementResult.NotAvalable;
            if (Faction.BulidPoints <= 0) return MyBuildImprovementResult.NoPoint;
            if (sector.Tag != Tag) return MyBuildImprovementResult.NotOwner;
            var (buyable, cost, _) = SMyEconomyConsts.SectorImprovements[(impId, 1)];
            if (!buyable) return MyBuildImprovementResult.NotAvalable;
            if (!Faction.Resourses.CheckCost(cost)) return MyBuildImprovementResult.NoResourses;
            sector.Improvement = (impId, 1);
            Faction.Resourses -= cost;
            --Faction.BulidPoints;
            return 0;
        }

        internal MySectorUpdateResult UpgrateImprovement(MySector sector)
        {
            if (Faction.Tag != sector.Tag) return MySectorUpdateResult.NotOwner;
            if (sector.Improvement.Type == SectorImprovementType.None) return MySectorUpdateResult.EmptySector;
            if (!sector.UpgradeCost.HasValue) return MySectorUpdateResult.NotAvalable;
            if (!Faction.Resourses.CheckCost(sector.UpgradeCost.Value)) return MySectorUpdateResult.NoResourses;
            if (Faction.BulidPoints <= 0) return MySectorUpdateResult.NoPoints;
            Faction.Resourses -= sector.UpgradeCost.Value;
            sector.Improvement = (sector.Improvement.Type, sector.Improvement.Level + 1);
            --Faction.BulidPoints;
            return MySectorUpdateResult.Ok;
        }

        internal MyDestroyImprovementResult DestroyImprovement(MySector sector)
        {
            if (sector.Tag != Tag) return MyDestroyImprovementResult.NotOwner;
            if (sector.Improvement.Type == 0) return MyDestroyImprovementResult.EmptySector;
            var (buyable, cost, _) = SMyEconomyConsts.SectorImprovements[sector.Improvement];
            if (!buyable) return MyDestroyImprovementResult.NotAvalable;
            sector.Improvement = (SectorImprovementType.None, 0);
            Faction.Resourses += cost / 2;
            return 0;
        }

        internal MySectorGoResult Go(MySector sector)
        {
            if (!Faction.Attack) return MySectorGoResult.NoAttack;
            if (sector.Tag == Tag) return MySectorGoResult.YourSector;
            if (!string.IsNullOrWhiteSpace(sector.Tag)) return MySectorGoResult.OtherFaction;
            bool contact = false;
            foreach (string item in sector.Contacts)
            {
                contact = MySave.Sectors.Exists(x => x.Name == item);
                if (contact) break;
            }

            contact |= sector.SectorType == SectorType.Monolith && MySave.Sectors.Exists(x => x.Tag == Faction.Tag && sector.SectorType == SectorType.Monolith);
            if (!contact) return MySectorGoResult.NoContacts;
            sector.Tag = Tag;
            Faction.Attack = false;
            return MySectorGoResult.Ok;
        }

        internal MySectorAttackResult Attack(MySector sector, TimeSpan time, out MySectorFight fight)
        {
            fight = null;
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (Go(sector))
            {
                case MySectorGoResult.Ok: return MySectorAttackResult.OkNoFight;
                case MySectorGoResult.YourSector: return MySectorAttackResult.YourSector;
                case MySectorGoResult.NoContacts: return MySectorAttackResult.NoContacts;
                case MySectorGoResult.NoAttack: return MySectorAttackResult.NoAttack;
                default:
                {
                    MyPolitic policits = MySave.Politics.Find(x => x.Factions.Item1 == Faction.Tag && (x.Factions.Item2 == sector.Tag) ^ (x.Factions.Item2 == Faction.Tag) && x.Factions.Item1 == sector.Tag) ?? throw new ArgumentException("Данные отношений не найдены", nameof(Faction));
                    if (policits.Status != MyPoliticStatus.War) return MySectorAttackResult.NoWar;
                    if (MySave.BotSettings.ActivityTime.Item1 > time || MySave.BotSettings.ActivityTime.Item2 < time) return MySectorAttackResult.InvalidAdminTime;
                    if (Faction.ActiveInterval.start > time || Faction.ActiveInterval.finish < time) return MySectorAttackResult.InvalidYourTime;
                    MyFaction enemyFaction = MySave.Factions.Find(x => x.Tag == sector.Tag);
                    if (enemyFaction.ActiveInterval.start > time || enemyFaction.ActiveInterval.finish < time) return MySectorAttackResult.InvalidEnemyTime;
                    DateTime date = DateTime.UtcNow.Date.AddDays(1.0) + time;
                    if (date < DateTime.UtcNow) date = date.AddDays(1.0);
                    fight = new MySectorFight(Tag, enemyFaction.Tag, date, sector.Name);
                    MySave.Fights = MySave.Fights.Add(fight);
                    return MySectorAttackResult.Ok;
                }
            }
        }

        internal enum MyTradeShipAttackResult
        {
            Ok,
            OkNoFight,
            NotInWar = -1,
            YourShip = -2,
            NoAttack = -3,
            NotFound = -4,
            InvalidYourTime = -5,
            InvalidEnemyTime = -6,
            InvalidAdminTime = -7
        }

        internal enum MySetBuildResult
        {
            Ok,
            Built,
            QueueIsBusy = -1
        }

        internal enum MyBuildImprovementResult
        {
            Ok,
            NoResourses = -1,
            NotOwner = -2,
            NoPoint = -3,
            NotAvalable = -4,
            UseDestroyImprovement = -5,
            SectorImproved = -6
        }

        internal enum MyDestroyImprovementResult
        {
            Ok,
            EmptySector = -1,
            NotOwner = -2,
            NotAvalable = -4
        }

        internal enum MySectorGoResult
        {
            Ok,
            YourSector = -1,
            NoContacts = -2,
            OtherFaction = -3,
            NoAttack = -4
        }

        internal enum MySectorAttackResult
        {
            Ok,
            OkNoFight,
            YourSector = -1,
            NoContacts = -2,
            NoAttack = -3,
            NoWar = -4,
            InvalidYourTime = -5,
            InvalidEnemyTime = -6,
            InvalidAdminTime = -7
        }

        internal enum MySectorUpdateResult
        {
            Ok,
            EmptySector = -1,
            NotOwner = -2,
            NoResourses = -3,
            NotAvalable = -4,
            NoPoints = -5
        }
    }
}
