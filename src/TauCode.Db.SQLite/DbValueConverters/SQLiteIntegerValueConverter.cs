using System;

namespace TauCode.Db.SQLite.DbValueConverters
{
    public class SQLiteIntegerValueConverter : DbValueConverterBase
    {
        protected override object ToDbValueImpl(object value)
        {
            if (value.GetType().IsIntegerType() || value is bool)
            {
                return Convert.ToInt64(value);
            }

            return null;
        }

        protected override object FromDbValueImpl(object dbValue)
        {
            if (dbValue is long longValue)
            {
                return longValue;
            }

            return null;
        }
    }
}
