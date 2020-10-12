using System.Data;
using System.Data.SQLite;

namespace TauCode.Db.SQLite
{
    // todo: check 'schemaName' is null! +ut.
    public class SQLiteUtilityFactory : IDbUtilityFactory
    {
        public static SQLiteUtilityFactory Instance { get; } = new SQLiteUtilityFactory();

        private SQLiteUtilityFactory()
        {
        }

        public IDbDialect GetDialect() => SQLiteDialect.Instance;

        public IDbScriptBuilder CreateScriptBuilder(string schemaName) => new SQLiteScriptBuilder();
        public IDbConnection CreateConnection() => new SQLiteConnection();

        public IDbInspector CreateInspector(IDbConnection connection, string schemaName) => new SQLiteInspector(connection);

        public IDbTableInspector CreateTableInspector(IDbConnection connection, string schemaName, string tableName) =>
            new SQLiteTableInspector(connection, tableName);

        public IDbCruder CreateCruder(IDbConnection connection, string schemaName) => new SQLiteCruder(connection);

        public IDbSerializer CreateSerializer(IDbConnection connection, string schemaName) => new SQLiteSerializer(connection);
    }
}
