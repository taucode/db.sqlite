using System;

namespace TauCode.Db.SQLite.Tests.DbCruder
{
    public class HealthInfoDto
    {
        public Guid Id { get; set; }
        public long PersonId { get; set; }
        public decimal Weight { get; set; }
        public short PersonMetaKey { get; set; }
        public decimal IQ { get; set; }
        public short Temper { get; set; }
        public byte PersonOrdNumber { get; set; }
        public int MetricB { get; set; }
        public int MetricA { get; set; }
        public int NotExisting { get; set; }
    }
}
