using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using TauCode.Db.Model;
using TauCode.Extensions;

namespace TauCode.Db.SQLite
{
    public static class SQLiteTools
    {
        /// <summary>
        /// Boosts SQLite insertions
        /// https://stackoverflow.com/questions/3852068/sqlite-insert-very-slow
        /// </summary>
        /// <param name="sqLiteConnection">SQLite connection to boost</param>
        public static void BoostSQLiteInsertions(this SQLiteConnection sqLiteConnection)
        {
            if (sqLiteConnection == null)
            {
                throw new ArgumentNullException(nameof(sqLiteConnection));
            }

            using var command = sqLiteConnection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode = WAL";
            command.ExecuteNonQuery();

            command.CommandText = "PRAGMA synchronous = NORMAL";
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Creates temporary .sqlite file and returns a SQLite connection string for this file.
        /// </summary>
        /// <returns>
        /// Tuple with two strings. Item1 is temporary file path, Item2 is connection string.
        /// </returns>
        public static Tuple<string, string> CreateSQLiteDatabase()
        {
            var tempDbFilePath = FileTools.CreateTempFilePath("zunit", ".sqlite");
            SQLiteConnection.CreateFile(tempDbFilePath);

            var connectionString = $"Data Source={tempDbFilePath};Version=3;";

            return Tuple.Create(tempDbFilePath, connectionString);
        }

        internal static TableMold ResolveExplicitPrimaryKey(this TableMold tableMold)
        {
            var pkColumn = tableMold.Columns.SingleOrDefault(x =>
                ((Dictionary<string, string>)x.Properties).GetValueOrDefault("#is_explicit_primary_key") == "true");

            if (pkColumn != null)
            {
                tableMold.PrimaryKey = new PrimaryKeyMold
                {
                    Name = $"PK_{tableMold.Name}",
                    Columns = new List<string>
                    {
                        pkColumn.Name
                    },
                };
            }

            return tableMold;
        }
    }
}
