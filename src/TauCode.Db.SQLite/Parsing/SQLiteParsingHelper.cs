using System;
using System.Collections.Generic;
using System.Linq;
using TauCode.Db.Model;
using TauCode.Extensions;

namespace TauCode.Db.SQLite.Parsing
{
    internal static class SQLiteParsingHelper
    {
        private static readonly HashSet<string> ReservedWords;

        static SQLiteParsingHelper()
        {
            ReservedWords = typeof(SQLiteParsingHelper)
                .Assembly
                .GetResourceText("reserved-words.txt", true)
                .Split(new[]
                    {
                        Environment.NewLine
                    },
                    StringSplitOptions.None)
                .Select(x => x.ToLowerInvariant())
                .ToHashSet();
        }

        internal static bool IsReservedWord(string text) => ReservedWords.Contains(text.ToLowerInvariant());

        internal static SortDirection SqlToSortDirection(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            switch (sql.ToLowerInvariant())
            {
                case "asc":
                    return SortDirection.Ascending;

                case "desc":
                    return SortDirection.Descending;

                default:
                    throw new ArgumentException($"Unknown sort direction: '{sql}'.", nameof(sql));
            }
        }

        internal static void SetLastConstraintName(this TableMold tableMold, string value)
        {
            tableMold.Properties["#last-constraint-name"] = value;
        }

        internal static string GetLastConstraintName(this TableMold tableMold)
        {
            return tableMold.Properties["#last-constraint-name"];
        }
    }
}
