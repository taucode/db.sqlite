namespace TauCode.Db.SQLite
{
    public class SQLiteScriptBuilder : DbScriptBuilderBase
    {
        #region Constructor

        public SQLiteScriptBuilder()
            : base(null)
        {
        }

        #endregion

        #region Overridden

        public override IDbUtilityFactory Factory => SQLiteUtilityFactory.Instance;

        #endregion
    }
}
