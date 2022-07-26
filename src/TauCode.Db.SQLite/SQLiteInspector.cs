using System.Data;
using System.Data.SQLite;

namespace TauCode.Db.SQLite
{
    public class SQLiteInspector : DbInspectorBase
    {
        public SQLiteInspector(SQLiteConnection connection)
            : base(connection, null)
        {
        }

        public override IDbUtilityFactory Factory => SQLiteUtilityFactory.Instance;
    }
}
