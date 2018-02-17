using System;
using System.Collections.Generic;
using System.Linq;
using EW.ObjectModel;

namespace EW.Utility.Api
{
    internal sealed class MyBotFactionApi : MyBasicApi
    {
        private readonly MyFaction _faction;

        internal string Tag => _faction.Tag;

        internal MyBotFactionApi(int sender)
        {
            Sender = GetSender(sender);
            if (Sender is null) throw new ArgumentNullException("Игрок не зарегистрирован", nameof(sender));
            if (Sender.IsBanned) throw new ArgumentException("Игрок заблокирован", nameof(sender));
            _faction = MySave.Factions.Find(x => x.Tag == Sender.Tag);
            if (_faction is null || !Sender.IsFactionLeader)
                throw new InvalidOperationException("Игрок не состоит во фракции или не является ее лидером");
        }

        internal MyBotFactionApi(MyPlayer sender)
        {
            Sender = sender;
            if (Sender is null) throw new ArgumentNullException("Игрок не зарегистрирован", nameof(sender));
            if (Sender.IsBanned) throw new ArgumentException("Игрок заблокирован", nameof(sender));
            _faction = MySave.Factions.Find(x => x.Tag == Sender.Tag);
            if (_faction is null || !Sender.IsFactionLeader)
                throw new InvalidOperationException("Игрок не состоит во фракции или не является ее лидером");
        }

        internal List<string> GetTradeShips()
        {
            List<string> factions = new List<string>();
            foreach (MyFaction item in MySave.Factions)
                if (item.TradeShipStatus == TradeShipStatus.InWay)
                    factions.Add(item.Tag);
            return factions;
        }

        internal MyTradeShipAttackResult AttackTradeShip(string tag, DateTime time)
        {
            string factionTag = GetTradeShips().Find(x => x == tag);
            if (string.IsNullOrWhiteSpace(factionTag))
                throw new ArgumentNullException(nameof(factionTag), "Не найден торговый корабль заданной фракции");
            MyPolitic politic = MySave.Politics.Find(x => (x.Factions.Item1 == factionTag && x.Factions.Item2 == _faction.Tag) ^ (x.Factions.Item1 == _faction.Tag && x.Factions.Item2 == factionTag));
            if (politic is null)
                throw new NullReferenceException("Экземляр класса политики не найден. Проверьте целостность данных");
            if (politic.Status == MyPoliticStatus.War || _faction.FactionType == FactionType.Pirate)
            {
                MySave.Fights = MySave.Fights.Add(new MyTradeShipFight(_faction.Tag, factionTag, time));
                return MyTradeShipAttackResult.Ok;
            }

            return MyTradeShipAttackResult.NotInWar;
        }

        internal MyFaction Status() => _faction;

        internal bool GrindShip(ShipType ship, int count = 1)
        {
            if (_faction.Ships[ship] <= 0) return false;

            _faction.Ships[ship] -= count;
            _faction.Resourses += (SMyEconomyConsts.Ships[ship].Cost / 2 + SMyEconomyConsts.Ships[ship].Service) * count;
            return true;
        }

        internal MySetBuildResult SetBuild(ShipType type)
        {
            if (_faction.Resourses.Production >= SMyEconomyConsts.Ships[type].Cost.Production)
            {
                _faction.Resourses -= SMyEconomyConsts.Ships[type].Cost;
                ++_faction.Ships[type];
                return MySetBuildResult.Built;
            }

            if (!_faction.ShipBuild.HasValue)
            {
                _faction.ShipBuild = type;
                _faction.CurrentShipBuild = 0;
                return MySetBuildResult.Ok;
            }

            return MySetBuildResult.QueueIsBusy;
        }

        internal bool CancelBuild()
        {
            if (_faction.ShipBuild.HasValue)
            {
                _faction.Resourses += SMyEconomyConsts.Ships[_faction.ShipBuild.Value].Cost.Basic / 2;
                _faction.ShipBuild = null;
                return true;
            }

            return false;
        }

        internal bool StartTradeShip()
        {
            if (_faction.Attack)
            {
                _faction.TradeShipStatus = TradeShipStatus.Started;
                _faction.Attack = false;
                return true;
            }

            return false;
        }

        internal bool CancelTradeShip()
        {
            if (_faction.TradeShipStatus == TradeShipStatus.Started)
            {
                _faction.TradeShipStatus = TradeShipStatus.None;
                _faction.Attack = true;
            }

            return false;
        }

