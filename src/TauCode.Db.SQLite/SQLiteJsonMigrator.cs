using System;
using System.Data;

namespace TauCode.Db.SQLite
{
    public class SQLiteJsonMigrator : DbJsonMigratorBase
    {
        public SQLiteJsonMigrator(IDbConnection connection, Func<string> metadataJsonGetter, Func<string> dataJsonGetter)
            : base(connection, null, metadataJsonGetter, dataJsonGetter)
        {
        }

        public override IDbUtilityFactory Factory => SQLiteUtilityFactory.Instance;
    }
}
