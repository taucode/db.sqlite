using System;

namespace TauCode.Db.SQLite.Tests.DbTableInspector
{
    public class ColumnIdentityMoldInfo : IEquatable<ColumnIdentityMoldInfo>
    {
        public ColumnIdentityMoldInfo(string seed, string increment)
        {
            this.Seed = seed;
            this.Increment = increment;
        }

        public string Seed { get; }
        public string Increment { get; }

        public bool Equals(ColumnIdentityMoldInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Seed == other.Seed && Increment == other.Increment;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ColumnIdentityMoldInfo) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Seed, Increment);
        }
    }
}
