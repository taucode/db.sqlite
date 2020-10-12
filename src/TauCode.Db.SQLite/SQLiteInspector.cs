using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TauCode.Db.SQLite
{
    public class SQLiteInspector : DbInspectorBase
    {
        #region Constructor

        public SQLiteInspector(IDbConnection connection)
            : base(connection, null)
        {
        }

        #endregion

        #region Overridden

        public override IDbUtilityFactory Factory => SQLiteUtilityFactory.Instance;

        protected override IReadOnlyList<string> GetTableNamesImpl(string schemaName)
        {
            using var command = this.Connection.CreateCommand();

            command.CommandText =
                @"
SELECT
    T.name TableName
FROM
    sqlite_master T
WHERE
    type = @p_type
    AND
    T.name <> @p_sequenceName";

            command.AddParameterWithValue("p_type", "table");
            command.AddParameterWithValue("p_sequenceName", "sqlite_sequence");

            return DbTools
                .GetCommandRows(command)
                .Select(x => (string)x.TableName)
                .ToArray();
        }

        protected override HashSet<string> GetSystemSchemata()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
