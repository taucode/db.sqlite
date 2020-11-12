using System;

namespace TauCode.Db.SQLite.DbValueConverters
{
    public class SQLiteTimeSpanValueConverter : DbValueConverterBase
    {
        protected override object ToDbValueImpl(object value)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan.ToString("c");
            }

            if (value is string stringValue)
            {
                var timeSpan2 = TimeSpan.Parse(stringValue);
                return timeSpan2.ToString("c");
            }

            return null;
        }

        protected override object FromDbValueImpl(object dbValue)
        {
            if (dbValue is DateTime dateTime)
            {
                return dateTime.TimeOfDay;
            }

            return null;
        }
    }
}
