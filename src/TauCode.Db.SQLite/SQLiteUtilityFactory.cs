using System;
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

        public IDbScriptBuilder CreateScriptBuilder(string schemaName)
        {
            CheckSchemaNameIsNull(schemaName);

            return new SQLiteScriptBuilder();
        }

        public IDbConnection CreateConnection() => new SQLiteConnection();

        public IDbSchemaExplorer CreateSchemaExplorer(IDbConnection connection)
        {
            return new SQLiteSchemaExplorer((SQLiteConnection)connection);
        }

        public IDbInspector CreateInspector(IDbConnection connection, string schemaName)
        {
            CheckSchemaNameIsNull(schemaName);

            return new SQLiteInspector((SQLiteConnection)connection);
        }

        public IDbTableInspector CreateTableInspector(IDbConnection connection, string schemaName, string tableName)
        {
            CheckSchemaNameIsNull(schemaName);

            return new SQLiteTableInspector((SQLiteConnection)connection, tableName);
        }

        public IDbCruder CreateCruder(IDbConnection connection, string schemaName)
        {
            CheckSchemaNameIsNull(schemaName);

            return new SQLiteCruder((SQLiteConnection)connection);
        }

        public IDbSerializer CreateSerializer(IDbConnection connection, string schemaName)
        {
            CheckSchemaNameIsNull(schemaName);

            return new SQLiteSerializer((SQLiteConnection)connection);
        }

        private static void CheckSchemaNameIsNull(string schemaName, string schemaArgumentName = "schemaName")
        {
            if (schemaName != null)
            {
                throw new ArgumentException($"'{schemaArgumentName}' must be null.");
            }
        }
    }
}
