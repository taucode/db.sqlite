namespace TauCode.Db.SQLite;

public class SQLiteDialect : Dialect
{
    public override IUtilityFactory Factory => SQLiteUtilityFactory.Instance;
    public override string Name => "SQL Server";

    public override string Undelimit(string identifier)
    {
        // todo temp!

        return identifier.Replace("[", "").Replace("]", "");
    }
}
