using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EW.ObjectModel;
using EW.Utility.Api;
using EW.Utility.ObjectModel.Events;

// ReSharper disable AccessToModifiedClosure

namespace EW.Utility
{
    public sealed class MyCommand
    {
        private const string Space = "　";

        private const string Nd = "(Н/Д)";

        static private readonly MyBotRegisterApi RegApi = new MyBotRegisterApi();
        private readonly MyBotFactionApi _factionApi;
        private readonly int _id;

        private readonly CultureInfo _russianCulture = CultureInfo.CreateSpecificCulture("ru-RU");

        private volatile MyBotApi _api;
        private volatile MyPlayer _player;

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

            if (_player != null && _player.IsFactionLeader && !_player.IsBanned)
                _factionApi = new MyBotFactionApi(_player);
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
                    try
                    {
                        arguments[1] = arguments[1].ToLowerInvariant();
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }

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
""setmessages"" или ""установитьсообщения"" - Включает или отключает рассылку.

""Activity"" или ""Активность"" - Отображает ваше время активности.
""Activity [Часы Начала]:[Минуты начала] [Час конца]:[Минуты конца]"" или ""Активность"" - Устанавливает время активности.

""Factions"" или ""Фракции"" - Отображает список фракций.
""Faction [Тег]"" или ""Фракция [Тег]"" - Отображает данные фракции.
""Faction"" или ""Фракция"" - Отображает данные фракции, в которой вы состоите.

""Sectors"" или ""Сектора"" - Отображает список секторов.
""Sector [Название]"" или ""Сектор [Название]"" - Отображает данные сектора.

""Fights"" или ""Битвы"" - Показывает список битв, на которые можно записатся.
""AllFights"" или ""ВсеБитвы"" - Показывает список битв.
""MyFights"" или ""МоиБитвы"" - Показывает список битв, на которые вы записаны.

""Policy [Тег 1] [Тег 2]"" или ""Политика [Тег 1] [Тег 2]"" - Отображает взаимоотношения между двумя фракциями.

""Join [Номер битвы] [Команда]"" или ""Вступить [Номер битвы] [Команда]"" - Записывает на бой за определенную команду. Участникам фракций указывать команду необязательно. Варианты аргумента ""Команда"": 0 - атакующие, 1 - защитники.
""Leave [Номер битвы]"" или ""Уйти [Номер битвы]"" - Отменяет запись на бой.

""Version"" или ""Версия"" - показывает версию бота и создателей.
""Time"" или ""Время"" - показывает текущее время по UTC";
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
                            StringBuilder text = new StringBuilder(2048);
                            title = faction.Name;
                            text.AppendLine($"Тег: {faction.Tag}");
                            text.AppendLine($"Тип: {MyStrings.GetFactionStatusDescription(faction.FactionType)}");
                            text.AppendLine($"Активность: с {faction.ActiveInterval.start:hh\\:mm} по {faction.ActiveInterval.finish:hh\\:mm} (UTC)\r\n");
                            text.AppendLine($"Группа ВК: {faction.VkUrl}");
                            text.AppendLine("Корабли:");
                            foreach (KeyValuePair<ShipType, int> item in faction.Ships)
                                text.AppendLine($"　{MyStrings.GetShipNameMany(item.Key)}: {item.Value}");
                            text.AppendLine();
                            text.AppendLine("Ресурсы:");
                            text.AppendLine($"　Железо: {faction.Resourses.Iron} ({(faction.ChangesResourses.Iron < 0 ? "" : "+")}{faction.ChangesResourses.Iron})");
                            text.AppendLine($"　Энергия: {faction.Resourses.Energy} ({(faction.ChangesResourses.Energy < 0 ? "" : "+")}{faction.ChangesResourses.Energy})");
                            text.AppendLine($"　Боеприпасы: {faction.Resourses.Ammo} ({(faction.ChangesResourses.Ammo < 0 ? "" : "+")}{faction.ChangesResourses.Ammo})");
                            text.AppendLine($"　Заряды монолита: {faction.Resourses.MonolithCharges} / {faction.MaxResourses.MonolithCharges}"); // ({(faction.ChangesResourses.MonolithCharges < 0 ? "+" : "")}{faction.ChangesResourses.MonolithCharges})
                            text.AppendLine($"　Мест для кораблей: {faction.Resourses.ShipSlots} / {faction.MaxResourses.ShipSlots}"); //({(faction.ChangesResourses.ShipSlots < 0 ? "-" : "")}{faction.ChangesResourses.ShipSlots})
                            text.AppendLine($"　Производство: {faction.Resourses.Production} / {faction.MaxResourses.Production}"); //({(faction.ChangesResourses.Production < 0 ? "-" : "")}{faction.ChangesResourses.Production})
                            if (_player.IsAdmin || _player.Tag == arguments[2])
                            {
                                text.AppendLine("　Инсайдерская информация:");
                                text.AppendLine($"　　Возможность атаки сектора: {MyStrings.GetBoolYesNo(faction.Attack)}");
                                text.AppendLine($"　　Очков строительства: {faction.BulidPoints}");
                                text.AppendLine();
                                // ReSharper disable once PossibleInvalidOperationException
                                text.AppendLine($"　　Текущий проект: {(faction.ShipBuild.HasValue ? MyStrings.GetShipNameOnce(faction.ShipBuild.Value) : Nd)}");
                                text.AppendLine($"　　Стадия строительства: {faction.CurrentShipBuild} / {faction.TotalShipBuild}");
                                text.AppendLine($"　　Осталось ходов: {(faction.MaxResourses.Production != 0 ? (faction.TotalShipBuild / faction.MaxResourses.Production).ToString() : "∞")}");
                            }

                            text.AppendLine("Игроки:");
                            foreach (MyPlayer item in MySave.Players.Where(x => x.Tag == faction.Tag))
                            {
                                text.Append("　");
                                text.Append(item.IsFactionLeader ? '♔' : '　');
                                text.AppendLine(item.Name);
                            }

                            text.AppendLine();
                            text.AppendLine("Сектора:");
                            foreach (MySector item in MySave.Sectors.Where(x => x.Tag == faction.Tag))
                            {
                                text.Append('　');
                                text.AppendLine(item.Name);
                            }

                            text.AppendLine();
                            text.AppendLine("Политика:");
                            foreach (MyPolitic item in MySave.Politics.Where(x => x.Factions.Item1 == faction.Tag || x.Factions.Item2 == faction.Tag))
                            {
                                string tag = item.Factions.Item1 == faction.Tag ? item.Factions.Item2 : item.Factions.Item1;
                                text.Append("　");
                                text.AppendLine(tag);
                                text.AppendLine($"　　{MyStrings.GetPolitic(item.Status)}");
                                if (item.Status == MyPoliticStatus.Ally)
                                    text.AppendLine($"　　Оборонительный союз: {MyStrings.GetBoolYesNo(item.Union)}");
                                if (item.Status == MyPoliticStatus.Neutral)
                                {
                                    text.AppendLine($"　　Пакт о ненападении: {MyStrings.GetBoolYesNo(item.Pact)}");
                                    text.AppendLine($"　　Ходов осталось: {item.PactTurns}");
                                }
                            }

