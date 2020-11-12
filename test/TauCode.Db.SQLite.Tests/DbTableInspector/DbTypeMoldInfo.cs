using System;

namespace TauCode.Db.SQLite.Tests.DbTableInspector
{
    public class DbTypeMoldInfo : IEquatable<DbTypeMoldInfo>
    {
        public DbTypeMoldInfo(string name, int? size = null, int? precision = null, int? scale = null)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Size = size;
            this.Precision = precision;
            this.Scale = scale;
        }

        public string Name { get; }
        public int? Size { get; }
        public int? Precision { get; }
        public int? Scale { get; }

        public bool Equals(DbTypeMoldInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                Name == other.Name &&
                Size == other.Size &&
                Precision == other.Precision &&
                Scale == other.Scale;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DbTypeMoldInfo) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Size, Precision, Scale);
        }
    }
}
