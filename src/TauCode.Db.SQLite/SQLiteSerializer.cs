using System.Data;

namespace TauCode.Db.SQLite
{
    public class SQLiteSerializer : DbSerializerBase
    {
        public SQLiteSerializer(IDbConnection connection)
            : base(connection, null)
        {
        }

        public override IDbUtilityFactory Factory => SQLiteUtilityFactory.Instance;
    }
}
