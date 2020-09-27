namespace TauCode.Db.SQLite
{
    [DbDialect(
        typeof(SQLiteDialect),
        "reserved-words.txt",
        "[],\"\",``")]
    public class SQLiteDialect : DbDialectBase
    {
        #region Static

        public static readonly SQLiteDialect Instance = new SQLiteDialect();

        #endregion

        #region Constructor

        private SQLiteDialect()
            : base(DbProviderNames.SQLite)
        {
        }

        #endregion

        #region Overridden

        public override IDbUtilityFactory Factory => SQLiteUtilityFactory.Instance;

        public override bool CanDecorateTypeIdentifier => false;

        #endregion
    }
}