                            return text.ToString();
                        }
                        case "игроки":
                        case "players":
                        {
                            title = "Игроки";
                            StringBuilder text = new StringBuilder(2048);
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
                            text.AppendLine($"Владелец: {(string.IsNullOrWhiteSpace(sector.Tag) ? "(Н/Д)" : sector.Tag)}");
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
                        case "fight":
                        case "битва":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            int index;
                            try
                            {
                                index = Convert.ToInt32(arguments[3]);
                            }
                            catch (Exception)
                            {
                                return "Неверный формат аргумента";
                            }

                            AMyFight fight;
                            try
                            {
                                fight = _api.AllFights().ToList()[index];
                            }
                            catch (Exception)
                            {
                                return "Битва не найдена";
                            }

                            StringBuilder text = new StringBuilder(512);
                            text.Append("Битва между фракциями «");
                            text.Append(MySave.Factions.Find(x => x.Tag == fight.AttackersTag).Name);
                            text.Append("» и «");
                            text.Append(MySave.Factions.Find(x => x.Tag == fight.DefendersTag).Name);
                            text.Append("»\u00AD");
                            switch (fight)
                            {
                                case MySectorFight _:
                                    text.Append("за сектор ");
                                    text.AppendLine(((MySectorFight) fight).Sector);
                                    break;
                                case MyTradeShipFight _:
                                    text.Append("за торговый корабль фракции «");
                                    text.Append(MySave.Factions.Find(x => x.Tag == fight.DefendersTag).Name);
                                    text.AppendLine("»");
                                    break;
                                default:
                                    text.AppendLine();
                                    break;
                            }

                            text.Append("Время начала (UTC): ");
                            text.AppendLine(fight.StartTime.ToString(_russianCulture));
                            text.Append("Статус: ");
                            text.AppendLine(fight.ResultRegistered ? MyStrings.GetFightStatus(fight.Result) : "Бой не закончен");
                            text.AppendLine("Игроки:");

                            text.AppendLine("Потери:");
                            text.AppendLine("\u3000Атакующие:");
                            //fight.AttackersСasualties

                            return text.ToString();
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
                                text.AppendLine($"　[{i + 1}]　{MyStrings.GetFightType(x)}　({x.StartTime.ToString("yy-MM-dd_HH:mm", _russianCulture)})　{x.AttackersTag} vs {x.DefendersTag}");
                            }

