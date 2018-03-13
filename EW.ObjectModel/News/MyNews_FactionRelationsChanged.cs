using System;

namespace EW.ObjectModel
{
    [Obsolete]
    public sealed class MyNewsFactionRelationsChanged : AMyNews
    {
        public readonly bool Faction2Iniciator;
        public readonly (string, string) Factions;
        public readonly MyOfferType OfferType;

        public MyNewsFactionRelationsChanged((string, string) factions, MyOfferType offerType, bool faction2Iniciator)
        {
            Factions = factions;
            OfferType = offerType;
            Faction2Iniciator = faction2Iniciator;
        }

        public override string ToString()
        {
            switch (OfferType)
            {
                case MyOfferType.WarToNeutral:
                    if (Faction2Iniciator)
                        return $"Фракция {Factions.Item2} заключает мир со фракцией {Factions.Item1}";
                    else return $"Фракция {Factions.Item1} заключает мир со фракцией {Factions.Item2}";
                case MyOfferType.NeutralToAlly: return $"Фракции {Factions.Item1} и {Factions.Item2} теперь союзники";
                case MyOfferType.AllyToNeutral:
                    if (Faction2Iniciator)
                        return $"Фракция {Factions.Item2} разрывает сотрудничество со фракцией {Factions.Item1}";
                    else return $"Фракция {Factions.Item1} разрывает сотрудничество со фракцией {Factions.Item2}";
                case MyOfferType.NeutralToWar:
                    if (Faction2Iniciator) return $"Фракция {Factions.Item2} объявляет войну фракции {Factions.Item1}";
                    else return $"Фракция {Factions.Item1} объявляет войну фракции {Factions.Item2}";
                default: return string.Empty;
            }
        }
    }
}
