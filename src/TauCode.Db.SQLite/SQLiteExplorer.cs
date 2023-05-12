using System.Data.SQLite;
using TauCode.Db.Collections;
using TauCode.Db.Model.Interfaces;
using TauCode.Db.Model.Molds;

namespace TauCode.Db.SQLite;

public class SQLiteExplorer : Explorer
{
    #region Constants

    private static readonly HashSet<string> SystemSchemaNames = new(new[]
    {
        "guest",
        "information_schema",
        "sys",
        "db_owner",
        "db_accessadmin",
        "db_securityadmin",
        "db_ddladmin",
        "db_backupoperator",
        "db_datareader",
        "db_datawriter",
        "db_denydatareader",
        "db_denydatawriter",
    });

    #endregion

    #region ctor

    public SQLiteExplorer()
    {
    }

    public SQLiteExplorer(SQLiteConnection connection)
        : base(connection)
    {
    }

    #endregion

    #region Private

    private IDialect GetDialect() => this.Factory.Dialect;

    #endregion

    #region Protected

    protected int GetTableObjectId(string schemaName, string tableName)
    {
        // todo checks

        using var command = (SQLiteCommand)this.Connection.CreateCommand();
        command.CommandText =
            @"
SELECT
    T.object_id
FROM
    sys.tables T
INNER JOIN
    sys.schemas S
ON
    T.schema_id = S.schema_id
WHERE
    T.name = @p_tableName AND
    S.name = @p_schemaName
";
        command.Parameters.AddWithValue("p_tableName", tableName);
        command.Parameters.AddWithValue("p_schemaName", schemaName);

        var objectResult = command.ExecuteScalar();

        if (objectResult == null)
        {
            // should not happen, we are checking table existence.
            throw new NotImplementedException();
        }

        var objectId = (int)objectResult;
        return objectId;
    }

    protected IColumnIdentityMold? ResolveIdentity(string schemaName, string tableName/*, IList<ColumnInfo> columnInfos*/)
    {
        var objectId = this.GetTableObjectId(schemaName, tableName);

        using var command = (SQLiteCommand)this.Connection.CreateCommand();
        command.CommandText =
            @"
SELECT
    IC.name             Name,
    IC.seed_value       SeedValue,
    IC.increment_value  IncrementValue
FROM
    sys.identity_columns IC
WHERE
    IC.object_id = @p_objectId
";
        command.Parameters.AddWithValue("p_objectId", objectId);

        var identities = (ListRowSet)command
            .ReadRowsFromCommand();

        if (identities.Count == 0)
        {
            return null; // no identity column
        }

        if (identities.Count != 1)
        {
            throw new NotImplementedException();
        }

        var row = identities.Cast<IRow>().Single();

        return new ColumnIdentityMold
        {
            ColumnName = (string)row["Name"],
            Seed = row["SeedValue"]?.ToString(),
            Increment = row["IncrementValue"].ToString()!,
        };
    }


    #endregion

    #region Overridden

    public override IUtilityFactory Factory => SQLiteUtilityFactory.Instance;

    public override IReadOnlyList<string> GetSchemaNames()
    {
        var connection = this.GetOpenSqlConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    S.schema_name SchemaName
FROM
    information_schema.schemata S";

        var schemaNames = command
            .ReadRowsFromCommand()
            .Select(x => (string)x["SchemaName"])
            .Where(x => !IsSystemSchemaName(x))
            .ToList();

        return schemaNames;
    }

    private bool IsSystemSchemaName(string schemaName)
    {
        return SystemSchemaNames.Contains(schemaName.ToLowerInvariant());
    }

    public override TableMold GetTable(
        string? schemaName,
        string tableName,
        bool getConstraints = true,
        bool getIndexes = true)
    {
        // todo: should throw if schema/table does not exist

        if (schemaName == null)
        {
            throw new ArgumentNullException(nameof(schemaName));
        }

        schemaName = this.GetDialect().Undelimit(schemaName);
        tableName = this.GetDialect().Undelimit(tableName);

        var connection = this.GetOpenSqlConnection();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    C.column_name                   ColumnName,
    C.is_nullable                   IsNullable,
    C.data_type                     DataType,
    C.character_maximum_length      MaxLength,
    C.numeric_precision             NumPrecision,
    C.numeric_scale                 NumScale
FROM
    information_schema.columns C
WHERE
    C.table_name = @p_tableName AND
    C.table_schema = @p_schemaName
ORDER BY
    C.ordinal_position
";

        command.Parameters.AddWithValue("p_tableName", tableName);
        command.Parameters.AddWithValue("p_schemaName", schemaName);

        var rows = command.ReadRowsFromCommand();

        var columnMolds = new List<IColumnMold>();

        foreach (var row in rows)
        {
            var name = (string)row["ColumnName"];
            var dataType = (string)row["DataType"];

            var columnMold = new ColumnMold
            {
                Name = name,
                Type = new DbTypeMold
                {
                    Name = dataType,
                },
            };

            var maxLength = row["MaxLength"];
            if (maxLength is not DBNull)
            {
                columnMold.Type.Size = (int)maxLength;
            }

            var precision = row["NumPrecision"];
            if (precision is not DBNull)
            {
                columnMold.Type.Precision = (byte)precision;
            }

            var scale = row["NumScale"];
            if (scale is not DBNull)
            {
                columnMold.Type.Scale = (int)scale;
            }

            columnMolds.Add(columnMold);
        }

        var tableMold = new TableMold
        {
            SchemaName = schemaName,
            Name = tableName,
            Columns = columnMolds,
        };

        #region deal with identity

        var identity = this.ResolveIdentity(schemaName, tableName);
        if (identity != null)
        {
            var identityColumn = columnMolds.Single(x => x.Name == identity.ColumnName);
            identityColumn.Identity = identity;
        }

        #endregion

        return tableMold;
    }

    private SQLiteConnection GetOpenSqlConnection()
    {
        var connection = this.GetOpenConnection();
        var sqlConnection = connection as SQLiteConnection;
        if (sqlConnection == null)
        {
            throw new NotImplementedException();
        }

        return sqlConnection;
    }

    protected override IReadOnlyList<string> GetTableNamesImpl(string schemaName)
    {
        if (schemaName == null)
        {
            throw new ArgumentNullException(nameof(schemaName));
        }

        // todo: support both quoted and non-quoted schema names, e.g. dbo/[dbo]

        var connection = (SQLiteConnection)this.GetOpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = @"
SELECT
    tc.TABLE_NAME TableName
FROM
    INFORMATION_SCHEMA.TABLES tc
WHERE
    tc.TABLE_SCHEMA = @p_schemaName
";

        command.Parameters.AddWithValue("p_schemaName", schemaName);

        var tableNames = command
            .ReadRowsFromCommand()
            .Select(x => (string)x["TableName"])
            .ToList();

        return tableNames;
    }

    #endregion
}