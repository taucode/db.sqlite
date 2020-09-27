using System;
using System.Data;
using System.Data.SQLite;
using TauCode.Db.DbValueConverters;
using TauCode.Db.Model;

namespace TauCode.Db.SQLite
{
    public class SQLiteCruder : DbCruderBase
    {
        private static readonly int GuidRepresentationLength = Guid.Empty.ToString().Length;

        public SQLiteCruder(IDbConnection connection)
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

                case "text":
                    return new StringValueConverter();

                case "datetime":
                    return new DateTimeValueConverter();

                case "integer":
                    return new SQLiteInt64ValueConverter();

                case "blob":
                    return new ByteArrayValueConverter();

                case "real":
                    return new DoubleValueConverter();

                case "numeric":
                    return new DecimalValueConverter();

                default:
                    throw new NotSupportedException($"Type name '{typeName}' not supported.");
            }
        }

        protected override IDbDataParameter CreateParameter(string tableName, ColumnMold column)
        {
            const string parameterName = "parameter_name_placeholder";

            var typeName = column.Type.Name.ToLowerInvariant();

            switch (typeName)
            {
                case "uniqueidentifier":
                    return new SQLiteParameter(parameterName, DbType.AnsiStringFixedLength, GuidRepresentationLength);

                case "text":
                    return new SQLiteParameter(parameterName, DbType.String, -1);

                case "datetime":
                    return new SQLiteParameter(parameterName, DbType.DateTime);

                case "integer":
                    return new SQLiteParameter(parameterName, DbType.Int64);

                case "numeric":
                    return new SQLiteParameter(parameterName, DbType.Decimal);

                case "real":
                    return new SQLiteParameter(parameterName, DbType.Double);

                case "blob":
                    return new SQLiteParameter(parameterName, DbType.Binary, -1);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
