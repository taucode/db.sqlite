namespace TauCode.Db.SQLite;

public class SQLiteScriptBuilder : ScriptBuilder
{
    #region Overridden

    public override IUtilityFactory Factory => SQLiteUtilityFactory.Instance;

    #endregion
}