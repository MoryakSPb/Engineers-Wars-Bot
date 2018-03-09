using System;
using System.Collections.Generic;

namespace EW.Utility
{
    static public class MyExtensions
    {
        static public DateTime CreateDateTimeFromUnixtime(this sbyte timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp);

        static public DateTime CreateDateTimeFromUnixtime(this byte timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp);

        static public DateTime CreateDateTimeFromUnixtime(this short timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp);

        static public DateTime CreateDateTimeFromUnixtime(this ushort timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp);

        static public DateTime CreateDateTimeFromUnixtime(this int timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp);

        static public DateTime CreateDateTimeFromUnixtime(this uint timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp);

        static public DateTime CreateDateTimeFromUnixtime(this long timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp);

        static public DateTime CreateDateTimeFromUnixtime(this ulong timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp);

        static public DateTime CreateDateTimeFromUnixtime(this float timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp);

        static public DateTime CreateDateTimeFromUnixtime(this double timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp);

        static public DateTime CreateDateTimeFromUnixtime(this decimal timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((double) timestamp);

        static public void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T obj in source) action(obj);
        }

        static public int Round(this double real) => (int) Math.Round(real, MidpointRounding.AwayFromZero);
    }
}
