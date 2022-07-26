using System.Data.SQLite;

namespace TauCode.Db.SQLite
{
    public class SQLiteSerializer : DbSerializerBase
    {
        public SQLiteSerializer(SQLiteConnection connection)
            : base(connection, null)
        {
        }

        public override IDbUtilityFactory Factory => SQLiteUtilityFactory.Instance;
    }
}
