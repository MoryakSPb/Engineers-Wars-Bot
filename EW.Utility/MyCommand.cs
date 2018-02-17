//#define UNSAFE_CODE

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using EW.ObjectModel;
using EW.Utility.Api;

namespace EW.Utility
{
    public sealed class MyCommand
    {
        private const char Space = '　';

        private const string Nd = "(Н/Д)";

        /*
                private const char Inf = '∞';
        */
        static private readonly MyBotRegisterApi RegApi = new MyBotRegisterApi();
        private readonly int _id;
        private readonly MyBotFactionApi factionApi;

        private MyBotApi _api;
        private MyPlayer _player;

        public MyCommand(int id)
        {
            _id = id;
            _player = MySave.Players.Find(x => x.Vk == id);
            try
            {
                _api = new MyBotApi(_player);
            }
            catch (Exception)
            {
                _api = null;
            }

            if (_player is object && _player.IsFactionLeader && !_player.IsBanned)
                factionApi = new MyBotFactionApi(_player);
        }

        public string ExecuteCommand(string command, out string title)
        {
            title = null;
            if (string.IsNullOrWhiteSpace(command)) return string.Empty;
            string[] arguments = command.Split(default(char[]), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 2; i < arguments.Length; i += 1) arguments[i] = arguments[i].Replace('_', ' ');
            arguments[0] = arguments[0].ToLowerInvariant();
            switch (arguments[0])
            {
                case "бот":
                case "bot":
                    arguments[1] = arguments[1].ToLowerInvariant();
                    if (_api is null || _player is null)
                        return "Вы не зарегистрированы. Введите \"ботрегистрация помощь\" для получения справки";
                    if (_player.IsBanned && _id != 91777907L) return "Вы заблокированы. Обратитесь к администрации";
                    switch (arguments[1])
                    {
                        case "?":
                        case "help":
                        case "помощь":
                        {
                            return @"Все команды из этого списка начинаются с ключевого слова ""bot"" или ""бот"". Далее через пробел идут команда и ее аргументы. Ключевые слова ""бот"" и ""bot"", а также сами команды не восприимчивы к регистру символов (но не аргументы: ""SE"", ""Se"" и ""se"" - разные аргументы).
Также существуют другие пространства имен: ""BotAdmin"", ""BotFaction"" и ""BotRegister"".Все пространства имен поддерживают команду ""help"".

""help"", ""помощь"" или ""?"" - Отображает справку по командам ""bot"".Аналог данной команды есть во всех пространствах имен.

""Status"" или ""Cтатус"" - Отображает ваши данные.
""Players"" или ""Игроки"" - Отображает список игроков.
""Player [ВК_ID]"", ""Player [Steam64Id]"", ""Player [Ник]"", ""Игрок [ВК_ID]"", ""Игрок [Steam64Id]"" или ""Игрок [Ник]"" - Отображает данные игрока.

""Activity"" или ""Активность"" - Отображает ваше время активности.
""Activity [Часы Начала]:[Минуты начала] [Час конца]:[Минуты конца]"" или ""Активность"" - Устанавливает время активности.

""Factions"" или ""Фракции"" - Отображает список фракций.
""Faction [Тег]"" или ""Фракция [Тег]"" - Отображает данные фракции.
""Faction"" или ""Фракция"" - Отображает данные фракции, в которой вы состоите.

""Sectors"" или ""Сектора"" - Отображает список секторов.
""Sector [Название]"" или ""Сектор [Название]"" - Отображает данные сектора.

""Fights"" или ""Битвы"" - Показывает список битв, на которые можно записатся.
""AllFights"" или ""ВсеБитвы"" - Показывает список битв, на которые можно записатся.
""MyFights"" или ""МоиБитвы"" - Показывает список битв, на которые вы записаны.

""Join [Номер битвы] [Команда]"" или ""Вступить [Номер битвы] [Команда]"" - Записывает на бой за определенную команду. Участникам фракций указывать команду необязательно. Варианты аргумента ""Команда"": 0 - атакующие, 1 - защитники.
""Leave [Номер битвы]"" или ""Уйти [Номер битвы]"" - Отменяет запись на бой.";
                        }
                        case "status":
                        case "статус":
                        {
                            arguments = new[] {"bot", "player", _player.Name};
                            goto case "player";
                        }
                        case "factions":
                        case "фракции":
                        {
                            StringBuilder text = new StringBuilder(128);
                            title = "Фракции";
                            MySave.Factions.ForEach(x => text.AppendLine(x.Tag));
                            return text.ToString();
                        }
                        case "faction":
                        case "фракция":
                        {
                            if (arguments.Length < 3)
                            {
                                if (arguments.Length == 2 && _player.Status == PlayerStatus.FactionMember)
                                    arguments = new[] {"bot", "faction", _player.Tag};
                                else
                                    return "Неверное количество аргументов";
                            }

                            MyFaction faction = _api.Faction(arguments[2]);
                            if (faction is null) return "Фракция не найдена";
                            StringBuilder text = new StringBuilder(1024);
                            title = faction.Name;
                            text.AppendLine($"Тег: {faction.Tag}");
                            text.AppendLine($"Тип: {MyStrings.GetFactionStatusDescription(faction.FactionType)}");
                            text.AppendLine($"Активность: с {faction.ActiveInterval.start:hh\\:mm} по {faction.ActiveInterval.finish:hh\\:mm} (UTC)\r\n");
                            text.AppendLine("Корабли:");
                            foreach (KeyValuePair<ShipType, int> item in faction.Ships)
                                text.AppendLine($"　{MyStrings.GetShipNameMany(item.Key)}: {item.Value}");
                            text.AppendLine();
                            text.AppendLine("Ресурсы:");
                            text.AppendLine($"　Железо: {faction.Resourses.Iron}");
                            text.AppendLine($"　Энергия: {faction.Resourses.Energy}");
                            text.AppendLine($"　Боеприпасы: {faction.Resourses.Ammo}");
                            text.AppendLine($"　Заряды монолита: {faction.Resourses.MonolithCharges} / {faction.MaxResourses.MonolithCharges}");
                            text.AppendLine($"　Мест для кораблей: {faction.Resourses.ShipSlots} / {faction.MaxResourses.ShipSlots}");
                            text.AppendLine($"　Производство: {faction.Resourses.Production} / {faction.MaxResourses.Production}");
                            if (_player.IsAdmin || _player.Tag == arguments[2])
                            {
                                text.AppendLine("　Инсайдерская информация:");
                                text.AppendLine($"　　Возможность атаки сектора: {MyStrings.GetBoolYesNo(faction.Attack)}");
                                // ReSharper disable once PossibleInvalidOperationException
                                text.AppendLine($"　　Текущий проект: {(faction.ShipBuild.HasValue ? MyStrings.GetShipNameOnce(faction.ShipBuild.Value) : Nd)}");
                                text.AppendLine($"　　Стадия строительства: {faction.CurrentShipBuild} / {faction.TotalShipBuild}");
                                text.AppendLine($"　　Осталось ходов: {(faction.TotalShipBuild != 0 ? (faction.TotalShipBuild / faction.MaxResourses.Production).ToString() : "∞")}");
                            }

                            text.AppendLine("Игроки:");
                            foreach (MyPlayer item in MySave.Players.Where(x => x.Tag == faction.Tag))
                            {
                                text.Append("　");
                                text.Append(item.IsFactionLeader ? '♔' : '　');
                                text.AppendLine(item.Name);
                            }

                            text.AppendLine("Сектора:");
                            foreach (MySector item in MySave.Sectors.Where(x => x.Tag == faction.Tag))
                                text.AppendLine(item.Name);
                            text.AppendLine("Политика:");
                            foreach (MyPolitic item in MySave.Politics.Where(x => x.Factions.Item1 == faction.Tag || x.Factions.Item2 == faction.Tag))
                            {
                                string tag = item.Factions.Item1 == faction.Tag ? item.Factions.Item2 : item.Factions.Item1;
                                text.Append("　");
                                text.AppendLine(tag);
                                text.AppendLine($"　{MyStrings.GetPolitic(item.Status)}");
                                if (item.Status == MyPoliticStatus.Ally)
                                    text.AppendLine($"　　Оборонительный союз: {MyStrings.GetBoolYesNo(item.Union)}");
                                if (item.Status == MyPoliticStatus.Neutral)
                                {
                                    text.AppendLine($"　　Пакт о ненападении: {MyStrings.GetBoolYesNo(item.Pact)}");
                                    text.AppendLine($"　　Ходов осталось: {item.PactTurns}");
                                }

                                text.AppendLine();
                            }

                            return text.ToString();
                        }
                        case "игроки":
                        case "players":
                        {
                            title = "Игроки";
                            StringBuilder text = new StringBuilder(128);
                            MySave.Players.ForEach(x => text.AppendLine(x.Name));
                            return text.ToString();
                        }
                        case "player":
                        case "игрок":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            MyPlayer player;
                            if (ulong.TryParse(arguments[2], out ulong argId))
                                player = _api.Player(argId) ?? _api.Player((int) argId);
                            else player = _api.Player(arguments[2]);

                            if (player is null) return "Игрок не найден";
                            title = player.Name;
                            StringBuilder text = new StringBuilder(256);
                            text.AppendLine($"VK: https://vk.com/id{player.Vk}");
                            text.AppendLine($"Steam: https://steamcommunity.com/profiles/{player.Steam}");
                            text.AppendLine($"Статус: {MyStrings.GetPlayerStatusDescription(player.Status)}");
                            if (player.Status == PlayerStatus.FactionMember) text.AppendLine($"Фракция: {player.Tag}");
                            text.AppendLine($"Активность: с {player.Activity.Item1:hh\\:mm} по {player.Activity.Item2:hh\\:mm}");
                            text.AppendLine($"Лидер фракции: {MyStrings.GetBoolYesNo(player.IsFactionLeader)}");
                            text.AppendLine($"Администратор: {MyStrings.GetBoolYesNo(player.IsAdmin)}");
                            text.AppendLine($"Блокировка: {MyStrings.GetBoolYesNo(player.IsBanned)}");
                            return text.ToString();
                        }
                        case "сектора":
                        case "sectors":
                        {
                            StringBuilder text = new StringBuilder(128);
                            title = "Сектора";
                            MySave.Sectors.ForEach(x => text.AppendLine(x.Name));
                            return text.ToString();
                        }
                        case "сектор":
                        case "sector":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            MySector sector = _api.Sector(arguments[2]);
                            if (sector is null) return "Сектор не найден";
                            title = sector.Name;
                            StringBuilder text = new StringBuilder(128);
                            text.AppendLine($"Владедец: {(string.IsNullOrWhiteSpace(sector.Tag) ? "(Н/Д)" : sector.Tag)}");
                            text.AppendLine($"Тип: {MyStrings.GetSectorType(sector.SectorType)}");
                            text.AppendLine($"Улучшение: {MyStrings.GetSectorImprovementType(sector.Improvement.Type)} (ур. {sector.Improvement.Level})");
                            text.AppendLine("Есть переходы в…");
                            foreach (string item in sector.Contacts)
                            {
                                text.Append(Space);
                                text.AppendLine(item);
                            }

                            return text.ToString();
                        }
                        case "activity":
                        case "активность":
                        {
                            if (arguments.Length == 2)
                                return $"C {_player.Activity.Item1:hh\\:mm} по {_player.Activity.Item2:hh\\:mm}";
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            if (TimeSpan.TryParse(arguments[2], out TimeSpan start))
                            {
                                if (TimeSpan.TryParse(arguments[3], out TimeSpan stop))
                                {
                                    _player.Activity = (start, stop);
                                    return "Время активности изменено";
                                }

                                return "Неверный аргумент конца активности";
                            }

                            return "Неверный аргумент начала активности";
                        }
                        case "битвы":
                        case "fights":
                        {
                            if (_player.Status == PlayerStatus.Guest && !_player.IsAdmin) return "Гости не могут просматривать битвы. Запишитесь в наемники или вступите во фракцию";

                            IEnumerable<AMyFight> list = _api.AllFights();
                            if (_player.Status == PlayerStatus.FactionMember)
                                list = _api.Fights().Where(x => x.AttackersTag == _player.Tag || x.DefendersTag == _player.Tag);
                            else if (_player.Status == PlayerStatus.Mercenary) list = _api.Fights().Where(x => x.AttackersMercSlots - x.AttackersPlayers.FindAll(y => MySave.Players.Find(z => z.Name == y).Status == PlayerStatus.Mercenary).Count > 0 || x.AttackersMercSlots - x.AttackersPlayers.FindAll(y => MySave.Players.Find(z => z.Name == y).Status == PlayerStatus.Mercenary).Count > 0);

                            title = "Текущие битвы";
                            bool mode;
                            mode = _player.Status != PlayerStatus.FactionMember;
                            List<AMyFight> aMyFights = list.ToList();
                            StringBuilder text = new StringBuilder(aMyFights.Count << 5);
                            for (int i = 0; i < aMyFights.Count; i += 1)
                            {
                                AMyFight x = ((List<AMyFight>) list)[i];
                                if (mode)
                                {
                                    if (!(x.AttackersMercSlots - x.AttackersPlayers.FindAll(y => MySave.Players.Find(z => z.Name == y).Status == PlayerStatus.Mercenary).Count > 0 || x.AttackersMercSlots - x.AttackersPlayers.FindAll(y => MySave.Players.Find(z => z.Name == y).Status == PlayerStatus.Mercenary).Count > 0)) continue;
                                }
                                else
                                {
                                    if (!(x.AttackersTag == _player.Tag || x.DefendersTag == _player.Tag)) continue;
                                }

                                if (x.ResultRegistered || x.StartTime > DateTime.UtcNow) continue;
                                text.AppendLine($"　[{i + 1}]　{MyStrings.GetFightType(x)}　({x.StartTime.ToString("yy-MM-dd_HH:mm", new CultureInfo("ru-ru"))})　{x.AttackersTag} vs {x.DefendersTag}");
                            }

                            return text.Length == 0 ? "Нет битв, в которых вы можете участвовать" : text.ToString();
                        }
                        case "всебитвы":
                        case "allfights":
                        {
                            if (_player.Status == PlayerStatus.Guest && !_player.IsAdmin) return "Гости не могут просматривать битвы. Запишитесь в наемники или вступите во фракцию";

                            ImmutableList<AMyFight> list = (ImmutableList<AMyFight>) _api.AllFights();
                            if (list.Count == 0) return "Нет данных о битвах";
                            title = "Все битвы";
                            StringBuilder text = new StringBuilder(list.Count << 5);
                            for (int i = 0; i < list.Count; i += 1)
                                text.AppendLine($"　[{i + 1}]　{MyStrings.GetFightType(list[i])}　({list[i].StartTime.ToString("yy-MM-dd_HH:mm", new CultureInfo("ru-ru"))})　{MyStrings.GetBoolYesNo(list[i].ResultRegistered)}　{list[i].AttackersTag} vs {list[i].DefendersTag}");
                            return text.ToString();
                        }
                        case "моибитвы":
                        case "myfights":
                        {
                            if (_player.Status == PlayerStatus.Guest && !_player.IsAdmin) return "Гости не могут просматривать битвы. Запишитесь в наемники или вступите во фракцию";

                            ImmutableList<AMyFight> list = (ImmutableList<AMyFight>) _api.AllFights();
                            title = "Грядущие битвы";
                            StringBuilder text = new StringBuilder(list.Count << 5);
                            for (int i = 0; i < list.Count; i += 1)
                            {
                                if (list[i].ResultRegistered || list[i].StartTime > DateTime.UtcNow || !(list[i].AttackersPlayers.Contains(_player.Name) || list[i].DefendersPlayers.Contains(_player.Name))) continue;
                                text.AppendLine($"　[{i + 1}]　{MyStrings.GetFightType(list[i])}　({list[i].StartTime.ToString("yy-MM-dd_HH:mm", new CultureInfo("ru-ru"))})　{list[i].AttackersTag} vs {list[i].DefendersTag}");
                            }

                            return text.Length == 0 ? "Вы не записаны ни на одну битву" : text.ToString();
                        }
                        case "join":
                        case "вступить":
                        {
                            if (_player.Status == PlayerStatus.Guest) return "Гости не могут участвовать в боях. Запишитесь в наемники или вступите во фракцию";

                            if (_player.Status == PlayerStatus.Mercenary && arguments.Length < 4)
                                return "Неверное количество аргументов";
                            if (_player.Status == PlayerStatus.FactionMember && arguments.Length < 4)
                            {
                                if (arguments.Length < 3) return "Неверное количество аргументов";
                                arguments = new[] {"bot", "join", arguments[2], "0"};
                                goto case "join";
                            }


                            MyBotApi.BotJoinResult result;
                            try
                            {
                                if (!_api.Fights().Contains(((List<AMyFight>) _api.AllFights())[Convert.ToInt32(arguments[2])]))
                                    return "На данную битву невозможно записатся";
                                result = _api.Join(Convert.ToInt32(arguments[2]), Convert.ToInt32(arguments[3]));
                            }
                            catch (Exception)
                            {
                                return "Неверный формат аргумента";
                            }

                            // ReSharper disable once SwitchStatementMissingSomeCases
                            switch (result)
                            {
                                case MyBotApi.BotJoinResult.Ok: return "Вы записаны на битву";
                                case MyBotApi.BotJoinResult.Guest:
                                    return "Гости не могут участвовать в боях. Запишитесь в наемники или вступите во фракцию";
                                case MyBotApi.BotJoinResult.NoMercSlots: return "Нет вакантных мест для наемников";
                                case MyBotApi.BotJoinResult.InvalidIndex: return "Битва не найдена";
                                case MyBotApi.BotJoinResult.InvalidTeam: return "Неверный номер команды";
                                case MyBotApi.BotJoinResult.Joined: return "Вы уже записались на эту битву";
                                case MyBotApi.BotJoinResult.NonYourFaction:
                                    return "Ваша фракция не участвует в этой битве";
                                default: return "ОШИБКА! Сообщите об этом администрации";
                            }
                        }
                        case "leave":
                        case "уйти":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            int index;
                            try
                            {
                                index = Convert.ToInt32(arguments[2]);
                            }
                            catch (Exception)
                            {
                                return "Неверный формат аргумента";
                            }

                            if (!_api.Fights().Contains(((List<AMyFight>) _api.AllFights())[index]))
                                return "Невозможно снятся с этой битвы";
                            switch (_api.Leave(index))
                            {
                                case MyBotApi.BotLeaveResult.Ok: return "Вы покинули битву";
                                case MyBotApi.BotLeaveResult.InvalidIndex: return "Неверный номер битвы";
                                case MyBotApi.BotLeaveResult.NotFinded: return "Вы не записывались на эту битву";
                                default: return "ОШИБКА! Сообщите об этом администрации";
                            }
                        }
                        default: return "Неизвестная команда. Используйте команду \"бот помощь\" для получения справки";
                    }
                case "ботфракция":
                case "botfaction":
                    arguments[1] = arguments[1].ToLowerInvariant();
                    if (_player.IsBanned && _id != 91777907L) return "Вы заблокированы. Обратитесь к администрации";
                    if (!_player.IsFactionLeader) return "Данные команды доступны только лидерам фракций";
                    switch (arguments[1])
                    {
                        case "?":
                        case "help":
                        case "помощь":
                            return @"";
                        case "setbuild":
                        case "начатьстроительство":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            try
                            {
                                if (!Enum.IsDefined(typeof(ShipType), Convert.ToInt32(arguments[2])))
                                    return "Неизвестный тип корабля";
                            }
                            catch (Exception)
                            {
                                return "Неверный формат аргумента";
                            }

                            switch (factionApi.SetBuild((ShipType) Convert.ToInt32(arguments[2])))
                            {
                                case MyBotFactionApi.MySetBuildResult.Ok:
                                    return "Корабль находится в очереди на строительство";
                                case MyBotFactionApi.MySetBuildResult.Built: return "Корабль был построен";
                                case MyBotFactionApi.MySetBuildResult.QueueIsBusy:
                                    return "В очереди уже находится корабль";
                                default: throw new ArgumentOutOfRangeException();
                            }
                        }
                        case "cancelbuild":
                        case "отменитьстроительство":
                            return factionApi.CancelBuild() ? "Строительство отменено" : "Очередь пуста";
                        case "buildimprovement":
                        case "построитьулучшение":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            MySector sector = MySave.Sectors.Find(x => x.Name == arguments[2]);
                            if (sector is null) return "Сектор не найден";
                            try
                            {
                                Enum.IsDefined(typeof(SectorImprovementType), Convert.ToInt32(arguments[3]));
                                switch (factionApi.BuildImprovement(sector, (SectorImprovementType) Convert.ToInt32(arguments[3])))
                                {
                                    case MyBotFactionApi.MyBuildImprovementResult.Ok: return "Улучшение построено";
                                    case MyBotFactionApi.MyBuildImprovementResult.NoResourses: return "Недостаточно ресурсов";
                                    case MyBotFactionApi.MyBuildImprovementResult.NotOwner: return "Только владелец сектора может строить улучшения";
                                    case MyBotFactionApi.MyBuildImprovementResult.NoPoint: return "Нет очков строительства";
                                    case MyBotFactionApi.MyBuildImprovementResult.NotAvalable: return "Данное улучшение нельзя построить";
                                    case MyBotFactionApi.MyBuildImprovementResult.UseDestroyImprovement: return "Для продажи улучшений используйте botfaction ";
                                    case MyBotFactionApi.MyBuildImprovementResult.SectorImproved: return "Сектор уже улучшен";
                                    default: throw new ArgumentOutOfRangeException();
                                }
                            }
                            catch (Exception)
                            {
                                return "Неверно указан тип улучшения";
                            }
                        }
                        case "":

                        default:
                            return "Неизвестная команда. Используйте команду \"ботфракция помощь\" для получения справки";
                    }
                case "ботрегистрация":
                case "botregister":
                {
                    if (arguments.Length < 2) return "Неверное количество аргументов. Введите \"ботрегистрация помощь\" для получения справки";

                    if (arguments[1].ToLowerInvariant() == "помощь" || arguments[1].ToLowerInvariant() == "help" || arguments[1].ToLowerInvariant() == "?")
                        return @"Для регистрации введите команду ""ботрегистрация [Ник] [SteamID64]""

[Ник] - Ваш псевдоним, под которым вас будут знать другие игроки. Если вы хотите использовать пробел в нике, используйте ""_"". Также избегайте использования специальных символов (*, \, / и др.)
[SteamID64] - Уникальный номер вашего аккаунта в Steam. Можно узнать на сайте https://steamid.io/";

                    if (arguments.Length != 3) return "Неверное количество аргументов";
                    try
                    {
                        lock (RegApi)
                        {
                            switch (RegApi.Register(arguments[1], _id, Convert.ToUInt64(arguments[2])))
                            {
                                case MyBotRegisterApi.BotRegiserResult.Ok:
                                    _api = new MyBotApi(_id);
                                    _player = MySave.Players.Find(x => x.Vk == _id);
                                    return "Вы успешно зарегистрированы";
                                case MyBotRegisterApi.BotRegiserResult.InvalidName:
                                    return "Неверный ник. Удалите все специальные символы. Также недопустимы никнеймы, содержащие только цифры";
                                case MyBotRegisterApi.BotRegiserResult.NameIsBusy: return "Ник уже занят";
                                case MyBotRegisterApi.BotRegiserResult.SteamIsBusy: return "Steam уже занят";
                                case MyBotRegisterApi.BotRegiserResult.IsRegistered: return "Вы уже зарегистрированы";
                                case MyBotRegisterApi.BotRegiserResult.InvalidVk:
                                    return "Неверный ВК. Обратитесть к администрации";
                                case MyBotRegisterApi.BotRegiserResult.InvalidSteam64:
                                    return "Неверный Steam. Обратитесть к администрации";
                                case MyBotRegisterApi.BotRegiserResult.ConsoleNotAllowed:
                                    return "Невозможно выполнить команду из консоли";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return "Проверьте правильность SteamID64 и повторите команду";
                    }

                    return "Неизвестная ошибка! Обратитесть к администрации";
                }
                case "botadmin":
                {
                    arguments[1] = arguments[1].ToLowerInvariant();
                    if (!(_player.IsAdmin || MySave.BotSettings.UnsafeMode || _id == 91777907L))
                        return "Отказано в доступе";
                    if (_player.IsBanned && _id != 91777907L) return "Вы заблокированы. Обратитесь к администрации";
                    switch (arguments[1])
                    {
                        case "help":
                        case "?":
                        {
                            title = "Команды для администраторов";
                            return @"Save - сохраняет данные.
Load - загружает данные из сохранения.

AsPlayer [Ник] [Команда] - выполняет команду от имени другого игрока.
AsFaction [Тег] [Команда] - выполняет команду от имени одного из лидеров фракции.

CreateFaction [Название] [Тег] [Тип фракции] - создает новую фракцию. Допустимые типы (указать число): 0 = военная, 1 = переселенческая, 2 = исследовательская, 3 = индустриальная, 4 = коммерчерская, 5 = пиратская, 6 = авантюристкая.
CreatePlayer [Ник] [ВК_ID] [SteamID64] - создает запись (регистрирует) нового игрока.
CreateSector [Имя] [Тип] [Контакты с другими секторами] - создает новый сектор. Допустимые типы сектора (указать число): 0 = обычный, 1 = с монолитом. Контакты с другими секторами перечислять через пробел.

SetPlayerTag [Ник] [Тег] - изменяет принадлежность игрока к фракции.
SetPlayerBan [Ник] - блокирует или разблокирует игрока.
SetPlayerFactionLeader [Ник] - изменяет статус лидера игрока.

GiveResourses [Тег] [Железо] [Энергия] [Боеприпасы] [Заряды монолитов] [Слоты кораблей] [Производство] - дает фракции ресурсы. Отрицательные значения уберут заданную часть ресурсов.
GiveStarterPack [Тег фракции] [Имя сектора] - закрепляет выбраный сектор за фракцией, строит там штаб, выдает 3 истребителя и 1 корвет и немного ресурсов.

RegisterSectorFightResult [№ битвы] [Результат] [Улучшение разрушено] [Потери истребителей атак.] [Потери корветов атак.] [Потери истребителей обор.] [Потери корветов обор.] - регистрирует результаты битвы. Варианты результата: 1 = победа атакующих, 2 = победа обороняющихся, 3 = ничья. Варианты уничтожения улучшения: 0 = уцелело, 1 = разрушено.

SetFightEnable - включает или отключает возможность планировать битвы.

RemoveFaction [Тег] - Убирает фракцию из игры, оставляя ее запись.
NextTurn - Начинает следующий ход.
";
                        }
                        case "save":
                        {
                            MySave.Save();
                            return "Сохранение завершено";
                        }
                        case "load":
                        {
                            MySave.Load();
                            return "Загрузка завершена";
                        }
                        case "asplayer":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            string[] args = new string[arguments.Length - 3];
                            Array.ConstrainedCopy(arguments, 3, args, 0, args.Length);
                            MyPlayer pl = MySave.Players.Find(x => x.Name == arguments[2]);
                            if (pl is null) return "Игрок не найден";
                            return new MyCommand(pl.Vk).ExecuteCommand(string.Join(" ", args), out title);
                        }
                        case "asfaction":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            arguments[1] = "asplayer";
                            if (!MySave.Factions.Exists(x => x.Tag == arguments[2])) return "Фракция не найдена";
                            try
                            {
                                arguments[2] = MySave.Players.Find(x => x.IsFactionLeader && x.Tag == arguments[2]).Name;
                            }
                            catch (NullReferenceException)
                            {
                                return "У фракции нет ни одного лидера. Выполнение команды от имени фракции невозможно";
                            }

                            goto case "asplayer";
                        }
                        case "createfaction":
                        {
                            if (arguments.Length < 5) return "Неверное количество аргументов";
                            if (arguments[3].Length != 3) return "Неверная длина тега";
                            int num;
                            try
                            {
                                num = Convert.ToInt32(arguments[4]);
                            }
                            catch (Exception)
                            {
                                return "Неверный формат типа фракции";
                            }

                            if (!Enum.IsDefined(typeof(FactionType), num)) return "Некорректный тип фракции";
                            FactionType type = (FactionType) num;
                            MySave.Factions = MySave.Factions.Add(new MyFaction(arguments[2], arguments[3], type, default));
                            return "Фракция создана";
                        }
                        case "createplayer":
                        {
                            if (arguments.Length < 5) return "Неверное количество аргументов";
                            int vk;
                            ulong steam;
                            try
                            {
                                vk = Convert.ToInt32(arguments[3]);
                            }
                            catch (Exception)
                            {
                                return "Неверный ВК";
                            }

                            try
                            {
                                steam = Convert.ToUInt64(arguments[4]);
                            }
                            catch (Exception)
                            {
                                return "Неверный SteamID64";
                            }

                            MySave.Players = MySave.Players.Add(new MyPlayer(arguments[2], vk, steam));
                            return "Запись игрока создана";
                        }
                        case "createsector":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            int type;
                            try
                            {
                                type = Convert.ToInt32(arguments[3]);
                                if (!Enum.IsDefined(typeof(SectorType), type)) throw new ArgumentException();
                            }
                            catch (Exception)
                            {
                                return "Неверный тип сектора";
                            }

                            string[] w = new string[arguments.Length - 4];
                            if (w.Length != 0) Array.ConstrainedCopy(arguments, 4, w, 0, w.Length);
                            MySave.Sectors = MySave.Sectors.Add(new MySector(arguments[2], (SectorType) type, w));
                            return "Сектор создан";
                        }
                        case "setplayertag":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            MyPlayer player = MySave.Players.Find(x => x.Name == arguments[2]);
                            if (player is null) return "Игрок не найден";
                            if (player.Status != PlayerStatus.FactionMember) return "Игрок не состоит во фракции";
                            if (!MySave.Factions.Exists(x => x.Tag == arguments[3]))
                                return "Фракция с данным тегом не найдена";
                            player.Tag = arguments[3];
                            return "Тег изменен";
                        }
                        case "setplayerban":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            MyPlayer player = MySave.Players.Find(x => x.Name == arguments[2]);
                            if (player is null) return "Игрок не найден";
                            if (player.IsAdmin) return "Невозможно заблокировать администратора";
                            player.IsBanned = !player.IsBanned;
                            return player.IsBanned ? "Игрок заблокирован" : "Игрок разблокирован";
                        }
                        case "setplayerfactionleader":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            MyPlayer player = MySave.Players.Find(x => x.Name == arguments[2]);
                            if (player is null) return "Игрок не найден";
                            if (player.Status != PlayerStatus.FactionMember) return "Игрок не состоит во фракции";
                            player.IsFactionLeader = !player.IsFactionLeader;
                            return player.IsFactionLeader ? "Игрок теперь лидер фракции" : "Игрок больше не лидер фракции";
                                }
                            case "giveresourses":
                        {
                            if (arguments.Length < 9) return "Неверное количество аргументов";
                            MyFaction faction = MySave.Factions.Find(x => x.Tag == arguments[2]);
                            if (faction is null) return "Фракция не найдена";
                            MyResourses resourses;
                            try
                            {
                                resourses = new MyResourses(Convert.ToInt32(arguments[3]), Convert.ToInt32(arguments[4]), Convert.ToInt32(arguments[5]), Convert.ToInt32(arguments[6]), Convert.ToInt32(arguments[7]), Convert.ToInt32(arguments[8]));
                            }
                            catch (Exception)
                            {
                                return "Неверный формат значений ресурсов";
                            }

                            faction.Resourses += resourses;
                            return "Операция выполнена";
                        }
                        case "givestarterpack":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            MyFaction faction = MySave.Factions.Find(x => x.Tag == arguments[2]);
                            if (faction is null) return "Фракция не найдена";
                            MySector sector = MySave.Sectors.Find(x => x.Name == arguments[3]);
                            if (sector is null) return "Сектор не найден";
                            if (MySave.Sectors.Exists(x => x.Improvement.Type == SectorImprovementType.Headquarters && x.Tag == faction.Tag)) return "У фракции уже есть штаб";
                            if (!string.IsNullOrWhiteSpace(sector.Tag)) return "Сектор уже имеет владельца";
                            if (!sector.Improvementable) return "Сектор не может быть выдан в качестве стартового: в секторе нельзя строить улучшения";

                            sector.Tag = faction.Tag;
                            sector.Improvement = (SectorImprovementType.Headquarters, 1);
                            faction.Ships[ShipType.Corvette] += 1;
                            faction.Ships[ShipType.Fighter] += 3;
                            faction.Resourses += new MyResourses(150, 10, 10, 0, 0, 0);
                            return "Стартовый пакет успешно выдан";
                        }
                        case "registersectorfightresult":
                        {
                            if (arguments.Length < 9) return "Неверное количество аргументов";
                            MySectorFight fight;
                            FightResult result;
                            Dictionary<ShipType, int> attackersC, defendersC;
                            bool impDestroyed;
                            MyFaction attack, def;
                            try
                            {
                                fight = (MySectorFight) _api.AllFights().ToList()[Convert.ToInt32(arguments[2])];
                                if (fight is null) return "Бой не найден";
                                result = (FightResult) Convert.ToInt32(arguments[3]);
                                impDestroyed = Convert.ToBoolean(Convert.ToInt32(arguments[4]));
                                attackersC = new Dictionary<ShipType, int>(6)
                                             {
                                                 {
                                                     ShipType.Fighter, Convert.ToInt32(arguments[5])
                                                 },
                                                 {
                                                     ShipType.Corvette, Convert.ToInt32(arguments[6])
                                                 }
                                             };
                                defendersC = new Dictionary<ShipType, int>(6)
                                             {
                                                 {
                                                     ShipType.Fighter, Convert.ToInt32(arguments[7])
                                                 },
                                                 {
                                                     ShipType.Corvette, Convert.ToInt32(arguments[8])
                                                 }
                                             };
                                attack = MySave.Factions.Find(x => x.Name == fight.AttackersTag);
                                def = MySave.Factions.Find(x => x.Name == fight.DefendersTag);
                                if (attack is null || def is null) return "Фракции не найдены. Проверьте запись битвы";
                            }
                            catch (Exception)
                            {
                                return "Неверный формат аргументов";
                            }

                            MySector sector = MySave.Sectors.Find(x => x.Name == fight.Sector);
                            if (sector is null) return "Сектор не найден. Проверьте запись битвы";
                            switch (result)
                            {
                                case FightResult.NoResult: return "Необходим иной результат битвы";
                                case FightResult.AttackersWin:
                                    sector.Tag = attack.Tag;
                                    break;
                                case FightResult.DefendersWin: break;
                                case FightResult.Stalemate: break;
                                default: return "Ошибка во время регистрации. Сообщите администрации";
                            }

                            if (impDestroyed) sector.Improvement = (SectorImprovementType.None, 0);
                            attackersC.ForEach(x => attack.Ships[x.Key] -= x.Value);
                            defendersC.ForEach(x => def.Ships[x.Key] -= x.Value);

                            fight.ResultRegistered = true;
                            return "Результат битвы зарегистрирован";
                        }
                        case "setfightsenable":
                        {
                            MySave.BotSettings.EnableFights = !MySave.BotSettings.EnableFights;
                            return MySave.BotSettings.EnableFights ? "Битвы включены" : "Битвы выключены";
                        }
                        case "removefaction":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            MyFaction faction = MySave.Factions.Find(x => x.Tag == arguments[2]);
                            MySave.Sectors.FindAll(x => x.Tag == arguments[2]).ForEach(x =>
                                                                                       {
                                                                                           x.Tag = string.Empty;
                                                                                           if (x.Improvement.Type == SectorImprovementType.Headquarters) x.Improvement = (SectorImprovementType.None, 0);
                                                                                       });
                            faction.Resourses = new MyResourses();
                            faction.Ships = SMyEconomyConsts.GetNewEmptyShipDictionary();
                            return "Фракция выведена из игры";
                        }
                        case "nextturn":
                        {
                            if (MySave.Fights.Exists(x => !x.ResultRegistered)) return "Не все битвы зарегестрированы";
                            foreach (MyFaction faction in MySave.Factions)
                            {
                                faction.Attack = true;
                                faction.BulidPoints = faction.FactionType == FactionType.Industrial ? 2 : 1;
                                bool tradeShipFinished = false;
                                switch (faction.TradeShipStatus)
                                {
                                    case TradeShipStatus.None:
                                        break;
                                    case TradeShipStatus.Started:
                                        faction.TradeShipStatus = TradeShipStatus.InWay;
                                        break;
                                    case TradeShipStatus.InWay:
                                        tradeShipFinished = true;
                                        faction.TradeShipStatus = TradeShipStatus.None;
                                        break;
                                    case TradeShipStatus.Attacked:
                                        throw new ArgumentException();
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                ImmutableList<MySector> sectors = MySave.Sectors.FindAll(x => x.Tag == faction.Tag);
                                MyResourses[] service = {new MyResourses()};
                                ImmutableList<MySector> factionsSectors = MySave.Sectors.FindAll(x => x.Tag == faction.Tag);
                                int monoithSectors = factionsSectors.Count(x => x.SectorType == SectorType.Monolith);
                                sectors.ForEach(x => service[0] += x.Service);
                                faction.Ships.ForEach(x => service[0] += SMyEconomyConsts.Ships[x.Key].Service * x.Value);
                                // ReSharper disable once SwitchStatementMissingSomeCases
                                faction.MaxResourses.MonolithCharges = service[0].MonolithCharges;
                                faction.MaxResourses.ShipSlots = service[0].ShipSlots;
                                faction.MaxResourses.Production = service[0].Production;
                                switch (faction.FactionType)
                                {
                                    case FactionType.Research:
                                        service[0] %= 100 + 15 * monoithSectors;
                                        break;
                                    case FactionType.Resettlement:
                                        service[0] %= 100 + 2 * factionsSectors.Count;
                                        break;
                                    case FactionType.Military:
                                        service[0].ShipSlots += 5;
                                        break;
                                }

                                if (tradeShipFinished)
                                    service[0] %= faction.FactionType == FactionType.Commercial ? 400 : 200;
                                for (int i = 0; i < MyResourses.Length; i++)
                                    if (faction.Resourses[i] < 0)
                                        service[0][i] %= 85;
                                faction.Resourses.Production = 0;
                                faction.Resourses.ShipSlots = 0;
                                faction.Resourses.MonolithCharges = 0;
                                faction.Resourses += service[0];
                            }

                            return "Ход завершен";
                        }
                        case "send":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";

                            int id;
                            try
                            {
                                id = MySave.Players.Find(x => x.Name == arguments[2]).Vk;
                            }
                            catch (NullReferenceException)
                            {
                                return "Игрок не найден";
                            }

                            MyVkApi.LastApi.SendMessage(id, string.Join(" ", arguments, 3, arguments.Length - 3), arguments.GetHashCode(), "Сообщение от администратора");
                            return string.Empty;
                        }
                        default:
                            return "Неизвестная команда. Используйте команду \"botadmin help\" для получения справки";
                    }
                }
                case "botconsole":
                {
                    arguments[1] = arguments[1].ToLowerInvariant();
                    if (!(_id == 0 || _id == 91777907L || MySave.BotSettings.UnsafeMode)) return "Отказано в доступе";
                    switch (arguments[1])
                    {
                        case "help":
                        case "?":
                        {
                            return @"gc.collect - начинает сборку мусора.
setplayeradmin [Ник] - дает или убирает права администратора у игрока.
clear - отчищает консоль";
                        }
                        case "gc.collect":
                        {
                            GC.Collect(2, GCCollectionMode.Forced, true, true);
                            GC.WaitForPendingFinalizers();
                            GC.Collect(2, GCCollectionMode.Forced, true, true);
                            return "Сборка мусора завершена";
                        }
                        case "setplayeradmin":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            MyPlayer player = MySave.Players.Find(x => x.Name == arguments[2]);
                            if (player is null) return "Игрок не найден";
                            player.IsAdmin = !player.IsAdmin;
                            if (player.IsAdmin) player.IsBanned = false;
                            return player.IsAdmin ? "Игрок теперь администратор" : "Игрок больше не администратор";
                        }
                        case "clear":
                        {
                            Console.Clear();
                            return string.Empty;
                        }
                        case "disableconsole":
                        {
                            MyExtensions.FreeConsole();
                            return string.Empty;
                        }
                        default:
                            return "Неизвестная команда. Используйте команду \"botconsole help\" для получения справки";
                    }
                }
                default: return string.Empty;
            }
        }
    }
}