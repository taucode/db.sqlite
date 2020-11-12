namespace TauCode.Db.SQLite
{
    [DbDialect(
        typeof(SQLiteDialect),
        "reserved-words.txt",
        "[],\"\"")]
    public class SQLiteDialect : DbDialectBase
    {
        public static readonly SQLiteDialect Instance = new SQLiteDialect();

        private SQLiteDialect()
            : base(DbProviderNames.SQLite)
        {
        }

        public override bool CanDecorateTypeIdentifier => false;

        public override IDbUtilityFactory Factory => SQLiteUtilityFactory.Instance;
    }
}