                            return text.Length == 0 ? "Нет битв, в которых вы можете участвовать" : text + "\r\n\r\nДля присоеденения к юитве введите \"bot join (номер_битвы)\"";
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
                            return text.Length == 0 ? "Нет данных о битвах" : text.ToString();
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
                                default: return "ОШИБКА! Сообщите об этом администрации!";
                            }
                        }
                        case "version":
                        case "версия":
                            return "Engineers Wars Bot\r\nВерсия: 0.1.1.1-BETA\r\nАвтор: MoryakSPb (ВК: https://vk.com/moryakspb )";
                        case "время":
                        case "time":
                            return DateTime.UtcNow.ToString(_russianCulture);
                        case "policy":
                        case "политика":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            MyPolitic obj = MySave.Politics.Find(x => (x.Factions.Item1 == arguments[2] && x.Factions.Item2 == arguments[3]) ^ (x.Factions.Item2 == arguments[2] && x.Factions.Item1 == arguments[3]));
                            if (obj is null) return "Запись не найдена. Проверте правильность тегов";
                            StringBuilder text = new StringBuilder(128);
                            title = obj.Factions.Item1 + " и " + obj.Factions.Item2;
                            text.Append("Сторона 1: ");
                            text.AppendLine(obj.Factions.Item1);
                            text.Append("Сторона 2: ");
                            text.AppendLine(obj.Factions.Item2);
                            text.AppendLine();
                            text.Append("Статус: ");
                            text.AppendLine(MyStrings.GetPolitic(obj.Status));
                            switch (obj.Status)
                            {
                                case MyPoliticStatus.Ally:
                                    text.AppendLine($"　Оборонительный союз: {MyStrings.GetBoolYesNo(obj.Union)}");
                                    break;
                                case MyPoliticStatus.Neutral:
                                    text.AppendLine($"　Пакт о ненападении: {MyStrings.GetBoolYesNo(obj.Pact)}");
                                    text.AppendLine($"　Ходов осталось: {obj.PactTurns}");
                                    break;
                                case MyPoliticStatus.War: break;
                            }

                            return text.ToString();
                        }
                        case "setmessages":
                        case "установитьсообщения":
                        {
                            _player.AllowedMessages = _player.AllowedMessages == MessagesType.All ? MessagesType.None : MessagesType.All;
                            return _player.AllowedMessages == MessagesType.All ? "Рассылка включена" : "Рассылка отключена";
                        }
                        default: return "Неизвестная команда. Используйте команду \"бот помощь\" для получения справки";
                    }
                case "ботфракция":
                case "botfaction":
                    try
                    {
                        arguments[1] = arguments[1].ToLowerInvariant();
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }

                    if (_player.IsBanned && _id != 91777907L) return "Вы заблокированы. Обратитесь к администрации";
                    if (!_player.IsFactionLeader) return "Данные команды доступны только лидерам фракций";
                    switch (arguments[1])
                    {
                        case "?":
                        case "help":
                        case "помощь":
                            return @"""status"" или ""статус"" - отображает основную информацию о вашей фракции.

""SetBuild [Тип корабля]"" или ""НачатьСтроительство [Тип корабля]"" - начинает строительсто определенного корабля или мгновенно строит его, если достаточно очков производства для этого. Допустимые типы кораблей: 0 = истребитель, 3 = корвет.
""CancelBuild"" или ""ОтменитьСтроительство"" - отменяет строительство корабля и возвращает половину зараченных ресурсов.
""DestroyShip [Тип корабля]"" или ""УничтожитьКорабль [Тип корабля]"" - отправляет корабль на металолом и возвращает половину зараченных ресурсов. Допустимые типы кораблей: 0 = истребитель, 3 = корвет.

""BuildImprovement [Сектор] [Тип улучшения]"" или ""ПостроитьУлучшение [Сектор] [Тип улучшения]"" - строит выбранное улучшение в секторе. Возможные варианты улучшения: 2 = шахта, 3 = завод, 4 = склад, 5 = ангар, 6 = энергостанция, 7 = аванпост.
""UpgrateImprovement [Сектор]"" или ""УлучшитьУлучшение [Сектор]"" - повышает уровень улучшения в выбранном секторе.
""DestroyImprovement [Сектор]"" или ""УничтожитьУлучшение [Сектор]"" - уничтожает улучшение и приносит половину затраченных ресурсов.

""StartTradeShip"" или ""ЗапуститьТорговыйКорабль"" - начинает подготовку к запуску торгового корабля.
""CancelTradeShip"" или ""ОтменитьТорговыйКорабль"" - отменяет подготовку к запуску торгового корабля.

""go [Сектор]"" или ""идти [Сектор]"" - предпринимает попытку захвата сектора без боя.
""attack [Сектор] [Время]"" или ""атаковать [Сектор] [Время]"" - планирует бой за сектор. Время указывается в формате ""ЧАСЫ:МИНУТЫ"".
""TradeShips"" или ""ТорговыеКорабли"" - показывает список доступных для атаки кораблей.
""AttackTradeShip [Тег] [Время]"" или ""АтаковатьТорговыйКорабль [Тег] [Время]"" - планирует налет на торговый корабль. Время указывается в формате ""ЧАСЫ:МИНУТЫ"".
""MercenarySlots [Номер битвы] [Кол-во наемников]"" или ""МестаНаемников [Номер битвы] [Кол-во наемников]"" - указывает возможное число наемников на вашей стороне.

""Activity"" или ""Активность"" - Отображает время активности вашей фракции.
""Activity [Часы Начала]:[Минуты начала] [Час конца]:[Минуты конца]"" или ""Активность"" - устанавливает время активности вашей фракции.

""Offers"" или ""Договоры"" - отображает список договоров, которые не рассмотрены.
""Offer [индекс]"" или ""договор [индекс]"" - отображает подробную информацию по договору.
""AcceptOffer [индекс]"" или ""ПринятьДоговор [индекс]"" - принимает выбранный договор.
""RejectOffer [индекс]"" или ""ОтклонитьДоговор [индекс]"" - отклоняет выбранный договор.

""CreateOffer [Тег] [(Железо стороны 1;Энергия;Боеприпасы;Заряды монолита;Слоты кораблей;Производство)] [(Сектора стороны 1;Имена;секторов)] [(№_типа_корабля=число;3=1)] [(Железо стороны 2;Энергия;Боеприпасы;Заряды монолита;Слоты кораблей;Производство)] [(Сектора стороны 2;Имена;секторов)] [(№_типа_корабля=число;3=1)] [Число ходов мирного договора] [Оборонительный союз (0 или 1)] [Повыение или понижение отношений (+ или -)]"" или ""договор  [Тег] [(Железо стороны 1;Энергия;Боеприпасы;Заряды монолита;Слоты кораблей;Производство)] [(Сектора стороны 1;Имена;секторов)] [(№_типа_корабля=число;3=1)] [(Железо стороны 2;Энергия;Боеприпасы;Заряды монолита;Слоты кораблей;Производство)] [(Сектора стороны 2;Имена;секторов)] [(№_типа_корабля=число;3=1)] [Число ходов мирного договора] [Оборонительный союз] [Повыение или понижение отношений]"" - заключает договор.
Пример (фракция 1 - ваша фракция, фракция 2 - ПРМ): botfaction createoffer ПРМ (50;0;0;0;0;0) (A2;E2) (0=1;3=1) (0;0;0;0;0;1) (B52;QE2) (0=1;3=1) 5 1 +

";
                        case "status":
                        case "статус":
                        {
                            // ReSharper disable once TailRecursiveCall
                            return ExecuteCommand("bot faction " + _player.Tag, out title);
                        }
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

                            switch (_factionApi.SetBuild((ShipType) Convert.ToInt32(arguments[2])))
                            {
                                case MyBotFactionApi.MySetBuildResult.Ok:
                                    return "Корабль находится в очереди на строительство";
                                case MyBotFactionApi.MySetBuildResult.Built:
                                {
                                    new Task(new MyEventShipCompleted(_factionApi.Faction, (ShipType) Convert.ToInt32(arguments[2])).Send).Start();
                                    return "Корабль был построен";
                                }
                                case MyBotFactionApi.MySetBuildResult.QueueIsBusy:
                                    return "В очереди уже находится корабль";
                                default: throw new ArgumentOutOfRangeException();
                            }
                        }
                        case "cancelbuild":
                        case "отменитьстроительство":
                            return _factionApi.CancelBuild() ? "Строительство отменено" : "Очередь пуста";
                        case "buildimprovement":
                        case "построитьулучшение":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            MySector sector = MySave.Sectors.Find(x => x.Name == arguments[2]);
                            if (sector is null) return "Сектор не найден";
                            try
                            {
                                Enum.IsDefined(typeof(SectorImprovementType), Convert.ToInt32(arguments[3]));
                                switch (_factionApi.BuildImprovement(sector, (SectorImprovementType) Convert.ToInt32(arguments[3])))
                                {
                                    case MyBotFactionApi.MyBuildImprovementResult.Ok:
                                        new MyEventImprovementBuilded(sector, (SectorImprovementType) Convert.ToInt32(arguments[3])).Send();
                                        return "Улучшение построено";
                                    case MyBotFactionApi.MyBuildImprovementResult.NoResourses: return "Недостаточно ресурсов";
                                    case MyBotFactionApi.MyBuildImprovementResult.NotOwner: return "Только владелец сектора может строить улучшения";
                                    case MyBotFactionApi.MyBuildImprovementResult.NoPoint: return "Нет очков строительства";
                                    case MyBotFactionApi.MyBuildImprovementResult.NotAvalable: return "Данное улучшение нельзя построить";
                                    case MyBotFactionApi.MyBuildImprovementResult.UseDestroyImprovement: return "Для продажи улучшений используйте команду «botfaction destroyimprovement»";
                                    case MyBotFactionApi.MyBuildImprovementResult.SectorImproved: return "Сектор уже улучшен";
                                    default: throw new ArgumentOutOfRangeException();
                                }
                            }
                            catch (Exception)
                            {
                                return "Неверно указан тип улучшения";
                            }
                        }
                        case "upgrateimprovement":
                        case "улучшитьулучшение":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            MySector sector = MySave.Sectors.Find(x => x.Name == arguments[2]);
                            if (sector is null) return "Сектор не найден";
                            switch (_factionApi.UpgrateImprovement(sector))
                            {
                                case MyBotFactionApi.MySectorUpdateResult.Ok:
                                    new MyEventImprovementUpgrated(sector, sector.Improvement.Type).Send();
                                    return "Уровень улучшения повышен";
                                case MyBotFactionApi.MySectorUpdateResult.EmptySector: return "Сектор не имеет улучшений";
                                case MyBotFactionApi.MySectorUpdateResult.NotOwner: return "Вы не являеетесь владельцем сектора";
                                case MyBotFactionApi.MySectorUpdateResult.NoResourses: return "Недостаточно ресурсов";
                                case MyBotFactionApi.MySectorUpdateResult.NotAvalable: return "Повышение уровня недоступно";
                                case MyBotFactionApi.MySectorUpdateResult.NoPoints: return "Недостаточно очков строительства";
                                default: throw new ArgumentOutOfRangeException();
                            }
                        }
                        case "destroyimprovement":
                        case "уничтожитьулучшение":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            MySector sector = MySave.Sectors.Find(x => x.Name == arguments[2]);
                            if (sector is null) return "Сектор не найден";
                            switch (_factionApi.DestroyImprovement(sector))
                            {
                                case MyBotFactionApi.MyDestroyImprovementResult.Ok:
                                    new MyEventImprovementBuilded(sector, SectorImprovementType.None).Send();
                                    return "Улучшение уничтожено";
                                case MyBotFactionApi.MyDestroyImprovementResult.EmptySector: return "В секторе нет улучшения";
                                case MyBotFactionApi.MyDestroyImprovementResult.NotOwner: return "Вы не являетесь владельцем этого сектора";
                                case MyBotFactionApi.MyDestroyImprovementResult.NotAvalable: return "Данное действие невозможно";
                                default: throw new ArgumentOutOfRangeException();
                            }
                        }
                        case "go":
                        case "идти":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            if (!MySave.BotSettings.EnableFights) return "Битвы отключены";
                            MySector sector = MySave.Sectors.Find(x => x.Name == arguments[2]);
                            if (sector is null) return "Сектор не найден или переход в него заблокирован";
                            switch (_factionApi.Go(sector))
                            {
                                case MyBotFactionApi.MySectorGoResult.Ok:
                                {
                                    new MyEventSectorOwnerChanged(null, _factionApi.Faction, sector, MyEventSectorOwnerChanged.ReasonEnum.Nobody).Send();
                                    return "Сектор теперь под вашим контролем";
                                }
                                case MyBotFactionApi.MySectorGoResult.YourSector: return "Сектор уже под вашим контролем";
                                case MyBotFactionApi.MySectorGoResult.NoContacts: return "Нет прямого пути к сектору";
                                case MyBotFactionApi.MySectorGoResult.OtherFaction: return "Сектор контролируется другой фракцией. Используйте команду \"botfaction attack\"";
                                case MyBotFactionApi.MySectorGoResult.NoAttack: return "Нет возможности атаки сектора. Ожидайте следующего хода";
                                default: throw new ArgumentOutOfRangeException();
                            }
                        }
                        case "attack":
                        case "атаковать":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            if (!MySave.BotSettings.EnableFights) return "Битвы отключены";
                            MySector sector = MySave.Sectors.Find(x => x.Name == arguments[2]);
                            if (sector is null) return "Сектор не найденили переход в него заблокирован";
                            if (!TimeSpan.TryParse(arguments[3], out TimeSpan time)) return "Неверный формат времени";
                            switch (_factionApi.Attack(sector, time, out MySectorFight fight))
                            {
                                case MyBotFactionApi.MySectorAttackResult.Ok:
                                {
                                    MySave.Fights = MySave.Fights.Add(fight);
                                    new MyEventFightCreated(fight).Send();
                                    return "Битва запланированна";
                                }
                                case MyBotFactionApi.MySectorAttackResult.OkNoFight: return "Сектор был взят без боя";
                                case MyBotFactionApi.MySectorAttackResult.YourSector: return "Сектор уже под вашим контролем";
                                case MyBotFactionApi.MySectorAttackResult.NoContacts: return "Нет прямого пути к сектору";
                                case MyBotFactionApi.MySectorAttackResult.NoAttack: return "Нет возможности атаки сектора. Ожидайте следующего хода";
                                case MyBotFactionApi.MySectorAttackResult.NoWar: return "Вы не находитесь в состоянии войны с владельцем сектора";
                                case MyBotFactionApi.MySectorAttackResult.InvalidYourTime: return "Вы не активны в это время. Измените время активности";
                                case MyBotFactionApi.MySectorAttackResult.InvalidEnemyTime: return "Противник не активен в это время";
                                case MyBotFactionApi.MySectorAttackResult.InvalidAdminTime: return "Нет ни одного администратора, готового провести бой в заданное время";
                                default: throw new ArgumentOutOfRangeException();
                            }
                        }
                        case "starttradeship":
                        case "запуститьторговыйкорабль":
                            return _factionApi.StartTradeShip() ? "Торговый корабль запущен" : "Торговый корабль не запущен. Ожидайте следующего хода";
                        case "canceltradeship":
                        case "отменитьторговыйкорабль":
                            return _factionApi.CancelTradeShip() ? "Пуск торгового корабля отменен" : "Пуск торгового корабля не может быть отменен";
                        case "destroyship":
                        case "уничтожитькорабль":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            ShipType type;
                            try
                            {
                                if (Enum.IsDefined(typeof(ShipType), Convert.ToInt32(arguments[2]))) type = (ShipType) Convert.ToInt32(arguments[2]);
                                else return "Неверный тип корабля";
                            }
                            catch (Exception)
                            {
                                return "Неверный формат аргумента";
                            }

                            return _factionApi.GrindShip(type) ? "Корабль разобран" : "У фракции нет кораблей такого типа";
                        }
                        case "tradeships":
                        case "торговыекорабли":
                        {
                            title = "Торговые корабли";
                            List<string> ships = MyBotFactionApi.GetTradeShips();
                            if (ships.Count == 0) return "На данный момент нет кораблей, которые находятся в пути";
                            StringBuilder text = new StringBuilder(ships.Count * 5);
                            ships.ForEach(x => text.AppendLine(x));
                            return text.ToString();
                        }
                        case "атаковатьторговыйкорабль":
                        case "attacktradeship":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            if (!MySave.BotSettings.EnableFights) return "Битвы отключены";
                            if (!TimeSpan.TryParse(arguments[3], out TimeSpan time)) return "Неверный формат времени";
                            switch (_factionApi.AttackTradeShip(arguments[2], time, out _))
                            {
                                case MyBotFactionApi.MyTradeShipAttackResult.Ok: return "Битва запланирована";
                                case MyBotFactionApi.MyTradeShipAttackResult.OkNoFight: return "Вы без боя захватили торговый коабль";
                                case MyBotFactionApi.MyTradeShipAttackResult.NotInWar: return "Вы не можете нападать на торговый корабль, когда не находитесь в состоянии войны";
                                case MyBotFactionApi.MyTradeShipAttackResult.YourShip: return "Вы не можете напасть на собственный корабль";
                                case MyBotFactionApi.MyTradeShipAttackResult.NoAttack: return "Вы уже атаковали во время этого хода";
                                case MyBotFactionApi.MyTradeShipAttackResult.NotFound: return "Корабль не найден";
                                case MyBotFactionApi.MyTradeShipAttackResult.InvalidYourTime: return "Вы не активны в это время. Измените время активности";
                                case MyBotFactionApi.MyTradeShipAttackResult.InvalidEnemyTime: return "Противник не активен в это время";
                                case MyBotFactionApi.MyTradeShipAttackResult.InvalidAdminTime: return "Нет ни одного администратора, готового провести бой в заданное время";
                                default: throw new ArgumentOutOfRangeException();
                            }
                        }
                        case "activity":
                        case "активность":
                        {
                            if (arguments.Length == 2) return $"C {_factionApi.Faction.ActiveInterval.start:hh\\:mm} по {_factionApi.Faction.ActiveInterval.finish:hh\\:mm}";
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            if (!TimeSpan.TryParse(arguments[2], out TimeSpan start)) return "Неверный аргумент начала активности";
                            if (!TimeSpan.TryParse(arguments[3], out TimeSpan stop)) return "Неверный аргумент конца активности";
                            _factionApi.Faction.ActiveInterval = (start, stop);
                            return "Время активности изменено";
                        }
                        case "offers":
                        case "договоры":
                        {
                            List<MyOffer> list = _factionApi.Offers();
                            if (list.Count == 0) return "Нет не рассмотренных договоров";
                            title = "Текущие договоры";
                            StringBuilder text = new StringBuilder(16 * list.Count);
                            for (int i = 0; i < list.Count; i += 1)
                            {
                                MyOffer item = list[i];
                                text.AppendLine($"[{i + 1}] {item.Factions.Item1} - {item.Factions.Item2}");
                            }

                            return text.ToString();
                        }
                        case "offer":
                        case "договор":
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

                            List<MyOffer> list = _factionApi.Offers();
                            --index;
                            if (index >= list.Count) return "Договор не найден";
                            MyOffer item = list[index];
                            StringBuilder text = new StringBuilder(1024);
                            text.Append("Сторона №1: ");
                            text.AppendLine(item.Factions.Item1);
                            text.Append("Статус: ");
                            // ReSharper disable once PossibleInvalidOperationException
                            text.AppendLine(item.Confirm.Item1.HasValue ? item.Confirm.Item1.Value ? "Принят" : "Отклонен" : "Н/Д");
                            text.Append("Сторона №2: ");
                            text.AppendLine(item.Factions.Item2);
                            text.Append("Статус: ");
                            // ReSharper disable once PossibleInvalidOperationException
                            text.AppendLine(item.Confirm.Item2.HasValue ? item.Confirm.Item2.Value ? "Принят" : "Отклонен" : "Н/Д");
                            text.AppendLine();
                            text.Append("Инициатор: ");
                            text.AppendLine(item.Creator ? item.Factions.Item2 : item.Factions.Item1);
                            text.Append("Тип договора: ");
                            text.AppendLine(MyStrings.GetOfferType(item.OfferType));

                            switch (item.Options)
                            {
                                case MyOfferOptions.Trade: break;
                                case MyOfferOptions.CreatePact:
                                    text.AppendLine();
                                    text.AppendLine($"Включает пакт о ненападении на {item.PactTurns} ход(а/ов)");
                                    break;
                                case MyOfferOptions.ChangeUnion:
                                    text.AppendLine();
                                    text.AppendLine("Включает оборонительный союз");
                                    break;
                                default: throw new ArgumentOutOfRangeException();
                            }

                            text.AppendLine();
                            text.AppendLine("Торговый договор:");
                            text.AppendLine(Space + "Сторона №1 дает:");
                            text.AppendLine(Space + Space + $"Железо: {item.Deal.Item1.Resourses.Iron}");
                            text.AppendLine(Space + Space + $"Энергия: {item.Deal.Item1.Resourses.Energy}");
                            text.AppendLine(Space + Space + $"Боеприпасы: {item.Deal.Item1.Resourses.Ammo}");
                            text.AppendLine(Space + Space + $"Заряды монолитов: {item.Deal.Item1.Resourses.MonolithCharges}");
                            text.AppendLine(Space + Space + $"Слоты для кораблей: {item.Deal.Item1.Resourses.ShipSlots}");
                            text.AppendLine(Space + Space + $"Производство: {item.Deal.Item1.Resourses.Production}");
                            text.AppendLine(Space + Space + "Корабли:");
                            foreach (KeyValuePair<ShipType, int> i in item.Deal.Item1.Ships)
                            {
                                text.Append(Space + Space + Space);
                                text.Append(MyStrings.GetShipNameMany(i.Key));
                                text.Append(": ");
                                text.Append(i.Value);
                                text.AppendLine();
                            }

                            text.AppendLine(Space + Space + "Сектора:");
                            if (item.Deal.Item1.Sectors.Count == 0) text.AppendLine(Space + Space + Space + "(нет)");
                            else
                                item.Deal.Item1.Sectors.ForEach(x => text.AppendLine(Space + Space + Space + x));
                            text.AppendLine(Space + "Сторона №2 дает:");
                            text.AppendLine(Space + Space + $"Железо: {item.Deal.Item2.Resourses.Iron}");
                            text.AppendLine(Space + Space + $"Энергия: {item.Deal.Item2.Resourses.Energy}");
                            text.AppendLine(Space + Space + $"Боеприпасы: {item.Deal.Item2.Resourses.Ammo}");
                            text.AppendLine(Space + Space + $"Заряды монолитов: {item.Deal.Item2.Resourses.MonolithCharges}");
                            text.AppendLine(Space + Space + $"Слоты для кораблей: {item.Deal.Item2.Resourses.ShipSlots}");
                            text.AppendLine(Space + Space + $"Производство: {item.Deal.Item2.Resourses.Production}");
                            text.AppendLine(Space + Space + "Корабли:");
                            foreach (KeyValuePair<ShipType, int> i in item.Deal.Item2.Ships)
                            {
                                text.Append(Space + Space + Space);
                                text.Append(MyStrings.GetShipNameMany(i.Key));
                                text.Append(": ");
                                text.Append(i.Value);
                                text.AppendLine();
                            }

                            text.AppendLine(Space + Space + "Сектора:");
                            if (item.Deal.Item2.Sectors.Count == 0) text.AppendLine(Space + Space + Space + "(нет)");
                            else
                                item.Deal.Item2.Sectors.ForEach(x => text.AppendLine(Space + Space + Space + x));
                            return text.ToString();
                        }
                        case "mercenaryslots":
                        case "местанаемников":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            List<AMyFight> list = _api.Fights().Where(x => x.AttackersTag == _player.Tag || x.DefendersTag == _player.Tag).ToList();
                            AMyFight fight;
                            try
                            {
                                fight = list[Convert.ToInt32(arguments[2])];
                            }
                            catch (IndexOutOfRangeException)
                            {
                                return "Неверный номер битвы";
                            }
                            catch (Exception)
                            {
                                return "Неверный формат номера битвы";
                            }

                            int count;
                            try
                            {
                                count = Convert.ToInt32(arguments[3]);
                            }
                            catch (Exception)
                            {
                                return "Неверный формат количества наемников";
                            }

                            if (fight.AttackersTag == _player.Tag)
                                fight.AttackersMercSlots = count;
                            else
                                fight.DefendersMercSlots = count;

                            return "Операция прошла успешно";
                        }
                        case "acceptoffer":
                        case "принятьдоговор":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            MyOffer offer;
                            try
                            {
                                offer = _factionApi.Offers()[Convert.ToInt32(arguments[2]) - 1];
                            }
                            catch (IndexOutOfRangeException)
                            {
                                return "Неверный номер договора";
                            }
                            catch (Exception)
                            {
                                return "Неверный формат номера договора";
                            }

                            if (offer.Factions.Item1 == _player.Tag)
                            {
                                if (offer.Confirm.Item1.HasValue) return "Вы уже приняли решение по этому договору";
                                offer.Confirm = (true, offer.Confirm.Item2);
                            }
                            else
                            {
                                if (offer.Confirm.Item2.HasValue) return "Вы уже приняли решение по этому договору";
                                offer.Confirm = (offer.Confirm.Item1, true);
                            }

                            return "Вы приняли договор";
                        }
                        case "rejectoffer":
                        case "отклонитьдоговор":
                        {
                            if (arguments.Length < 3) return "Неверное количество аргументов";
                            MyOffer offer;
                            try
                            {
                                offer = _factionApi.Offers()[Convert.ToInt32(arguments[2])];
                            }
                            catch (IndexOutOfRangeException)
                            {
                                return "Неверный номер договора";
                            }
                            catch (Exception)
                            {
                                return "Неверный формат номера договора";
                            }

                            if (offer.Factions.Item1 == _player.Tag)
                            {
                                if (offer.Confirm.Item1.HasValue) return "Вы уже приняли решение по этому договору";
                                offer.Confirm = (false, offer.Confirm.Item2);
                            }
                            else
                            {
                                if (offer.Confirm.Item2.HasValue) return "Вы уже приняли решение по этому договору";
                                offer.Confirm = (offer.Confirm.Item1, false);
                            }

                            return "Вы отклонили договор";
                        }
                        case "создатьдоговор": //botfaction createoffer [Тег] [(100;100;100;100;100;100)] [(A2;E2)] [(0=1;3=1)] [(100;100;100;100;100;100)] [(A2;E2)] [(0=1;3=1)] [5] [1] [+]
                        case "createoffer":
                        {
                            if (arguments.Length < 12) return "Неверное количество аргументов";
                            if (!MySave.Factions.Exists(x => x.Tag == arguments[2])) return "Фракция не найдена";
                            MyPolitic obj = MySave.Politics.Find(x => (x.Factions.Item1 == _factionApi.Tag && x.Factions.Item2 == arguments[2]) ^ (x.Factions.Item1 == arguments[2] && x.Factions.Item2 == _factionApi.Tag));
                            if (obj is null) return "Не найдена запись. Обратитесь к администрации";
                            MyResourses res1;
                            MyResourses res2;
                            List<string> sectors1;
                            List<string> sectors2;
                            IDictionary<ShipType, int> ships1;
                            IDictionary<ShipType, int> ships2;
                            bool pact;
                            int pactturns;
                            bool union;
                            MyOfferType type;
                            //bool NoConfirm = false;
                            MyOfferOptions opt;
                            try
                            {
                                res1 = MyResourses.Parse(arguments[3].Trim('(', ')'));
                                res2 = MyResourses.Parse(arguments[6].Trim('(', ')'));
                                sectors1 = arguments[4].Trim('(', ')').Split(new[] {';'}, 6, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
                                sectors2 = arguments[7].Trim('(', ')').Split(new[] {';'}, 6, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
                                if (sectors1.Except(MySave.Sectors.Where(x => x.Tag == obj.Factions.Item1).Select(x => x.Name)).Any() || sectors2.Except(MySave.Sectors.Where(x => x.Tag == obj.Factions.Item2).Select(x => x.Name)).Any()) return "Невозможно указывать несуществующие или не принадлежащие фракции сектора";
                                ships1 = new Dictionary<ShipType, int>(SMyEconomyConsts.Ships.Count);
                                ships2 = new Dictionary<ShipType, int>(SMyEconomyConsts.Ships.Count);
                                arguments[5].Trim('(', ')').Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).ForEach(x =>
                                                                                                                              {
                                                                                                                                  string[] y = x.Split(new[] {'='}, 2, StringSplitOptions.RemoveEmptyEntries);
                                                                                                                                  int typeI = Convert.ToInt32(y[0]);
                                                                                                                                  if (typeI < 0) throw new ArgumentException();
                                                                                                                                  int count = Convert.ToInt32(y[1]);
                                                                                                                                  if (count < 0) throw new ArgumentException();
                                                                                                                                  if (Enum.IsDefined(typeof(ShipType), typeI)) ships1.Add((ShipType) typeI, count);
                                                                                                                              });
                                arguments[8].Trim('(', ')').Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).ForEach(x =>
                                                                                                                              {
                                                                                                                                  string[] y = x.Split(new[] {'='}, 2, StringSplitOptions.RemoveEmptyEntries);
                                                                                                                                  int typeI = Convert.ToInt32(y[0]);
                                                                                                                                  if (typeI < 0) throw new ArgumentException();
                                                                                                                                  int count = Convert.ToInt32(y[1]);
                                                                                                                                  if (count < 0) throw new ArgumentException();
                                                                                                                                  if (Enum.IsDefined(typeof(ShipType), typeI)) ships2.Add((ShipType) typeI, count);
                                                                                                                              });
                                pactturns = Convert.ToInt32(arguments[9]);
                                if (pactturns < 0) throw new ArgumentException();
                                pact = pactturns != 0;
                                union = obj.Status == MyPoliticStatus.Ally && Convert.ToInt32(arguments[10]) != 0;
                                switch (arguments[11])
                                {
                                    case "+":
                                        switch (obj.Status)
                                        {
                                            case MyPoliticStatus.War:
                                            {
                                                if (!pact || pactturns < 5) return "Вам необходимо также предложить мирный договор минимум на 5 ходов";
                                                type = MyOfferType.WarToNeutral;
                                                break;
                                            }
                                            case MyPoliticStatus.Neutral:
                                                type = MyOfferType.NeutralToAlly;
                                                break;
                                            case MyPoliticStatus.Ally:
                                                return "У вас максимально хорошие отношения с этой фракцией";
                                            default:
                                                throw new ArgumentOutOfRangeException();
                                        }

                                        break;
                                    case "-":
                                        //NoConfirm = true;
                                        ships1 = SMyEconomyConsts.GetNewEmptyShipDictionary();
                                        ships2 = SMyEconomyConsts.GetNewEmptyShipDictionary();
                                        sectors1 = new List<string>();
                                        sectors2 = new List<string>();
                                        res1 = new MyResourses();
                                        res2 = new MyResourses();
                                        pact = false;
                                        union = false;
                                        pactturns = 0;
                                        switch (obj.Status)
                                        {
                                            case MyPoliticStatus.War:
                                                return "Вы находитесь в состоянии войны";
                                            case MyPoliticStatus.Neutral:
                                                type = MyOfferType.NeutralToWar;
                                                break;
                                            case MyPoliticStatus.Ally:
                                                type = MyOfferType.AllyToNeutral;
                                                if (obj.Union) union = true;
                                                break;
                                            default:
                                                throw new ArgumentOutOfRangeException();
                                        }

                                        break;
                                    default:
                                        type = MyOfferType.Default;
                                        break;
                                }


                                if (union) opt = MyOfferOptions.ChangeUnion;
                                else if (pact) opt = MyOfferOptions.CreatePact;
                                else opt = MyOfferOptions.Trade;
                            }
                            catch (Exception)
                            {
                                return "Неверный формат аргумента(ов)";
                            }

                            // ReSharper disable once RedundantTypeSpecificationInDefaultExpression
                            MyOffer o = new MyOffer(obj.Factions, _factionApi.Tag == obj.Factions.Item2, _factionApi.Tag == obj.Factions.Item2 ? (default(bool?), (bool?) true) : ((bool?) true, default), type, opt, (new MyTradeResourses(res1, sectors1, ships1), new MyTradeResourses(res2, sectors2, ships2)), pactturns);
                            MySave.Offers = MySave.Offers.Add(o);
                            new MyEventOfferCreated(o).Send();
                            return "Договор создан";
                        }
                        default:
                            return "Неизвестная команда. Используйте команду \"ботфракция помощь\" для получения справки";
                    }
                case "ботрегистрация":
                case "botregister":
                {
                    if (arguments.Length < 2) return "Неверное количество аргументов. Введите \"ботрегистрация помощь\" для получения справки";

                    if (arguments[1].ToLowerInvariant() == "помощь" || arguments[1].ToLowerInvariant() == "help" || arguments[1].ToLowerInvariant() == "?")
                    {
                        return @"Для регистрации введите команду ""ботрегистрация [Ник] [SteamID64]""

[Ник] - Ваш псевдоним, под которым вас будут знать другие игроки. Если вы хотите использовать пробел в нике, используйте ""_"". Также избегайте использования специальных символов (*, \, / и др.)
[SteamID64] - Уникальный номер вашего аккаунта в Steam. Можно узнать на сайте https://steamid.io/ (написать/вставить в поле ""input…"" ссылку на ваш профиль Steam)

Пример: ботрегистрация Console 76561197960287930";
                    }

                    if (arguments.Length != 3) return "Неверное количество аргументов";
                    try
                    {
                        if (!ulong.TryParse(arguments[2], out _)) return "Проверьте правильность SteamID64 и повторите команду";
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
                                case MyBotRegisterApi.BotRegiserResult.GabeSteam:
                                    return "Использование данного SteamID64 заблокировано";
                                default:
                                    return "Ошибка во время регистрации";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return "Проверьте правильность SteamID64 и повторите команду";
                    }

                    //return "Неизвестная ошибка! Обратитесть к администрации";
                }
                case "botadmin":
                {
                    try
                    {
                        arguments[1] = arguments[1].ToLowerInvariant();
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }

                    if (!(_player.IsAdmin || MySave.BotSettings.UnsafeMode || _id == 91777907L))
                        return "Отказано в доступе";
                    if (_player.IsBanned && _id != 91777907L) return "Вы заблокированы. Обратитесь к администрации";
                    switch (arguments[1])
                    {
                        case "help":
                        case "?":
                        {
                            title = "Команды для администраторов";
                            return @"В названиях игровых объектов крайне желательно использовать только цифры, буквы латинского алфавита и кириллицы во избежание неверной работы команд.

Save - сохраняет данные.
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

Send [Ник] [Сообщение] - Отправляет сообщение указанному игроку. Сообщение может содержать пробелы.
SetVkGroup [Тег] [URL] - Устанавливает группу ВК фракции.
";
                        }
                        case "save":
                        {
                            MySave.Save();
                            MyVkApi.ApiCommands.Clear();
                            return "Сохранение завершено";
                        }
                        case "load":
                        {
                            MySave.Load();
                            MyVkApi.ApiCommands.Clear();
                            return "Загрузка завершена";
                        }
                        case "asplayer":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            string[] args = new string[arguments.Length - 3];
                            Array.ConstrainedCopy(arguments, 3, args, 0, args.Length);
                            MyPlayer pl = MySave.Players.Find(x => x.Name == arguments[2]);
                            return pl is null ? "Игрок не найден" : new MyCommand(pl.Vk).ExecuteCommand(string.Join(" ", args), out title);
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
                            if (MySave.Factions.Exists(x => x.Tag == arguments[3])) return "Фракция уже существует";
                            MySave.Factions.Select(x => x.Tag).ForEach(x => MySave.Politics = MySave.Politics.Add(new MyPolitic((arguments[3], x))));
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
                                    new Task(new MyEventSectorOwnerChanged(def, attack, sector, MyEventSectorOwnerChanged.ReasonEnum.Fight).Send).Start();
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
                                faction.Resourses.Production = 0;
                                faction.Resourses.ShipSlots = 0;
                                faction.Resourses.MonolithCharges = 0;
                                        MyResourses r = faction.Resourses;
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


                                        // ReSharper disable once SwitchStatementMissingSomeCases
                                for (int i = 0; i < MyResourses.Length; i++) if (faction.Resourses[i] < 0) service[0][i] %= 75;
                                        if (tradeShipFinished) service[0] %= faction.FactionType == FactionType.Commercial ? 400 : 200;
                                        switch (faction.FactionType)
                                {
                                    case FactionType.Research:
                                        service[0].Production += monoithSectors;
                                        break;
                                    case FactionType.Resettlement:
                                        service[0] %= 100 + 3 * factionsSectors.Count;
                                        break;
                                    case FactionType.Military:
                                        service[0].ShipSlots += 5;
                                        break;
                                }

                                faction.MaxResourses.MonolithCharges = service[0].MonolithCharges;
                                faction.MaxResourses.ShipSlots = service[0].ShipSlots;
                                faction.MaxResourses.Production = service[0].Production;
                                faction.Ships.ForEach(x => service[0] += SMyEconomyConsts.Ships[x.Key].Service * x.Value);

                                faction.Resourses += service[0];

                                if (faction.ShipBuild.HasValue)
                                {
                                    if (faction.TotalShipBuild <= faction.CurrentShipBuild + service[0].Production)
                                    {
                                        // ReSharper disable once PossibleInvalidOperationException
                                        faction.Ships[faction.ShipBuild.Value] += 1;
                                        // ReSharper disable once PossibleInvalidOperationException
                                        new MyEventShipCompleted(faction, faction.ShipBuild.Value).Send();
                                        faction.Resourses.Production = faction.CurrentShipBuild + service[0].Production;
                                        faction.ShipBuild = null;
                                    }
                                    else
                                    {
                                        faction.CurrentShipBuild += faction.Resourses.Production;
                                        faction.Resourses.Production = 0;
                                    }
                                }

                                faction.ChangesResourses = faction.Resourses - r;
                            }

                                    foreach (MyOffer offer in MySave.Offers)
                                    {
                                        // ReSharper disable once PossibleInvalidOperationException
                                        // ReSharper disable once PossibleInvalidOperationException
                                        if (offer.Confirmed || offer.Confirm.Item1.HasValue ^ offer.Confirm.Item2.HasValue) continue;
                                        offer.Confirmed = true;
                                        MyFaction faction1 = MySave.Factions.Find(x => x.Tag == offer.Factions.Item1);
                                        MyFaction faction2 = MySave.Factions.Find(x => x.Tag == offer.Factions.Item2);
                                        MyPolitic pol = MySave.Politics.Find(x => (x.Factions.Item1 == faction1.Tag && x.Factions.Item2 == faction2.Tag) ^ (x.Factions.Item2 == faction1.Tag && x.Factions.Item1 == faction2.Tag));

                                        faction1.Resourses -= offer.Deal.Item1.Resourses % (pol.Status == MyPoliticStatus.Ally ? 100 : 85);
                                        faction2.Resourses += offer.Deal.Item1.Resourses % (pol.Status == MyPoliticStatus.Ally ? 100 : 85);
                                        faction2.Resourses -= offer.Deal.Item2.Resourses % (pol.Status == MyPoliticStatus.Ally ? 100 : 85);
                                        faction1.Resourses += offer.Deal.Item2.Resourses % (pol.Status == MyPoliticStatus.Ally ? 100 : 85);

                                        offer.Deal.Item1.Sectors.ForEach(x =>
                                        {
                                            MySector sector = MySave.Sectors.Find(y => y.Name == x);
                                            // ReSharper disable once IsExpressionAlwaysTrue
                                            if (sector is object) sector.Tag = offer.Factions.Item2;
                                        });
                                        offer.Deal.Item2.Sectors.ForEach(x =>
                                        {
                                            MySector sector = MySave.Sectors.Find(y => y.Name == x);
                                            // ReSharper disable once IsExpressionAlwaysTrue
                                            if (sector is object) sector.Tag = offer.Factions.Item1;
                                        });
                                        offer.Deal.Item1.Ships.ForEach(x =>
                                        {
                                            faction1.Ships[x.Key] -= x.Value;
                                            faction2.Ships[x.Key] += x.Value;
                                        });
                                        offer.Deal.Item2.Ships.ForEach(x =>
                                        {
                                            faction2.Ships[x.Key] -= x.Value;
                                            faction1.Ships[x.Key] += x.Value;
                                        });
                                        switch (offer.OfferType)
                                        {
                                            case MyOfferType.Default:
                                                break;
                                            case MyOfferType.WarToNeutral:
                                                pol.Status = MyPoliticStatus.Neutral;
                                                new MyEventRelationsChanged(MySave.Factions.Find(x => x.Tag == offer.Factions.Item1), MySave.Factions.Find(x => x.Tag == offer.Factions.Item2), offer.OfferType).Send();
                                                break;
                                            case MyOfferType.NeutralToAlly:
                                                pol.Status = MyPoliticStatus.Ally;
                                                new MyEventRelationsChanged(MySave.Factions.Find(x => x.Tag == offer.Factions.Item1), MySave.Factions.Find(x => x.Tag == offer.Factions.Item2), offer.OfferType).Send();
                                                break;
                                            case MyOfferType.AllyToNeutral:
                                                new MyEventRelationsChanged(MySave.Factions.Find(x => x.Tag == offer.Factions.Item1), MySave.Factions.Find(x => x.Tag == offer.Factions.Item2), offer.OfferType).Send();
                                                pol.Union = false;
                                                pol.Status = MyPoliticStatus.Neutral;
                                                break;
                                            case MyOfferType.NeutralToWar:
                                                new MyEventRelationsChanged(MySave.Factions.Find(x => x.Tag == offer.Factions.Item1), MySave.Factions.Find(x => x.Tag == offer.Factions.Item2), offer.OfferType).Send();
                                                pol.Status = MyPoliticStatus.War;
                                                string enemy = faction1 == _factionApi.Faction ? faction2.Tag : faction1.Tag;
                                                IEnumerable<MyPolitic> pols = MySave.Politics.Where(x => (x.Factions.Item1 == enemy) ^ (x.Factions.Item2 == enemy) && x.Union);
                                                List<MyFaction> factions = new List<MyFaction>();
                                                pols.ForEach(x =>
                                                {
                                                    if (x.Factions.Item1 == enemy) factions.Add(MySave.Factions.Find(y => y.Tag == x.Factions.Item2));
                                                    else factions.Add(MySave.Factions.Find(y => y.Tag == x.Factions.Item1));
                                                });
                                                factions.ForEach(y => MySave.Politics.Find(x => (x.Factions.Item1 == enemy) ^ (x.Factions.Item2 == enemy) && (x.Factions.Item1 == _factionApi.Faction.Tag) ^ (x.Factions.Item2 == _factionApi.Faction.Tag)).Status = MyPoliticStatus.War);
                                                break;
                                            default:
                                                throw new ArgumentOutOfRangeException();
                                        }

                                        if (offer.Options == MyOfferOptions.ChangeUnion) pol.Union = !pol.Union;
                                        else if (offer.Options == MyOfferOptions.CreatePact)
                                        {
                                            pol.Pact = true;
                                            pol.PactTurns = offer.PactTurns;
                                        }
                                    }
                                    foreach (MyPolitic p in MySave.Politics)
                            {
                                if (!p.Pact || p.Status == MyPoliticStatus.Ally) continue;
                                --p.PactTurns;
                                if (p.PactTurns <= 0)
                                {
                                    p.Pact = false;
                                    new MyEventPactEnded(p).Send();
                                }
                            }

                            new MyEventNextTurn().Send();
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
                        //case "interned": return arguments.Length < 3 ? "Неверное количество аргументов" : (string.IsInterned(arguments[2]) != null).ToString();
                        case "setvkgroup":
                        {
                            if (arguments.Length < 4) return "Неверное количество аргументов";
                            MyFaction faction = MySave.Factions.Find(x => x.Tag == arguments[2]);
                            if (faction is null) return "Фракция не найдена";
                            faction.VkUrl = arguments[3].Replace(' ','_');
                            return "Данные изменены";
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
clear - отчищает консоль.";
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
                        case "calls": return MyVkApi.Calls.ToString(CultureInfo.InvariantCulture);
                        /*case "disableconsole":
                        {
                            MyExtensions.FreeConsole();
                            return string.Empty;
                        }*/
                        default:
                            return "Неизвестная команда. Используйте команду \"botconsole help\" для получения справки";
                    }
                }
                default: return string.Empty;
            }
        }
    }
}
