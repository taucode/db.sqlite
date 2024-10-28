using System.Data.SQLite;

namespace TauCode.Db.SQLite;

public class SqlInstructionProcessor : InstructionProcessor
{
    #region ctor

    public SqlInstructionProcessor()
    {

    }

    public SqlInstructionProcessor(SQLiteConnection connection)
        : base(connection)
    {

    }

    #endregion

    #region Overridden

    public override IUtilityFactory Factory => SQLiteUtilityFactory.Instance;

    #endregion
}