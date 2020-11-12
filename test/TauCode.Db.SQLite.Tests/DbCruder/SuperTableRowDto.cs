using System;

namespace TauCode.Db.SQLite.Tests.DbCruder
{
    public class SuperTableRowDto
    {
        public long Id { get; set; }
        public Guid TheGuid { get; set; }
        public long TheBigInt { get; set; }
        public decimal TheDecimal { get; set; }
        public double TheReal { get; set; }
        public DateTime TheDateTime { get; set; }
        public TimeSpan TheTime { get; set; }
        public string TheText { get; set; }
        public byte[] TheBlob { get; set; }
        
        public int? NotExisting { get; set; }
    }
}
