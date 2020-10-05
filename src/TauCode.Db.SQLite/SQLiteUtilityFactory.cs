using System.Data;
using System.Data.SQLite;

namespace TauCode.Db.SQLite
{
    public class SQLiteUtilityFactory : IDbUtilityFactory
    {
        public static SQLiteUtilityFactory Instance { get; } = new SQLiteUtilityFactory();

        private SQLiteUtilityFactory()
        {
        }

        public IDbDialect GetDialect() => SQLiteDialect.Instance;

        public IDbScriptBuilder CreateScriptBuilder(string schema) => new SQLiteScriptBuilder();
        public IDbConnection CreateConnection() => new SQLiteConnection();

        public IDbInspector CreateInspector(IDbConnection connection, string schema) => new SQLiteInspector(connection);

        public IDbTableInspector CreateTableInspector(IDbConnection connection, string schema, string tableName) =>
            new SQLiteTableInspector(connection, tableName);

        public IDbCruder CreateCruder(IDbConnection connection, string schema) => new SQLiteCruder(connection);

        public IDbSerializer CreateSerializer(IDbConnection connection, string schema) => new SQLiteSerializer(connection);
    }
}
