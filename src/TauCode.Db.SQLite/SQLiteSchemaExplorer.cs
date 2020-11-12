using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using TauCode.Db.Model;
using TauCode.Db.SQLite.Parsing;

namespace TauCode.Db.SQLite
{
    public class SQLiteSchemaExplorer : DbSchemaExplorerBase
    {
        public SQLiteSchemaExplorer(SQLiteConnection connection)
            : base(connection, "[]")
        {
        }

        public override IReadOnlyList<string> GetSchemaNames() => new string[] { };

        public override IReadOnlyList<string> GetTableNames(string schemaName)
        {
            using var command = this.Connection.CreateCommand();
            command.CommandText = @"
SELECT
    T.name  Name,
    T.sql   Sql
FROM
    sqlite_master T
WHERE
    T.type = 'table' AND
    T.name NOT LIKE 'sqlite_%'
ORDER BY
    T.name
";

            var rows = command.GetCommandRows();

            var tableNames = rows
                .Select(x => (string)x.Name)
                .ToList();

            return tableNames;
        }

        public override IReadOnlyList<string> GetTableNames(string schemaName, bool independentFirst)
        {
            var tables = this.GetTables(
                schemaName,
                true,
                true,
                true,
                false,
                independentFirst);

            return tables.Select(x => x.Name).ToList();
        }

        public override IReadOnlyList<TableMold> GetTables(
            string schemaName,
            bool includeColumns,
            bool includePrimaryKey,
            bool includeForeignKeys,
            bool includeIndexes,
            bool? independentFirst)
        {
            if (schemaName != null)
            {
                throw new ArgumentException($"'{nameof(schemaName)}' must be null.", nameof(schemaName));
            }

            var validArgs =
                (includeColumns && includePrimaryKey && includeForeignKeys) ^
                (!includeColumns && !includePrimaryKey && !includeForeignKeys);

            if (!validArgs)
            {
                throw new ArgumentException(
                    $"'{nameof(includeColumns)}', '{nameof(includePrimaryKey)}', '{nameof(includeForeignKeys)}' must be all false or all true.");
            }

            if (independentFirst.HasValue && !includeForeignKeys)
            {
                throw new ArgumentException(
                    $"If '{nameof(independentFirst)}' has value, '{nameof(includeForeignKeys)}' must be true.");
            }

            using var command = this.Connection.CreateCommand();
            command.CommandText = @"
SELECT
    T.name  Name,
    T.sql   Sql
FROM
    sqlite_master T
WHERE
    T.type = 'table' AND
    T.name NOT LIKE 'sqlite_%'
ORDER BY
    T.name
";

            var rows = command.GetCommandRows();

            if (rows.Count == 0)
            {
                return new List<TableMold>();
            }

            var tableMolds = new List<TableMold>();

            foreach (var row in rows)
            {
                var tableName = (string)row.Name;
                var sql = (string)row.Sql;

                var tableMold = this.ParseTableCreationSql(sql);

                if (includeIndexes)
                {
                    var indexes = this.GetTableIndexes(null, tableName, false);
                    tableMold.Indexes = indexes.ToList();
                }

                tableMolds.Add(tableMold);
            }

            if (independentFirst.HasValue)
            {
                tableMolds = DbTools.ArrangeTables(tableMolds, independentFirst.Value);
            }

            return tableMolds;
        }

        public override TableMold GetTable(
            string schemaName,
            string tableName,
            bool includeColumns,
            bool includePrimaryKey,
            bool includeForeignKeys,
            bool includeIndexes)
        {
            if (schemaName != null)
            {
                throw new ArgumentException($"'{nameof(schemaName)}' must be null.", nameof(schemaName));
            }

            var validArgs =
                (includeColumns && includePrimaryKey && includeForeignKeys) ^
                (!includeColumns && !includePrimaryKey && !includeForeignKeys);

            if (!validArgs)
            {
                throw new ArgumentException(
                    $"'{nameof(includeColumns)}', '{nameof(includePrimaryKey)}', '{nameof(includeForeignKeys)}' must be all false or all true.");
            }

            using var command = this.Connection.CreateCommand();
            command.CommandText = @"
SELECT
    T.name  Name,
    T.sql   Sql
FROM
    sqlite_master T
WHERE
    T.type = 'table' AND
    T.name NOT LIKE 'sqlite_%' AND
    T.name = @p_tableName
ORDER BY
    T.name
";

            command.AddParameterWithValue("p_tableName", tableName);

            var rows = command.GetCommandRows();

            if (rows.Count == 0)
            {
                throw DbTools.CreateTableDoesNotExistException(null, tableName);
            }

            var row = rows.Single();
            var tableMold = ParseTableCreationSql((string)row.Sql).ResolveExplicitPrimaryKey();

            if (includeIndexes)
            {
                var indexes = this.GetTableIndexes(null, tableName, false);
                tableMold.Indexes = indexes.ToList();
            }

            return tableMold;
        }

