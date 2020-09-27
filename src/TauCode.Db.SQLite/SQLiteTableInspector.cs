using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TauCode.Db.Exceptions;
using TauCode.Db.Model;
using TauCode.Db.SQLite.Parsing;

namespace TauCode.Db.SQLite
{
    public sealed class SQLiteTableInspector : DbTableInspectorBase
    {
        #region Constructor

        public SQLiteTableInspector(
            IDbConnection connection,
            string tableName)
            : base(connection, null, tableName)
        {
        }

        #endregion

        #region Private

        private string GetTableCreationSqlFromDb()
        {
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText =
@"
SELECT
    T.name  Name,
    T.sql   Sql
FROM
    sqlite_master T
WHERE
    T.type = @p_type
    AND
    T.name = @p_tableName
";

                command.AddParameterWithValue("p_type", "table");
                command.AddParameterWithValue("p_tableName", this.TableName);

                var rows = DbTools.GetCommandRows(command);
                if (rows.Count == 0)
                {
                    throw DbTools.CreateTableNotFoundException(this.TableName);
                }

                if (rows.Count > 1)
                {
                    throw new TauDbException($"Internal error: more than one metadata row returned.");
                }

                return rows
                    .Single()
                    .Sql;
            }
        }

        private TauDbException UseGetTableMethodException() => new TauDbException($"Use '{nameof(GetTable)}' method instead.");

        #endregion

        #region Overridden

        public override TableMold GetTable()
        {
            var sql = this.GetTableCreationSqlFromDb();
            var parser = SQLiteParser.Instance;
            var table = (TableMold)parser.Parse(sql).Single();

            table.Indexes = this.GetIndexes().ToList();

            return table;
        }

        public override IDbUtilityFactory Factory => SQLiteUtilityFactory.Instance;

        protected override List<ColumnInfo> GetColumnInfos() => throw new NotSupportedException(); // shouldn't ever be called

        protected override ColumnMold ColumnInfoToColumnMold(ColumnInfo columnInfo) => throw new NotSupportedException(); // shouldn't ever be called

        protected override Dictionary<string, ColumnIdentityMold> GetIdentities() => throw this.UseGetTableMethodException();

        public override IReadOnlyList<ColumnMold> GetColumns() => throw this.UseGetTableMethodException();

        public override PrimaryKeyMold GetPrimaryKey() => throw this.UseGetTableMethodException();

        public override IReadOnlyList<ForeignKeyMold> GetForeignKeys() => throw this.UseGetTableMethodException();

        public override IReadOnlyList<IndexMold> GetIndexes()
        {
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText =
@"
SELECT
    T.[name]    Name,
    T.[sql]     Sql
FROM
    sqlite_master T
WHERE
    T.[type] = @p_type
    AND
    T.[tbl_name] = @p_tableName
    AND
    T.[name] NOT LIKE @p_antiPattern
";
                command.AddParameterWithValue("p_type", "index");
                command.AddParameterWithValue("p_tableName", this.TableName);
                command.AddParameterWithValue("p_antiPattern", "sqlite_autoindex_%");

                var parser = SQLiteParser.Instance;

                var indexes = DbTools
                    .GetCommandRows(command)
                    .Select(x => (IndexMold)parser.Parse((string)x.Sql).Single())
                    .ToList();

                return indexes;
            }
        }

        #endregion
    }
}
