using System;
using System.Data.SQLite;
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

        // todo: move this to taucode.db
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
    }
}
