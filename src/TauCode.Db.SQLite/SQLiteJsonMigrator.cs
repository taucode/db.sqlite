using System;
using System.Data;
using System.Data.SQLite;

namespace TauCode.Db.SQLite
{
    public class SQLiteJsonMigrator : DbJsonMigratorBase
    {
        public SQLiteJsonMigrator(
            SQLiteConnection connection,
            Func<string> metadataJsonGetter,
            Func<string> dataJsonGetter,
            Func<string, bool> tableNamePredicate = null)
            : base(
                connection,
                null,
                metadataJsonGetter,
                dataJsonGetter,
                tableNamePredicate)
        {
        }

        protected override void CheckSchema()
        {
            // idle
        }

        public SQLiteConnection SQLiteConnection => (SQLiteConnection)this.Connection;

        public override IDbUtilityFactory Factory => SQLiteUtilityFactory.Instance;

        protected override IDbSchemaExplorer CreateSchemaExplorer(IDbConnection connection)
        {
            return new SQLiteSchemaExplorer(this.SQLiteConnection);
        }
    }
}
