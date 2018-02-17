using EW.ObjectModel;

namespace EW.Utility
{
    static internal class MyStrings
    {
        internal const string Rocket = "🚀";
        internal const string Flag = "🏴";

        static internal string GetPlayerStatusDescription(PlayerStatus playerStatus)
        {
            switch (playerStatus)
            {
                case PlayerStatus.Guest: return "Гость";
                case PlayerStatus.Mercenary: return "Наемник";
                case PlayerStatus.FactionMember: return "Член фракции";
                default: return default;
            }
        }

        static internal string GetFactionStatusDescription(FactionType factionType)
        {
            switch (factionType)
            {
                case FactionType.Military: return "Военная";
                case FactionType.Resettlement: return "Переселенческая";
                case FactionType.Research: return "Исследовательская";
                case FactionType.Industrial: return "Индусриальная";
                case FactionType.Commercial: return "Комерчерская";
                case FactionType.Pirate: return "Пиратская";
                case FactionType.Adventurous: return "Авантюристская";
                default: return default;
            }
        }

        static internal string GetShipNameMany(ShipType shipType)
        {
            switch (shipType)
            {
                case ShipType.Fighter: return "Истребители";
                case ShipType.Corvette: return "Корветы";
                default: return default;
            }
        }

        static internal string GetShipNameOnce(ShipType shipType)
        {
            switch (shipType)
            {
                case ShipType.Fighter: return "Истребитель";
                case ShipType.Corvette: return "Корвет";
                default: return shipType.ToString();
            }
        }

        static internal string GetSectorImprovementType(SectorImprovementType improvement)
        {
            switch (improvement)
            {
                case SectorImprovementType.Headquarters: return "Штаб";
                case SectorImprovementType.Mine: return "Шахта";
                case SectorImprovementType.Factory: return "Завод";
                case SectorImprovementType.Storage: return "Склад";
                case SectorImprovementType.Hangar: return "Ангар";
                case SectorImprovementType.Powerstation: return "Энергостанция";
                case SectorImprovementType.Outpost: return "Аванпост";
                case SectorImprovementType.None: return "(нет)";
                default: return default;
            }
        }

        static internal string GetSectorType(SectorType sectorType)
        {
            switch (sectorType)
            {
                case SectorType.Default: return "Обычный";
                case SectorType.Monolith: return "С монолитом";
                default: return default;
            }
        }

        static internal string GetPolitic(MyPoliticStatus status)
        {
            switch (status)
            {
                case MyPoliticStatus.War: return "Война";
                case MyPoliticStatus.Neutral: return "Нейтралитет";
                case MyPoliticStatus.Ally: return "Сотрудничество";
                default: return default;
            }
        }

        static internal string GetFightType(AMyFight fight)
        {
            if (fight is MySectorFight) return Flag;
            if (fight is MyTradeShipFight) return Rocket;
            return "⚔";
        }

        static internal string GetBoolOnOff(bool logic) => logic ? "включено" : "выключено";

        static internal string GetBoolYesNo(bool logic) => logic ? "🗹" : "🗷";
    }
}