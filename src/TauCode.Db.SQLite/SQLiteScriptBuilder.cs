using System.Text;
using TauCode.Db.Model;

namespace TauCode.Db.SQLite
{
    public class SQLiteScriptBuilder : DbScriptBuilderBase
    {
        public SQLiteScriptBuilder()
            : base(null)
        {
        }

        public override IDbUtilityFactory Factory => SQLiteUtilityFactory.Instance;

        protected override string BuildInsertScriptWithDefaultValues(TableMold table)
        {
            var decoratedTableName = this.Dialect.DecorateIdentifier(
                DbIdentifierType.Table,
                table.Name,
                this.CurrentOpeningIdentifierDelimiter);

            var sb = new StringBuilder();
            sb.Append("INSERT INTO ");
            this.WriteSchemaPrefixIfNeeded(sb);
            sb.Append($"{decoratedTableName} DEFAULT VALUES");

            return sb.ToString();
        }
    }
}
