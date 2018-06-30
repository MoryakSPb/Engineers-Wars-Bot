using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace EW.ObjectModel
{
    [Serializable]
    [DataContract]
    public struct MyResourses : IEquatable<MyResourses>, IComparable<MyResourses>
    {
        [DataMember]
        public int Iron { get; set; }

        [DataMember]
        public int Energy { get; set; }

        [DataMember]
        public int Ammo { get; set; }

        [DataMember]
        public int MonolithCharges { get; set; }

        [DataMember]
        public int ShipSlots { get; set; }

        [DataMember]
        public int Production { get; set; }

        public const int Length = 6;

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Iron;
                    case 1: return Energy;
                    case 2: return Ammo;
                    case 3: return MonolithCharges;
                    case 4: return ShipSlots;
                    case 5: return Production;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Iron = value;
                        return;
                    case 1:
                        Energy = value;
                        return;
                    case 2:
                        Ammo = value;
                        return;
                    case 3:
                        MonolithCharges = value;
                        return;
                    case 4:
                        ShipSlots = value;
                        return;
                    case 5:
                        Production = value;
                        return;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public MyResourses(int iron, int energy, int ammo, int monolithcharges, int shipslots, int production)
        {
            Iron = iron;
            Energy = energy;
            Ammo = ammo;
            MonolithCharges = monolithcharges;
            ShipSlots = shipslots;
            Production = production;
        }

        public MyResourses((int, int, int, int, int, int) tuple) => (Iron, Energy, Ammo, MonolithCharges, ShipSlots, Production) = tuple;

        public (int Iron, int Energy, int Ammo, int MonolithCharges, int ShipSlots, int Production) ToValueTuple() => (Iron, Energy, Ammo, MonolithCharges, ShipSlots, Production);
        public Tuple<int, int, int, int, int, int> ToTuple() => new Tuple<int, int, int, int, int, int>(Iron, Energy, Ammo, MonolithCharges, ShipSlots, Production);

        static public MyResourses operator -(MyResourses resourses) => Negate(resourses);

        static public MyResourses Negate(MyResourses resourses) => new MyResourses
        {
            Ammo = -resourses.Ammo,
            Energy = -resourses.Energy,
            Iron = -resourses.Iron,
            MonolithCharges = -resourses.MonolithCharges,
            Production = -resourses.Production,
            ShipSlots = -resourses.ShipSlots
        };

        static public MyResourses operator +(MyResourses summand1, MyResourses summand2) => Add(summand1, summand2);

        static public MyResourses Add(MyResourses summand1, MyResourses summand2) => new MyResourses
        {
            Iron = summand1.Iron + summand2.Iron,
            Ammo = summand1.Ammo + summand2.Ammo,
            Energy = summand1.Energy + summand2.Energy,
            MonolithCharges = summand1.MonolithCharges + summand2.MonolithCharges,
            Production = summand1.Production + summand2.Production,
            ShipSlots = summand1.ShipSlots + summand2.ShipSlots
        };

        static public MyResourses operator -(MyResourses minuend, MyResourses subtrahend) => Subtract(minuend, subtrahend);

        static public MyResourses Subtract(MyResourses minuend, MyResourses subtrahend) => new MyResourses
        {
            Iron = minuend.Iron - subtrahend.Iron,
            Ammo = minuend.Ammo - subtrahend.Ammo,
            Energy = minuend.Energy - subtrahend.Energy,
            MonolithCharges = minuend.MonolithCharges - subtrahend.MonolithCharges,
            Production = minuend.Production - subtrahend.Production,
            ShipSlots = minuend.ShipSlots - subtrahend.ShipSlots
        };

        static public MyResourses operator *(MyResourses multiplicand, double multiplier) => Multiply(multiplicand, multiplier);

        static public MyResourses Multiply(MyResourses multiplicand, double multiplier) => new MyResourses
        {
            Iron = (int) Math.Round(multiplicand.Iron * multiplier),
            Ammo = (int) Math.Round(multiplicand.Ammo * multiplier),
            Energy = (int) Math.Round(multiplicand.Energy * multiplier),
            MonolithCharges = (int) Math.Round(multiplicand.MonolithCharges * multiplier),
            Production = (int) Math.Round(multiplicand.Production * multiplier),
            ShipSlots = (int) Math.Round(multiplicand.ShipSlots * multiplier)
        };

        static public MyResourses operator *(MyResourses multiplicand, int multiplier) => Multiply(multiplicand, multiplier);

        static public MyResourses Multiply(MyResourses multiplicand, int multiplier) => new MyResourses
        {
            Iron = multiplicand.Iron * multiplier,
            Ammo = multiplicand.Ammo * multiplier,
            Energy = multiplicand.Energy * multiplier,
            MonolithCharges = multiplicand.MonolithCharges * multiplier,
            Production = multiplicand.Production * multiplier,
            ShipSlots = multiplicand.ShipSlots * multiplier
        };

        static public MyResourses operator *(MyResourses multiplicand, MyResourses multiplier) => Multiply(multiplicand, multiplier);

        static public MyResourses Multiply(MyResourses multiplicand, MyResourses multiplier) => new MyResourses
        {
            Iron = multiplicand.Iron * multiplier.Iron,
            Ammo = multiplicand.Ammo * multiplier.Ammo,
            Energy = multiplicand.Energy * multiplier.Energy,
            MonolithCharges = multiplicand.MonolithCharges * multiplier.MonolithCharges,
            Production = multiplicand.Production * multiplier.Production,
            ShipSlots = multiplicand.ShipSlots * multiplier.ShipSlots
        };

        static public MyResourses operator /(MyResourses multiplicand, double multiplier) => Divide(multiplicand, multiplier);

        static public MyResourses Divide(MyResourses multiplicand, double multiplier) => new MyResourses
        {
            Iron = (int) Math.Round(multiplicand.Iron / multiplier),
            Ammo = (int) Math.Round(multiplicand.Ammo / multiplier),
            Energy = (int) Math.Round(multiplicand.Energy / multiplier),
            MonolithCharges = (int) Math.Round(multiplicand.MonolithCharges / multiplier),
            Production = (int) Math.Round(multiplicand.Production / multiplier),
            ShipSlots = (int) Math.Round(multiplicand.ShipSlots / multiplier)
        };

        static public MyResourses operator /(MyResourses multiplicand, int multiplier) => Divide(multiplicand, multiplier);

        static public MyResourses Divide(MyResourses multiplicand, int multiplier) => new MyResourses
        {
            Iron = multiplicand.Iron / multiplier,
            Ammo = multiplicand.Ammo / multiplier,
            Energy = multiplicand.Energy / multiplier,
            MonolithCharges = multiplicand.MonolithCharges / multiplier,
            Production = multiplicand.Production / multiplier,
            ShipSlots = multiplicand.ShipSlots / multiplier
        };

        static public MyResourses operator /(MyResourses multiplicand, MyResourses multiplier) => Divide(multiplicand, multiplier);

        static public MyResourses Divide(MyResourses multiplicand, MyResourses multiplier) => new MyResourses
        {
            Iron = multiplicand.Iron / multiplier.Iron,
            Ammo = multiplicand.Ammo / multiplier.Ammo,
            Energy = multiplicand.Energy / multiplier.Energy,
            MonolithCharges = multiplicand.MonolithCharges / multiplier.MonolithCharges,
            Production = multiplicand.Production / multiplier.Production,
            ShipSlots = multiplicand.ShipSlots / multiplier.ShipSlots
        };

        static public MyResourses operator %(MyResourses resourses, int precent) => Precent(resourses, precent);

        static public MyResourses Precent(MyResourses resourses, int precent)
        {
            if (precent <= 0) throw new ArgumentException();
            (double iron, double energy, double ammo, double monolithCharges, double shipSlots, double production) = resourses;
            iron = iron / 100d * (precent - 100d);
            energy = energy / 100d * (precent - 100d);
            ammo = ammo / 100d * (precent - 100d);
            monolithCharges = monolithCharges / 100d * (precent - 100d);
            shipSlots = shipSlots / 100d * (precent - 100d);
            production = production / 100d * (precent - 100d);
            MyResourses mod = new MyResourses(iron.Round(), energy.Round(), ammo.Round(), monolithCharges.Round(), shipSlots.Round(), production.Round());

            return new MyResourses
            {
                Iron = resourses.Iron > 0 ? resourses.Iron + mod.Iron : resourses.Iron - mod.Iron,
                Energy = resourses.Energy > 0 ? resourses.Energy + mod.Energy : resourses.Energy - mod.Energy,
                Ammo = resourses.Ammo > 0 ? resourses.Ammo + mod.Ammo : resourses.Ammo - mod.Ammo,
                MonolithCharges = resourses.MonolithCharges > 0 ? resourses.MonolithCharges + mod.MonolithCharges : resourses.MonolithCharges - mod.MonolithCharges,
                ShipSlots = resourses.ShipSlots > 0 ? resourses.ShipSlots + mod.ShipSlots : resourses.ShipSlots - mod.ShipSlots,
                Production = resourses.Production > 0 ? resourses.Production + mod.Production : resourses.Production - mod.Production
            };
        }

        [Obsolete("", true)]
        static public MyResourses operator %(MyResourses resourses, double precent) => Precent(resourses, precent);

        [Obsolete("", true)]
        static public MyResourses Precent(MyResourses resourses, double precent)
        {
            if (precent < 0) throw new ArgumentException();
            if (double.IsInfinity(1d / precent)) return new MyResourses();
            return resourses / 100 * precent;
        }

        [Obsolete("", true)]
        static public MyResourses operator %(MyResourses resourses, decimal precent) => Precent(resourses, precent);

        [Obsolete("", true)]
        static public MyResourses Precent(MyResourses resourses, decimal precent)
        {
            if (precent < 0) throw new ArgumentException();
            if (precent == 0m) return new MyResourses();
            return resourses / 100 * (double) precent;
        }

        public new MyResourses MemberwiseClone() => (MyResourses) base.MemberwiseClone();
        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj) => obj is MyResourses && Equals((MyResourses) obj);

        public bool Equals(MyResourses other) => Iron == other.Iron && Energy == other.Energy && Ammo == other.Ammo && MonolithCharges == other.MonolithCharges && ShipSlots == other.ShipSlots && Production == other.Production;

        public bool CheckCost(MyResourses cost) => Iron >= cost.Iron && Energy >= cost.Energy && Ammo >= cost.Ammo && MonolithCharges >= cost.MonolithCharges && Production >= cost.Production && ShipSlots >= cost.ShipSlots;

        public override string ToString() => $"{{{Iron};{Energy};{Ammo};{MonolithCharges};{ShipSlots};{Production}}}";

        static public bool TryParse(string s, out MyResourses resourses)
        {
            resourses = default;
            try
            {
                resourses = Parse(s);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        static public MyResourses Parse(string s)
        {
            string[] x = s.Trim('{', '}').Split(';');
            return new MyResourses
            {
                Iron = int.Parse(x[0], CultureInfo.InvariantCulture),
                Energy = int.Parse(x[1], CultureInfo.InvariantCulture),
                Ammo = int.Parse(x[2], CultureInfo.InvariantCulture),
                MonolithCharges = int.Parse(x[3], CultureInfo.InvariantCulture),
                ShipSlots = int.Parse(x[4], CultureInfo.InvariantCulture),
                Production = int.Parse(x[5], CultureInfo.InvariantCulture)
            };
        }

        public void Deconstruct(out int iron, out int energy, out int ammo, out int monolithCharges, out int shipSlots, out int production)
        {
            iron = Iron;
            energy = Energy;
            ammo = Ammo;
            monolithCharges = MonolithCharges;
            shipSlots = ShipSlots;
            production = Production;
        }

        public int CompareTo(MyResourses other)
        {
            //if (other is null) return 1;
            int num = 0;
            num += Iron.CompareTo(other.Iron);
            num += Ammo.CompareTo(other.Ammo);
            num += Energy.CompareTo(other.Energy);
            if (num < 0) return -1;
            if (num > 0) return 1;
            return 0;
        }

        [IgnoreDataMember]
        public MyResourses Basic => new MyResourses(Iron, Energy, Ammo, 0, 0, 0);

        [IgnoreDataMember]
        public MyResourses Cost => new MyResourses(Iron, Energy, Ammo, MonolithCharges, 0, 0);
    }
}