        internal List<MyOffer> Offers() => MySave.Offers.FindAll(x => (x.Factions.Item1 == _faction.Tag) ^ (x.Factions.Item2 == _faction.Tag) && x.Confirm.Item1 is null || x.Confirm.Item2 is null).ToList();

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
            if (_faction.BulidPoints <= 0) return MyBuildImprovementResult.NoPoint;
            if (sector.Tag != Tag) return MyBuildImprovementResult.NotOwner;
            var (buyable, cost, _) = SMyEconomyConsts.SectorImprovements[(impId, 1)];
            if (!buyable) return MyBuildImprovementResult.NotAvalable;
            if (!_faction.Resourses.CheckCost(cost)) return MyBuildImprovementResult.NoResourses;
            sector.Improvement = (impId, 1);
            _faction.Resourses -= cost;
            --_faction.BulidPoints;
            return 0;
        }

        internal MySectorUpdateResult UpgrateImprovement(MySector sector)
        {
            if (_faction.Tag != sector.Tag) return MySectorUpdateResult.NotOwner;
            if (sector.Improvement.Type == SectorImprovementType.None) return MySectorUpdateResult.EmptySector;
            if (!sector.UpgradeCost.HasValue) return MySectorUpdateResult.NotAvalable;
            if (!_faction.Resourses.CheckCost(sector.UpgradeCost.Value)) return MySectorUpdateResult.NoResourses;
            if (_faction.BulidPoints <= 0) return MySectorUpdateResult.NoPoints;
            _faction.Resourses -= sector.UpgradeCost.Value;
            sector.Improvement = (sector.Improvement.Type, sector.Improvement.Level + 1);
            --_faction.BulidPoints;
            return MySectorUpdateResult.Ok;
        }

        internal MyDestroyImprovementResult DestroyImprovement(MySector sector)
        {
            if (sector.Tag != Tag) return MyDestroyImprovementResult.NotOwner;
            if (sector.Improvement.Type == 0) return MyDestroyImprovementResult.EmptySector;
            var (buyable, cost, _) = SMyEconomyConsts.SectorImprovements[sector.Improvement];
            if (!buyable) return MyDestroyImprovementResult.NotAvalable;
            sector.Improvement = (SectorImprovementType.None, 0);
            _faction.Resourses += cost / 2;
            return 0;
        }

        internal MySectorGoResult Go(MySector sector)
        {
            if (!_faction.Attack) return MySectorGoResult.NoAttack;
            if (sector.Tag == Tag) return MySectorGoResult.YourSector;
            if (!string.IsNullOrWhiteSpace(sector.Tag)) return MySectorGoResult.OtherFaction;
            bool contact = false;
            foreach (string item in sector.Contacts)
            {
                contact = MySave.Sectors.Exists(x => x.Name == item);
                if (contact) break;
            }

            if (!contact) return MySectorGoResult.NoContacts;
            sector.Tag = Tag;
            _faction.Attack = false;
            return MySectorGoResult.Ok;
        }

        internal MySectorAttackResult Attack(MySector sector, TimeSpan time, out MySectorFight fight)
        {
            fight = null;
            switch (Go(sector))
            {
                case MySectorGoResult.Ok: return MySectorAttackResult.OkNoFight;
                case MySectorGoResult.YourSector: return MySectorAttackResult.YourSector;
                case MySectorGoResult.NoContacts: return MySectorAttackResult.NoContacts;
                case MySectorGoResult.NoAttack: return MySectorAttackResult.NoAttack;
                default:
                {
                    MyPolitic policits = MySave.Politics.Find(x => x.Factions.Item1 == _faction.Tag && (x.Factions.Item2 == sector.Tag) ^ (x.Factions.Item2 == _faction.Tag) && x.Factions.Item1 == sector.Tag) ?? throw new ArgumentException("Данные отношений не найдены", nameof(_faction));
                    if (policits.Status == MyPoliticStatus.War)
                    {
                        if (MySave.BotSettings.ActivityTime.Item1 <= time && MySave.BotSettings.ActivityTime.Item2 >= time)
                        {
                            if (_faction.ActiveInterval.start <= time && _faction.ActiveInterval.finish >= time)
                            {
                                MyFaction enemyFaction = MySave.Factions.Find(x => x.Tag == sector.Tag);
                                if (enemyFaction.ActiveInterval.start <= time && enemyFaction.ActiveInterval.finish >= time)
                                {
                                    DateTime date = DateTime.UtcNow.Date + time;
                                    fight = new MySectorFight(Tag, enemyFaction.Tag, date, sector.Name);
                                    MySave.Fights = MySave.Fights.Add(fight);
                                    return MySectorAttackResult.Ok;
                                }

                                return MySectorAttackResult.InvalidEnemyTime;
                            }

                            return MySectorAttackResult.InvalidYourTime;
                        }

                        return MySectorAttackResult.InvalidAdminTime;
                    }

                    return MySectorAttackResult.NoWar;
                }
            }
        }

        internal enum MyTradeShipAttackResult
        {
            Ok,
            OkNoFight,
            NotInWar = -1
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