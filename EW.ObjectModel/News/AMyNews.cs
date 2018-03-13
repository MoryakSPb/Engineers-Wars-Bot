using System;
using System.Globalization;
using System.Runtime.Serialization;

// ReSharper disable ImpureMethodCallOnReadonlyValueField

namespace EW.ObjectModel
{
    [DataContract]
    [Obsolete]
    public abstract class AMyNews
    {
        public readonly DateTime CreateTime;

        protected AMyNews() => CreateTime = DateTime.UtcNow;

        public override int GetHashCode() => CreateTime.GetHashCode();

        public override string ToString() => CreateTime.ToString(CultureInfo.InvariantCulture);
    }
}
