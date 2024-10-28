using System.Data;
using System.Data.SQLite;
using TauCode.Db.Model.Interfaces;

namespace TauCode.Db.SQLite;

public class SQLiteCruder : Cruder
{
    #region ctor

    public SQLiteCruder()
    {
    }

    public SQLiteCruder(SQLiteConnection? connection)
        : base(connection)
    {

    }

    #endregion

    #region Overridden

    public override IUtilityFactory Factory => SQLiteUtilityFactory.Instance;

    protected override void TuneParameter(IDbDataParameter parameter, IColumnMold columnMold)
    {
        DbType dbType;
        int size = 0;

        switch (columnMold.Type.Name)
        {
            case "bit":
                dbType = DbType.Boolean;
                break;

            case "int":
                dbType = DbType.Int32;
                break;

            case "smallint":
                dbType = DbType.Int16;
                break;

            case "bigint":
                dbType = DbType.Int64;
                break;

            case "decimal":
                dbType = DbType.Decimal;
                break;

            case "uniqueidentifier":
                dbType = DbType.Guid;
                break;

            case "varchar":
                dbType = DbType.String;
                size = columnMold.Type.Size ?? 0;
                break;

            case "datetimeoffset":
                dbType = DbType.DateTimeOffset;
                size = 10; // todo hardcoded; without this, command.Prepare() throws an exception.
                break;

            default:
                throw new NotImplementedException();
        }

        parameter.DbType = dbType;
        parameter.Size = size;
    }

    protected override void OnBeforeInsertRows(ITableMold tableMold, IRowSet rows, Func<string, bool>? fieldSelector = null)
    {
        var identityColumn = tableMold.Columns.SingleOrDefault(x => x.Identity != null);

        if (identityColumn != null)
        {
            var schemaName = tableMold.SchemaName;
            var tableName = tableMold.Name;

            var sql = $@"SET IDENTITY_INSERT {schemaName}.{tableName} ON";
            this.Connection.ExecuteSql(sql);
        }
    }

    protected override void OnAfterInsertRows(ITableMold tableMold, IRowSet rows, Func<string, bool>? fieldSelector = null)
    {
        var identityColumn = tableMold.Columns.SingleOrDefault(x => x.Identity != null);

        if (identityColumn != null)
        {
            var schemaName = tableMold.SchemaName;
            var tableName = tableMold.Name;

            var sql = $@"SET IDENTITY_INSERT {schemaName}.{tableName} OFF";
            this.Connection.ExecuteSql(sql);
        }
    }

    #endregion
}
