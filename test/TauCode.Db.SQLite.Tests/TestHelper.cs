using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using TauCode.Db.Extensions;

namespace TauCode.Db.SQLite.Tests
{
    internal static class TestHelper
    {
        internal static SQLiteConnection CreateConnection(bool open = true, bool boost = true)
        {
            var tuple = SQLiteTools.CreateSQLiteDatabase();

            var connectionString = tuple.Item2;
            var connection = new SQLiteConnection(connectionString);

            if (open)
            {
                connection.Open();
            }

            if (boost)
            {
                connection.BoostSQLiteInsertions();
            }

            return connection;
        }

        internal static void PurgeDatabase(this SQLiteConnection connection)
        {
            new SQLiteSchemaExplorer(connection).DropAllTables(null);
        }

        internal static void WriteDiff(string actual, string expected, string directory, string fileExtension, string reminder)
        {
            if (reminder != "to" + "do")
            {
                throw new InvalidOperationException("don't forget this call with mark!");
            }

            fileExtension = fileExtension.Replace(".", "");

            var actualFileName = $"0-actual.{fileExtension}";
            var expectedFileName = $"1-expected.{fileExtension}";

            var actualFilePath = Path.Combine(directory, actualFileName);
            var expectedFilePath = Path.Combine(directory, expectedFileName);

            File.WriteAllText(actualFilePath, actual, Encoding.UTF8);
            File.WriteAllText(expectedFilePath, expected, Encoding.UTF8);
        }

        internal static IReadOnlyDictionary<string, object> LoadRow(
            SQLiteConnection connection,
            string tableName,
            object id)
        {
            IDbTableInspector tableInspector = new SQLiteTableInspector(connection, tableName);
            var table = tableInspector.GetTable();
            var pkColumnName = table.GetPrimaryKeySingleColumn().Name;

            using var command = connection.CreateCommand();
            command.CommandText = $@"
SELECT
    *
FROM
    [{tableName}]
WHERE
    [{pkColumnName}] = @p_id
";
            command.Parameters.AddWithValue("p_id", id);
            using var reader = command.ExecuteReader();

            var read = reader.Read();
            if (!read)
            {
                return null;
            }

            var dictionary = new Dictionary<string, object>();

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var value = reader[fieldName];

                if (value == DBNull.Value)
                {
                    value = null;
                }

                dictionary[fieldName] = value;
            }

            return dictionary;
        }

        internal static long GetTableRowCount(SQLiteConnection connection, string tableName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM [{tableName}]";
            var count = (long)command.ExecuteScalar();
            return count;
        }

        internal static long GetLastIdentity(this SQLiteConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT last_insert_rowid()";
            return (long)command.ExecuteScalar();
        }

        internal static void DropTable(this SQLiteConnection connection, string tableName)
            => new SQLiteSchemaExplorer(connection).DropTable(null, tableName);
    }
}
