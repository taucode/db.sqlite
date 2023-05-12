namespace TauCode.Db.SQLite;

// todo regions

public class SQLiteUtilityFactory : IUtilityFactory
{
    public static SQLiteUtilityFactory Instance { get; } = new();

    private SQLiteUtilityFactory()
    {
    }

    public IDialect Dialect { get; } = new SQLiteDialect();

    public IScriptBuilder CreateScriptBuilder() => new SQLiteScriptBuilder();

    public IExplorer CreateExplorer() => new SQLiteExplorer();

    public ICruder CreateCruder() => new SQLiteCruder();
}