        private NotSupportedException CreateNotSupportedException()
        {
            throw new NotSupportedException($"Use method '{nameof(GetTable)}'.");
        }

        public override IReadOnlyList<ColumnMold> GetTableColumns(
            string schemaName,
            string tableName,
            bool checkExistence)
            => throw this.CreateNotSupportedException();

        public override IReadOnlyList<ForeignKeyMold> GetTableForeignKeys(
            string schemaName,
            string tableName,
            bool loadColumns,
            bool checkExistence)
            => throw this.CreateNotSupportedException();

        public override PrimaryKeyMold GetTablePrimaryKey(string schemaName, string tableName, bool checkExistence)
            => throw this.CreateNotSupportedException();

        private TableMold ParseTableCreationSql(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            var parser = SQLiteParser.Instance;
            var objs = parser.Parse(sql);

            if (objs.Length != 1 || !(objs.Single() is TableMold))
            {
                throw new ArgumentException($"Could not build table definition from script:{Environment.NewLine}{sql}",
                    nameof(sql));
            }

            return objs.Single() as TableMold;
        }

        protected override ColumnMold ColumnInfoToColumn(ColumnInfo columnInfo)
        {
            throw new NotSupportedException();
        }

        protected override IReadOnlyList<IndexMold> GetTableIndexesImpl(string schemaName, string tableName)
        {
            throw new NotSupportedException();
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            if (schemaName != null)
            {
                throw new ArgumentException($"'{nameof(schemaName)}' must be null.", nameof(schemaName));
            }

            using var command = this.Connection.CreateCommand();
            command.CommandText = @"
SELECT
    T.name  Name,
    T.sql   Sql
FROM
    sqlite_master T
WHERE
    T.type = 'table' AND
    T.name NOT LIKE 'sqlite_%' AND
    T.name = @p_tableName
";

            command.AddParameterWithValue("p_tableName", tableName);
            using var reader = command.ExecuteReader();
            var exists = reader.Read();

            return exists;
        }

        public override bool SchemaExists(string schemaName) => false; // no schema support for SQLite

        public override IReadOnlyList<IndexMold> GetTableIndexes(string schemaName, string tableName, bool checkExistence)
        {
            if (schemaName != null)
            {
                throw new ArgumentException($"'{nameof(schemaName)}' must be null.", nameof(schemaName));
            }

            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (checkExistence)
            {
                if (!this.TableExists(null, tableName))
                {
                    throw DbTools.CreateTableDoesNotExistException(null, tableName);
                }
            }

            using var command = this.Connection.CreateCommand();
            command.CommandText = @"
SELECT
    T.name    Name,
    T.sql     Sql
FROM
    sqlite_master T
WHERE
    T.type = 'index' AND
    T.tbl_name = @p_tableName AND
    T.name NOT LIKE 'sqlite_%'
ORDER BY
    T.name
";

            command.AddParameterWithValue("p_tableName", tableName);

            var parser = SQLiteParser.Instance;

            var indexes = command
                .GetCommandRows()
                .Select(x => (IndexMold)parser.Parse((string)x.Sql).Single())
                .ToList();

            return indexes;
        }

        protected override void ResolveIdentities(string schemaName, string tableName, IList<ColumnInfo> columnInfos)
        {
            throw new NotSupportedException();
        }

        public override IReadOnlyList<string> GetSystemSchemaNames() => new string[] { };

        public override string DefaultSchemaName => null;
    }
}
