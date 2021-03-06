﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EW.ObjectModel;
using EW.Utility.ObjectModel;

namespace EW.Utility
{
    static public class MySave
    {
        private const string SavePathPlayers = @"Save\Players\";
        private const string SavePathFactions = "Save\\Factions\\";
        private const string SavePathSectors = "Save\\Sectors\\";
        private const string SavePathPolitics = "Save\\Politics\\";
        private const string SavePathFights = "Save\\Fights\\";
        private const string SavePathOffers = "Save\\Offers\\";
        private const string SavePathScripts = "Save\\Scripts\\";
        private const string SavePathTimers = "Save\\Timers\\";
        private const string BackupPath = "Backups\\";

        private const string BotSettingsFile = "BotSettings.json";
        /*
        private const string SavePath = "C:\\ProgramData\\EngineersWars\\Save\\";
*/
        /*private const string SavePathPlayers = @"C:\ProgramData\EngineersWars\Save\Players\";
        private const string SavePathFactions = "C:\\ProgramData\\EngineersWars\\Save\\Factions\\";
        private const string SavePathSectors = "C:\\ProgramData\\EngineersWars\\Save\\Sectors\\";
        private const string SavePathPolitics = "C:\\ProgramData\\EngineersWars\\Save\\Politics\\";
        private const string SavePathFights = "C:\\ProgramData\\EngineersWars\\Save\\Fights\\";
        private const string SavePathOffers = "C:\\ProgramData\\EngineersWars\\Save\\Offers\\";
        private const string SavePathScripts = "C:\\ProgramData\\EngineersWars\\Save\\Scripts\\";
        private const string SavePathTimers = "C:\\ProgramData\\EngineersWars\\Save\\Timers\\";
        private const string BotSettingsFile = "C:\\ProgramData\\EngineersWars\\BotSettings.json";*/

        static readonly internal DataContractJsonSerializerSettings SerializerSettings = new DataContractJsonSerializerSettings
        {
            IgnoreExtensionDataObject = false,
            EmitTypeInformation = EmitTypeInformation.AsNeeded,
            UseSimpleDictionaryFormat = true,
            SerializeReadOnlyTypes = true
        };

        static readonly private DataContractJsonSerializer MyPlayerSerializer = new DataContractJsonSerializer(typeof(MyPlayer), SerializerSettings);

        static readonly private DataContractJsonSerializer MyFactionSerializer = new DataContractJsonSerializer(typeof(MyFaction), SerializerSettings);

        static readonly private DataContractJsonSerializer MySectorSerializer = new DataContractJsonSerializer(typeof(MySector), SerializerSettings);

        static readonly private DataContractJsonSerializer MyPoliticSerializer = new DataContractJsonSerializer(typeof(MyPolitic), SerializerSettings);

        static readonly private DataContractJsonSerializer MySectorFightSerializer = new DataContractJsonSerializer(typeof(MySectorFight), SerializerSettings);

        static readonly private DataContractJsonSerializer MyTradeShipFightSerializer = new DataContractJsonSerializer(typeof(MyTradeShipFight), SerializerSettings);

        static readonly private DataContractJsonSerializer MyCustomFightSerializer = new DataContractJsonSerializer(typeof(MyCustomFight), SerializerSettings);

        static readonly private DataContractJsonSerializer MyOfferSerializer = new DataContractJsonSerializer(typeof(MyOffer), SerializerSettings);

        static readonly private DataContractJsonSerializer MyScriptSerializer = new DataContractJsonSerializer(typeof(MyScript), SerializerSettings);

        static readonly private DataContractJsonSerializer MyTimerSerializer = new DataContractJsonSerializer(typeof(MyTimer), SerializerSettings);

        static public volatile ImmutableList<MyPlayer> Players;
        static internal volatile ImmutableList<MyFaction> Factions;
        static internal volatile ImmutableList<MySector> Sectors;
        static internal volatile ImmutableList<MyPolitic> Politics;
        static internal volatile ImmutableList<AMyFight> Fights;
        static internal volatile ImmutableList<MyOffer> Offers;
        static internal volatile ImmutableList<MyScript> Scripts;
        static internal volatile ImmutableList<MyTimer> Timers;

