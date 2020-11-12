using System;
using System.Data;
using System.Data.SQLite;
using TauCode.Db.DbValueConverters;
using TauCode.Db.Model;
using TauCode.Db.SQLite.DbValueConverters;

namespace TauCode.Db.SQLite
{
    public class SQLiteCruder : DbCruderBase
    {
        public SQLiteCruder(SQLiteConnection connection)
            : base(connection, null)
        {
        }

        public override IDbUtilityFactory Factory => SQLiteUtilityFactory.Instance;
        protected override IDbValueConverter CreateDbValueConverter(ColumnMold column)
        {
            var typeName = column.Type.Name.ToLowerInvariant();

            switch (typeName)
            {
                case "uniqueidentifier":
                    return new GuidValueConverter();

                case "integer":
                    return new SQLiteIntegerValueConverter();

                case "numeric":
                    return new DecimalValueConverter();

                case "real":
                    return new DoubleValueConverter();

                case "datetime":
                    return new DateTimeValueConverter();

                case "time":
                    return new SQLiteTimeSpanValueConverter();

                case "text":
                    return new StringValueConverter();

                case "blob":
                    return new ByteArrayValueConverter();

                default:
                    throw new NotImplementedException();
            }
        }

        protected override IDbDataParameter CreateParameter(string tableName, ColumnMold column)
        {
            const string parameterName = "parameter_name_placeholder";
            var typeName = column.Type.Name.ToLowerInvariant();

            switch (typeName)
            {
                case "uniqueidentifier":
                    return new SQLiteParameter(parameterName, DbType.Guid);

                case "integer":
                    return new SQLiteParameter(parameterName, DbType.Int64);

                case "numeric":
                    return new SQLiteParameter(parameterName, DbType.Decimal);

                case "real":
                    return new SQLiteParameter(parameterName, DbType.Double);

                case "datetime":
                    return new SQLiteParameter(parameterName, DbType.DateTime);

                case "time":
                    return new SQLiteParameter(parameterName, DbType.String);

                case "text":
                    return new SQLiteParameter(parameterName, DbType.String);

                case "blob":
                    return new SQLiteParameter(parameterName, DbType.Binary);


                default:
                    throw new NotImplementedException();
            }
        }
    }
}
