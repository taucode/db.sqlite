using System;

namespace TauCode.Db.SQLite
{
    public class SQLiteInt64ValueConverter : IDbValueConverter
    {
        public object ToDbValue(object value)
        {
            if (
                value is bool ||
                value is byte ||
                value is sbyte ||
                value is short ||
                value is ushort ||
                value is int ||
                value is uint ||
                value is long ||
                value is ulong)
            {
                return Convert.ToInt64(value);
            }

            return null;
        }

        public object FromDbValue(object dbValue)
        {
            if (dbValue is long longValue)
            {
                return longValue;
            }

            return null;
        }
    }
}