        static public MyBotSettings BotSettings;

        // ReSharper disable once UnusedMember.Local
        static private Timer _autoSave;

        static private void AutoSaveMethod(object arg) => Save();

        static public void CreateDirectories()
        {
            Directory.CreateDirectory(SavePathPlayers);
            Directory.CreateDirectory(SavePathFactions);
            Directory.CreateDirectory(SavePathSectors);
            Directory.CreateDirectory(SavePathPolitics);
            Directory.CreateDirectory(SavePathOffers);
            Directory.CreateDirectory(SavePathFights);
            Directory.CreateDirectory(SavePathScripts);
            Directory.CreateDirectory(SavePathTimers);
            Directory.CreateDirectory(BackupPath);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        static public void Save()
        {
            try
            {
                Console.WriteLine($"{DateTime.Now}: Начат процесс сохранения…");
                ZipFile.CreateFromDirectory("Save\\", BackupPath + DateTime.UtcNow.ToBinary().ToString(CultureInfo.InvariantCulture) + ".zip", CompressionLevel.Optimal, true, Encoding.UTF8);
                MyBotSettings.Save();
                List<string> files = new List<string>(64);
                files.AddRange(Directory.GetFiles(SavePathPlayers));
                files.AddRange(Directory.GetFiles(SavePathFactions));
                files.AddRange(Directory.GetFiles(SavePathSectors));
                files.AddRange(Directory.GetFiles(SavePathFights));
                files.AddRange(Directory.GetFiles(SavePathOffers));
                files.AddRange(Directory.GetFiles(SavePathPolitics));
                files.AddRange(Directory.GetFiles(SavePathTimers));
                files.AddRange(Directory.GetFiles(SavePathScripts));
                Parallel.ForEach(files, File.Delete);
                CreateDirectories();
                Parallel.Invoke(() => Parallel.ForEach(Players, item =>
                {
                    using (StreamWriter stream = File.CreateText(SavePathPlayers + item.Name + ".json"))
                    {
                        MyPlayerSerializer.WriteObject(stream.BaseStream, item);
                    }
                }), () => Parallel.ForEach(Factions, item =>
                {
                    using (StreamWriter stream = File.CreateText(SavePathFactions + item.Tag + ".json"))
                    {
                        MyFactionSerializer.WriteObject(stream.BaseStream, item);
                    }
                }), () => Parallel.ForEach(Sectors, item =>
                {
                    using (StreamWriter stream = File.CreateText(SavePathSectors + item.Name + ".json"))
                    {
                        MySectorSerializer.WriteObject(stream.BaseStream, item);
                    }
                }), () => Parallel.ForEach(Politics, item =>
                {
                    using (StreamWriter stream = File.CreateText(SavePathPolitics + item.Id + ".json"))
                    {
                        MyPoliticSerializer.WriteObject(stream.BaseStream, item);
                    }
                }), () => Parallel.ForEach(Offers, item =>
                {
                    using (StreamWriter stream = File.CreateText(SavePathOffers + $"{item.CreateTime:yy\\-mm\\-dd\\_HH\\-mm\\-ss}^{item.Factions.Item1}-{item.Factions.Item2}" + ".json"))
                    {
                        MyOfferSerializer.WriteObject(stream.BaseStream, item);
                    }
                }), () => Parallel.ForEach(Fights, item =>
                {
                    string prefix;
                    switch (item)
                    {
                        case MySectorFight _:
                            prefix = "S";
                            break;
                        case MyTradeShipFight _:
                            prefix = "T";
                            break;
                        case MyCustomFight _:
                            prefix = "C";
                            break;
                        default: throw new InvalidDataException();
                    }

                    using (StreamWriter stream = File.CreateText($"{SavePathOffers}{item.StartTime:yy\\-mm\\-dd\\_HH\\-mm\\-ss}^{item.AttackersTag}-{item.DefendersTag}@{prefix}.json"))
                    {
                        switch (prefix)
                        {
                            case "S":
                                MySectorFightSerializer.WriteObject(stream.BaseStream, item);
                                break;
                            case "T":
                                MyTradeShipFightSerializer.WriteObject(stream.BaseStream, item);
                                break;
                            case "C":
                                MyCustomFightSerializer.WriteObject(stream.BaseStream, item);
                                break;
                            default: throw new ArgumentException("Неизвестный тип боя", nameof(item));
                        }
                    }
                }), () => Parallel.ForEach(Scripts, item =>
                {
                    using (StreamWriter stream = File.CreateText(SavePathOffers + item.Name + ".json"))
                    {
                        MyScriptSerializer.WriteObject(stream.BaseStream, item);
                    }
                }), () => Parallel.ForEach(Timers, item =>
                {
                    using (StreamWriter stream = File.CreateText(SavePathOffers + item.Name + ".json"))
                    {
                        MyTimerSerializer.WriteObject(stream.BaseStream, item);
                    }
                }));

                Console.WriteLine($"{DateTime.Now}: Сохранение завершено");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{DateTime.Now}: Исключение во время попытки сохранения: {e.Message}\r\n{e.StackTrace}");
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        static public void Load()
        {
            try
            {
                Console.WriteLine($"{DateTime.Now}: Начат процесс загрузки…");
                MyBotSettings.Load();
                ImmutableList<MyPlayer>.Builder playersBuilder = ImmutableList.CreateBuilder<MyPlayer>();
                ImmutableList<MyFaction>.Builder factionsBuilder = ImmutableList.CreateBuilder<MyFaction>();
                ImmutableList<MySector>.Builder sectorsBuilder = ImmutableList.CreateBuilder<MySector>();
                ImmutableList<MyPolitic>.Builder politicsBuilder = ImmutableList.CreateBuilder<MyPolitic>();
                ImmutableList<AMyFight>.Builder fightsBuilder = ImmutableList.CreateBuilder<AMyFight>();
                ImmutableList<MyOffer>.Builder offersBuilder = ImmutableList.CreateBuilder<MyOffer>();
                ImmutableList<MyScript>.Builder scriptsBuilder = ImmutableList.CreateBuilder<MyScript>();
                ImmutableList<MyTimer>.Builder timersBuilder = ImmutableList.CreateBuilder<MyTimer>();

                Parallel.Invoke(() => Parallel.ForEach(Directory.GetFiles(SavePathPlayers), item =>
                {
                    using (FileStream stream = File.OpenRead(item))
                    {
                        MyPlayer player = (MyPlayer) MyPlayerSerializer.ReadObject(stream);
                        lock (playersBuilder)
                        {
                            playersBuilder.Add(player);
                        }
                    }
                }), () => Parallel.ForEach(Directory.GetFiles(SavePathFactions), item =>
                {
                    using (FileStream stream = File.OpenRead(item))
                    {
                        MyFaction faction = (MyFaction) MyFactionSerializer.ReadObject(stream);
                        lock (factionsBuilder)
                        {
                            factionsBuilder.Add(faction);
                        }
                    }
                }), () => Parallel.ForEach(Directory.GetFiles(SavePathSectors), item =>
                {
                    using (FileStream stream = File.OpenRead(item))
                    {
                        MySector sector = (MySector) MySectorSerializer.ReadObject(stream);
                        lock (sectorsBuilder)
                        {
                            sectorsBuilder.Add(sector);
                        }
                    }
                }), () => Parallel.ForEach(Directory.GetFiles(SavePathPolitics), item =>
                {
                    using (FileStream stream = File.OpenRead(item))
                    {
                        MyPolitic pol = (MyPolitic) MyPoliticSerializer.ReadObject(stream);
                        lock (politicsBuilder)
                        {
                            politicsBuilder.Add(pol);
                        }
                    }
                }), () => Parallel.ForEach(Directory.GetFiles(SavePathFights), item =>
                {
                    using (FileStream stream = File.OpenRead(item))
                    {
                        AMyFight fight;
                        if (item.Contains("@S"))
                            fight = (MySectorFight) MySectorFightSerializer.ReadObject(stream);
                        else if (item.Contains("@T"))
                            fight = (MyTradeShipFight) MyTradeShipFightSerializer.ReadObject(stream);
                        else if (item.Contains("@C"))
                            fight = (MyCustomFight) MyCustomFightSerializer.ReadObject(stream);
                        else
                            throw new FileLoadException("Неизвестный тип боя", item);

                        lock (fightsBuilder)
                        {
                            fightsBuilder.Add(fight);
                        }
                    }
                }), () => Parallel.ForEach(Directory.GetFiles(SavePathOffers), item =>
                {
                    using (FileStream stream = File.OpenRead(item))
                    {
                        MyOffer offer = (MyOffer) MyOfferSerializer.ReadObject(stream);
                        lock (offersBuilder)
                        {
                            offersBuilder.Add(offer);
                        }
                    }
                }), () => Parallel.ForEach(Directory.GetFiles(SavePathScripts), item =>
                {
                    using (FileStream stream = File.OpenRead(item))
                    {
                        MyScript scr = (MyScript) MyScriptSerializer.ReadObject(stream);
                        lock (scriptsBuilder)
                        {
                            scriptsBuilder.Add(scr);
                        }
                    }
                }), () => Parallel.ForEach(Directory.GetFiles(SavePathTimers), item =>
                {
                    using (FileStream stream = File.OpenRead(item))
                    {
                        MyTimer timer = (MyTimer) MyTimerSerializer.ReadObject(stream);
                        lock (timersBuilder)
                        {
                            timersBuilder.Add(timer);
                        }
                    }
                }));

                playersBuilder.Sort();
                factionsBuilder.Sort();
                sectorsBuilder.Sort();
                Players = playersBuilder.ToImmutable();
                Factions = factionsBuilder.ToImmutable();
                Sectors = sectorsBuilder.ToImmutable();
                Politics = politicsBuilder.ToImmutable();
                Fights = fightsBuilder.ToImmutable();
                Offers = offersBuilder.ToImmutable();
                Scripts = scriptsBuilder.ToImmutable();
                Timers = timersBuilder.ToImmutable();
                Console.WriteLine($"{DateTime.Now}: Загрузка завершена");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{DateTime.Now}: Исключение во время попытки загрузки: {e.Message}\r\n{e.StackTrace}");
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }

        [DataContract]
        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        public struct MyBotSettings
        {
            [IgnoreDataMember]
            static readonly private DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(MyBotSettings));

            [DataMember]
            readonly public bool UnsafeMode;
            [DataMember]
            readonly public bool ConfirmMode;
            [DataMember]
            readonly public string ConfirmString;
            [DataMember]
            readonly public string SecretCode;
            [DataMember]
            readonly public string VkToken;
            [DataMember]
            public bool EnableFights;
            [DataMember]
            public (TimeSpan, TimeSpan) ActivityTime;
            [DataMember]
            public int AutoSaveInterval;

            // ReSharper disable once UnusedMember.Global
            public MyBotSettings(bool unsafeMode, bool confirmMode, string confirmString, string secretCode, string vkToken, bool enableFights, (TimeSpan, TimeSpan) activityTime, int autosave)
            {
                UnsafeMode = unsafeMode;
                ConfirmMode = confirmMode;
                ConfirmString = confirmString ?? throw new ArgumentNullException(nameof(confirmString));
                SecretCode = secretCode ?? throw new ArgumentNullException(nameof(secretCode));
                VkToken = vkToken ?? throw new ArgumentNullException(nameof(vkToken));
                EnableFights = enableFights;
                ActivityTime = activityTime;
                AutoSaveInterval = autosave;
            }

            static internal void Load()
            {
                try
                {
                    using (FileStream stream = File.OpenRead(BotSettingsFile))
                    {
                        BotSettings = (MyBotSettings) Serializer.ReadObject(stream);
                    }
                }
                catch (FileNotFoundException)
                {
                    Save();
                    Load();
                }

                int interval = BotSettings.AutoSaveInterval == 0 ? Timeout.Infinite : BotSettings.AutoSaveInterval * 60000;
                _autoSave = new Timer(AutoSaveMethod, null, interval, interval);
            }

            static internal void Save()
            {
                using (StreamWriter stream = File.CreateText(BotSettingsFile))
                {
                    Serializer.WriteObject(stream.BaseStream, BotSettings);
                }
            }
        }
    }
}
