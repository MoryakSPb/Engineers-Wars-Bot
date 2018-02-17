using System;
using System.Linq;
using EW.ObjectModel;

namespace EW.Utility.Api
{
    internal sealed class MyBotRegisterApi : MyBasicApi
    {
        static private readonly char[] IncorrectChars =
        {
            '"', '<', '>', '|', '\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\u000e', '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f', ':', '*', '?', '\\', '/'
        };

        public BotRegiserResult Register(string name, int vkId, ulong steamId)
        {
            if (vkId == 0) return BotRegiserResult.ConsoleNotAllowed;
            try
            {
                if (MySave.Players.Exists(x => x.Vk == vkId)) return BotRegiserResult.IsRegistered;
                if (MySave.Players.Exists(x => x.Steam == steamId)) return BotRegiserResult.SteamIsBusy;
                if (CheckName(name)) return BotRegiserResult.NameIsBusy;
                for (int i = 0; i < IncorrectChars.Length; i += 1)
                    if (name.Contains(IncorrectChars[i]))
                        return BotRegiserResult.InvalidName;
                if (long.TryParse(name, out var _)) return BotRegiserResult.InvalidName;
                MySave.Players = MySave.Players.Add(new MyPlayer(name, vkId, steamId));
                return 0;
            }
            catch (Exception)
            {
                return BotRegiserResult.Error;
            }
        }

        internal enum BotRegiserResult
        {
            Ok = 0,
            InvalidName = -1,
            NameIsBusy = -2,
            SteamIsBusy = -3,
            IsRegistered = -4,
            InvalidVk = -5,
            InvalidSteam64 = -6,
            ConsoleNotAllowed = -7,
            Error = int.MinValue
        }
    }
}