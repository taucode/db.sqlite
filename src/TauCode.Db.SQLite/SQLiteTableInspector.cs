using System.Data;
using System.Data.SQLite;

namespace TauCode.Db.SQLite
{
    public class SQLiteTableInspector : DbTableInspectorBase
    {
        public SQLiteTableInspector(SQLiteConnection connection, string tableName)
            : base(connection, null, tableName)
        {
        }

        public override IDbUtilityFactory Factory => SQLiteUtilityFactory.Instance;

        protected override IDbSchemaExplorer CreateSchemaExplorer(IDbConnection connection) =>
            new SQLiteSchemaExplorer((SQLiteConnection) this.Connection);
    }
}
