using System;

namespace EW.Utility.ObjectModel
{
    public class MyTimer : IComparable<MyTimer>
    {
        public bool Enabled;
        public bool EveryDay;
        public string Name;
        public string ScriptName;
        public DateTime Time;

        public MyTimer(string name, bool enabled, DateTime time, string scriptName)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Enabled = enabled;
            if (time.Year == 1 && time.Month == 1 && time.Day == 1)
            {
                time = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, time.Hour, time.Minute, 0);
                if (time <= DateTime.UtcNow) time = time.AddDays(1d);
                EveryDay = true;
            }
            else
            {
                EveryDay = false;
            }

            Time = time;
            ScriptName = scriptName ?? throw new ArgumentNullException(nameof(scriptName));
        }

        public int CompareTo(MyTimer other) => Time.CompareTo(other.Time);
    }
}
